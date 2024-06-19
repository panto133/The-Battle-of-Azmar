using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] private float camSpeed = 30f;
    [SerializeField] private float camYspeed = 50f;
    [SerializeField] private float camYMax = 20f;
    [SerializeField] private float camYMin = 5f;

    [SerializeField] Transform cam;

    private void FixedUpdate()
    {
        MoveCameraY();
        MoveCameraXZ();
    }

    private void MoveCameraXZ()
    {
        if (Input.GetKey(KeyCode.W))
        {
            cam.position += new Vector3(0, 0, camSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.S))
        {
            cam.position += new Vector3(0, 0, -camSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.D))
        {
            cam.position += new Vector3(camSpeed * Time.deltaTime, 0, 0);
        }
        if (Input.GetKey(KeyCode.A))
        {
            cam.position += new Vector3(-camSpeed * Time.deltaTime, 0, 0);
        }
    }
    private void MoveCameraY()
    {
        if (Input.mouseScrollDelta.y > 0f)
        {
            cam.position += new Vector3(0, -camYspeed * Time.deltaTime, 0);
        }
        if (Input.mouseScrollDelta.y < 0f)
        {
            cam.position += new Vector3(0, camYspeed * Time.deltaTime, 0);
        }
        cam.position = new Vector3(cam.position.x, Mathf.Clamp(cam.position.y, camYMin, camYMax), cam.position.z);
    }
}
