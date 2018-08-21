using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour {

    public Transform targetToFollow;

    public float smoothSpeed = 0.125f;

    public Vector3 offset;


    private void LateUpdate()
    {
        if (targetToFollow != null)
        {
            Vector3 desirePosition = targetToFollow.position + offset;
            Vector3 smoothPosition = Vector3.Lerp(transform.position, desirePosition, smoothSpeed);
            transform.position = smoothPosition;
        }
    }
}
