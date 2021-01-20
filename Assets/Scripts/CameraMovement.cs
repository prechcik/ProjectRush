using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public Transform target;
    public Vector3 offset;

    public float smoothing = 0.125f;


    // Update is called once per frame
    void LateUpdate()
    {
        if (target != null)
        {
            Vector3 desiredPos = target.position + offset;
            Vector3 smoothPos = Vector3.Lerp(transform.position, desiredPos, smoothing);
            transform.position = smoothPos;
            transform.LookAt(target);
        } else
        {

        }
    }
}
