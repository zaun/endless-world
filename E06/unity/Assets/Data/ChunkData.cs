using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;

[Serializable]
public class ChunkData {
  public int size;
  public Vector2 location;
  public double[,] heightMap;

  public ChunkData(int _size, Vector2 _location, double[,] _heightMap) {
    size = _size;
    location = _location;
    heightMap = _heightMap;
  }

  public static ChunkData fromJSON(string jsonString) {
    JSONNode node  = JSON.Parse(jsonString);
    
    int dataSize = node["size"].AsInt;
    Vector2 dataLocation = node["location"].ReadVector2();
    JSONArray rows = node["heightMap"].AsArray;

    double[,] dataHeightMap = new double[dataSize, dataSize];

    for (int y = 0; y < dataSize; y++) {
      JSONArray cols = rows[y].AsArray;
      for (int x = 0; x < dataSize; x++) {
        dataHeightMap[y, x] = cols[x].AsDouble;
      }
    }

    return new ChunkData(dataSize, dataLocation, dataHeightMap);
  }
}

