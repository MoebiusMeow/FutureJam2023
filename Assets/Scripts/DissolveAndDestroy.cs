using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class DissolveAndDestroy : MonoBehaviour
{
    public float destroyTime = 3.0f;
    public float fadeSpeed = 0.8f;
    public List<MeshRenderer> meshRenderers = new List<MeshRenderer>();

    void Start()
    {
        Destroy(gameObject, destroyTime);
        if (meshRenderers.Count == 0)
            meshRenderers.Add(GetComponent<MeshRenderer>());
    }

    void Update()
    {
        foreach (var meshRenderer in meshRenderers)
        {
            foreach (var material in meshRenderer.materials)
            {
                float value = material.GetFloat("_Value");
                material.SetFloat("_Value", math.clamp(value, 0, 1) - Time.deltaTime * fadeSpeed);
            }
        }
    }
}
