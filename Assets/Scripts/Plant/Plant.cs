using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Plant : MonoBehaviour
{
    [Header("��������")]
    public List<Vector3Int> fertilityAcquire = new List<Vector3Int>();

    [Header("��ֵ����Ч��")]
    public List<Vector3Int> fertilityEffect = new List<Vector3Int>();

    [Header("X����Ч��")]
    public Vector2Int XPos= new Vector2Int(0,0);
    public List<Vector3Int> fertilityXEffect = new List<Vector3Int>();

    [Header("���")]
    public float tickToBearFruit = 0;
    public float tickgrown = 0;

    public int rotateCnt = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void grow() { 
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
