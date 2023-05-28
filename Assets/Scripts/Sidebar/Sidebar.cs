using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

/*
 Sidebar.prefab ��һ���ױ������ṩ�������ܣ���ά����ǰѡ�е�ֲ�
 ���󶨵����ű������ű��ṩ��������ӿڣ�

    public void AddPlant(int type, int id);
    ������������������Ϊ `type`�����Ϊ `id` ��ֲ�ﵽ�ױ�����
    
    public int GetCurrentPlantId();
    ����������᷵�ص�ǰѡ�е�ֲ�� id���ر�أ�id=-1 ��ʾ���ӣ�id=0 ��ʾû��ѡ��ֲ�

    public void SetCurrentPlantId(int id);
    �����������ѵ�ǰѡ�е�ֲ�� id ǿ�����ó� `id`��

    public void Tuck();
    �������������е���������Ŀ����

    public void SetSeedNumber(int id, int number);
    ��������� id Ϊ `id` ��ֲ��������������Ϊ `number`��
 
 Scene ui �е��ĸ���ť�ֱ��Ӧ���ĸ����ܵĲ��ԡ�

 Sidebar ����Ҫ������ʽ�ǣ�����Ŀ�е��ֲ�������Ӧ�İ�ť�󣬵�������Ŀ��
 �ٵ����Ӧֲ��İ�ť����ǰѡ�е�ֲ�� id ���趨�ɶ�Ӧֲ�� id��
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