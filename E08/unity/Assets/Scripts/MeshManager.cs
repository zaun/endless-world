using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class MeshManager : MonoBehaviour{
	public float heightMultiplier = 200;
  private Dictionary<Vector2, Mesh> meshCache = new Dictionary<Vector2, Mesh>();
  public static MeshManager instance;

  Queue<MeshThreadInfo> meshDataThreadInfoQueue = new Queue<MeshThreadInfo>();

  public static void RequestChunkMesh(Vector2 mapChunkCoord, Action<Mesh> callback) {
    if (instance != null) {
      instance._RequestChunkMesh(mapChunkCoord, callback);
    } else {
      Debug.LogError("No MeshManager in scene.");
    }
  }

  // Verify there is only one MeshManager in the world.
  private void Awake() {
    if (instance == null) {
      instance = this;
    } else if (instance != this) {
      Destroy(gameObject);
    }
  }

  private void Update() {
		if (meshDataThreadInfoQueue.Count > 0) {
			for (int i = 0; i < meshDataThreadInfoQueue.Count; i++) {
				MeshThreadInfo threadInfo = meshDataThreadInfoQueue.Dequeue();

        Mesh mesh = new Mesh();
        mesh.vertices = threadInfo.meshData.vertices;
        mesh.triangles = threadInfo.meshData.triangles;
        mesh.uv = threadInfo.meshData.uvs;
				mesh.normals = threadInfo.meshData.bakedNormals;
        meshCache[threadInfo.location] = mesh;
				threadInfo.callback(mesh);
			}
		}
	}

	private void _RequestChunkMesh(Vector2 mapChunkCoord, Action<Mesh> callback) {
    if (meshCache.ContainsKey(mapChunkCoord)) {
      callback(meshCache[mapChunkCoord]);
      return;
    }

    MapManager.RequestMapInfo(mapData => {
      MapManager.RequestMapChunk(mapChunkCoord, chunkData => {
        ThreadStart threadStart = delegate {
          _RequestChunkMeshThreadA(mapData.heightCurve, chunkData, callback);
        };
        new Thread(threadStart).Start ();
      });
    });
	}

  private void _RequestChunkMeshThreadA(AnimationCurve _heightCurve, ChunkData chunkData, Action<Mesh> callback) {
		AnimationCurve heightCurve = new AnimationCurve (_heightCurve.keys);
    int levelOfDetail = 0;
		int size = chunkData.size + chunkData.borderSize;
		float topLeftX = (size - 1) / -2f;
		float topLeftZ = (size - 1) / 2f;

		int meshSimplificationIncrement = (levelOfDetail == 0) ? 1 : levelOfDetail * 2;
		int verticesPerLine = (size - 1) / meshSimplificationIncrement + 1;

		MeshData meshData = new MeshData (verticesPerLine, false);
		int vertexIndex = 0;

		for (int y = 0; y < size; y += meshSimplificationIncrement) {
			for (int x = 0; x < size; x += meshSimplificationIncrement) {
				// meshData.vertices[vertexIndex] = new Vector3 (topLeftX + x, heightCurve.Evaluate(chunkData.heightMap[y + chunkData.borderSize, x + chunkData.borderSize]) * heightMultiplier, topLeftZ - y);
				meshData.vertices[vertexIndex] = new Vector3 (topLeftX + x, chunkData.heightMap[y + chunkData.borderSize, x + chunkData.borderSize] * heightMultiplier, topLeftZ - y);
				meshData.uvs[vertexIndex] = new Vector2 (x / (float)size, y / (float)size);


				if (x < size - 1 && y < size - 1) {
					meshData.AddTriangle (vertexIndex, vertexIndex + verticesPerLine + 1, vertexIndex + verticesPerLine);
					meshData.AddTriangle (vertexIndex + verticesPerLine + 1, vertexIndex, vertexIndex + 1);
				}

				vertexIndex++;
			}
		}

		meshData.ProcessMesh();
    lock (meshDataThreadInfoQueue) {
			meshDataThreadInfoQueue.Enqueue (new MeshThreadInfo(chunkData.location, callback, meshData));
		}
  }

  private void _RequestChunkMeshThread(AnimationCurve _heightCurve, ChunkData chunkData, Action<Mesh> callback) {
		AnimationCurve heightCurve = new AnimationCurve (_heightCurve.keys);
    float heightMultiplier = 1;
    int levelOfDetail = 0;
    bool useFlatShading = false;

		int meshSimplificationIncrement = (levelOfDetail == 0) ? 1 : levelOfDetail * 2;

		int borderedSize = chunkData.size + (chunkData.borderSize * 2);
		int meshSize = chunkData.size * meshSimplificationIncrement;
		int meshSizeUnsimplified = chunkData.size;


		float topLeftX = (meshSizeUnsimplified - 1) / -2f;
		float topLeftZ = (meshSizeUnsimplified - 1) / 2f;
		int verticesPerLine = (meshSize - 1) / meshSimplificationIncrement + 1;

    Debug.Log($"{chunkData.borderSize} {borderedSize} {meshSize} {meshSizeUnsimplified} {verticesPerLine}");

		MeshData meshData = new MeshData(verticesPerLine, useFlatShading);

		int[,] vertexIndicesMap = new int[borderedSize, borderedSize];
		int meshVertexIndex = 0;
		int borderVertexIndex = -1;

		for (int y = 0; y < borderedSize; y += meshSimplificationIncrement) {
			for (int x = 0; x < borderedSize; x += meshSimplificationIncrement) {
				bool isBorderVertex = y < chunkData.borderSize || y >= borderedSize - chunkData.borderSize || x < chunkData.borderSize || x >= borderedSize - chunkData.borderSize;
				if (isBorderVertex) {
					vertexIndicesMap[x, y] = borderVertexIndex;
					borderVertexIndex--;
				} else {
					vertexIndicesMap[x, y] = meshVertexIndex;
					meshVertexIndex++;
				}
			}
		}

		for (int y = 0; y < borderedSize; y += meshSimplificationIncrement) {
			for (int x = 0; x < borderedSize; x += meshSimplificationIncrement) {
				int vertexIndex = vertexIndicesMap[x, y];
				Vector2 percent = new Vector2 ((x - meshSimplificationIncrement) / (float)meshSize, (y - meshSimplificationIncrement) / (float)meshSize);
        percent.x = (float)(x - meshSimplificationIncrement) / (float)meshSize;

				// float height = heightCurve.Evaluate(heightMap [y, x]) * heightMultiplier;
        float height = chunkData.heightMap[y, x] * heightMultiplier;
				Vector3 vertexPosition = new Vector3 (topLeftX + percent.x * meshSizeUnsimplified, height, topLeftZ - percent.y * meshSizeUnsimplified);

        meshData.AddVertex(vertexPosition, percent, vertexIndex);

				bool isBorderVertex = y < chunkData.borderSize || y >= borderedSize - chunkData.borderSize || x < chunkData.borderSize || x >= borderedSize - chunkData.borderSize;
				if (!isBorderVertex) {
          int a = vertexIndicesMap[x, y];
          int b = vertexIndicesMap[x + meshSimplificationIncrement, y];
          int c = vertexIndicesMap[x, y + meshSimplificationIncrement];
          int d = vertexIndicesMap[x + meshSimplificationIncrement, y + meshSimplificationIncrement];
          meshData.AddTriangle (a,d,c);
          meshData.AddTriangle (d,a,b);
				}
				vertexIndex++;
			}
		}

		meshData.ProcessMesh();
    lock (meshDataThreadInfoQueue) {
			meshDataThreadInfoQueue.Enqueue (new MeshThreadInfo(chunkData.location, callback, meshData));
		}
	}
}

struct MeshThreadInfo {
  public readonly Action<Mesh> callback;
  public readonly MeshData meshData;
  public readonly Vector2 location;

  public MeshThreadInfo(Vector2 location, Action<Mesh> callback, MeshData meshData) {
    this.callback = callback;
    this.meshData = meshData;
    this.location = location;
  }
}
