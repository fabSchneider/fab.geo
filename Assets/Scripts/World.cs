using Unity.Mathematics;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;
using NaughtyAttributes;
using Unity.Collections;
using Unity.Burst;
using System.Collections.Generic;

namespace Fab.Geo
{
	public struct WorldChunk
	{
		public int3 id;
		public int lod;

		public WorldChunk(int3 id, int lod)
		{
			this.id = id;
			this.lod = lod;
		}
	}

	[System.Serializable]
	public struct ChunkLOD
	{
		public int resolution;
		public float distance;

		public ChunkLOD(int resolution, float distance)
		{
			this.resolution = resolution;
			this.distance = distance;
		}
	}

	public class World : MonoBehaviour
	{
		[Tooltip("Number of chunks per row/column on each side of the sphere. Must be multiple of 2 and cannot be bigger than the chunk resolution.")]
		[Range(1, 32)]
		public int chunkCount = 8;

		[SerializeField]
		private Material worldMaterial;

		[SerializeField]
		private ChunkLOD[] chunkLODs;

		private WorldChunk[] chunks;
		public IEnumerable<WorldChunk> Chunks => chunks;

		private MeshRenderer[] chunkMeshRenderers;
		public MeshRenderer GetChunk(int3 id) => chunkMeshRenderers[id.x + id.y * chunkCount + (chunkCount * chunkCount) * id.z];

		[SerializeField]
		private Texture2D heightMap;

		private void Start()
		{
			Debug.Log("Generating World...");

			// Copy material;
			worldMaterial = new Material(worldMaterial);

			// System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
			// stopwatch.Start();
			GenerateWorld();
			//Debug.Log($"Generated world with {chunkCount * chunkCount * 6} chunks @ res {chunkResolution} in {(stopwatch.ElapsedMilliseconds / 1000.0):0.00s}");

			Texture2D copy = new Texture2D(heightMap.width, heightMap.height, heightMap.format, false, false);
			Graphics.CopyTexture(heightMap, copy);
			heightMap = copy;
		}

		/// <summary>
		/// Returns the altitude at a given longitude and latitude
		/// </summary>
		/// <param name="lon"></param>
		/// <param name="lat"></param>
		/// <returns></returns>
		public float GetAltitude(float lon, float lat)
		{
			float2 uv = GeoUtils.NormalizeLonLat(lon, lat);
			float height = heightMap.GetPixelBilinear(uv.x, uv.y).r;
			float altitude = height * 19832 - 10984;
			return altitude;
		}

		public int GetChunkLOD(float dist)
		{
			for (int i = 0; i < chunkLODs.Length - 1; i++)
			{
				if (dist < chunkLODs[i].distance)
					return i;
			}

			return chunkLODs.Length - 1;
		}

		public float HeightScale
		{
			get => worldMaterial.GetFloat("_HeightScale");
			set => worldMaterial.SetFloat("_HeightScale", value);
		}

		public float WaterLevel
		{
			get => worldMaterial.GetFloat("_WaterLevel");
			set => worldMaterial.SetFloat("_WaterLevel", value);
		}

		public float OverlayStrength
		{
			get => worldMaterial.GetFloat("_OverlayScale");
			set => worldMaterial.SetFloat("_OverlayScale", value);
		}

		public float OverlayScale
		{
			get => worldMaterial.GetFloat("_OverlayExp");
			set => worldMaterial.SetFloat("_OverlayExp", value);
		}



		public void SetChunkActive(int3 id, bool active)
		{
			GetChunk(id).gameObject.SetActive(active);
		}

		private List<Mesh> lodMeshes = new List<Mesh>();
		public void RegenerateChunks(List<WorldChunk> chunks)
		{
			Mesh.MeshDataArray meshDataArray = Mesh.AllocateWritableMeshData(chunks.Count);
			NativeArray<JobHandle> chunkJobs = new NativeArray<JobHandle>(chunks.Count, Allocator.TempJob);

			lodMeshes.Clear();

			for (int i = 0; i < chunks.Count; i++)
			{
				WorldChunk chunk = chunks[i];
				int index = chunk.id.x + chunk.id.y * chunkCount + (chunkCount * chunkCount) * chunk.id.z;

				float3x3 faceT = GetFaceTransform(chunk.id.z);
				chunkJobs[i] = GenerateChunk(meshDataArray[i], chunk.id, (ushort)chunkLODs[chunk.lod].resolution, 2f / chunkCount, in faceT);
				this.chunks[index] = chunk;

				lodMeshes.Add(chunkMeshRenderers[index].GetComponent<MeshFilter>().sharedMesh);
			}

			JobHandle.CompleteAll(chunkJobs);
			chunkJobs.Dispose();

			Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, lodMeshes);
		}

