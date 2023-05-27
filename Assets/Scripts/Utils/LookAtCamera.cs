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
       
        cameraDirection.z = 0;
        var angle = Vector3.Angle(Vector3.up, cameraDirection.normalized);
        Debug.LogFormat("{0:s} {1:f} {2:s}", cameraDirection.ToString(), angle, transform.TransformDirection(Vector3.up).ToString());
        transform.Rotate(new Vector3(0, 0 ,-angle));
    }
}
