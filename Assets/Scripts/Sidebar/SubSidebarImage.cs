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
            "使一个相邻地块的肥力等级+1",
                "使三个相邻地块的肥力等级+1",
                "根据相邻地块的肥力等级来增幅另外一个地块",
                "根据相邻地块的肥力等级来增幅另外三个地块",
                "根据相邻地块的肥力等级来增幅多个地块，至衰减为0",
                "能产出可供巨龙食用的果实\r\n效率尚可",
                "能产出可供巨龙食用的果实\r\n效率较高",
                "中心占用地块的肥力等级越高，结果效率越高",
                "散发着芬芳气味的娇小花朵，能够吸引昆虫",
                "舒展着美丽花瓣的奇异花朵，能够吸引鸟儿",
                "传说中的巨大花朵，其名讳无法拼读，巨龙凭此与同族相互呼唤。但也可能引来别的什么……"
        };
        sidebar.hintbar.GetComponent<Hintbar>().intro.GetComponent<TMP_Text>().text = intros[id - 1];
        var mottos = new string[]
        {
            "每10s产出1颗种子",
            "每20s产出1颗种子",
            "每30s产出1颗种子",
            "每40s产出1颗种子",
            "每40s产出1颗种子",
            "每10s产出1颗种子，1颗果实",
            "每30s产出3颗种子，30颗果实",
            "每30s产出3颗种子，3X颗果实",
            "不产种",
            "不产种",
            "不产种"
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