		[Button("Generate World")]
		private void GenerateWorld()
		{
			int lod = chunkLODs.Length - 1;
			int chunkResolution = chunkLODs[chunkLODs.Length - 1].resolution;
			JobHandle worldJobHandle = GenerateWorldMeshes((ushort)chunkResolution, (ushort)chunkCount, out Mesh.MeshDataArray meshDataArray);

			InitializeChunks(chunkCount, lod, out chunks, out Mesh[] chunkMeshes);

			GameObject worldChunksParent = new GameObject("WorldChunks");
			worldChunksParent.transform.SetParent(transform, false);
			chunkMeshRenderers = CreateWorldChunkObjects(worldChunksParent.transform, worldMaterial, chunkMeshes);

			worldJobHandle.Complete();
			//apply mesh data 
			Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, chunkMeshes, MeshUpdateFlags.DontRecalculateBounds);

		}

		private static void InitializeChunks(int chunkCount, int lod, out WorldChunk[] chunks, out Mesh[] chunkMeshes)
		{
			int chunksPerFace = chunkCount * chunkCount;
			chunks = new WorldChunk[chunksPerFace * 6];
			chunkMeshes = new Mesh[chunks.Length];

			//iterate through each face
			for (int face = 0; face < 6; face++)
			{
				//initialize meshes
				for (int chunk = 0; chunk < chunksPerFace; chunk++)
				{
					Mesh m = new Mesh();
					int3 chunkId = new int3(chunk % chunkCount, chunk / chunkCount, face);
					m.name = $"WorldChunk_x{chunkId.x}.y{chunkId.y}.f{chunkId.z}";
					Bounds bounds = CalculateChunkBounds(chunkId, 2f / chunkCount);
					m.bounds = bounds;

					int index = face * chunksPerFace + chunk;
					chunks[index] = new WorldChunk(chunkId, lod);
					chunkMeshes[index] = m;
				}
			}
		}

		private static MeshRenderer[] CreateWorldChunkObjects(Transform parent, Material material, Mesh[] chunkMeshes)
		{
			MeshRenderer[] meshRenderers = new MeshRenderer[chunkMeshes.Length];

			//instantiate mesh chunk game object for each mesh
			for (int i = 0; i < chunkMeshes.Length; i++)
			{
				GameObject chunkObj = new GameObject(chunkMeshes[i].name);
				chunkObj.AddComponent<MeshFilter>().mesh = chunkMeshes[i];
				MeshRenderer mr = chunkObj.AddComponent<MeshRenderer>();
				mr.sharedMaterial = material;
				meshRenderers[i] = mr;
				chunkObj.transform.parent = parent;
			}

			return meshRenderers;
		}

		private static JobHandle GenerateWorldMeshes(ushort chunkResolution, ushort chunkCount, out Mesh.MeshDataArray meshDataArray)
		{
			if (chunkResolution < 1 || chunkResolution > 255)
				Debug.LogError("Chunk resolution has to be in between 1 and 255");

			chunkResolution = (ushort)math.clamp(chunkResolution, 1, 255);

			float chunkSize = 2f / chunkCount;

			meshDataArray = Mesh.AllocateWritableMeshData(chunkCount * chunkCount * 6);
			NativeArray<JobHandle> chunkJobs = new NativeArray<JobHandle>(meshDataArray.Length, Allocator.TempJob);

			//iterate each face
			for (int i = 0; i < 6; i++)
			{
				float3x3 faceTransform = GetFaceTransform(i);
				int chunkBaseIndex = i * chunkCount * chunkCount;

				//iterate each chunk of the face
				for (int y = 0; y < chunkCount; y++)
				{
					for (int x = 0; x < chunkCount; x++)
					{
						int index = chunkBaseIndex + x + y * chunkCount;
						chunkJobs[index] = GenerateChunk(meshDataArray[index], new int3(x, y, i), chunkResolution, chunkSize, in faceTransform);
					}
				}
			}

			JobHandle combinedJobs = JobHandle.CombineDependencies(chunkJobs);
			chunkJobs.Dispose();
			return combinedJobs;
		}

		private static JobHandle GenerateChunk(Mesh.MeshData meshData, int3 id, ushort resolution, float size, in float3x3 faceTransform)
		{
			meshData.subMeshCount = 1;
			int chunkVertexCount = (resolution + 1) * (resolution + 1);

			meshData.SetVertexBufferParams(chunkVertexCount,
				new VertexAttributeDescriptor(VertexAttribute.Position),
				new VertexAttributeDescriptor(VertexAttribute.Normal, stream: 1),
				new VertexAttributeDescriptor(VertexAttribute.TexCoord0, dimension: 2, stream: 2));

			int chunkIndexCount = resolution * resolution * 6;
			meshData.SetIndexBufferParams(chunkIndexCount, IndexFormat.UInt16);

			GenerateWorldChunkJob chunkJob = new GenerateWorldChunkJob()
			{
				meshData = meshData,
				faceTransform = faceTransform,
				chunkId = id,
				chunkResolution = resolution,
				chunkSize = size
			};

			return chunkJob.Schedule();
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

					float2 texCoord = GeoUtils.NormalizeCoordinate(GeoUtils.PointToCoordinate(p)).xy;

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
