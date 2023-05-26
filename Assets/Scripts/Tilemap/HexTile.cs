using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class HexTile : MonoBehaviour
{
    [Header("Êý¾Ý")]
    public int fertility = 0;
    public int coordQ = 0;
    public int coordR = 0;

    [Header("ÔÓÏî")]
    public bool destroying = false;
    public MeshRenderer innerRenderer;
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
    }
}
