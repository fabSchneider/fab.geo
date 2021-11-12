using System;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace FabGeo
{
    public static class MeshUtils
    {
        public static void CreatePlane(float3 normal, float offset, float2 extent, int2 resolution,
            out NativeArray<float3> positions,
            out NativeArray<int> indices)
        {
            resolution = new int2(math.max(1, resolution.x), math.max(1, resolution.y));
            float3 axisY = new float3(normal.y, normal.z, normal.x);
            float3 axisX = math.cross(normal, axisY);
            float3 normalOffset = normal * offset;
            float2 dimensions = extent * 2f;

             positions = new NativeArray<float3>((resolution.x + 1) * (resolution.y + 1), Allocator.Temp);
             indices = new NativeArray<int>(resolution.x * resolution.y * 6, Allocator.Temp);

            for (int y = 0; y <= resolution.y; y++)
            {
                for (int x = 0; x <= resolution.x; x++)
                {
                    int id = x + y * (resolution.x + 1);
                    positions[id] = normalOffset
                                + (x / (float)resolution.x * axisX * dimensions.x - extent.x * axisX)
                                + (y / (float)resolution.y * axisY * dimensions.y - extent.y * axisY);
                }
            }
            
            int iId = 0;
            for (int y = 0; y < resolution.y; y++)
            {
                for (int x = 0; x < resolution.x; x++)
                {
                    int posId = (x + y * (resolution.x + 1));
                    indices[iId++] = posId;
                    indices[iId++] = (posId + resolution.x + 1);
                    indices[iId++] = (posId + resolution.x + 2);
                    indices[iId++] = posId;
                    indices[iId++] = (posId + resolution.x + 2);
                    indices[iId++] = (posId + 1);
                }
            }
        }

        public static void CreatePlane(Mesh mesh, float3 normal, float offset, float2 extent, int2 resolution)
        {
            CreatePlane(normal, offset, extent, resolution, out NativeArray<float3> positions, out NativeArray<int> indices);
            var meshDataArray = Mesh.AllocateWritableMeshData(1);
            var meshData = meshDataArray[0];

            meshData.SetVertexBufferParams(positions.Length,
                new VertexAttributeDescriptor(VertexAttribute.Position),
                new VertexAttributeDescriptor(VertexAttribute.Normal, stream: 1));

            meshData.SetIndexBufferParams(indices.Length, IndexFormat.UInt32);

            var meshPositions = meshData.GetVertexData<float3>();
            var meshNormals = meshData.GetVertexData<float3>(1);
            var meshIndices = meshData.GetIndexData<int>();

            for (int i = 0; i < positions.Length; i++)
            {
                meshPositions[i] = positions[i];
                meshNormals[i] = normal;
            }

            for (int i = 0; i < indices.Length; i++)
            {
                meshIndices[i] = indices[i];
            }

            positions.Dispose();
            indices.Dispose();

            meshData.subMeshCount = 1;
            meshData.SetSubMesh(0, new SubMeshDescriptor(0, meshIndices.Length));
            Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, mesh);
        }

        private static readonly float3[] cubeFaces = new float3[] { math.forward(), math.right(), math.back(), math.left(), math.down(), math.up() };

        public static Mesh.MeshDataArray CreateCube(float extent, int resolution)
        {
            resolution = math.max(1, resolution);
            float2 cubeExtent = new float2(extent, extent);
            int2 cubeResolution = new int2(resolution, resolution);

            var meshDataArray = Mesh.AllocateWritableMeshData(1);
            var meshData = meshDataArray[0];

            meshData.subMeshCount = 6;

            int faceVertexCount = (resolution + 1) * (resolution + 1);
            int faceIndexCount = resolution * resolution * 6;

            meshData.SetVertexBufferParams(faceVertexCount * 6,
                new VertexAttributeDescriptor(VertexAttribute.Position),
                new VertexAttributeDescriptor(VertexAttribute.Normal, stream: 1));

            meshData.SetIndexBufferParams(faceIndexCount * 6, IndexFormat.UInt32);

            var positions = meshData.GetVertexData<float3>();
            var normal = meshData.GetVertexData<float3>(1);
            var indices = meshData.GetIndexData<int>();

            for (int i = 0; i < cubeFaces.Length; i++)
            {
                CreatePlane(cubeFaces[i], extent, cubeExtent, cubeResolution,
                    out NativeArray<float3> facePositions,
                    out NativeArray<int> faceIndices);

                for (int j = 0; j < facePositions.Length; j++)
                {
                    positions[faceVertexCount * i + j] = facePositions[j];
                    normal[faceVertexCount * i + j] = cubeFaces[i];
                }

                for (int k = 0; k < faceIndices.Length; k++)
                    indices[faceIndexCount * i + k] = faceVertexCount * i + faceIndices[k];

                meshData.SetSubMesh(i, new SubMeshDescriptor(faceIndexCount * i, faceIndexCount));
                facePositions.Dispose();
                faceIndices.Dispose();
            }
            return meshDataArray;
        }

    }
}
