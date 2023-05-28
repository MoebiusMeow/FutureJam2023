using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class DissolveAndFadein : MonoBehaviour
{
    public float fadeinSpeed = 1.8f;
    public List<MeshRenderer> meshRenderers = new List<MeshRenderer>();

    public void RestartFadein()
    {
        if (meshRenderers.Count == 0)
            meshRenderers.Add(GetComponent<MeshRenderer>());
        foreach (var meshRenderer in meshRenderers)
        {
            foreach (var material in meshRenderer.materials)
            {
                float value = material.GetFloat("_Value");
                material.SetFloat("_Value", 0);
            }
        }
    }

    void Start()
    {
        if (meshRenderers.Count == 0)
            meshRenderers.Add(GetComponent<MeshRenderer>());
    }

    void Update()
    {
        if (GetComponent<DissolveAndDestroy>() != null)
            return;
        foreach (var meshRenderer in meshRenderers)
        {
            foreach (var material in meshRenderer.materials)
            {
                float value = material.GetFloat("_Value");
                material.SetFloat("_Value", math.clamp(value, 0, 1) + Time.deltaTime * fadeinSpeed);
            }
        }
    }
}
