using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Xsl;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class HexTilemap : MonoBehaviour
{
    [Header("棋盘地图半径（单位为格）")]
    [Range(-1.0f, 10.0f)]
    public int MapSize = 1;

    [Header("棋盘排布间隔")]
    public float CellSize = 1.0f;

    [Header("是否依据棋盘尺寸自动生成、移动、删除图格")]
    public bool AutoFillTile = false;
    public GameObject TilePrefab = null;

    [Header("当前选择的要种植的植物")]
    public GameObject PlantPrefab = null;
    private GameObject CurrentPlant = null;

    private int rotateCnt = 0;

    /// 六边形的长轴为z轴
    static public readonly Vector3 UnitQ = new Vector3(Mathf.Sqrt(3) * 0.5f, 0, 0.5f);
    static public readonly Vector3 UnitR = new Vector3(0, 0, -1);
    static public readonly Vector3 UnitS = new Vector3(-Mathf.Sqrt(3) * 0.5f, 0, 0.5f);
    static public Vector3 CoordToPosition(int q, int r, float cellSize) => (q * UnitQ + r * UnitR - (q + r) * UnitS) * cellSize;
    static public int CoordDistance(int q1, int r1, int q2 = 0, int r2 = 0) => (math.abs(q1 - q2) + math.abs(q1 + r1 - q2 - r2) + math.abs(r1 - r2)) / 2;
    public Vector3 CoordToPosition(int q, int r) => CoordToPosition(q, r, CellSize);

    static public (int,int) RotateCoord(int q, int r) => (-r, q + r);
    static public (int,int) RotateCoord(int q, int r, int cnt)
    {
        for (int i=0; i<cnt; i++)
        {
            (q, r) = RotateCoord(q, r);
        }
        return (q,r);
    }
    /// <summary>
    /// 包含的图格的GameObject，其应该挂着一个HexTile行为
    /// </summary>
    public Dictionary<(int, int), GameObject> tiles = new();
    public HexTile GetTile(int q, int r)
    {
        if (!tiles.ContainsKey((q, r))) return null;
        var tile = tiles[(q, r)].GetComponent<HexTile>();
        if (tile == null || tile.destroying) return null;
        return tile;
    }

    public void RecomputeAllFertility()
    {
        // clear everthing
        foreach (var tile in tiles.Values)
            tile.GetComponent<HexTile>().ClearAll();
        // compute basic fertility
        foreach (var tile in tiles.Values)
            tile.GetComponent<HexTile>().UpdateFertility();
        // compute X fertility

    }

    void AddRotate(int det) // 1 or -1
    {
        rotateCnt += det;
        if (rotateCnt >= 6) rotateCnt = 0;
        if (rotateCnt < 0) rotateCnt = 5;
    }

    /// <summary>
    /// Gizmos 就是在编辑器内显示的那个线框，通过写这两个函数可以决定绘制什么东西
    /// </summary>
    void OnDrawGizmos()
    {
        QuickDrawGizmo(Vector3.zero, Vector3.right * CellSize * Mathf.Sqrt(3) * MapSize);
        QuickDrawGizmo(Vector3.zero, Vector3.right * CellSize * Mathf.Sqrt(3) * (MapSize + 2 / 3f));
    }

    void OnDrawGizmosSelected()
    {
        for (int q = -MapSize; q <= MapSize; q++)
            for (int r = -math.min(0, q) - MapSize; r <= -math.max(0, q) + MapSize; r++)
                QuickDrawGizmo(CoordToPosition(q, r), Vector3.forward * CellSize);
    }

    private void QuickDrawGizmo(Vector3 center, Vector3 r, Vector3? up = null)
    {
        var orinMatrix = Gizmos.matrix;
        Gizmos.matrix = transform.localToWorldMatrix;
        for (var i = 0; i < 6; i++)
        {
            Vector3 next = Quaternion.AngleAxis(60f, up ?? Vector3.up) * r;
            Gizmos.DrawLine(center + r, center + next);
            r = next;
        }
        Gizmos.matrix = orinMatrix;
    }

    void DoAutoFillTile()
    {
        tiles.Keys.Where((x) => CoordDistance(x.Item1, x.Item2) > MapSize).ToList().ForEach((x) => {
            tiles[x].GetComponent<HexTile>().destroying = true;
            Destroy(tiles[x].gameObject, 3);
            tiles.Remove(x);
        });
        for (int q = -MapSize; q <= MapSize; q++)
            for (int r = -math.min(0, q) - MapSize; r <= -math.max(0, q) + MapSize; r++)
            {
                if (!tiles.ContainsKey((q, r)))
                    tiles[(q, r)] = Instantiate(TilePrefab, transform);
                var tileObject = tiles[(q, r)];
                tileObject.transform.localPosition = CoordToPosition(q, r);
                if (tileObject.GetComponent<HexTile>() != null)
                {
                    tileObject.GetComponent<HexTile>().tilemap = this;
                    tileObject.GetComponent<HexTile>().coordQ = q;
                    tileObject.GetComponent<HexTile>().coordR = r;
                }
            }
    }

    void Start()
    {
        CurrentPlant = Instantiate(PlantPrefab, transform);
        CurrentPlant.transform.Translate(transform.position + new Vector3(10000,10000,1000));
    }

    


    void Update()
    {
        if (AutoFillTile && !Input.GetMouseButton(0))
            DoAutoFillTile();

        if (Input.GetKeyDown(KeyCode.Q)){ // rotate plant
            AddRotate(-1);
        }
        if (Input.GetKeyDown(KeyCode.E)) // rotate plant
        {
            AddRotate(1);
        }

        foreach (var tile in tiles.Values)
        {
            tile.GetComponent<HexTile>().tempFertility= 0;// clear temp values
        }

        foreach (var hit in Physics.RaycastAll(Camera.main.ScreenPointToRay(Input.mousePosition)))
        {
            var target = hit.collider.gameObject;
            if (target.GetComponent<HexTile>() != null)
            {
                var tile = target.GetComponent<HexTile>();
                int q, r;
                (q, r) = (tile.coordQ, tile.coordR);
                Debug.Log((q, r));
                if (CurrentPlant != null)
                {
                    var plant = CurrentPlant.GetComponent<Plant>();
                    if (Input.GetMouseButtonDown(0)) //左键点击
                    { 
                        if (Input.GetKey(KeyCode.R))// 铲除植物
                        {
                            tile.RemovePlantFromTile();
                        }
                        else if (tile.CanPlant(plant, rotateCnt))//种植植物
                        {
                            tile.AddPlantToTile(Instantiate(PlantPrefab, tile.transform), rotateCnt);
                        }
                    }
                    else
                    { //预览种植
                        if (tile.CanPlant(plant, rotateCnt))
                        {
                            foreach (var effect in plant.fertilityEffect)
                            {
                                int x, y;
                                (x, y) = RotateCoord(effect.x, effect.y, rotateCnt);
                                int newq = q + x, newr = r + y;
                                if (tiles.ContainsKey((newq, newr)))
                                {
                                    var tile_fertilize = tiles[(newq, newr)].GetComponent<HexTile>();
                                    tile_fertilize.tempFertility = effect.z;
                                }
                            }
                        }
                    }
                        
                }

            }
        }
    }
}
