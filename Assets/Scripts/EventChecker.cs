using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventChecker : MonoBehaviour
{
    public UI ui;
    public Sidebar sidebar;
    public HexTilemap tilemap;
    bool downedEvent1 = false;
    bool downedEvent2 = false;
    bool downedEvent3 = false;
    Dictionary<int, int> plantCount;

    void Start()
    {
        StartCoroutine(Check());
    }

    void UpdatePlantCount()
    {
        plantCount.Clear();
        foreach (var k in tilemap.tiles.Keys)
        {
            var v = tilemap.GetTile(k.Item1, k.Item2);
            if (v == null) continue;
            // 反正是计一下数
        }
    }

    IEnumerator Check()
    {
        while (true)
        {
            if (!downedEvent1)
            {
                ui.Popup("植物是这样种的！（1/2）", "打开肥力植物背包，试着种一棵肥肥豆吧！单击选中地块，Q/E键旋转，再次单击确认种植。", "收下 10 肥肥豆的种子");
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
            if (!downedEvent3)
            {
                ui.Popup("已经是一位成熟的种植者了！", "花朵会吸引动物来访，动物也许会带来什么……", "收下 3  绮莲的种子");
                while (!ui.EventAllClear()) yield return new WaitForEndOfFrame();
                sidebar.AddSeedNumber(9, 10);
                downedEvent3 = true;
            }
            yield return new WaitForEndOfFrame();
        }
    }

    // Update is called once per frame
    void Update()
    {
    }
}
