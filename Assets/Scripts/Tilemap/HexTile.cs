using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class HexTile : MonoBehaviour
{
    [Serializable]
    public struct PlantPrefabEntry
    {
        public string Name;
        public GameObject Prefab;
    };

    [Header("种类和Prefab的对应关系，同种类可以多次添加作为随机变体")]
    public List<PlantPrefabEntry> PlantPrefabs = new();

    [Header("数据")]
    public bool unlocked = true;
    public float fertility = 0;
    public int coordQ = 0;
    public int coordR = 0;
    public string plantType = "";
    private string plantedType = "";

    [Header("显示")]
    public float heightPerFertility = 0.01f;
    public GameObject container;
    public GameObject soil;
    public GameObject sparkling;

    [Header("特效预设")]
    public GameObject smokeParticle;
    public GameObject sparkParticle;

    public float detFertility = 0; // 植物造成的肥力影响
    public float tempFertility = 0;// 预览种植效果时的临时影响
    
    [Header("数值显示")]
    public GameObject fertilityDisplay = null;
    public GameObject increaseDisplay = null;

    [Header("高亮框")]
    public int highlight = 0;
    public Color hightlightColorFertilizer = Color.green;
    public Color hightlightColorXPos = Color.blue;
    public Color hightlightColorDie = Color.red;
    public Color hightlightColorWarning = Color.yellow;
    public Color hightlightAcquire= Color.white;
    public Color endcolor = Color.green;
    public GameObject highlightFrame = null;

    [Header("杂项")]
    public List<AudioSource> SoundEffects = new();
    public List<MeshRenderer> innerRenderer = new();
    public List<MeshRenderer> stoneRenderer = new();
    public HexTilemap tilemap;

    [Header("植物数据结构")]
    public GameObject Plant = null;

    [Header("判环用，别动")]
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
        return CheckPlantCanGrow(plant, rotationIndex);
    }

    public void ClearAll()
    {
        detFertility = tempFertility = search_count = search_tag = 0;
    }

    public void SetPlantType(string plant)
    {
        plantType = plant;
        Debug.LogFormat("SetPlantType:{0:s}", plant);
    }

    public void RemovePlantModel()
    {
        if (container == null)
            return;
        for (int i = 0; i < container.transform.childCount; i++)
        {
            var child = container.transform.GetChild(i).gameObject;
            if (child.GetComponent<MeshRenderer>() != null && child.GetComponent<DissolveAndDestroy>() == null)
                child.AddComponent<DissolveAndDestroy>();
        }
    }

    public void RemoveAllModels()
    {
        var destroyer = gameObject.AddComponent<DissolveAndDestroy>();
        destroyer.meshRenderers.AddRange(innerRenderer);
        destroyer.meshRenderers.AddRange(stoneRenderer);
        RemovePlantModel();
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

    public void AddPlantToTile(GameObject newPlant, int rotationIndex, string plant_name)
    {
        Plant = newPlant;
        var plant = Plant.GetComponent<Plant>();
        plant.rotateCnt = rotationIndex;
        SetPlantType(plant_name);
    }

    public void RemovePlant()
    {
        //Debug.Log("Try Remove Plant");
        if (Plant == null) return;
        //Debug.Log("Destorying Plant");
        Destroy(Plant);
        Plant = null;
        SetPlantType("");
    }

    void Start()
    {
        foreach (var renderer in innerRenderer)
            renderer.material.SetFloat("_Value", 0);
        foreach (var renderer in stoneRenderer)
            renderer.material.SetFloat("_Value", 0);
    }

    void Update()
    {
        if (sparkling != null)
        {
            if (plantType != "C_04")
            {
                sparkling.GetComponentInChildren<ParticleSystem>().Stop();
            }
            else if (!sparkling.GetComponentInChildren<ParticleSystem>().isPlaying)
            {
                sparkling.GetComponentInChildren<ParticleSystem>().Play();
            }
        }

        if (container != null && (plantedType != plantType || plantType == ""))
        {
            RemovePlantModel();
            var result = PlantPrefabs.Where((v) => v.Name == plantType).ToList();
            GameObject particle;
            if (result.Count > 0)
            {
                GameObject prefab = result[Random.Range(0, result.Count)].Prefab;
                var plant = Instantiate(prefab, container.transform);
                plant.transform.RotateAround(plant.transform.position, Vector3.up, Random.Range(0, 360));
                // plant.transform.localRotation *= Quaternion.Euler(0, Random.Range(0, 360), 0);
                var fadein = plant.AddComponent<DissolveAndFadein>();
                fadein.RestartFadein();

                particle = Instantiate(sparkParticle, soil.transform);
                particle.transform.localScale = sparkling.transform.localScale;
                particle.transform.localPosition = sparkling.transform.localPosition;

                particle = Instantiate(smokeParticle, soil.transform);
                particle.transform.localScale = container.transform.localScale;

                SoundEffects[0].Play();

                plantedType = plantType;
            }
            else if (plantType == "" && plantedType != "")
            {
                plantedType = "";
                particle = Instantiate(smokeParticle, soil.transform);
                particle.transform.localScale = container.transform.localScale;

                SoundEffects[1].Play();
            }
        }

        if (container != null)
            container.transform.localPosition = fertility * heightPerFertility * Vector3.up;
        if (soil != null)
            soil.transform.localPosition = fertility * heightPerFertility * Vector3.up;

        foreach (var renderer in innerRenderer)
        {
            float value = renderer.material.GetFloat("_Value");
            if (GetComponent<DissolveAndDestroy>() == null)
                renderer.material.SetFloat("_Value", math.clamp(value, 0, 1) + Time.deltaTime * 0.8f);
            else
                renderer.material.SetFloat("_Value", math.clamp(value, 0, 1) - Time.deltaTime * 0.2f);
        }

        foreach (var renderer in stoneRenderer)
        {
            float value = renderer.material.GetFloat("_Value");
            if (unlocked)
                renderer.material.SetFloat("_Value", math.clamp(value, 0, 1) - Time.deltaTime * 0.4f);
            else
                renderer.material.SetFloat("_Value", math.clamp(value, 0, 1) + Time.deltaTime * 0.8f);
        }

        fertilityDisplay.SetActive(tilemap.CurrentPlant != null);
        fertilityDisplay.GetComponent<TextMeshPro>().SetText(string.Format("{0:d}", (int)(fertility + detFertility + tempFertility)));
        var textmesh = increaseDisplay.GetComponent<TextMeshPro>();
        if (tempFertility != 0)
        {
            if (tempFertility > 0)
            {
                textmesh.color = Color.green;
                textmesh.SetText(string.Format("(+{0:d})", (int)tempFertility));
            }
            else
            {
                textmesh.color = Color.red;
                textmesh.SetText(string.Format("({0:d})", (int)tempFertility));
            }
            textmesh.enabled = true;
        }
        else
        {
            textmesh.enabled = false;
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
                    linerenderer.startColor = hightlightColorFertilizer;
                    linerenderer.endColor = endcolor;
                }
                if (highlight == 2) {
                    linerenderer.startColor = hightlightColorXPos;
                    linerenderer.endColor = endcolor;
                }
                if (highlight == 3)
                {
                    linerenderer.startColor = hightlightColorDie;
                    linerenderer.endColor   = hightlightColorDie;
                }
                if (highlight == 4)
                {
                    linerenderer.startColor = hightlightColorWarning;
                    linerenderer.endColor = hightlightColorWarning;
                }
                if (highlight == 5)
                {
                    linerenderer.startColor = hightlightAcquire;
                    linerenderer.endColor = hightlightAcquire;
                }
            }
        }
    }
}
