using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using NaughtyAttributes;

namespace Fab.Geo
{
    public class World : MonoBehaviour
    {
        [Tooltip("Mesh resolution of the generated chunks. Number relates to the number of quads for each row of the chunk. Must be multiple of two.")]
        public int chunkResolution = 4;
        [Tooltip("Number of chunks per row on each side of the sphere. Must be multiple of 2 and cannot be bigger than the chunk resolution.")]
        public int chunkCount = 2;
        [Tooltip("The chunk prefab to instantiate")]
        public MeshFilter chunkPrefab;

        void Start()
        {
            Debug.Log("Generating World...");
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            GenerateWorld();
            Debug.Log($"Generated world in {(stopwatch.ElapsedMilliseconds / 1000.0):0.00s} (Resolution: {chunkResolution})");
        }

        [Button("Generate World")]
        private void GenerateWorld()
        {
            Mesh.MeshDataArray[] meshDataArrays = GenerateWorldMeshes(chunkResolution, chunkCount);
            GameObject worldChunksParent = new GameObject("WorldChunks");
            worldChunksParent.transform.SetParent(transform, false);
            CreateWorldChunkObjects(worldChunksParent.transform, chunkPrefab, meshDataArrays, chunkCount);
        }
        private static void CreateWorldChunkObjects(Transform parent, MeshFilter chunkPrefab, Mesh.MeshDataArray[] meshDataArrays, int chunkCount)
        {
            for (int i = 0; i < meshDataArrays.Length; i++)
            {
                Mesh.MeshDataArray meshDataArray = meshDataArrays[i];
                int chunksPerFace = chunkCount * chunkCount;
                int face = i / chunksPerFace;
                int chunkX;
                int chunkY;
                chunkX = (i - chunksPerFace * face) % chunkCount;
                chunkY = (i - chunksPerFace * face) / chunkCount;

                MeshFilter chunkInst = Instantiate(chunkPrefab, parent);
                chunkInst.name = $"WorldChunk_{face}({chunkX},{chunkY})";

                Mesh mesh = new Mesh();
                mesh.name = chunkInst.name;

                Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, mesh, MeshUpdateFlags.DontRecalculateBounds);
                mesh.RecalculateBounds();
              //  mesh.RecalculateTangents();
                chunkInst.mesh = mesh;

                chunkInst.gameObject.AddComponent<BoxCollider>();
            }
        }

        private static Mesh.MeshDataArray[] GenerateWorldMeshes(int resolution, int chunks)
        {
            //make sure the resolution and cell count is a multiple of 2
            resolution = math.max(2, resolution - resolution % 2);
            chunks = math.clamp(chunks - chunks % 2, 2, resolution);

            Mesh.MeshDataArray[] meshDataArrays = new Mesh.MeshDataArray[6 * chunks * chunks];

            for (int i = 0; i < 6; i++)
                GenerateFace(i, meshDataArrays, resolution, chunks);

            return meshDataArrays;
        }

        private static void GenerateFace(int face, Mesh.MeshDataArray[] meshDataArrays, int resolution, int chunks)
        {
            //number of quads per chunk row
            int chunkResolution = resolution / chunks;

            float chunkExtent = 1f / chunks;

            for (int chunkY = 0; chunkY < chunks; chunkY++)
                for (int chunkX = 0; chunkX < chunks; chunkX++)
                {
                    int meshDataIndex = face * chunks * chunks + chunkY * chunks + chunkX;
                    meshDataArrays[meshDataIndex] = Mesh.AllocateWritableMeshData(1);
                    GenerateWorldChunk(meshDataArrays[meshDataIndex], face, chunkResolution, chunkExtent, chunkX, chunkY);
                }
        }

        private static void GenerateWorldChunk(Mesh.MeshDataArray meshDataArray, int face, int chunkResolution, float chunkExtent, int chunkX, int chunkY)
        {
            float chunkDimension = chunkExtent * 2;

            float3 faceNormal = MeshUtils.cubeFaces[face];
            float3 faceAxisY = new float3(faceNormal.y, faceNormal.z, faceNormal.x);
            float3 faceAxisX = math.cross(faceNormal, faceAxisY);

            Mesh.MeshData meshData = meshDataArray[0];
            meshData.subMeshCount = 1;

            int chunkVertexCount = (chunkResolution + 1) * (chunkResolution + 1);

            meshData.SetVertexBufferParams(chunkVertexCount,
                new VertexAttributeDescriptor(VertexAttribute.Position),
                new VertexAttributeDescriptor(VertexAttribute.Normal, stream: 1),
                new VertexAttributeDescriptor(VertexAttribute.TexCoord0, dimension: 2, stream: 2));

            int chunkIndexCount = chunkResolution * chunkResolution * 6;

            meshData.SetIndexBufferParams(chunkIndexCount, IndexFormat.UInt32);

            var positions = meshData.GetVertexData<float3>();
            var normals = meshData.GetVertexData<float3>(1);
            var uv = meshData.GetVertexData<float2>(2);
            var indices = meshData.GetIndexData<int>();

            int vertexID = 0;
            int indexID = 0;

            //populate vertices
            for (int y = 0; y <= chunkResolution; y++)
            {
                for (int x = 0; x <= chunkResolution; x++)
                {
                    float2 chunkParam = new float2(
                        x / (float)chunkResolution,
                        y / (float)chunkResolution);

                    float3 p = faceNormal
                                + chunkParam.x * chunkDimension * faceAxisX - faceAxisX + faceAxisX * chunkDimension * chunkX
                                + chunkParam.y * chunkDimension * faceAxisY - faceAxisY + faceAxisY * chunkDimension * chunkY;

                    p = GeoUtils.PointOnCubeToPointOnSphere(p);

                    positions[vertexID] = p;
                    normals[vertexID] = p;

                    float2 texCoord = GeoUtils.NormalizeCoordinate(GeoUtils.PointToCoordinate(p));

                    //handle seam wrapping where the latitude is -180/180
                    if (texCoord.x == 1f)
                    {
                        if ((face == 0 && x == 0)  // front face
                        || (face == 1 && y == 0)  // bottom face
                        || (face == 4 && y != 0)) //top face
                        {
                            texCoord.x = 0f;

                            if (texCoord.y == 0f || texCoord.y == 1f)
                                texCoord.x = 0.25f;
                        }
                        else if (texCoord.y == 0f || texCoord.y == 1f)
                        {
                            texCoord.x = 0.75f;
                        }
                    }


                    uv[vertexID] = texCoord;
                    vertexID++;
                }
            }

            //populate indices
            for (int y = 0; y < chunkResolution; y++)
            {
                for (int x = 0; x < chunkResolution; x++)
                {
                    int posId = x + y * (chunkResolution + 1);
                    indices[indexID++] = posId;
                    indices[indexID++] = (posId + chunkResolution + 1);
                    indices[indexID++] = (posId + chunkResolution + 2);
                    indices[indexID++] = posId;
                    indices[indexID++] = (posId + chunkResolution + 2);
                    indices[indexID++] = (posId + 1);
                }
            }

            meshData.SetSubMesh(0, new SubMeshDescriptor(0, indices.Length));
        }
    }
}