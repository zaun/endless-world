using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;

[Serializable]
public class ChunkData {
  public int size;
  public int borderSize;
  public Vector2 location;
  public float[,] heightMap;

  public ChunkData(int _size, int _borderSize, Vector2 _location, float[,] _heightMap) {
    size = _size;
    borderSize = _borderSize;
    location = _location;
    heightMap = _heightMap;
  }

  public static ChunkData fromJSON(string jsonString) {
    JSONNode node  = JSON.Parse(jsonString);
    
    int dataSize = node["size"].AsInt;
    int dataBorderSize = node["borderSize"].AsInt;
    Vector2 dataLocation = node["location"].ReadVector2();
    JSONArray rows = node["heightMap"].AsArray;

    int hmSize = dataSize + (dataBorderSize * 2);
    float[,] dataHeightMap = new float[hmSize, hmSize];
    for (int y = 0; y < hmSize; y++) {
      JSONArray cols = rows[y].AsArray;
      for (int x = 0; x < hmSize; x++) {
        dataHeightMap[y, x] = cols[x].AsFloat;
      }
    }

    return new ChunkData(dataSize, dataBorderSize, dataLocation, dataHeightMap);
  }
}

