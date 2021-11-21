using Unity.Mathematics;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;
using NaughtyAttributes;
using Unity.Collections;
using Unity.Burst;

namespace Fab.Geo
{
    public class World : MonoBehaviour
    {
        [Tooltip("Mesh resolution of the generated chunks. Number relates to the number of quads for each row/column of the chunk.")]
        [Range(1, 255)]
        public int chunkResolution = 127;
        [Tooltip("Number of chunks per row/column on each side of the sphere. Must be multiple of 2 and cannot be bigger than the chunk resolution.")]
        [Range(1, 16)]
        public int chunkCount = 8;
        [Tooltip("The chunk prefab to instantiate")]
        public MeshFilter chunkPrefab;

        private void Start()
        {
            Debug.Log("Generating World...");
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            GenerateWorld();
            Debug.Log($"Generated world with {chunkCount * chunkCount * 6} chunks @ res {chunkResolution} in {(stopwatch.ElapsedMilliseconds / 1000.0):0.00s}");
        }

        [Button("Generate World")]
        private void GenerateWorld()
        {
            Mesh.MeshDataArray meshDataArray = GenerateWorldMeshes(chunkResolution, chunkCount);
            GameObject worldChunksParent = new GameObject("WorldChunks");
            worldChunksParent.transform.SetParent(transform, false);
            CreateWorldChunkObjects(worldChunksParent.transform, chunkPrefab, meshDataArray, chunkCount);
        }

        private static void CreateWorldChunkObjects(Transform parent, MeshFilter chunkPrefab, Mesh.MeshDataArray meshDataArray, int chunkCount)
        {
            Mesh[] meshes = new Mesh[meshDataArray.Length];
            int chunksPerFace = chunkCount * chunkCount;

            //iterate through each face
            for (int face = 0; face < 6; face++)
            {
                //initialize meshes
                for (int chunk = 0; chunk < chunksPerFace; chunk++)
                {
                    Mesh m = new Mesh();
                    int3 chunkId = new int3(chunk % chunkCount, chunk / chunkCount, face);
                    m.name = $"WorldChunk_x{chunkId.x}.y{chunkId.y}.f{chunkId.z}";
                    m.bounds = CalculateChunkBounds(chunkId, 2f / chunkCount);
                    meshes[face * chunksPerFace + chunk] = m;
                }
            }

            //apply mesh data 
            Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, meshes, MeshUpdateFlags.DontRecalculateBounds);

            //instantiate mesh chunk game object for each mesh
            for (int j = 0; j < meshes.Length; j++)
            {
                MeshFilter chunkInst = Instantiate(chunkPrefab, parent);
                chunkInst.mesh = meshes[j];
                chunkInst.name = meshes[j].name;
                chunkInst.gameObject.AddComponent<BoxCollider>();
            }
        }

        private static Mesh.MeshDataArray GenerateWorldMeshes(int chunkResolution, int chunkCount)
        {
            if (chunkResolution < 1 || chunkResolution > 255)
                Debug.LogError("Chunk resolution has to be in between 1 and 255");

            chunkResolution = math.clamp(chunkResolution, 1, 255);  

            Mesh.MeshDataArray meshDataArray = Mesh.AllocateWritableMeshData(chunkCount * chunkCount * 6);

            NativeArray<JobHandle> jobs = new NativeArray<JobHandle>(6, Allocator.TempJob);

            for (int i = 0; i < 6; i++)
                jobs[i] = GenerateFace(i, meshDataArray, (ushort)chunkResolution, (ushort)chunkCount);

            JobHandle.CombineDependencies(jobs).Complete();
            jobs.Dispose();
            return meshDataArray;
        }

        private static JobHandle GenerateFace(int face, Mesh.MeshDataArray meshDataArray, ushort chunkResolution, ushort chunkCount)
        {
            float chunkSize = 2f / chunkCount;
            float3x3 faceTransform = GetFaceTransform(face);

            int chunkBaseIndex = face * chunkCount * chunkCount;
            NativeArray<JobHandle> chunkJobs = new NativeArray<JobHandle>(meshDataArray.Length, Allocator.TempJob);

            //iterate each chunk of the face
            for (int y = 0; y < chunkCount; y++)
            {
                for (int x = 0; x < chunkCount; x++)
                {
                    int index = chunkBaseIndex + x + y * chunkCount;
                    Mesh.MeshData meshData = meshDataArray[index];
                    meshData.subMeshCount = 1;
                    int chunkVertexCount = (chunkResolution + 1) * (chunkResolution + 1);

                    meshData.SetVertexBufferParams(chunkVertexCount,
                        new VertexAttributeDescriptor(VertexAttribute.Position),
                        new VertexAttributeDescriptor(VertexAttribute.Normal, stream: 1),
                        new VertexAttributeDescriptor(VertexAttribute.TexCoord0, dimension: 2, stream: 2));

                    int chunkIndexCount = chunkResolution * chunkResolution * 6;
                    meshData.SetIndexBufferParams(chunkIndexCount, IndexFormat.UInt16);

                    GenerateWorldChunkJob chunkJob = new GenerateWorldChunkJob()
                    {
                        meshData = meshData,
                        faceTransform = faceTransform,
                        chunkId = new int3(x, y, face),
                        chunkResolution = chunkResolution,
                        chunkSize = chunkSize
                    };

                    chunkJobs[index] = chunkJob.Schedule();
                }
            }

            JobHandle combinedJobs = JobHandle.CombineDependencies(chunkJobs);
            chunkJobs.Dispose();
            return combinedJobs;
        }

