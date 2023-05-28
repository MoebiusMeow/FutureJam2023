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
        var cameraDirection = transform.parent.InverseTransformDirection(Camera.main.transform.forward).normalized;
        cameraDirection.y = 0;
        transform.localRotation = Quaternion.LookRotation(cameraDirection, Vector3.up);
    }
}
