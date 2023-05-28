using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class HexTile : MonoBehaviour
{
    [Header("数据")]
    public float fertility = 0;
    public int coordQ = 0;
    public int coordR = 0;

    public float detFertility = 0; // 植物造成的肥力影响
    public float tempFertility = 0;// 预览种植效果时的临时影响
    
    [Header("数值显示")]
    public GameObject fertilityDisplay = null;
    public GameObject increaseDisplay = null;

    [Header("高亮框")]
    public int highlight = 0;
    public Color hightlightColor1 = Color.green;
    public Color hightlightColor2 = Color.blue;
    public Color hightlightColor3 = Color.red;
    public Color endcolor = Color.green;
    public GameObject highlightFrame = null;

    [Header("杂项")]
    public bool destroying = false;
    public MeshRenderer innerRenderer;
    public HexTilemap tilemap;

    [Header("植物")]
    public GameObject Plant = null;

    static public int search_count = 0;
    public int search_tag = 0;
    public int goingToDie = 0; // 在删除植物时用于标记植物是否将要死去,0无需删除，1待计算删除影响，2已计算删除影响

    public List<HexTile> GetExistingNeighbors()
    {
        List<HexTile> result = new();
        if (tilemap == null)
            return result;
        for (int q = -1; q <= 1; q++)
            for (int r = -math.min(0, q) - 1; r <= -math.max(0, q) + 1; r++)
                if (tilemap.GetTile(q, r) != null)
                    result.Add(tilemap.GetTile(q, r));
        return result;
    }

    public float GetAllFertility()
    {
        return fertility + detFertility + tempFertility;
    }

    public bool CheckPlantCanGrow(Plant plant, int rotationIndex)
    {
        foreach (var acquire in plant.GetComponent<Plant>().fertilityAcquire)
        {
            int x, y;
            (x, y) = HexTilemap.RotateCoord(acquire.x, acquire.y, rotationIndex);
            int newq = coordQ + x, newr = coordR + y;
            var tile_acquire = tilemap.tiles[(newq, newr)].GetComponent<HexTile>();
            if (tile_acquire.GetAllFertility() < acquire.z) return false;
        }
        return true;
    }

    public bool CanPlant(Plant plant, int rotationIndex)
    {
        if (Plant != null) return false;
        return CheckPlantCanGrow(plant, rotationIndex); ;
    }

    public void ClearAll()
    {
        detFertility = tempFertility = search_count = search_tag = 0;
    }

    public bool isXTile()
    {
        if (Plant == null) return false;
        var plant = Plant.GetComponent<Plant>();
        if (!plant.useX) return false;
        return true;
    }

    public bool isXInput(int q, int r)
    {
        if (!isXTile()) return false;
        var plant = Plant.GetComponent<Plant>();
        int xposx, xposy;
        (xposx, xposy) = HexTilemap.RotateCoord(plant.XPos.x, plant.XPos.y, plant.rotateCnt);
        return q == coordQ + xposx && r == coordR + xposy;
    }
    
    public void clearTempValue()
    {
        tempFertility = search_tag = 0;
        goingToDie = 0;
    }

    public bool isEmpty()
    {
        return Plant== null;
    }

    public void UpdateFertility()
    {
        var plant = Plant.GetComponent<Plant>();
        foreach (var effect in plant.fertilityEffect)
        {
            int x, y;
            (x, y) = HexTilemap.RotateCoord(effect.x, effect.y, plant.rotateCnt);
            int newq = coordQ + x, newr = coordR + y;
            var tile_fertilize = tilemap.GetTile(newq, newr);
            if (tile_fertilize != null)
            {
                tile_fertilize.detFertility += effect.z;
            }
        }
    }

    public void AddPlantToTile(GameObject newPlant, int rotationIndex)
    {
        Plant = newPlant;
        var plant = Plant.GetComponent<Plant>();
        plant.rotateCnt = rotationIndex;
    }

    public void RemovePlant()
    {
        if (Plant == null) return;
        Destroy(Plant);
        Plant = null;
    }

    public void RemovePlantFromTile()
    {
        if (Plant == null) return;
        foreach (var effect in Plant.GetComponent<Plant>().fertilityEffect)
        {
            int x, y;
            (x, y) = HexTilemap.RotateCoord(effect.x, effect.y, Plant.GetComponent<Plant>().rotateCnt);
            int newq = coordQ + x, newr = coordR + y;
            var tile_fertilize = tilemap.tiles[(newq, newr)].GetComponent<HexTile>();
            tile_fertilize.detFertility -= effect.z;
        }
        Destroy(Plant);
        Plant = null;
    }

    void Start()
    {
        if (innerRenderer != null)
        {
            innerRenderer.material.SetFloat("_Value", 0);
        }
    }

    public void updateHighlight(int value)
    {
        if (highlightFrame != null)
        {
            highlight = value;
            var linerenderer = highlightFrame.GetComponent<LineRenderer>();
            if (linerenderer != null)
            {
                linerenderer.enabled = highlight > 0;
                if (highlight == 1)
                {
                    linerenderer.startColor = hightlightColor1;
                    linerenderer.endColor = endcolor;
                }
                if (highlight == 2) {
                    linerenderer.startColor = hightlightColor2;
                    linerenderer.endColor = endcolor;
                }
                if (highlight == 3)
                {
                    linerenderer.startColor = hightlightColor3;
                    linerenderer.endColor   = hightlightColor3;
                }
            }
        }
    }

    void Update()
    {
        if (innerRenderer != null)
        {
            float value = innerRenderer.material.GetFloat("_Value");
            if (destroying)
                innerRenderer.material.SetFloat("_Value", math.clamp(value, 0, 1) - Time.deltaTime * 0.4f);
            else
                innerRenderer.material.SetFloat("_Value", math.clamp(value, 0, 1) + Time.deltaTime * 0.8f);
        }
        fertilityDisplay.GetComponent<TextMeshPro>().SetText(string.Format("{0:d}", (int)(fertility + detFertility + tempFertility)));
        var textmesh = increaseDisplay.GetComponent<TextMeshPro>();
        if (tempFertility != 0)
        {
            if (tempFertility > 0)
            {
                textmesh.color= Color.green;
                textmesh.SetText(string.Format("(+{0:d})", (int)tempFertility));
            }
            else
            {
                textmesh.color= Color.red;
                textmesh.SetText(string.Format("({0:d})", (int)tempFertility));
            }
            textmesh.enabled = true;
        }
        else
        {
            textmesh.enabled = false;
        }
        
    }
}
