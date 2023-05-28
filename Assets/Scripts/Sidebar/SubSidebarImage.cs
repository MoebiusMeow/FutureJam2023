using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SubSidebarImage : MonoBehaviour
{
    public int id;
    public GameObject image;
    public GameObject text;
    public GameObject number;
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

    public void OnHover()
    {
        sidebar.hintbar.GetComponent<Image>().sprite = sidebar.hintbarSprites[id - 1];
        sidebar.hintbar.SetActive(true);
    }

    public void OnLeave()
    {
        if (sidebar.currentPlantId > 0)
        {
            sidebar.hintbar.GetComponent<Image>().sprite = sidebar.hintbarSprites[sidebar.currentPlantId - 1];
            sidebar.hintbar.SetActive(true);
        }
        else sidebar.hintbar.SetActive(false);
    }
}
