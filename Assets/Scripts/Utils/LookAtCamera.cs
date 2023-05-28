using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        var cameraDirection = transform.InverseTransformDirection(Camera.main.transform.forward);
        Debug.Log(cameraDirection);
        cameraDirection.z = 0;
        var angle = Vector3.Angle(Vector3.up, cameraDirection.normalized);
        transform.Rotate(new Vector3(0, 0 ,-angle));
    }
}
