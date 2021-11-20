using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour {
  public Transform target;

  public Vector3 offsetPosition;
  public Space offsetPositionSpace = Space.Self;

  public static PlayerCamera instance;

  public static void SetTarget(Transform _target) {
    if (instance != null) {
      instance.target = _target;
    } else {
      Debug.LogError("No PlayerCamera in scene.");
    }
  }

  // Verify there is only one PlayerCamera in the world.
  private void Awake() {
    if (instance == null) {
      instance = this;
    } else if (instance != this) {
      Destroy(gameObject);
    }
  }

  private void LateUpdate() {
    if (target == null) {
      return;
    }

    if (offsetPositionSpace == Space.Self) {
      transform.position = target.TransformPoint(offsetPosition);
    } else {
      transform.position = target.position + offsetPosition;
    }

    transform.rotation = target.rotation;
  }
}
