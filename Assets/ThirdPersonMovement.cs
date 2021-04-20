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
  public float rotateSpeedGrounded = 0.1f;
  public float inAirSpeed = 2.0f;
  public float inAirDrift = 0.5f;


  private Vector3 moveDirection = Vector3.zero;
  void Update()
  {
    Debug.Log(transform.position.y);
    if (transform.position.y < -1)
    {
      Debug.Log("Should be moving");
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
      transform.Rotate(0, Input.GetAxis("Horizontal") * rotateSpeedGrounded, 0);

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
      //transform.Rotate(0, Input.GetAxis ("Horizontal") * inAirRotation*0.5, 0); // Uncomment this to let the player change the face direction in air
    }
    // ---------------------------------------------------------------------------------------------        
    // Apply gravity
    moveDirection.y -= gravity * Time.deltaTime;
    // Move the controller
    controller.Move(moveDirection * Time.deltaTime);
  }

  // public float speed = 6f;
  // public float turnSmoothTime = 0.1f;
  // float turnSmoothVelocity;
  // public float gravity = 8;
  // private float vSpeed = 0;
  // public float jumpSpeed = 10;
  // public float inAirDrift = 0.5f;
  // public float inAirSpeed = 2.0f;

  // void Update()
  // {
  //   bool dead = transform.position.y < -1;
  //   if (dead)
  //   {
  //     transform.position = new Vector3(0, 0, 0);
  //   }

  //   float horizontal = Input.GetAxisRaw("Horizontal");
  //   float vertical = Input.GetAxisRaw("Vertical");

  //   Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

  //   if (controller.isGrounded)
  //   {
  //     vSpeed = 0;
  //     if (Input.GetKeyDown("space"))
  //     { // unless it jumps:
  //       vSpeed = jumpSpeed;
  //     }
  //   }
  //   else
  //   {
  //     direction += transform.TransformDirection(new Vector3(Input.GetAxis("Horizontal") * Time.deltaTime * inAirDrift, 0, Input.GetAxis("Vertical") * Time.deltaTime * inAirSpeed));

  //   }

  //   if (direction.magnitude >= 0.1f)
  //   {
  //     float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
  //     float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
  //     transform.rotation = Quaternion.Euler(0f, angle, 0f);

  //     Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
  //     vSpeed -= gravity * Time.deltaTime;
  //     moveDir.y = vSpeed;

  //     controller.Move(moveDir.normalized * speed * Time.deltaTime);
  //   }
  // }
}
