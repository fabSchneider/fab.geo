using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace FabGeo.Samples
{
    [RequireComponent(typeof(MeshFilter))]
    public class CubeSample : MonoBehaviour
    {
        public float extent = 0.5f;
        public int resolution = 4;

        private MeshFilter mf;
        void Start()
        {
            var mesh = new Mesh();
            mesh.name = "Cube";
            mf = GetComponent<MeshFilter>();
            mf.mesh = mesh;
        }

        private void Update()
        {
            if (mf)
            {
                Mesh.ApplyAndDisposeWritableMeshData(MeshUtils.CreateCube(extent, resolution), mf.mesh);        
            }
        }
    }
}