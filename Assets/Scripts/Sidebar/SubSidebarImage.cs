using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.EventSystems;

public class SubSidebarImage : MonoBehaviour
{
    public int id;
    Vector3 rotationEuler;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnClick()
    {
        Debug.Log("SubImage click.");
        id += 1;
    }
}
