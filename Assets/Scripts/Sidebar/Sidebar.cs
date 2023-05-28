using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

/*
 Sidebar.prefab 是一个底边栏，提供交互功能，并维护当前选中的植物。
 它绑定到本脚本，本脚本提供如下五个接口：

    public void AddPlant(int type, int id);
    这个函数将会添加类型为 `type`，编号为 `id` 的植物到底边栏。
    
    public int GetCurrentPlantId();
    这个函数将会返回当前选中的植物 id。特别地，id=-1 表示铲子，id=0 表示没有选中植物。

    public void SetCurrentPlantId(int id);
    这个函数将会把当前选中的植物 id 强制设置成 `id`。

    public void Tuck();
    这个函数会把所有弹出的子栏目收起。

    public void SetSeedNumber(int id, int number);
    这个函数将 id 为 `id` 的植物种子数量设置为 `number`。
 
 Scene ui 中的四个按钮分别对应这四个功能的测试。

 Sidebar 的主要工作方式是，在栏目中点击植物种类对应的按钮后，弹出子栏目；
 再点击对应植物的按钮，则当前选中的植物 id 被设定成对应植物 id。
 */


public class Sidebar : MonoBehaviour
{
    public GameObject imagePrefab;
    public Sprite[] closeSprites;
    public Sprite[] openSprites;
    public Sprite[] tags;
    public List<GameObject> images = new List<GameObject>();
    public int currentPlantId;

    void Start()
    {
        AddPlantType(1);
        AddPlantType(2);
        AddPlantType(3);
        AddPlantType(-1);
        images[3].GetComponent<SidebarImage>().image.GetComponent<Button>().interactable = true;
    }

    // interface starts
    public void AddPlant(int type, int id)
    {
        int idx;
        for(idx=0; idx<images.Count; ++idx)
        {
            SidebarImage img = images[idx].GetComponent<SidebarImage>();
            if (img.type == type)
            {
                img.typeTag.GetComponent<Image>().sprite = tags[type];
                img.image.GetComponent<Button>().interactable = true;
                break;
            }
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
            img.GetComponent<SidebarImage>().OnClose();
        }
    }

    public void SetSeedNumber(int id, int number)
    {
        for(int idx=0;idx<3;++idx)
        {
            foreach(GameObject subImg in images[idx].GetComponent<SidebarImage>().subSidebar.GetComponent<SubSidebar>().images)
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

        SidebarImage img = imgObj.GetComponent<SidebarImage>();
        img.type = type;
        img.InitSidebar();
        if(type>0)
            img.image.GetComponent<Image>().sprite = closeSprites[type];
        else if(type==-1)
            img.image.GetComponent<Image>().sprite = closeSprites[0];

        img.parent = this;
        images.Add(imgObj);
    }

    void ActivatePlantType(int type)
    {

    }

    // following: debug use only
    public void DebugAddPlant37()
    {
        AddPlant(3, 7);
        AddPlant(1, 3);
        AddPlant(2, 5);
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