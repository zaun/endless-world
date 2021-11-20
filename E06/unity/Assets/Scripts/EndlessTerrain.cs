using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndlessTerrain : MonoBehaviour {
	public float maxViewDistance = 450;
	public Transform player;

	private bool initilized = false;
  private MapData mapInfo;
	private Vector2 playerPosition;
	private int chunkSize;
	private int chunksVisibleInView;

	Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
	List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();
  
  void Start() {
    MapManager.RequestMapInfo(data => {
      mapInfo = data;
      Initilize();
    });
  }

	void Update() {
		playerPosition = new Vector2 (player.position.x, player.position.z);
		UpdateVisibleChunks ();
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