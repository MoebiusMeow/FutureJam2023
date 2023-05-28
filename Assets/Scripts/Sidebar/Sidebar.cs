using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

/*
 Sidebar.prefab ��һ���ױ������ṩ�������ܣ���ά����ǰѡ�е�ֲ�
 ���󶨵����ű������ű��ṩ�����ĸ��ӿڣ�

    public void AddPlant(int type, int id);
    ������������������Ϊ `type`�����Ϊ `id` ��ֲ�ﵽ�ױ�����
    
    public int GetCurrentPlantId();
    ����������᷵�ص�ǰѡ�е�ֲ�� id���ر�أ�id=-1 ��ʾ���ӣ�id=0 ��ʾû��ѡ��ֲ�

    public void SetCurrentPlantId(int id);
    �����������ѵ�ǰѡ�е�ֲ�� id ǿ�����ó� `id`��

    public void Tuck();
    �������������е���������Ŀ����
 
 Scene ui �е��ĸ���ť�ֱ��Ӧ���ĸ����ܵĲ��ԡ�

 Sidebar ����Ҫ������ʽ�ǣ�����Ŀ�е��ֲ�������Ӧ�İ�ť�󣬵�������Ŀ��
 �ٵ����Ӧֲ��İ�ť����ǰѡ�е�ֲ�� id ���趨�ɶ�Ӧֲ�� id��
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