using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MenteBacata.ScivoloCharacterController;

[RequireComponent(typeof(CharacterMover), typeof(Gravity))]
public class PlayerController : MonoBehaviour {
  private CharacterMover mover;
  private Gravity gravity;

  public float moveSpeed = 5f;
  public float jumpSpeed = 8f;
  public float rotationSpeed = 720f;

  private float verticalSpeed = 0f;

  private List<MoveContact> moveContacts = new List<MoveContact>(CharacterMover.MaxContactsCount);

  void Awake() {
    mover = GetComponent<CharacterMover>();
    gravity = GetComponent<Gravity>();

    PlayerCamera.SetTarget(transform);
  }

  void Update() {
    float x = 0;
    float y = 0;

    if (Input.GetKey(KeyCode.W)) {
      y = 1;
    } else if (Input.GetKey(KeyCode.S)) {
      y = -1;
    }

    if (Input.GetKey(KeyCode.A)) {
      x = -1;
    } else if (Input.GetKey(KeyCode.D)) {
      x = 1;
    }

    if (Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.S)) {
      y = 0;
    }
    if (Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.D)) {
      x = 0;
    }

    Vector3 forward = Vector3.ProjectOnPlane(transform.forward, transform.up).normalized;
    Vector3 right = Vector3.Cross(transform.up, forward);

    Vector3 velocity = y * forward * moveSpeed;

    if (gravity.isGrounded) {
      mover.isInWalkMode = true;
      verticalSpeed = 0f;
    } else {
      mover.isInWalkMode = false;
      velocity += verticalSpeed * transform.up;
    }

    RotateTowards(x * right * rotationSpeed);
    mover.Move(velocity * Time.deltaTime, moveContacts);
  }

  private void RotateTowards(Vector3 direction) {
    Vector3 direzioneOrizz = Vector3.ProjectOnPlane(direction, transform.up);

    if (direzioneOrizz.sqrMagnitude < 1E-06f) {
      return;
    }

    Quaternion rotazioneObbiettivo = Quaternion.LookRotation(direzioneOrizz, transform.up);
    transform.rotation = Quaternion.RotateTowards(transform.rotation, rotazioneObbiettivo, rotationSpeed * Time.deltaTime);
  }
}
