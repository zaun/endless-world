using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;

[Serializable]
public class MapData {
  public int size;
  public AnimationCurve heightCurve;

  public MapData(int _size, AnimationCurve _heightCurve) {
    size = _size;
    heightCurve = new AnimationCurve(_heightCurve.keys);
  }

  public static MapData fromJSON(string jsonString) {
    JSONNode node  = JSON.Parse(jsonString);
    
    int dataSize = node["size"].AsInt;
    JSONArray keys = node["heightCurve"].AsArray;

    AnimationCurve dataCurve = new AnimationCurve();
    for (int i = 0; i < keys.Count; i++) {
      JSONNode key = keys[i];
      float time = key["t"].AsFloat;
      float value = key["v"].AsFloat;
      dataCurve.AddKey(time, value);
    }

    return new MapData(dataSize, dataCurve);
  }
}

