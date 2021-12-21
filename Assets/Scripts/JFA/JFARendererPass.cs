using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Fab.Geo
{
    public class JFARendererPass : ScriptableRenderPass
    {
        // used to label this pass in Unity's Frame Debug utility
        string profilerTag;

        RenderTargetIdentifier cameraColorTargetIdent;
        RenderTargetHandle temp1Texture;
        RenderTargetHandle temp2Texture;
        RenderTextureDescriptor tempDescriptor;

        private int InitSeedKernel;
        private int JFAKernel;
        private int FillVoronoiDiagramKernel;
        private int FillDistanceTransformKernel;

        private ComputeShader JFAShader;
        private JFAType DisplayType = JFAType.VoronoiDiagram;
        private float speed;

        private Vector2Int[] Seeds;
        private Vector2[] SeedPos;
        private Vector2[] SeedSpeeds;
        private Vector3[] Colors;

        private ComputeBuffer seedBuffer;
        private ComputeBuffer colorBuffer;

        private Material BlitMat;

        public JFARendererPass(
            string profilerTag,
            RenderPassEvent renderPassEvent,
            ComputeShader JFAShader,
            int seedAmount,
            Material blitMat)
        {
            this.profilerTag = profilerTag;
            this.renderPassEvent = renderPassEvent;
            this.JFAShader = JFAShader;
            this.BlitMat = blitMat;


            Seeds = new Vector2Int[seedAmount];
            SeedSpeeds = new Vector2[seedAmount];
            SeedPos = new Vector2[seedAmount];

            Colors = new Vector3[seedAmount];
            for (int i = 0; i < seedAmount; i++)
            {
                Seeds[i] = new Vector2Int(Random.Range(1, 2000), Random.Range(1, 2000));

                SeedSpeeds[i] = new Vector2(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f));
                SeedPos[i] = Seeds[i];

                Colors[i] = new Vector3(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
            }
            seedBuffer = new ComputeBuffer(seedAmount, sizeof(int) * 2);
            seedBuffer.SetData(Seeds);
            colorBuffer = new ComputeBuffer(seedAmount, sizeof(float) * 3);
            colorBuffer.SetData(Colors);

            InitSeedKernel = JFAShader.FindKernel("InitSeed");
            JFAKernel = JFAShader.FindKernel("JFA");
            FillVoronoiDiagramKernel = JFAShader.FindKernel("FillVoronoiDiagram");
            FillDistanceTransformKernel = JFAShader.FindKernel("FillDistanceTransform");
        }

        // This isn't part of the ScriptableRenderPass class and is our own addition.
        // For this custom pass we need the camera's color target, so that gets passed in.
        public void Setup(RenderTargetIdentifier cameraColorTargetIdent, float speed)
        {
            this.cameraColorTargetIdent = cameraColorTargetIdent;
            this.speed = speed;
        }

        // called each frame before Execute, use it to set up things the pass will need
        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            // create a temporary render texture that matches the camera
            tempDescriptor = cameraTextureDescriptor;
            tempDescriptor.enableRandomWrite = true;
            tempDescriptor.bindMS = false;

            cmd.GetTemporaryRT(temp1Texture.id, tempDescriptor);
            cmd.GetTemporaryRT(temp2Texture.id, tempDescriptor);



            for (int i = 0; i < Seeds.Length; i++)
            {
                float tempX = SeedPos[i].x + SeedSpeeds[i].x * Time.deltaTime * speed;
                float tempY = SeedPos[i].y + SeedSpeeds[i].y * Time.deltaTime * speed;
                float SpeedX = SeedSpeeds[i].x;
                float SpeedY = SeedSpeeds[i].y;
                if (tempX < 0 || tempX >= cameraTextureDescriptor.width)
                {
                    SpeedX = -SpeedX;
                    tempX = SeedPos[i].x + SpeedX * Time.deltaTime * speed;
                }
                if (tempY < 0 || tempY >= cameraTextureDescriptor.height)
                {
                    SpeedY = -SpeedY;
                    tempY = SeedPos[i].y + SpeedY * Time.deltaTime * speed;
                }
                SeedPos[i] = new Vector2(tempX, tempY);
                SeedSpeeds[i] = new Vector2(SpeedX, SpeedY);
                Seeds[i] = new Vector2Int((int)SeedPos[i].x, (int)SeedPos[i].y);
            }
        }

        // Execute is called for every eligible camera every frame. It's not called at the moment that
        // rendering is actually taking place, so don't directly execute rendering commands here.
        // Instead use the methods on ScriptableRenderContext to set up instructions.
        // RenderingData provides a bunch of (not very well documented) information about the scene
        // and what's being rendered.
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            // fetch a command buffer to use
            CommandBuffer cmd = CommandBufferPool.Get(profilerTag);
            cmd.Clear();

            int width = tempDescriptor.width;
            int height = tempDescriptor.height;

            // Init Seed
            seedBuffer.SetData(Seeds);

            cmd.SetComputeBufferParam(JFAShader, InitSeedKernel, "Seeds", seedBuffer);
            cmd.SetComputeTextureParam(JFAShader, InitSeedKernel, "Source", temp1Texture.id);
            cmd.SetComputeIntParam(JFAShader, "Width", width);
            cmd.SetComputeIntParam(JFAShader, "Height", height);
            cmd.DispatchCompute(JFAShader, InitSeedKernel, Seeds.Length, 1, 1);

            // JFA
            int stepAmount = (int)Mathf.Log(Mathf.Max(width, height), 2);
            //Debug.Log("stepAmount:"+ stepAmount);
            int threadGroupsX = Mathf.CeilToInt(width / 8.0f);
            int threadGroupsY = Mathf.CeilToInt(height / 8.0f);
            for (int i = 0; i < stepAmount; i++)
            {
                int step = (int)Mathf.Pow(2, stepAmount - i - 1);
                //Debug.Log("step:" + step);
                cmd.SetComputeIntParam(JFAShader, "Step", step);
                cmd.SetComputeTextureParam(JFAShader, JFAKernel, "Source", temp1Texture.id);
                cmd.SetComputeTextureParam(JFAShader, JFAKernel, "Result", temp2Texture.id);

                cmd.DispatchCompute(JFAShader, JFAKernel, threadGroupsX, threadGroupsY, 1);
                cmd.Blit(temp2Texture.Identifier(), temp1Texture.Identifier());
            }

            // Fill with Color
            switch (DisplayType)
            {
                case JFAType.VoronoiDiagram:
                    cmd.SetComputeBufferParam(JFAShader, FillVoronoiDiagramKernel, "Colors", colorBuffer);
                    cmd.SetComputeTextureParam(JFAShader, FillVoronoiDiagramKernel, "Source", temp1Texture.id);
                    cmd.SetComputeTextureParam(JFAShader, FillVoronoiDiagramKernel, "Result", temp2Texture.id);
                    cmd.DispatchCompute(JFAShader, FillVoronoiDiagramKernel, threadGroupsX, threadGroupsY, 1);
                    break;
                case JFAType.DistanceTransform:
                    cmd.SetComputeTextureParam(JFAShader, FillDistanceTransformKernel, "Source", temp1Texture.id);
                    cmd.SetComputeTextureParam(JFAShader, FillDistanceTransformKernel, "Result", temp2Texture.id);
                    cmd.DispatchCompute(JFAShader, FillDistanceTransformKernel, threadGroupsX, threadGroupsY, 1);
                    break;
            }
            
            cmd.Blit(temp2Texture.Identifier(), cameraColorTargetIdent);

            //cmd.Blit(cameraColorTargetIdent, temp1Texture.Identifier(), BlitMat, 0);
            //cmd.Blit(temp1Texture.Identifier(), cameraColorTargetIdent);

            // don't forget to tell ScriptableRenderContext to actually execute the commands
            context.ExecuteCommandBuffer(cmd);


            // tidy up after ourselves
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }

        // called after Execute, use it to clean up anything allocated in Configure
        public override void FrameCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(temp1Texture.id);
            cmd.ReleaseTemporaryRT(temp2Texture.id);
        }
    }
}
