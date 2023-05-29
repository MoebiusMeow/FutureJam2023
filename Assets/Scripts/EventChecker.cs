using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventChecker : MonoBehaviour
{
    static public EventChecker Instance;

    public UI ui;
    public Sidebar sidebar;
    public HexTilemap tilemap;
    bool downedEvent1 = false;
    bool downedEvent2 = false;
    bool downedEvent3 = false;
    bool downedEventC1 = false;
    bool downedEventC2 = false;
    public bool downedEventC3 = false;
    bool downedEventLoop = false;
    Dictionary<int, int> plantCount = new();
    public int days = 0;
    public int fruitRequired = 0;
    public bool loopTriggered = false;

    void Start()
    {
        Instance = this;
        fruitRequired = (int)Mathf.Floor(20 * Mathf.Log(3 - 1.5f));
        StartCoroutine(DailyCheck());
        StartCoroutine(Check());
    }

    void UpdatePlantCount()
    {
        plantCount.Clear();
        foreach (var k in tilemap.tiles.Keys)
        {
            var v = tilemap.GetTile(k.Item1, k.Item2);
            if (v == null) continue;
            if (v.Plant == null) continue;
            int type = v.Plant.GetComponent<Plant>().typeId;
            plantCount[type] = plantCount.GetValueOrDefault(type, 0) + 1;
        }
    }

    int GetFruitCount()
    {
        return ui.infobar.GetComponent<Infobar>().fruit_cnt;
    }

    IEnumerator DailyCheck()
    {
        while (true)
        {
            yield return new WaitForSeconds(120);
            days += 1;
            var X = Mathf.Floor(20 * Mathf.Log(3 * days - 1.5f));
            if (GetFruitCount() >= X)
            {
                ui.Popup("一天结束了…", "旅行令龙饥饿，它需要果实。", string.Format("交付 {0} 果实", X));
                while (!ui.EventAllClear()) yield return new WaitForEndOfFrame();
                ui.infobar.GetComponent<Infobar>().fruit_cnt -= (int)X;

                if (Random.value <= 0.1)
                {
                    ui.Popup("一般通过小鸟", "咦？有什么东西从天而降。", "收下一包种子");
                    while (!ui.EventAllClear()) yield return new WaitForEndOfFrame();
                    for (int i = 1; i <= 11; i++)
                        sidebar.AddSeedNumber(i, 10);
                }
            }
            else
            {
                ui.Popup("果实不足", "饥肠辘辘的龙被迫暂停了它的旅途……", "返回标题页");
                while (!ui.EventAllClear()) yield return new WaitForEndOfFrame();
                Application.Quit();
                yield break;
            }
            fruitRequired = (int)Mathf.Floor(20 * Mathf.Log(3 * (days + 1) - 1.5f));
        }
    }

    IEnumerator Check()
    {
        while (true)
        {
            UpdatePlantCount();
            Debug.Log(plantCount);
            if (!downedEvent1)
            {
                ui.Popup("植物是这样种的！（1/2）", "打开肥力植物背包，试着种一棵肥肥豆吧！单击选中植物，Q/E键旋转，再次单击确认种植。", "收下 10 肥肥豆的种子");
                while (!ui.EventAllClear()) yield return new WaitForEndOfFrame();
                sidebar.AddSeedNumber(1, 10);
                downedEvent1 = true;
            }
            if (!downedEvent2)
            {
                ui.Popup("植物是这样种的！（2/2）", "打开结果植物背包，试着种一棵龙掌·α吧！请注意它对肥力的要求。", "收下 10 龙掌·α的种子");
                while (!ui.EventAllClear()) yield return new WaitForEndOfFrame();
                sidebar.AddSeedNumber(6, 10);
                downedEvent2 = true;
            }
            if (!downedEvent3 && plantCount.GetValueOrDefault(1, 0) >= 1 && plantCount.GetValueOrDefault(6, 0) >= 1)
            {
                ui.Popup("已经是一位成熟的种植者了！", "花朵会吸引动物来访，动物也许会带来什么……", "收下 3  绮莲的种子");
                while (!ui.EventAllClear()) yield return new WaitForEndOfFrame();
                sidebar.AddSeedNumber(9, 10);
                downedEvent3 = true;
            }

            if (!downedEventC1 && plantCount.GetValueOrDefault(9, 0) >= 1)
            {
                ui.Popup("绮莲盛开…", "做得好！被花朵吸引的飞虫带来了新的种子。", "收下一包种子！");
                while (!ui.EventAllClear()) yield return new WaitForEndOfFrame();
                sidebar.AddSeedNumber(10, 10);
                sidebar.AddSeedNumber(2, 20);
                sidebar.AddSeedNumber(3, 20);
                sidebar.AddSeedNumber(7, 20);
                sidebar.AddSeedNumber(8, 20);
                downedEventC1 = true;
            }

            if (!downedEventC2 && plantCount.GetValueOrDefault(10, 0) >= 1)
            {
                ui.Popup("擎羽盛开…", "了不起！被花朵吸引的飞鸟带来了新的种子。", "收下一包种子！");
                while (!ui.EventAllClear()) yield return new WaitForEndOfFrame();
                sidebar.AddSeedNumber(11, 10);
                sidebar.AddSeedNumber(4, 20);
                sidebar.AddSeedNumber(5, 20);
                downedEventC2 = true;
            }

            if (!downedEventC3 && plantCount.GetValueOrDefault(11, 0) >= 1)
            {
                ui.Popup("▇▇盛开…", "也许，连龙也没有想到▇▇能够再现于世。", "……");
                while (!ui.EventAllClear()) yield return new WaitForEndOfFrame();
                downedEventC3 = true;
            }

            if (!downedEventLoop && loopTriggered)
            {
                ui.Popup("…植物不是这样种的！", "幻想世界也要讲基本法。不可以把沃土兰这样的植物连成环哦！", "知道了");
                while (!ui.EventAllClear()) yield return new WaitForEndOfFrame();
                downedEventLoop = true;
            }

            yield return new WaitForEndOfFrame();
        }
    }

    // Update is called once per frame
    void Update()
    {
    }
}
