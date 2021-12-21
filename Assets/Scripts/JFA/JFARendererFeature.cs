using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Fab.Geo
{
    public enum JFAType { VoronoiDiagram, DistanceTransform };

    public class JFARendererFeature : ScriptableRendererFeature
    {


        [System.Serializable]
        public class JFASettings
        {
            public RenderPassEvent WhenToInsert = RenderPassEvent.AfterRendering;

            public ComputeShader JFAShader;
            public int SeedAmount = 5;
            public float Speed = 1f;
            public JFAType DisplayType = JFAType.VoronoiDiagram;
            public Material BlitMat;

        }

        [SerializeField]
        private JFASettings settings = new JFASettings();

        JFARendererPass jfaRendererPass;
        
        public override void Create()
        {
            if(settings.JFAShader != null)
                jfaRendererPass = new JFARendererPass(name, settings.WhenToInsert, settings.JFAShader, settings.SeedAmount, settings.BlitMat);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (!isActive)
                return;

            if (jfaRendererPass == null)
                return;

            // Gather up and pass any extra information our pass will need.
            // In this case we're getting the camera's color buffer target
            var cameraColorTargetIdent = renderer.cameraColorTarget;
            jfaRendererPass.Setup(cameraColorTargetIdent, settings.Speed);

            // Ask the renderer to add our pass.
            // Could queue up multiple passes and/or pick passes to use
            renderer.EnqueuePass(jfaRendererPass);
        }
    }
}
