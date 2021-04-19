using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InterpolateMovement : MonoBehaviour
{
  public Vector3 endPosition;
  public Vector3 endRotation;
  public float smoothTime = 0.6f;
  private Vector3 velocity = Vector3.zero;

  void Update()
  {
    transform.position = Vector3.SmoothDamp(transform.position, endPosition, ref velocity, smoothTime);
    transform.eulerAngles = Vector3.SmoothDamp(transform.rotation.eulerAngles, endRotation, ref velocity, smoothTime);
  }
}
