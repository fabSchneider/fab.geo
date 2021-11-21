using NaughtyAttributes;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Fab.Geo.Editor
{
    public class GenerateDensity : MonoBehaviour
    {
        [SerializeField]
        private ComputeShader computeShader;

        [SerializeField]
        private TextAsset citiesAsset;

        [SerializeField]
        private int2 resolution = 512;

        [SerializeField]
        private bool accumulative;


        public RenderTexture output;

        [Button]
        private void Compute()
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

            output = rt;

#if UNITY_EDITOR
            if (accumulative)
              EditorUtils.SaveTextureEXR(rt.ToTexture2D(), Texture2D.EXRFlags.None);
            else
            EditorUtils.SaveTexturePNG(rt.ToTexture2D());
#endif
        }
    }
}
