using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonMovement : MonoBehaviour
{
  public CharacterController controller;
  public Transform cam;

  public float speed = 6.0f;
  public float jumpSpeed = 8.0f;
  public float gravity = 20.0f;
  public float rotateSpeedGrounded = 2f;
  public float inAirRotation = 0.5f;
  public float inAirSpeed = 2.0f;
  public float inAirDrift = 0.5f;


#if !UNITY_WEBGL || UNITY_EDITOR
  private float rotationMultiplier = 0.75f;
#else
  private float rotationMultiplier = 1.5f;
#endif

  private Vector3 moveDirection = Vector3.zero;
  void Update()
  {
    if (transform.position.y < -1)
    {
      transform.position = new Vector3(0, 0.5f, 0);
      return;
    }

    // ---------------------------------------- At Ground -----------------------------------------
    if (controller.isGrounded)
    {
      // We are grounded, so recalculate move direction directly from axes. Z is movement direction

      moveDirection = new Vector3(0, 0, Input.GetAxis("Vertical"));
      moveDirection = transform.TransformDirection(moveDirection);
      moveDirection *= speed;
      // Rotate around y - axis. This is to change direction
      transform.Rotate(0, Input.GetAxis("Horizontal") * rotateSpeedGrounded * rotationMultiplier, 0);

      //     float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;

      if (Input.GetButton("Jump"))
      {
        moveDirection.y = jumpSpeed;
      }
    }
    // ---------------------------------------------------- In Air  ----------------------------
    else
    {
      // We are not grounded. We can still influence the movement with the horizontal and vertical axis, but not so strong.
      moveDirection += transform.TransformDirection(new Vector3(Input.GetAxis("Horizontal") * Time.deltaTime * inAirDrift, 0, Input.GetAxis("Vertical") * Time.deltaTime * inAirSpeed));
      transform.Rotate(0, Input.GetAxis("Horizontal") * inAirRotation * rotationMultiplier, 0); // Uncomment this to let the player change the face direction in air
    }
    // ---------------------------------------------------------------------------------------------        
    // Apply gravity
    moveDirection.y -= gravity * Time.deltaTime;
    // Move the controller
    controller.Move(moveDirection * Time.deltaTime);
  }

}
