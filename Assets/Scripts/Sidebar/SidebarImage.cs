using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.EventSystems;

public class SidebarImage : MonoBehaviour
{
    public int type;
    public GameObject subSidebar;
    public Sidebar parent;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnClick()
    {
        List<GameObject> objs = parent.images;
        foreach(GameObject obj in objs)
        {
            obj.GetComponent<SidebarImage>().subSidebar.SetActive(false);
        }
        if (type > 0)
            subSidebar.SetActive(true);
        else
            parent.currentPlantId = type;

        Debug.Log("Sidebar Click.");
    }
}
