using Unity.Collections;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace Fab.Geo.Editor
{
    public class GenerateDensity : EditorWindow
    {
        private static readonly string ComputeShaderPath = "Assets/Shaders/Compute/GenerateDensity.compute";

        [SerializeField]
        private ComputeShader computeShader;
        [SerializeField]
        private TextAsset citiesAsset;
        [SerializeField]
        private Vector2Int resolution = new Vector2Int(512, 512);
        [SerializeField]
        private bool accumulative;

        [MenuItem("FabGeo/Generate/Density")]
        private static void Init()
        {
            GenerateDensity window = (GenerateDensity)EditorWindow.GetWindow(typeof(GenerateDensity));
            window.computeShader = AssetDatabase.LoadAssetAtPath<ComputeShader>(ComputeShaderPath);
            window.Show();
        }

        private void OnGUI()
        {
            SerializedObject sObj = new SerializedObject(this);

            EditorGUILayout.ObjectField(sObj.FindProperty(nameof(computeShader)));

            GUILayout.Label("Generator Settings", EditorStyles.boldLabel);
            EditorGUILayout.ObjectField(sObj.FindProperty(nameof(citiesAsset)));
            SerializedProperty resProp = sObj.FindProperty(nameof(resolution));
            resProp.vector2IntValue = EditorGUILayout.Vector2IntField("Resolution", resProp.vector2IntValue);
            SerializedProperty accProp = sObj.FindProperty(nameof(accumulative));
            accProp.boolValue = EditorGUILayout.Toggle("Accumulative", accProp.boolValue);
            sObj.ApplyModifiedProperties();
            GUILayout.Space(13);
            if (GUILayout.Button("Generate"))
                Generate();
        }


        private void Generate()
        {
            RenderTexture rt = new RenderTexture(new RenderTextureDescriptor(resolution.x, resolution.y, RenderTextureFormat.RFloat));
            rt.name = "CityDensity";
            rt.filterMode = FilterMode.Point;
            rt.enableRandomWrite = true;

            NativeArray<Coordinate> cityData = CityDataCollection.Create(citiesAsset).GetCityCoordinates(Allocator.Temp);

            ComputeBuffer cityBuffer = new ComputeBuffer(cityData.Length, 2 * sizeof(float));
            cityBuffer.SetData(cityData);

            int threadY = 1024;
            int threadX = (int)math.ceil((cityData.Length / (float)threadY));

            int kernel = accumulative ?
                computeShader.FindKernel("GenerateDensityAccumulative") :
                computeShader.FindKernel("GenerateDensity");

            computeShader.SetBuffer(kernel, "Positions", cityBuffer);
            computeShader.SetInt("ThreadX", threadX);
            computeShader.SetTexture(kernel, "Result", rt);
            computeShader.SetInts("Resolution", resolution.x, resolution.y);

            computeShader.Dispatch(kernel, threadX, threadY, 1);

            cityBuffer.Release();
            cityData.Dispose();

#if UNITY_EDITOR
            if (accumulative)
                EditorUtils.SaveTextureEXR(rt.ToTexture2D(), Texture2D.EXRFlags.None);
            else
                EditorUtils.SaveTexturePNG(rt.ToTexture2D());
#endif
        }
    }
}