        private static float3x3 GetFaceTransform(int face)
        {
            float3 faceNormal = MeshUtils.cubeFaces[face];
            float3 faceAxisY = new float3(faceNormal.y, faceNormal.z, faceNormal.x);
            float3 faceAxisX = math.cross(faceNormal, faceAxisY);

            return new float3x3(faceAxisX, faceAxisY, faceNormal);
        }

        private static Bounds CalculateChunkBounds(int3 chunkId, float chunkSize)
        {
            float3x3 faceTransform = GetFaceTransform(chunkId.z);

            float3 p00 = GeoUtils.PointOnCubeToPointOnSphere(faceTransform.c2
                + faceTransform.c0 * chunkSize * chunkId.x - faceTransform.c0
                + faceTransform.c1 * chunkSize * chunkId.y - faceTransform.c1);
            float3 p01 = GeoUtils.PointOnCubeToPointOnSphere(faceTransform.c2
                + faceTransform.c0 * chunkSize * chunkId.x - faceTransform.c0
                + chunkSize * faceTransform.c1 + faceTransform.c1 * chunkSize * chunkId.y - faceTransform.c1);
            float3 p10 = GeoUtils.PointOnCubeToPointOnSphere(faceTransform.c2
                + chunkSize * faceTransform.c0 + faceTransform.c0 * chunkSize * chunkId.x - faceTransform.c0
                + faceTransform.c1 * chunkSize * chunkId.y - faceTransform.c1);
            float3 p11 = GeoUtils.PointOnCubeToPointOnSphere(faceTransform.c2
                + chunkSize * faceTransform.c0 + faceTransform.c0 * chunkSize * chunkId.x - faceTransform.c0
                + chunkSize * faceTransform.c1 + faceTransform.c1 * chunkSize * chunkId.y - faceTransform.c1);

            Bounds b = new Bounds(p00, Vector3.zero);
            b.Encapsulate(p01);
            b.Encapsulate(p10);
            b.Encapsulate(p11);
            return b;
        }
    }

    [BurstCompile]
    public struct GenerateWorldChunkJob : IJob
    {
        public Mesh.MeshData meshData;
        public float3x3 faceTransform;
        /// <summary>
        /// x: face column id, y: face row id, z: face id
        /// </summary>
        public int3 chunkId;
        public ushort chunkResolution;
        public float chunkSize;
        public void Execute()
        {
            var positions = meshData.GetVertexData<float3>();
            var normals = meshData.GetVertexData<float3>(1);
            var uv = meshData.GetVertexData<float2>(2);
            var indices = meshData.GetIndexData<ushort>();

            int vertexID = 0;
            int indexID = 0;

            //populate vertices
            for (ushort y = 0; y <= chunkResolution; y++)
            {
                for (ushort x = 0; x <= chunkResolution; x++)
                {
                    float2 chunkParam = new float2(
                        x / (float)chunkResolution,
                        y / (float)chunkResolution);

                    float3 p = faceTransform.c2
                                + chunkParam.x * chunkSize * faceTransform.c0 + faceTransform.c0 * chunkSize * chunkId.x - faceTransform.c0
                                + chunkParam.y * chunkSize * faceTransform.c1 + faceTransform.c1 * chunkSize * chunkId.y - faceTransform.c1;

                    p = GeoUtils.PointOnCubeToPointOnSphere(p);

                    positions[vertexID] = p;
                    normals[vertexID] = p;

                    float2 texCoord = GeoUtils.NormalizeCoordinate(GeoUtils.PointToCoordinate(p));

                    //handle seam wrapping where the latitude is -180/180
                    if (texCoord.x == 1f)
                    {
                        if ((chunkId.z == 0 && x == 0)  //front face
                        || (chunkId.z == 1 && y == 0)  //bottom face
                        || (chunkId.z == 4 && y != 0)) //top face
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
            for (ushort y = 0; y < chunkResolution; y++)
            {
                for (ushort x = 0; x < chunkResolution; x++)
                {
                    ushort posId = (ushort)(x + y * (chunkResolution + 1));
                    indices[indexID++] = posId;
                    indices[indexID++] = (ushort)(posId + chunkResolution + 1);
                    indices[indexID++] = (ushort)(posId + chunkResolution + 2);
                    indices[indexID++] = posId;
                    indices[indexID++] = (ushort)(posId + chunkResolution + 2);
                    indices[indexID++] = (ushort)(posId + 1);
                }
            }
            meshData.SetSubMesh(0, new SubMeshDescriptor(0, indices.Length));
        }
    }

}