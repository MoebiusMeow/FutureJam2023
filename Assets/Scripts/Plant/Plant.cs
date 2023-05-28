using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Plant : MonoBehaviour
{
    [Header("肥力需求")]
    public List<Vector3Int> fertilityAcquire = new List<Vector3Int>();

    [Header("数值增肥效果")]
    public List<Vector3Int> fertilityEffect = new List<Vector3Int>();

    [Header("X增肥效果")]
    public bool useX = false;
    public Vector2Int XPos= new Vector2Int(0,0);
    public List<Vector3Int> fertilityXEffect = new List<Vector3Int>();

    [Header("结果")]
    public float TimeToBearFruit = 0;
    public float timeGrown = 0;

    [Header("枯萎")]
    public float maxheath = 3;
    public float health = 0;

    public int rotateCnt = 0;

    // Start is called before the first frame update
    void Start()
    {
        RestoreHealth();
    }

    public bool Grow(float val = 1)
    {// return true if bear fruit
        if (timeGrown < TimeToBearFruit)
            timeGrown += val;
        return timeGrown >= TimeToBearFruit;
    }

    public bool LooseHelth(float val =1) // return true if die
    {
        health -= val;
        if(health <= 0)
        {
            health = 0;
            return true;
        }
        return false;
    }

    public void RestoreHealth()
    {
        health = maxheath;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
