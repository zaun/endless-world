using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndlessTerrain : MonoBehaviour {
	public float maxViewDistance = 450;
	public Transform player;

	private bool initilized = false;
	private bool ready = false;
  private MapData mapInfo;
	private Vector2 playerPosition;
	private int chunkSize;
	private int chunksVisibleInView;
	private Action onReady;

  public static EndlessTerrain instance;

	Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
	List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();

  public static void Startup(Action _onReady) {
    if (instance != null) {
			instance.onReady = _onReady;
			MapManager.RequestMapInfo(data => {
				instance.mapInfo = data;
				instance.Initilize();
			});
    }
  }

	public static void SetPlayer(Transform _player) {
		if (instance != null) {
			instance.player = _player;
		}
	}
  
  // Verify there is only one EndlessTerrain in the world.
  private void Awake() {
    if (instance == null) {
      instance = this;
    } else if (instance != this) {
      Destroy(gameObject);
    }
  }

	void Update() {
		if (player) {
			playerPosition = new Vector2(player.position.x, player.position.z);
		} else {
			playerPosition = new Vector2(0, 0);
		}
		UpdateVisibleChunks();

		if (initilized && !ready) {
			ready = true;
			foreach(TerrainChunk chunk in terrainChunkDictionary.Values) {
				if (!chunk.IsLoaded()) {
					ready = false;
				}
			}
			if (ready) {
				onReady();
			}
		}
	}

  void Initilize() {
		if (mapInfo != null) {
			chunkSize = mapInfo.size;
			chunksVisibleInView = Mathf.RoundToInt(maxViewDistance / chunkSize);
			initilized = true;
		} else {
			Debug.Log("Could not connect to server");
		}
  }
		
	void UpdateVisibleChunks() {
		if (!initilized) {
			return;
		}

		for (int i = 0; i < terrainChunksVisibleLastUpdate.Count; i++) {
			terrainChunksVisibleLastUpdate[i].SetVisible(false);
		}
		terrainChunksVisibleLastUpdate.Clear();
			
		int currentChunkCoordX = Mathf.RoundToInt(playerPosition.x / chunkSize);
		int currentChunkCoordY = Mathf.RoundToInt(playerPosition.y / chunkSize);

		for (int yOffset = -chunksVisibleInView; yOffset <= chunksVisibleInView; yOffset++) {
			for (int xOffset = -chunksVisibleInView; xOffset <= chunksVisibleInView; xOffset++) {
				Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

				if (terrainChunkDictionary.ContainsKey(viewedChunkCoord)) {
					terrainChunkDictionary[viewedChunkCoord].UpdateTerrainChunk(playerPosition, maxViewDistance);
					if (terrainChunkDictionary[viewedChunkCoord].IsVisible()) {
						terrainChunksVisibleLastUpdate.Add(terrainChunkDictionary[viewedChunkCoord]);
					}
				} else {
					terrainChunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize, transform));
				}
			}
		}
	}
}