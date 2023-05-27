using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Mathematics;
using UnityEngine;

public class HexTile : MonoBehaviour
{
    [Header("数据")]
    public int fertility = 0;
    public int coordQ = 0;
    public int coordR = 0;

    public int detFertility = 0; // 植物造成的肥力影响
    public int tempFertility = 0;// 预览种植效果时的临时影响
    
    [Header("数值显示")]
    public GameObject fertilityDisplay = null;
    public GameObject increaseDisplay = null;

    [Header("杂项")]
    public bool destroying = false;
    public MeshRenderer innerRenderer;
    public HexTilemap tilemap;

    [Header("植物")]
    public GameObject Plant = null;

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

    public bool CanPlant(Plant plant, int rotationIndex)
    {
        if (Plant != null) return false;
        foreach (var acquire in Plant.GetComponent<Plant>().fertilityAcquire)
        {
            int x, y;
            (x, y) = HexTilemap.RotateCoord(acquire.x, acquire.y, rotationIndex);
            int newq = coordQ + x, newr = coordR + y;
            var tile_acquire = tilemap.tiles[(newq, newr)].GetComponent<HexTile>();
            if (tile_acquire.fertility < acquire.z) return false;
        }
        return true;
    }

    public bool isEmpty()
    {
        return Plant== null;
    }

    public void AddPlantToTile(GameObject plant, int rotationIndex)
    {
        Plant = plant;
        foreach (var effect in Plant.GetComponent<Plant>().fertilityEffect)
        {
            int x, y;
            (x, y) = HexTilemap.RotateCoord(effect.x, effect.y, rotationIndex);
            int newq = coordQ + x, newr = coordR + y;
            var tile_fertilize = tilemap.tiles[(newq, newr)].GetComponent<HexTile>();
            tile_fertilize.detFertility += effect.z;
        }
        Plant.GetComponent<Plant>().rotateCnt = rotationIndex;
    }

    public void RemovePlantFromTile()
    {
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
        fertilityDisplay.GetComponent<TextMeshPro>().SetText(string.Format("{0:d}", fertility + detFertility + tempFertility));
        var textmesh = increaseDisplay.GetComponent<TextMeshPro>();
        if (tempFertility != 0)
        {
            if (tempFertility > 0)
            {
                textmesh.color= Color.green;
                textmesh.SetText(string.Format("(+{0:d})", tempFertility));
            }
            else
            {
                textmesh.color= Color.red;
                textmesh.SetText(string.Format("({0:d})", tempFertility));
            }
            textmesh.enabled = true;
        }
        else
        {
            textmesh.enabled = false;
        }
    }
}
