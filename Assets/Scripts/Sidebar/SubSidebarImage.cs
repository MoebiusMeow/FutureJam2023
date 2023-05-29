using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

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
        var intros = new string[]{
            "ʹһ�����ڵؿ�ķ����ȼ�+1",
                "ʹ�������ڵؿ�ķ����ȼ�+1",
                "�������ڵؿ�ķ����ȼ�����������һ���ؿ�",
                "�������ڵؿ�ķ����ȼ����������������ؿ�",
                "�������ڵؿ�ķ����ȼ�����������ؿ飬��˥��Ϊ0",
                "�ܲ����ɹ�����ʳ�õĹ�ʵ\r\nЧ���п�",
                "�ܲ����ɹ�����ʳ�õĹ�ʵ\r\nЧ�ʽϸ�",
                "����ռ�õؿ�ķ����ȼ�Խ�ߣ����Ч��Խ��",
                "ɢ���ŷҷ���ζ�Ľ�С���䣬�ܹ���������",
                "��չ��������������컨�䣬�ܹ��������",
                "��˵�еľ޴󻨶䣬�������޷�ƴ��������ƾ����ͬ���໥��������Ҳ�����������ʲô����"
        };
        sidebar.hintbar.GetComponent<Hintbar>().intro.GetComponent<TMP_Text>().text = intros[id - 1];
        var mottos = new string[]
        {
            "ÿ10s����1������",
            "ÿ20s����1������",
            "ÿ30s����1������",
            "ÿ40s����1������",
            "ÿ40s����1������",
            "ÿ10s����1�����ӣ�1�Ź�ʵ",
            "ÿ30s����3�����ӣ�30�Ź�ʵ",
            "ÿ30s����3�����ӣ�3X�Ź�ʵ",
            "������",
            "������",
            "������"
        };
        sidebar.hintbar.GetComponent<Hintbar>().motto.GetComponent<TMP_Text>().text = mottos[id - 1];
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
