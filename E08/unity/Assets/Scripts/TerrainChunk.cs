using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainChunk {
  public GameObject chunkObject;
  public Vector2 position;

  private Bounds bounds;
  private bool loaded = false;

  public TerrainChunk(Vector2 coord, int size, Transform parent) {
    position = new Vector2(coord.x * size, coord.y * size);
    bounds = new Bounds(position, Vector2.one * size);
    Vector3 positionV3 = new Vector3(position.x, 0, position.y);

    chunkObject = new GameObject();
    chunkObject.name = coord.ToString();
    chunkObject.transform.position = positionV3;
    chunkObject.transform.localScale = Vector3.one;
    chunkObject.transform.parent = parent;

    MeshFilter meshFilter = chunkObject.AddComponent(typeof(MeshFilter)) as MeshFilter;
    MeshRenderer meshRenderer = chunkObject.AddComponent(typeof(MeshRenderer)) as MeshRenderer;
    MeshCollider meshCollider = chunkObject.AddComponent(typeof(MeshCollider)) as MeshCollider;

    MeshManager.RequestChunkMesh(new Vector2(coord.x, -coord.y), mesh => {
      meshFilter.sharedMesh = mesh;
      meshCollider.sharedMesh = mesh;
      loaded = true;
    });

    MapManager.RequestMapInfo(mapData => {
      meshFilter.sharedMesh = createQuad(size);
      meshRenderer.material = new Material(Shader.Find("Custom/Heightmap"));
      meshRenderer.material.SetFloat("minHeight", 0);
      meshRenderer.material.SetFloat("maxHeight", MeshManager.instance.heightMultiplier);
    });

    SetVisible(false);
  }

  public void UpdateTerrainChunk(Vector2 viewerPosition, float maxViewDst) {
    float viewerDstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
    bool visible = viewerDstFromNearestEdge <= maxViewDst;
    SetVisible(visible);
  }

  public void SetVisible(bool visible) {
    chunkObject.SetActive(visible);
  }

  public bool IsVisible() {
    return chunkObject.activeSelf;
  }

  public bool IsLoaded() {
    return loaded;
  }

  private Mesh createQuad(float size) {
    Mesh mesh = new Mesh();

    Vector3[] vertices = new Vector3[4] {
        new Vector3(-size / 2, 0, -size / 2),
        new Vector3(size / 2, 0, -size / 2),
        new Vector3(-size / 2, 0, size / 2),
        new Vector3(size / 2, 0, size / 2)
    };

    int[] tris = new int[6] {
        // lower left triangle
        0, 2, 1,
        // upper right triangle
        2, 3, 1
    };

    Vector3[] normals = new Vector3[4] {
        -Vector3.forward,
        -Vector3.forward,
        -Vector3.forward,
        -Vector3.forward
    };

    Vector2[] uv = new Vector2[4] {
        new Vector2(0, 0),
        new Vector2(1, 0),
        new Vector2(0, 1),
        new Vector2(1, 1)
    };

    mesh.vertices = vertices;
    mesh.triangles = tris;
    mesh.normals = normals;
    mesh.uv = uv;

    return mesh;
  }
}
