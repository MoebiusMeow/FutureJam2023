using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SidebarImage : MonoBehaviour
{
    public int type;
    public GameObject subSidebarPrefab;
    public GameObject subSidebar;
    public GameObject image;
    public GameObject typeTag;
    bool isOpen = false;
    public Sidebar parent;

    public void InitSidebar()
    {
        if (type > 0)
        {
            subSidebar = Instantiate(subSidebarPrefab);
            subSidebar.SetActive(false);
            subSidebar.transform.SetParent(transform.parent.parent);
        }
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(type>0) subSidebar.transform.position = transform.localPosition + transform.parent.position + Vector3.up * 200;
    }

    public void OnClose()
    {
        isOpen = false;
        if (type > 0)
        {
            image.GetComponent<Image>().sprite = parent.closeSprites[type];
            subSidebar.SetActive(false);
        }
        else
        {
            parent.currentPlantId = 0;
            image.GetComponent<Image>().sprite = parent.closeSprites[0];
        }
    }

    public void OnOpen()
    {
        isOpen = true;
        if (type > 0)
        {
            if (subSidebar.GetComponent<SubSidebar>().images.Count == 0)
                return;
            subSidebar.SetActive(true);
            image.GetComponent<Image>().sprite = parent.openSprites[type];
        }
        else
        {
            parent.currentPlantId = type;
            image.GetComponent<Image>().sprite = parent.openSprites[0];
        }
    }

    public void OnClick()
    {
        if(isOpen)
        {
            OnClose();
            return;
        }
        List<GameObject> objs = parent.images;
        foreach(GameObject obj in objs)
        {
            obj.GetComponent<SidebarImage>().OnClose();
        }
        OnOpen();
        Debug.Log("Sidebar Click.");
    }
}
