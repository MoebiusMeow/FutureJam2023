using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public int fertility = 0;
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

    [Header("杂项")]
    public List<AudioSource> SoundEffects = new();
    public List<MeshRenderer> innerRenderer = new();
    public List<MeshRenderer> stoneRenderer = new();
    public HexTilemap tilemap;

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

    public void SetPlantType(string plant)
    {
        plantType = plant;
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
    }
}
