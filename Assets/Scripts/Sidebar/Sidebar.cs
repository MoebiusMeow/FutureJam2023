using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

/*
 Sidebar.prefab 是一个底边栏，提供交互功能，并维护当前选中的植物。
 它绑定到本脚本，本脚本提供如下四个接口：

    public void AddPlant(int type, int id);
    这个函数将会添加类型为 `type`，编号为 `id` 的植物到底边栏。
    
    public int GetCurrentPlantId();
    这个函数将会返回当前选中的植物 id。特别地，id=-1 表示铲子，id=0 表示没有选中植物。

    public void SetCurrentPlantId(int id);
    这个函数将会把当前选中的植物 id 强制设置成 `id`。

    public void Tuck();
    这个函数会把所有弹出的子栏目收起。
 
 Scene ui 中的四个按钮分别对应这四个功能的测试。

 Sidebar 的主要工作方式是，在栏目中点击植物种类对应的按钮后，弹出子栏目；
 再点击对应植物的按钮，则当前选中的植物 id 被设定成对应植物 id。
 */


public class Sidebar : MonoBehaviour
{
    public GameObject imagePrefab;
    public List<GameObject> images = new List<GameObject>();
    public int currentPlantId;

    void Start()
    {
        AddPlantType(-1);
        AddPlantType(0);
        AddPlant(1, 1);
        AddPlant(2, 3);
        AddPlant(2, 4);
    }

    // interface starts
    public void AddPlant(int type, int id)
    {
        int idx;
        for(idx=0; idx<images.Count; ++idx)
        {
            if (images[idx].GetComponent<SidebarImage>().type == type)
                break;
        }
        if(idx==images.Count)
        {
            AddPlantType(type);
        }
        images[idx].GetComponent<SidebarImage>().subSidebar.GetComponent<SubSidebar>().AddPlantId(this, id);
    }
    
    public int GetCurrentPlantId()
    {
        return currentPlantId;
    }

    public void SetCurrentPlantId(int id)
    {
        currentPlantId = id;
    }

    public void Tuck()
    {
        foreach(GameObject img in images)
        {
            img.GetComponent<SidebarImage>().subSidebar.SetActive(false);
        }
    }

    public void SetSeedNumber(int id, int number)
    {
        foreach(GameObject img in images)
        {
            foreach(GameObject subImg in img.GetComponent<SidebarImage>().subSidebar.GetComponent<SubSidebar>().images)
            {
                SubSidebarImage im = subImg.GetComponent<SubSidebarImage>();
                if(im.id==id)
                {
                    im.number.GetComponent<TMP_Text>().text = $"{number}";
                    if(number==0)
                    {
                        im.image.GetComponent<Button>().interactable = false;
                    }
                    else
                    {
                        im.image.GetComponent<Button>().interactable = true;
                    }
                }
            }
        }
    }

    // interface ends

    void AddPlantType(int type)
    {
        GameObject imgObj = Instantiate(imagePrefab);
        imgObj.transform.SetParent(transform);


        if(type>0)imgObj.transform.SetSiblingIndex(imgObj.transform.GetSiblingIndex() - 2);
        SidebarImage img = imgObj.GetComponent<SidebarImage>();
        img.InitSidebar();
        img.type = type;
        img.image.GetComponent<Image>().color = new Color32((byte)(type * 30 + 150), 0, 0, 100); // TODO: set the image

        var TypeName = new string[] { "Shovel", "Cancel", "Fat", "Flower", "Fruit" };
        img.text.GetComponent<TMP_Text>().text = TypeName[type+1];

        img.parent = this;
        images.Add(imgObj);
    }

    // following: debug use only
    public void DebugAddPlant37()
    {
        AddPlant(3, 7);
    }
    public void DebugSetCurrentPlantIdTo233()
    {
        SetCurrentPlantId(233);
    }
    public void DebugCurrentPlantId()
    {
        Debug.Log($"Current Plant Id = {GetCurrentPlantId()}");
    }
    public void DebugNumber399()
    {
        SetSeedNumber(3, 99);
    }
    public void DebugNumber30()
    {
        SetSeedNumber(3, 0);
    }
}