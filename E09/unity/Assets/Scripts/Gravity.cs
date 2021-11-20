using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MenteBacata.ScivoloCharacterController;

[RequireComponent(typeof(CharacterMover), typeof(GroundDetector))]
public class Gravity : MonoBehaviour {
  public float gravity = -25f;
  public bool isGrounded { get; private set; }
  public bool groundDetected { get; private set; }
  public GroundInfo groundInfo { get; private set; }

  private CharacterMover mover;
  private GroundDetector groundDetector;

  private List<MoveContact> moveContacts = new List<MoveContact>(CharacterMover.MaxContactsCount);
  private float verticalSpeed = 0;
  private const float minVerticalSpeed = -25f;
  private float nextUngroundedTime = -1f;
  private const float timeBeforeUngrounded = 0.02f;

  void Awake() {
    mover = GetComponent<CharacterMover>();
    groundDetector = GetComponent<GroundDetector>();
  }

  void Update() {
    // Check if we are grounded
    groundDetected = groundDetector.DetectGround(out GroundInfo info);
    groundInfo = info;
    
    if (groundDetected) {
      if (groundInfo.isOnFloor && verticalSpeed < 0.1f) {
        nextUngroundedTime = Time.time + timeBeforeUngrounded;
      }
    } else {
      nextUngroundedTime = -1f;
    }
    isGrounded = Time.time < nextUngroundedTime;

    // Apply gravity
    if (!isGrounded) {
      verticalSpeed += gravity * Time.deltaTime;
      if (verticalSpeed < minVerticalSpeed)
        verticalSpeed = minVerticalSpeed;

      Vector3 velocity = verticalSpeed * transform.up;
      mover.Move(velocity * Time.deltaTime, moveContacts);
    } else {
      verticalSpeed = 0f;
    }
  }
}
