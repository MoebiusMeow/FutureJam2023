using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class CountdownAndDestroy : MonoBehaviour
{
    public float destroyTime = 3.0f;

    void Start()
    {
        Destroy(gameObject, destroyTime);
    }

    void Update()
    {
    }
}
