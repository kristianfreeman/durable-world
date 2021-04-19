using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InterpolateMovement : MonoBehaviour
{
  public Vector3 endPosition;
  public Quaternion endRotation;

  public float rotationSmoothTime = 0.3f;
  public float positionSmoothTime = 0.6f;
  private Vector3 posVelocity = Vector3.zero;
  private float rotVelocity = 0.0f;

  void Update()
  {
    transform.position = Vector3.SmoothDamp(transform.position, endPosition, ref posVelocity, positionSmoothTime);
    transform.rotation = endRotation;

    // float delta = Quaternion.Angle(transform.rotation, endRotation);
    // if (delta > 0f)
    // {
    //   float t = Mathf.SmoothDampAngle(delta, 0.0f, ref rotVelocity, rotationSmoothTime);
    //   t = 1.0f - (t / delta);
    //   transform.rotation = Quaternion.Slerp(transform.rotation, endRotation, t);
    // }
  }
}
