using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldManager : MonoBehaviour{
  public GameObject playerPrefab;

  void Start() {
    EndlessTerrain.Startup(() => {
      CreatePlayer();
    });
  }

  void CreatePlayer() {
    Debug.Log("CreatePlayer");
    GameObject player = Instantiate(playerPrefab, new Vector3(0, 110, 0), Quaternion.identity);
    EndlessTerrain.SetPlayer(player.transform);
  }
}
