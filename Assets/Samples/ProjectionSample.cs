using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace FabGeo.Samples
{
    [RequireComponent(typeof(MeshFilter))]
    public class ProjectionSample : MonoBehaviour
    {
        public int resolution = 4;

        public float project;

        private MeshFilter mf;
        void Start()
        {
            var mesh = new Mesh();
            mesh.name = "Sphere";
            mf = GetComponent<MeshFilter>();
            mf.mesh = mesh;
        }

        private void Update()
        {
            if (mf)
            {
                var mesh = mf.mesh;
                var sphereMeshDataArray = GenerateUnitSphere(resolution);
                var sphereMeshData = sphereMeshDataArray[0];

                var spherePositions = sphereMeshData.GetVertexData<float3>();
                var sphereNormals = sphereMeshData.GetVertexData<float3>(1);


                for (int i = 0; i < spherePositions.Length; i++)
                {
                    Coordinate projected = Geo.PointToCoordinate(spherePositions[i]);
                    float3 proj = new float3(projected.latitude, projected.longitude, -1f);
                    float3 p = math.lerp(spherePositions[i], proj, project);
                    spherePositions[i] = p;
                    sphereNormals[i] = math.lerp(sphereNormals[i], math.forward(), project);
                }

                Mesh.ApplyAndDisposeWritableMeshData(sphereMeshDataArray, mesh);
                mf.mesh = mesh;
            }
        }

        private Mesh.MeshDataArray GenerateUnitSphere(int resolution)
        {
            Mesh mesh = new Mesh();
            var meshDataArray = MeshUtils.CreateCube(1f, resolution);
            var meshData = meshDataArray[0];

            var positions = meshData.GetVertexData<float3>();
            var normals = meshData.GetVertexData<float3>(1);

            for (int i = 0; i < positions.Length; i++)
            {
                float3 p = Geo.PointOnCubeToPointOnSphere(positions[i]);
                positions[i] = p;
                normals[i] = math.normalize(p);
            }
            return meshDataArray;
        }
    }
}