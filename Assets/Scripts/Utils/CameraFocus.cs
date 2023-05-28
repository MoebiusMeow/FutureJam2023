using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class CameraFocus : MonoBehaviour
{

    public float rotationX = 0.0f;
    public float rotationY = 40.0f;
    public float xRotateSpeed = 200; 
    public float yRotateSpeed = 200;
    public float maxYAngle = 88;
    public float minYAngle = 5;
    public float zoomSpeed = 200;
    public float distance = 10;
    public float minDiatance = 1;
    public float maxDistance = 100;
    public float damping = 5.0f;
    Vector3 pos = Vector3.zero;
    public float moveSpeed = 5;
    public float R = 9;


    void Start()
    {
        Vector3 angle = transform.eulerAngles;
        rotationX = angle.y;
        rotationY = angle.x;
    }

    void FixedUpdate()
    {
        if (Input.GetMouseButton(1))
        {
            rotationX += Input.GetAxis("Mouse X") * xRotateSpeed * Time.fixedDeltaTime;
            rotationY -= Input.GetAxis("Mouse Y") * yRotateSpeed * Time.fixedDeltaTime;
            rotationY = ClamAngle(rotationY, minYAngle, maxYAngle);
        }
        Vector3 direction  = new Vector2(0,0);
        if (Input.GetKey(KeyCode.W))
            direction += Camera.main.transform.forward;
        if (Input.GetKey(KeyCode.S))
            direction -= Camera.main.transform.forward;
        if (Input.GetKey(KeyCode.D))
            direction += Camera.main.transform.right;
        if (Input.GetKey(KeyCode.A))
            direction -= Camera.main.transform.right;
        direction = transform.InverseTransformDirection(direction);
        direction.y = 0;
        direction = direction.normalized;
        Vector3 npos = pos + direction * Time.fixedDeltaTime * moveSpeed;
        if (npos.magnitude > R) npos = pos;
        pos = npos;

        distance -= Input.GetAxis("Mouse ScrollWheel") * zoomSpeed * Time.fixedDeltaTime;
        distance = Mathf.Clamp(distance, minDiatance, maxDistance);
        Quaternion rotation = Quaternion.Euler(rotationY, rotationX, 0.0f);
        Vector3 disVector = new Vector3(0.0f, 0.0f, -distance);
        Vector3 position = rotation * disVector + transform.position + pos;
        Camera.main.transform.rotation = Quaternion.Lerp(Camera.main.transform.rotation, rotation, Time.fixedDeltaTime * damping);
        Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, position, Time.fixedDeltaTime * damping);

    }
    static float ClamAngle(float angle, float min, float max)
    {
        if (angle < -360) angle += 360;
        if (angle > 360) angle -= 360;
        return Mathf.Clamp(angle, min, max);
    }
}
