using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class SubSidebar : MonoBehaviour
{
    public GameObject subImagePrefab;
    public List<GameObject> images = new List<GameObject>();

    void Start()
    {

    }

    public void AddPlantId(Sidebar sidebar, int id)
    {
        GameObject imgObj = Instantiate(subImagePrefab);
        imgObj.transform.SetParent(transform);


        SubSidebarImage img = imgObj.GetComponent<SubSidebarImage>();
        img.id = id;
        // img.image.GetComponent<Image>().color = new Color32(0, 0, (byte)(id * 30 + 40), 100); // TODO: set the image

        var TypeName = new string[] { "Flora1", "Flora2", "Flora3", "Flora4", "Flora5", "Flora6", "Flora7" };
        img.text.GetComponent<TMP_Text>().text = TypeName[id-1];

        img.sidebar = sidebar;
        images.Add(imgObj);
    }
}