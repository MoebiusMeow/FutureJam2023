using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.EventSystems;

public class SidebarImage : MonoBehaviour
{
    public int type;
    public GameObject subSidebarPrefab;
    public GameObject subSidebar;
    public GameObject image;
    public GameObject text;
    public Sidebar parent;

    public void InitSidebar()
    {
        subSidebar = Instantiate(subSidebarPrefab);
        subSidebar.SetActive(false);
        subSidebar.transform.SetParent(transform.parent.parent);
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        subSidebar.transform.position = transform.localPosition + transform.parent.position + Vector3.up * 80;
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
