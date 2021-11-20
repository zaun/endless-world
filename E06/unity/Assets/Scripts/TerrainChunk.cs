using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainChunk {
  public GameObject meshObject;
  public Vector2 position;

  private Bounds bounds;

  public TerrainChunk(Vector2 coord, int size, Transform parent) {
    position = coord * size;
    bounds = new Bounds(position, Vector2.one * size);
    Vector3 positionV3 = new Vector3(position.x, 0, position.y);

    meshObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
    meshObject.transform.position = positionV3;
    meshObject.transform.localScale = Vector3.one * size / 10f;
    meshObject.transform.parent = parent;
    SetVisible(false);
  }

  public void UpdateTerrainChunk(Vector2 viewerPosition, float maxViewDst) {
    float viewerDstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
    bool visible = viewerDstFromNearestEdge <= maxViewDst;
    SetVisible(visible);
  }

  public void SetVisible(bool visible) {
    meshObject.SetActive(visible);
  }

  public bool IsVisible() {
    return meshObject.activeSelf;
  }
}
