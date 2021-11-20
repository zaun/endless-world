using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldManager : MonoBehaviour {
  void Start() {
    EndlessTerrain.Startup(() => {
      Debug.Log("Ready");
    });
  }
}
