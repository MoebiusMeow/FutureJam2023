using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Xsl;
using Unity.Mathematics;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.Rendering.Universal;
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
    public List<GameObject> PlantPrefab = new List<GameObject>();
    private GameObject CurrentPlant = null;
    private int CurrentPlantIndex = -1;

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

    void ClearAllTempValue()
    {
        foreach(var Tile in tiles.Values)
        {
            Tile.GetComponent<HexTile>().clearTempValue();
        }
    }

    void ApplyAllTempFertility()
    {
        foreach (var Tile in tiles.Values)
        {
            var tile = Tile.GetComponent<HexTile>();
            tile.detFertility += tile.tempFertility;
            tile.tempFertility = 0;
        }
    }

    bool RecursiveUpdateFertility(int q, int r, float detf, int search_id)
    {
        var tile = GetTile(q, r);
        if (tile == null) return true;
        if (search_id>0 && search_id == tile.search_tag)
        {
            Debug.Log((q, r));
            return false;
        }
            
        tile.tempFertility += detf;
        tile.search_tag = search_id;
        foreach (var Tile in tiles.Values)
        {
            var t_tile = Tile.GetComponent<HexTile>();
            if (t_tile.isXInput(q, r))
            {
                var plant = t_tile.Plant.GetComponent<Plant>();
                int x, y, tq = t_tile.coordQ, tr = t_tile.coordR;
                foreach (var xtoken in plant.fertilityXEffect)
                {
                    (x, y) = RotateCoord(xtoken.x, xtoken.y, plant.rotateCnt);
                    (x, y) = (tq + x, tr + y);
                    if(!RecursiveUpdateFertility(x, y, detf / xtoken.z, search_id))
                        return false;
                }
            }
        }
        tile.search_tag = -1;
        return true;
    }

    bool AddPlantTemporary(int q, int r, Plant plant, int rotationIndex)
    {
        ClearAllTempValue();
        foreach (var normaltoken in plant.fertilityEffect)
        {
            int x, y;
            (x, y) = RotateCoord(normaltoken.x, normaltoken.y, rotationIndex);
            int newq = q + x, newr = r + y;
            if (!RecursiveUpdateFertility(newq, newr, normaltoken.z, 0))
            {
                ClearAllTempValue();
                return false;
            }
                
        }
        if (plant.useX)
        {
            int search_id = 1;
            int x, y;
            (x, y) = RotateCoord(plant.XPos.x, plant.XPos.y, rotationIndex);
            int xq = x + q, xr = y + r;
            float xval = GetTile(xq, xr).GetAllFertility();
            GetTile(xq, xr).search_tag = search_id;
            foreach (var xtoken in plant.fertilityXEffect)
            {
                (x, y) = RotateCoord(xtoken.x, xtoken.y, rotationIndex);
                int newq = q + x, newr = r + y;
                if(!RecursiveUpdateFertility(newq, newr, xval / xtoken.z, search_id))
                {
                    ClearAllTempValue();
                    return false;
                }
            }
        }
        return true;
    }

    void RemovePlant(int q, int r)
    {
        var tile = GetTile(q, r);
        if (tile == null || tile.Plant == null) return;
        var plant  = tile.Plant.GetComponent<Plant>();
        if (plant == null) return;
        if (plant.useX)
        {
            int search_id = 1;
            int x, y;
            (x, y) = RotateCoord(plant.XPos.x, plant.XPos.y, plant.rotateCnt);
            int xq = x + q, xr = y + r;
            float xval = GetTile(xq, xr).GetAllFertility();
            GetTile(xq, xr).search_tag = search_id;
            foreach (var xtoken in plant.fertilityXEffect)
            {
                (x, y) = RotateCoord(xtoken.x, xtoken.y, plant.rotateCnt);
                int newq = q + x, newr = r + y;
                RecursiveUpdateFertility(newq, newr, -xval / xtoken.z, 1);
            }
        }
        foreach (var normaltoken in plant.fertilityEffect)
        {
            int x, y;
            (x, y) = RotateCoord(normaltoken.x, normaltoken.y, plant.rotateCnt);
            (x, y) = (q + x, r + y);
            RecursiveUpdateFertility(x, y, -normaltoken.z, 1);
        }
        tile.goingToDie = 2;
    }

    bool TagAllDeadPlant()
    {
        bool fg = false;
        foreach (var Tile in tiles.Values)
        {
            var tile = Tile.GetComponent<HexTile>();
            if (tile.Plant == null) continue;
            var plant = tile.Plant.GetComponent<Plant>();
            if (tile.goingToDie==0 && !tile.CheckPlantCanGrow(plant, plant.rotateCnt))
            {
                tile.goingToDie= 1;
                fg = true;
            }
        }
        return fg;
    }

    void ApplyAllKillPlant()
    {
        foreach (var Tile in tiles.Values)
        {
            var tile = Tile.GetComponent<HexTile>();
            if (tile.goingToDie > 0) tile.RemovePlant();
            tile.goingToDie = 0;
        }
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

    public void SwitchPlant(int plantIndex)
    {
        if (CurrentPlant != null)
        {
            Destroy(CurrentPlant);
            CurrentPlant = null;
        }
        if(plantIndex >=0 &&plantIndex < PlantPrefab.Count())
        {
            CurrentPlant = Instantiate(PlantPrefab[plantIndex], transform);
            CurrentPlant.transform.Translate(transform.position + new Vector3(10000, 10000, 1000));
            CurrentPlantIndex = plantIndex;
            Debug.LogFormat("Current Plant:{0:d}", plantIndex);
        }
    }

    void Start()
    {
        SwitchPlant(-1);
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
            tile.GetComponent<HexTile>().updateHighlight(0);
        }

        foreach (var hit in Physics.RaycastAll(Camera.main.ScreenPointToRay(Input.mousePosition)))
        {
            var target = hit.collider.gameObject;
            if (target.GetComponent<HexTile>() != null)
            {
                var tile = target.GetComponent<HexTile>();
                int q, r;
                (q, r) = (tile.coordQ, tile.coordR);
                // Debug.Log((q, r));

                if (Input.GetKey(KeyCode.R))// 铲除植物
                {
                    ClearAllTempValue();
                    if (Input.GetMouseButtonDown(0)) // 确定铲除
                    {
                        RemovePlant(q, r);
                        ApplyAllTempFertility();
                        ApplyAllKillPlant();
                    }
                    else
                    {
                        RemovePlant(q, r);
                        while (TagAllDeadPlant())
                        {
                            foreach (var Tile in tiles.Values)
                            {
                                var k_tile = Tile.GetComponent<HexTile>();
                                if (k_tile.goingToDie == 1)
                                {
                                    RemovePlant(k_tile.coordQ, k_tile.coordR);
                                }
                            }
                        }
                        foreach (var Tile in tiles.Values)
                        {
                            var k_tile = Tile.GetComponent<HexTile>();
                            if (k_tile.goingToDie > 0)
                                k_tile.updateHighlight(3);
                            k_tile.goingToDie = 0;
                        }
                    }
                }
                else if (CurrentPlant != null && CurrentPlantIndex >= 0 &&CurrentPlantIndex < PlantPrefab.Count())
                {
                    var plant = CurrentPlant.GetComponent<Plant>();
                    bool fg = tile.CanPlant(plant, rotateCnt);
                    Debug.LogFormat("{0:d},{1:d}:{2:s}", q, r, fg.ToString());
                    if (fg)
                    {
                        fg &= AddPlantTemporary(q, r, plant, rotateCnt);
                    }
                    if (Input.GetMouseButtonDown(0)) //左键点击
                    { 
                        if (fg)//种植植物
                        {
                            tile.AddPlantToTile(Instantiate(PlantPrefab[CurrentPlantIndex], tile.transform), rotateCnt);
                            ApplyAllTempFertility();
                        }
                    }
                    else
                    { //预览种植相关
                        if (fg)
                        { //高亮加成格子
                            int x, y;
                            HexTile t_tile;
                            foreach (var token in plant.fertilityEffect)
                            {
                                (x, y) = RotateCoord(token.x, token.y, rotateCnt);
                                (x, y) = (q + x, r + y);
                                t_tile = GetTile(x, y);
                                if (t_tile) t_tile.updateHighlight(1);
                            }
                            foreach (var token in plant.fertilityXEffect)
                            {
                                (x, y) = RotateCoord(token.x, token.y, rotateCnt);
                                (x, y) = (q + x, r + y);
                                t_tile = GetTile(x, y);
                                if (t_tile) t_tile.updateHighlight(1);
                            }
                            if (plant.useX)
                            {
                                (x, y) = RotateCoord(plant.XPos.x, plant.XPos.y, rotateCnt);
                                (x, y) = (q + x, r + y);
                                t_tile = GetTile(x, y);
                                if (t_tile) t_tile.updateHighlight(2);
                            }
                        }
                        else//不能种
                        {
                            tile.updateHighlight(3);
                        }
                    }
                        
                }

            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SwitchPlant(0);
            Debug.Log("swicth 0");
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SwitchPlant(1);
            Debug.Log("swicth 1");
        }
    }
}
