using NaughtyAttributes;
using Unity.Mathematics;
using UnityEngine;
using Fab.Geo.Editor;

namespace Fab.Geo
{
    public class GenerateDistanceField : MonoBehaviour
    {
        private static readonly int MaxIterations = 5000;

        [SerializeField]
        private ComputeShader compute;
        [SerializeField]
        private Texture2D source;
        [SerializeField]
        private RenderTexture maskOut;
        [SerializeField]
        private Texture2D output;


        private ComputeBuffer changedFlagBuffer;

        [Button]
        public void Generate()
        {
            if (compute == null)
                return;

            if(changedFlagBuffer == null)
                changedFlagBuffer = new ComputeBuffer(1, sizeof(int));

            Generate(source);
        }

        private void CreateMask()
        {
            RenderTexture mask = new RenderTexture(new RenderTextureDescriptor(source.width, source.height, RenderTextureFormat.RFloat));
            mask.wrapMode = TextureWrapMode.Repeat;
            mask.enableRandomWrite = true;
            mask.name = "Mask";
            Graphics.Blit(source, mask);

            int prepKernel = compute.FindKernel("PrepareMask");
            compute.SetTexture(prepKernel, "Mask", mask);

            int groupX = (int)math.ceil(mask.width / 8f);
            int groupY = (int)math.ceil(mask.height / 8f);
            compute.Dispatch(prepKernel, groupX, groupY, 1);
            maskOut = mask;
        }

        private void Generate(Texture2D source)
        {
            CreateMask();

            int passKernel = compute.FindKernel("DistancePass");

            RenderTexture distance = new RenderTexture(new RenderTextureDescriptor(source.width, source.height, RenderTextureFormat.RFloat));
            distance.wrapMode = TextureWrapMode.Repeat;
            distance.enableRandomWrite = true;
            distance.name = "Distance";
            Graphics.Blit(maskOut, distance);

            compute.SetTexture(passKernel, "SqrDistanceField", distance);
            ExecutePass(distance, new int2(0, 1));
            ExecutePass(distance, new int2(1, 0));

            output = EditorUtils.SaveTextureEXR(distance.ToTexture2D(), Texture2D.EXRFlags.OutputAsFloat);
        }

        private void ExecutePass(RenderTexture texture, int2 offset)
        {
            compute.SetInts("passOffest", offset.x, offset.y);
            compute.SetInts("resolution", texture.width, texture.height);
            int kernel = compute.FindKernel("DistancePass");

            for (int i = 0; i < MaxIterations; i++)
            {
                changedFlagBuffer.SetData(new int[] { 0 });
                compute.SetBuffer(kernel, "ChangedFlag", changedFlagBuffer);
                compute.SetInt("iterationCount", i);

                int groupX = (int)math.ceil(texture.width / 8f);
                int groupY = (int)math.ceil(texture.height / 8f);
                compute.Dispatch(kernel, groupX, groupY, 1);

                int[] numChanges = new int[1];
                changedFlagBuffer.GetData(numChanges);
                if (numChanges[0] == 0)
                {
                    Debug.Log("Runs " + i);
                    return;
                }
            }

            Debug.LogError("Max iterations exceeded");
        }
    }
}
