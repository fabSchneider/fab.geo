using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using Random = Unity.Mathematics.Random;

namespace Fab.Geo
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

		//ordering corresponds to face ordering of a dice (where dice 1 => 0)
		public static readonly float3[] cubeFaces = new float3[]
		{
			math.forward(),
			math.down(),
			math.right(),
			math.left(),
			math.up(),
			math.back() };

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
		public static Mesh.MeshDataArray GenerateUnitSphere(int resolution, int cells)
		{
			//make sure the resolution and cell count is a multiple of 2
			resolution = math.max(2, resolution - resolution % 2);
			cells = math.clamp(cells - cells % 2, 2, resolution);

			//number of quads per cell row
			int cellResolution = resolution / cells;

			float cellExtent = 1f / cells;
			float cellDimension = cellExtent * 2;

			int cellVertexCount = (cellResolution + 1) * (cellResolution + 1);
			int cellIndexCount = cellResolution * cellResolution * 6;

			var meshDataArray = Mesh.AllocateWritableMeshData(1);
			var meshData = meshDataArray[0];
			meshData.subMeshCount = 1;

			meshData.SetVertexBufferParams(cellVertexCount * cells * cells * 6,
				new VertexAttributeDescriptor(VertexAttribute.Position),
				new VertexAttributeDescriptor(VertexAttribute.Normal, stream: 1),
				new VertexAttributeDescriptor(VertexAttribute.TexCoord0, dimension: 2, stream: 2),
				new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.UNorm8, dimension: 4, stream: 3));

			meshData.SetIndexBufferParams(cellIndexCount * cells * cells * 6, IndexFormat.UInt32);

			var positions = meshData.GetVertexData<float3>();
			var normals = meshData.GetVertexData<float3>(1);
			var uv = meshData.GetVertexData<float2>(2);
			var color = meshData.GetVertexData<Color32>(3);
			var indices = meshData.GetIndexData<int>();

			int vertexID = 0;
			int indexID = 0;

			Random rand = Random.CreateFromIndex(23535);

			for (int faceID = 0; faceID < 6; faceID++)
			{
				float3 faceNormal = cubeFaces[faceID];
				float3 faceAxisY = new float3(faceNormal.y, faceNormal.z, faceNormal.x);
				float3 faceAxisX = math.cross(faceNormal, faceAxisY);

				float faceHue = rand.NextFloat();

				for (int cellY = 0; cellY < cells; cellY++)
				{
					for (int cellX = 0; cellX < cells; cellX++)
					{
						Color32 cellColor = Color.HSVToRGB(faceHue, (cellX + cellY) % 2 == 0 ? 0.4f : 0.7f, 0.8f);

						int cellBaseIndex = vertexID;

						for (int y = 0; y <= cellResolution; y++)
						{
							for (int x = 0; x <= cellResolution; x++)
							{
								float2 cellParam = new float2(
									x / (float)cellResolution,
									y / (float)cellResolution);

								float3 p = faceNormal
											+ cellParam.x * cellDimension * faceAxisX - faceAxisX + faceAxisX * cellDimension * cellX
											+ cellParam.y * cellDimension * faceAxisY - faceAxisY + faceAxisY * cellDimension * cellY;

								p = GeoUtils.PointOnCubeToPointOnSphere(p);

								positions[vertexID] = p;
								normals[vertexID] = p;

								float2 texCoord = GeoUtils.NormalizeCoordinate(GeoUtils.PointToCoordinate(p)).xy;

								//handle seam wrapping where the latitude is -180/180
								if (texCoord.x == 1f)
								{
									if ((faceID == 0 && x == 0)  // front face
									|| (faceID == 1 && y == 0)  // bottom face
									|| (faceID == 4 && y != 0)) //top face
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

								color[vertexID] = cellColor;
								uv[vertexID] = texCoord;

								vertexID++;
							}
						}

						for (int y = 0; y < cellResolution; y++)
						{
							for (int x = 0; x < cellResolution; x++)
							{
								int posId = cellBaseIndex + (x + y * (cellResolution + 1));
								indices[indexID++] = posId;
								indices[indexID++] = (posId + cellResolution + 1);
								indices[indexID++] = (posId + cellResolution + 2);
								indices[indexID++] = posId;
								indices[indexID++] = (posId + cellResolution + 2);
								indices[indexID++] = (posId + 1);
							}
						}
					}
				}


			}

			meshData.SetSubMesh(0, new SubMeshDescriptor(0, indices.Length));
			return meshDataArray;
		}
	}
}
