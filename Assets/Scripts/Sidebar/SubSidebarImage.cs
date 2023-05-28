using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.EventSystems;

public class SubSidebarImage : MonoBehaviour
{
    public int id;
    public GameObject plant;
    public Sidebar sidebar;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnClick()
    {
        sidebar.currentPlantId = id;
        Debug.Log($"SubImage click id {id}");
    }
}
