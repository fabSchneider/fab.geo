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
        [Tooltip("Mesh resolution of the generated chunks. Number relates to the number of quads for each row/column of the chunk. Must be multiple of two.")]
        public int chunkResolution = 4;
        [Tooltip("Number of chunks per row/column on each side of the sphere. Must be multiple of 2 and cannot be bigger than the chunk resolution.")]
        public int chunkCount = 2;
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

        private void OnValidate()
        {
            chunkResolution = math.max(1, chunkResolution);
            chunkCount = math.max(1, chunkCount);
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
                    m.name = $"WorldChunk_x{chunk % chunkCount}.y{chunk / chunkCount}.f{face}";
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
            //make sure the resolution and chunk count is a multiple of 2
            //chunkResolution = math.max(2, chunkResolution - chunkResolution % 2);
            //chunkCount = math.clamp(chunkCount - chunkCount % 2, 2, chunkResolution);

            Mesh.MeshDataArray meshDataArray = Mesh.AllocateWritableMeshData(chunkCount * chunkCount * 6);

            NativeArray<JobHandle> jobs = new NativeArray<JobHandle>(6, Allocator.TempJob);

            for (int i = 0; i < 6; i++)
                jobs[i] = GenerateFace(i, meshDataArray, chunkResolution, chunkCount);

            JobHandle.CombineDependencies(jobs).Complete();
            jobs.Dispose();
            return meshDataArray;
        }

        private static JobHandle GenerateFace(int face, Mesh.MeshDataArray meshDataArray, int chunkResolution, int chunkCount)
        {
            float chunkExtent = 1f / chunkCount;

            float3 faceNormal = MeshUtils.cubeFaces[face];
            float3 faceAxisY = new float3(faceNormal.y, faceNormal.z, faceNormal.x);
            float3 faceAxisX = math.cross(faceNormal, faceAxisY);

            float3x3 faceTransform = new float3x3(faceAxisX, faceAxisY, faceNormal);

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
                    meshData.SetIndexBufferParams(chunkIndexCount, IndexFormat.UInt32);

                    GenerateWorldChunkJob chunkJob = new GenerateWorldChunkJob()
                    {
                        meshData = meshData,
                        face = face,
                        faceTransform = faceTransform,
                        chunkId = new int2(x, y),
                        chunkResolution = chunkResolution,
                        chunkExtent = chunkExtent
                    };

                    chunkJobs[index] = chunkJob.Schedule();
                   // GenerateWorldChunk(meshData, face, new int2(x, y), chunkResolution, chunkExtent, faceTransform);
                }
            }

            JobHandle combinedJobs = JobHandle.CombineDependencies(chunkJobs);
            chunkJobs.Dispose();
            return combinedJobs;
        }

        private static void GenerateWorldChunk(Mesh.MeshData meshData, int face, int2 chunkId, int chunkResolution, float chunkExtent, float3x3 faceTransform)
        {
            float chunkDimension = chunkExtent * 2;
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

                    float3 p = faceTransform.c2
                                + chunkParam.x * chunkDimension * faceTransform.c0 + faceTransform.c0 * chunkDimension * chunkId.x - faceTransform.c0
                                + chunkParam.y * chunkDimension * faceTransform.c1 + faceTransform.c1 * chunkDimension * chunkId.y - faceTransform.c1;

                    p = GeoUtils.PointOnCubeToPointOnSphere(p);

                    positions[vertexID] = p;
                    normals[vertexID] = p;

                    float2 texCoord = GeoUtils.NormalizeCoordinate(GeoUtils.PointToCoordinate(p));

                    //handle seam wrapping where the latitude is -180/180
                    if (texCoord.x == 1f)
                    {
                        if ((face == 0 && x == 0)  //front face
                        ||  (face == 1 && y == 0)  //bottom face
                        ||  (face == 4 && y != 0)) //top face
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

    [BurstCompile]
    public struct GenerateWorldChunkJob : IJob
    {
        public Mesh.MeshData meshData;
        public int face;
        public float3x3 faceTransform;
        public int2 chunkId;
        public int chunkResolution;
        public float chunkExtent;
        public void Execute()
        {
            float chunkDimension = chunkExtent * 2;

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

                    float3 p = faceTransform.c2
                                + chunkParam.x * chunkDimension * faceTransform.c0 + faceTransform.c0 * chunkDimension * chunkId.x - faceTransform.c0
                                + chunkParam.y * chunkDimension * faceTransform.c1 + faceTransform.c1 * chunkDimension * chunkId.y - faceTransform.c1;

                    p = GeoUtils.PointOnCubeToPointOnSphere(p);

                    positions[vertexID] = p;
                    normals[vertexID] = p;

                    float2 texCoord = GeoUtils.NormalizeCoordinate(GeoUtils.PointToCoordinate(p));

                    //handle seam wrapping where the latitude is -180/180
                    if (texCoord.x == 1f)
                    {
                        if ((face == 0 && x == 0)  //front face
                        || (face == 1 && y == 0)  //bottom face
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