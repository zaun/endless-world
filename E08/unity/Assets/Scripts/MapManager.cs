using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class MapManager : MonoBehaviour {
  public string API = "http://localhost:7000";
  private Dictionary<Vector2, ChunkData> chunkCache = new Dictionary<Vector2, ChunkData>();
  private MapData infoCache;
  public static MapManager instance;

  public static void RequestMapInfo(Action<MapData> callback) {
    if (instance != null) {
      instance._RequestMapInfo(callback);
    } else {
      Debug.LogError("No MapManager in scene.");
    }
  }

  public static void RequestMapChunk(Vector2 mapChunkCoord, Action<ChunkData> callback) {
    if (instance != null) {
      instance._RequestMapChunk(mapChunkCoord, callback);
    } else {
      Debug.LogError("No MapManager in scene.");
    }
  }

  public static int MapChunkSize() {
    if (instance != null && instance.infoCache != null) {
      return instance.infoCache.size;
    }
    return -1;
  }

  // Verify there is only one MapManager in the world.
  private void Awake() {
    if (instance == null) {
      instance = this;
    } else if (instance != this) {
      Destroy(gameObject);
    }
  }

	private void _RequestMapChunk(Vector2 mapChunkCoord, Action<ChunkData> callback) {
    if (chunkCache.ContainsKey(mapChunkCoord)) {
      callback(chunkCache[mapChunkCoord]);
      return;
    }

    StartCoroutine(MapChunkCoroutine(mapChunkCoord, data => {
      chunkCache[mapChunkCoord] = data;
      callback(data);
    }));
	}

	private IEnumerator MapChunkCoroutine(Vector2 mapChunkCoord, Action<ChunkData> callback) {
    string uri = $"{API}/chunk/{mapChunkCoord.x}/{mapChunkCoord.y}";
    UnityWebRequest webRequest = UnityWebRequest.Get(uri);
    yield return webRequest.SendWebRequest();

    switch(webRequest.result) {
      case UnityWebRequest.Result.ConnectionError:
      case UnityWebRequest.Result.DataProcessingError:
      case UnityWebRequest.Result.ProtocolError:
        callback(null);
        break;
      case UnityWebRequest.Result.Success:
        ChunkData chunkData = ChunkData.fromJSON(webRequest.downloadHandler.text);
        callback(chunkData);
        break;

      default:
        Debug.Log(webRequest.result);
        break;
    }
	}

	private void _RequestMapInfo(Action<MapData> callback) {
    if (infoCache != null) {
      callback(infoCache);
      return;
    }

    StartCoroutine(MapInfoCoroutine(data => {
      infoCache = data;
      callback(data);
    }));
	}

	private IEnumerator MapInfoCoroutine(Action<MapData> callback) {
    string uri = $"{API}/info";
    UnityWebRequest webRequest = UnityWebRequest.Get(uri);
    yield return webRequest.SendWebRequest();

    switch(webRequest.result) {
      case UnityWebRequest.Result.ConnectionError:
      case UnityWebRequest.Result.DataProcessingError:
      case UnityWebRequest.Result.ProtocolError:
        callback(null);
        break;
      case UnityWebRequest.Result.Success:
        MapData mapData = MapData.fromJSON(webRequest.downloadHandler.text);
        callback(mapData);
        break;

      default:
        Debug.Log(webRequest.result);
        break;
    }
	}
}
