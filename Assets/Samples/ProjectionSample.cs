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

        public int cells = 2;

        public float project;

        private MeshFilter mf;


        List<float3> pos = new List<float3>();

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
                var sphereMeshDataArray = MeshUtils.GenerateUnitSphere(resolution, cells);
                var sphereMeshData = sphereMeshDataArray[0];

                var spherePositions = sphereMeshData.GetVertexData<float3>();
                var sphereNormals = sphereMeshData.GetVertexData<float3>(1);
                var sphereUVs = sphereMeshData.GetVertexData<float2>(2);

                pos.Clear();

                var frontSubMesh = sphereMeshData.GetSubMesh(0);

                for (int i = 0; i < spherePositions.Length; i++)
                {
                    //Coordinate coord = Geo.PointToCoordinate(spherePositions[i]);

                    //if (i < spherePositions.Length - 1)
                    //{
                    //    var nextCoord = Geo.PointToCoordinate(spherePositions[i + 1]);
                    //    if (coord.latitude == math.PI && nextCoord.latitude < 0f)
                    //    {         
                    //        coord.latitude = -math.PI;
                    //        pos.Add(new float3(coord.latitude, coord.longitude, 0f));
                    //    }
                    //}

                    float3 proj = new float3(sphereUVs[i].x, sphereUVs[i].y, 0f);
                    float3 p = math.lerp(spherePositions[i], proj, project);

                    spherePositions[i] = p;
                    sphereNormals[i] = math.lerp(sphereNormals[i], math.forward(), project);
                }

                Mesh.ApplyAndDisposeWritableMeshData(sphereMeshDataArray, mesh);
                mf.mesh = mesh;
            }
        }

        private void OnDrawGizmos()
        {
            float axisScale = 0.2f;

            Gizmos.matrix = transform.localToWorldMatrix;
            UnityEditor.Handles.matrix = transform.localToWorldMatrix;

            foreach (var p in pos)
            {
                Gizmos.DrawSphere(p, 0.02f);
            }


            for (int i = 0; i < MeshUtils.cubeFaces.Length; i++)
            {
                Vector3 z = MeshUtils.cubeFaces[i];
                //Vector3 x = MeshUtils.cubeFaces[(i + 3) % 6];
                //Vector3 y = MeshUtils.cubeFaces[(i + 4) % 6];

                Vector3 y = new float3(z.y, z.z, z.x);
                Vector3 x = math.cross(z, y);

                //Quaternion q = Quaternion.LookRotation(z);
                //Vector3 x = q * Vector3.right;
                //Vector3 y = q * Vector3.up;

                UnityEditor.Handles.Label(z, (i + 1).ToString());

                Gizmos.color = Color.blue;
                Gizmos.DrawLine(z, z + z * axisScale);
                Gizmos.color = Color.red;
                Gizmos.DrawLine(z, z + x * axisScale);
                Gizmos.color = Color.green;
                Gizmos.DrawLine(z, z + y * axisScale);
            }

            UnityEditor.Handles.matrix = Matrix4x4.identity;
            Gizmos.matrix = Matrix4x4.identity;
        }
    }
}