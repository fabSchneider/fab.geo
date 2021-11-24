using Unity.Mathematics;
using UnityEngine;
using UnityEditor;

namespace Fab.Geo.Editor
{
    public class GenerateDistanceField : EditorWindow
    {
        private static readonly string ComputeShaderPath = "Assets/Shaders/Compute/GenerateDistanceField.compute";
        private static readonly int MaxIterations = 5000;

        [SerializeField]
        private ComputeShader computeShader;
        [SerializeField]
        private Texture2D maskSource;

        private ComputeBuffer changedFlagBuffer;


        [MenuItem("FabGeo/Generate/Distance Field")]
        private static void Init()
        {
            GenerateDistanceField window = (GenerateDistanceField)EditorWindow.GetWindow(typeof(GenerateDistanceField));
            window.computeShader = AssetDatabase.LoadAssetAtPath<ComputeShader>(ComputeShaderPath);
            window.Show();
        }

        private void OnGUI()
        {
            SerializedObject sObj = new SerializedObject(this);

            EditorGUILayout.ObjectField(sObj.FindProperty(nameof(computeShader)));
            GUILayout.Label("Generator Settings", EditorStyles.boldLabel);
            EditorGUILayout.ObjectField(sObj.FindProperty(nameof(maskSource)));
            
            sObj.ApplyModifiedProperties();
            GUILayout.Space(13);
            if (GUILayout.Button("Generate"))
                Generate();
        }



        public void Generate()
        {
            if (computeShader == null)
                return;

            if (changedFlagBuffer == null)
                changedFlagBuffer = new ComputeBuffer(1, sizeof(int));

            Generate(maskSource);
        }

        private RenderTexture CreateMask()
        {
            RenderTexture mask = new RenderTexture(new RenderTextureDescriptor(maskSource.width, maskSource.height, RenderTextureFormat.RFloat));
            mask.wrapMode = TextureWrapMode.Repeat;
            mask.enableRandomWrite = true;
            mask.name = "Mask";
            Graphics.Blit(maskSource, mask);

            int prepKernel = computeShader.FindKernel("PrepareMask");
            computeShader.SetTexture(prepKernel, "Mask", mask);

            int groupX = (int)math.ceil(mask.width / 8f);
            int groupY = (int)math.ceil(mask.height / 8f);
            computeShader.Dispatch(prepKernel, groupX, groupY, 1);
            return mask;
        }

        private void Generate(Texture2D source)
        {
            RenderTexture mask = CreateMask();

            int passKernel = computeShader.FindKernel("DistancePass");

            RenderTexture distance = new RenderTexture(new RenderTextureDescriptor(source.width, source.height, RenderTextureFormat.RFloat));
            distance.wrapMode = TextureWrapMode.Repeat;
            distance.enableRandomWrite = true;
            distance.name = "Distance";
            Graphics.Blit(mask, distance);

            computeShader.SetTexture(passKernel, "SqrDistanceField", distance);
            ExecutePass(distance, new int2(0, 1));
            ExecutePass(distance, new int2(1, 0));

            EditorUtils.SaveTextureEXR(distance.ToTexture2D(), Texture2D.EXRFlags.OutputAsFloat);
        }

        private void ExecutePass(RenderTexture texture, int2 offset)
        {
            computeShader.SetInts("passOffest", offset.x, offset.y);
            computeShader.SetInts("resolution", texture.width, texture.height);
            int kernel = computeShader.FindKernel("DistancePass");

            for (int i = 0; i < MaxIterations; i++)
            {
                changedFlagBuffer.SetData(new int[] { 0 });
                computeShader.SetBuffer(kernel, "ChangedFlag", changedFlagBuffer);
                computeShader.SetInt("iterationCount", i);

                int groupX = (int)math.ceil(texture.width / 8f);
                int groupY = (int)math.ceil(texture.height / 8f);
                computeShader.Dispatch(kernel, groupX, groupY, 1);

                int[] numChanges = new int[1];
                changedFlagBuffer.GetData(numChanges);
                if (numChanges[0] == 0)
                    return;
            }

            Debug.LogError("Max iterations exceeded");
        }
    }
}
