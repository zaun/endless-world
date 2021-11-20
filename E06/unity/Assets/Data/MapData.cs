using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;

[Serializable]
public class MapData {
  public int size;

  public MapData(int _size) {
    size = _size;
  }

  public static MapData fromJSON(string jsonString) {
    JSONNode node  = JSON.Parse(jsonString);
    
    int dataSize = node["size"].AsInt;

    return new MapData(dataSize);
  }
}

