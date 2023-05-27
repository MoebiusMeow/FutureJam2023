using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Sidebar : MonoBehaviour
{
    public GameObject imagePrefab;
    List<GameObject> images = new List<GameObject>();

    void Start()
    {
        for (int i = 0; i < 13; ++i)
        {
            GameObject img = Instantiate(imagePrefab);
            img.transform.SetParent(transform);
            img.GetComponent<SidebarImage>().id = i;
            images.Add(img);
        }
    }

    void AddPlantType()
    {

    }
}