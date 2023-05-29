using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class RandomHexDir : MonoBehaviour
{
    void Start()
    {
        transform.localRotation *= Quaternion.Euler(0, Random.Range(0, 6) * 60, 0);
    }

    void Update()
    {
    }
}
