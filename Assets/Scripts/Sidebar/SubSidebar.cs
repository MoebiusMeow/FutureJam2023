using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SubSidebar : MonoBehaviour
{
    public GameObject subImagePrefab;
    List<GameObject> images = new List<GameObject>();

    void Start()
    {

    }

    public void AddPlantId(Sidebar sidebar, int id)
    {
        GameObject imgObj = Instantiate(subImagePrefab);
        imgObj.transform.SetParent(transform);

        imgObj.GetComponent<Image>().color = new Color32(0, 0, (byte)(id * 30+40), 100); // TODO: set the image

        SubSidebarImage img = imgObj.GetComponent<SubSidebarImage>();
        img.id = id;
        img.sidebar = sidebar;
        images.Add(imgObj);
    }
}