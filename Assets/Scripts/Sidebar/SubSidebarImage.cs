using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.EventSystems;

public class SubSidebarImage : MonoBehaviour
{
    public int id;
    public GameObject image;
    public GameObject text;
    public GameObject number;
    public GameObject hintbar;
    public Sidebar sidebar;
    public GameObject hintbarPrefab;

    void Start()
    {
        hintbar = Instantiate(hintbarPrefab);
        hintbar.transform.SetParent(sidebar.GetComponent<Sidebar>().transform.parent);
        hintbar.SetActive(false);
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
        hintbar.SetActive(true);
    }

    public void OnLeave()
    {
        hintbar.SetActive(false);
    }
}
