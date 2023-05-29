using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Xsl;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

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
    public GameObject CurrentPlant = null;
    private int CurrentPlantIndex = -1; // -2 delete -1 none >=0 plantindex
    public bool NeedDisplayFertility => CurrentPlantIndex != -1;

    private int rotateCnt = 0;

    /// 六边形的长轴为z轴
    static public readonly Vector3 UnitQ = new Vector3(Mathf.Sqrt(3) * 0.5f, 0, 0.5f);
    static public readonly Vector3 UnitR = new Vector3(0, 0, -1);
    static public readonly Vector3 UnitS = new Vector3(-Mathf.Sqrt(3) * 0.5f, 0, 0.5f);
    static public Vector3 CoordToPosition(int q, int r, float cellSize) => (q * UnitQ + r * UnitR - (q + r) * UnitS) * cellSize;
    static public int CoordDistance(int q1, int r1, int q2 = 0, int r2 = 0) => (math.abs(q1 - q2) + math.abs(q1 + r1 - q2 - r2) + math.abs(r1 - r2)) / 2;
    public Vector3 CoordToPosition(int q, int r) => CoordToPosition(q, r, CellSize);

    private List<string> PlantPrefabNames = new List<string>();
    private string PlantPrefabName = null;

    public GameObject SideBar = null;
    public GameObject InfoBar = null;

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
        if (tile == null || tile.GetComponent<DissolveAndDestroy>() != null) return null;
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
            // Debug.Log((q, r));
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
                GetTile(newq, newr).requiredFertility = -1;
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
        if (tile == null || tile.Plant == null)
        {
            //Debug.Log("Null plant or tile");
            return;
        }
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
            if (tile.goingToDie > 0)
                tile.RemovePlant();
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
            var tile = tiles[x].GetComponent<HexTile>();
            tile.RemoveAllModels();
            tiles.Remove(x);
        });
        for (int q = -MapSize; q <= MapSize; q++)
            for (int r = -math.min(0, q) - MapSize; r <= -math.max(0, q) + MapSize; r++)
            {
                if (!tiles.ContainsKey((q, r)))
                {
                    tiles[(q, r)] = Instantiate(TilePrefab, transform);
                    tiles[(q, r)].GetComponent<HexTile>().unlocked =
                        CoordDistance(q, r) <= 5 ? true :
                        (Random.value >= CoordDistance(q, r) * 0.1f);
                }
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
        if (plantIndex < 0)
        {
            CurrentPlantIndex = plantIndex;
            return;
        }
        if (plantIndex >= 0 && plantIndex < PlantPrefab.Count() && PlantPrefab[plantIndex] != null)
        {
            CurrentPlant = Instantiate(PlantPrefab[plantIndex], transform);
            CurrentPlant.transform.Translate(transform.position + new Vector3(10000, 10000, 1000));
            CurrentPlantIndex = plantIndex;
            PlantPrefabName = PlantPrefabNames[plantIndex];
            Debug.LogFormat("PlantPrefabName:{0:s}", PlantPrefabName);
            //Debug.LogFormat("Current Plant:{0:d}", plantIndex);
        }
    }

    void Start()
    {
        PlantPrefabNames.Add("A_01");
        PlantPrefabNames.Add("A_02");
        PlantPrefabNames.Add("A_03");
        PlantPrefabNames.Add("A_04");
        PlantPrefabNames.Add("A_05");
        PlantPrefabNames.Add("B_01");
        PlantPrefabNames.Add("B_02");
        PlantPrefabNames.Add("B_03");
        PlantPrefabNames.Add("C_01");
        PlantPrefabNames.Add("C_02");
        PlantPrefabNames.Add("C_03");

        SwitchPlant(-1);
    }

    void Update()
    {
        if (AutoFillTile && !Input.GetKey(KeyCode.D))
            DoAutoFillTile();
        /*
        foreach (var hit in Physics.RaycastAll(Camera.main.ScreenPointToRay(Input.mousePosition)))
        {
            var target = hit.collider.gameObject;
            if (target.GetComponent<HexTile>() != null)
            {
                var tile = target.GetComponent<HexTile>();
                if (tile.GetComponent<DissolveAndDestroy>() != null)
                    continue;
                // 测试添加植物
                if (Input.GetMouseButton(0))
                    tile.SetPlantType("C_01");
                if (Input.GetKey(KeyCode.Alpha1)) tile.SetPlantType("A_01");
                if (Input.GetKey(KeyCode.Alpha2)) tile.SetPlantType("A_02");
                if (Input.GetKey(KeyCode.Alpha3)) tile.SetPlantType("A_03");
                if (Input.GetKey(KeyCode.Alpha4)) tile.SetPlantType("A_04");
                if (Input.GetKey(KeyCode.Alpha5)) tile.SetPlantType("");
                if (Input.GetKey(KeyCode.Alpha6)) tile.SetPlantType("B_01");
                if (Input.GetKey(KeyCode.Alpha7)) tile.SetPlantType("B_02");
                if (Input.GetKey(KeyCode.Alpha8)) tile.SetPlantType("B_03");
                if (Input.GetKey(KeyCode.Alpha9)) tile.SetPlantType("C_01");
                if (Input.GetKey(KeyCode.Alpha0)) tile.SetPlantType("C_02");
                if (Input.GetKey(KeyCode.Minus)) tile.SetPlantType("C_03");
                if (Input.GetKey(KeyCode.Equals)) tile.SetPlantType("C_04");
                // 测试删除植物
                if (Input.GetMouseButton(1))
                    tile.SetPlantType("");
            }
        }
        
        // 测试删除
        if (Input.GetKey(KeyCode.D))
            foreach (var hit in Physics.RaycastAll(Camera.main.ScreenPointToRay(Input.mousePosition)))
            {
                var target = hit.collider.gameObject;
                if (target.GetComponent<HexTile>() != null)
                {
                    var tile = target.GetComponent<HexTile>();
                    if (tile.GetComponent<DissolveAndDestroy>() != null)
                        continue;
                    if (tiles.ContainsKey((tile.coordQ, tile.coordR)))
                        tiles.Remove((tile.coordQ, tile.coordR));
                    tile.RemoveAllModels();
                }
            }
        */
        /*
        // 测试升高
        if (Input.GetKey(KeyCode.U))
            foreach (var hit in Physics.RaycastAll(Camera.main.ScreenPointToRay(Input.mousePosition)))
            {
                var target = hit.collider.gameObject;
                if (target.GetComponent<HexTile>() != null)
                {
                    var tile = target.GetComponent<HexTile>();
                    tile.fertility += 1;
                }
            }
        */

        if (Input.GetKeyDown(KeyCode.Q)){ // rotate plant
            AddRotate(-1);
        }
        if (Input.GetKeyDown(KeyCode.E)) // rotate plant
        {
            AddRotate(1);
        }
        ClearAllTempValue();
        foreach (var Tile in tiles.Values)
        {
            var tile = Tile.GetComponent<HexTile>();
            if (tile.Plant!=null && tile.Plant.GetComponent<Plant>().health <= 0)
            {
                RemovePlant(tile.coordQ, tile.coordR);
                //Debug.LogFormat("TryRemove {0:d} {1:d} {2:d}", tile.coordQ, tile.coordR, tile.goingToDie);
            }
            tile.GetComponent<HexTile>().updateHighlight(0);
        }
        ApplyAllTempFertility();
        ApplyAllKillPlant();

        if (!EventSystem.current.IsPointerOverGameObject())
            foreach (var hit in Physics.RaycastAll(Camera.main.ScreenPointToRay(Input.mousePosition)))
            {
                var target = hit.collider.gameObject;
                if (target.GetComponentInParent<HexTile>() != null)
                {
                    var tile = target.GetComponentInParent<HexTile>();
                    int q, r;
                    (q, r) = (tile.coordQ, tile.coordR);
                    // Debug.Log((q, r));
                    // Debug.Log($"{CurrentPlantIndex}");
                    if (CurrentPlantIndex == -2)// 铲除植物
                    {
                        ClearAllTempValue();
                        if (Input.GetMouseButtonDown(0)) // 确定铲除
                        {
                            RemovePlant(q, r);
                            ApplyAllTempFertility();
                            ApplyAllKillPlant();
                        }
                        else //预览铲除
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
                                    k_tile.updateHighlight(4);// 警告受牵连的植物
                                k_tile.goingToDie = 0;
                            }
                            tile.updateHighlight(3); //标记删除植物位置
                        }
                    }
                    else if (CurrentPlant != null && CurrentPlantIndex >= 0 &&CurrentPlantIndex < PlantPrefab.Count())
                    {
                        var plant = CurrentPlant.GetComponent<Plant>();
                        bool fg = tile.CanPlant(plant, rotateCnt);
                        // Debug.LogFormat("{0:d},{1:d}:{2:s}", q, r, fg.ToString());
                        if (fg)
                        {
                            fg &= AddPlantTemporary(q, r, plant, rotateCnt);
                        }
                        if (Input.GetMouseButtonDown(0)) //左键点击
                        { 
                            if (fg)//种植植物
                            {
                                tile.AddPlantToTile(Instantiate(PlantPrefab[CurrentPlantIndex], tile.transform), rotateCnt, PlantPrefabName);
                                ApplyAllTempFertility();
                                var sidebar = SideBar.GetComponent<Sidebar>();
                                sidebar.SetSeedNumber(CurrentPlantIndex + 1, sidebar.GetSeedNumber(CurrentPlantIndex + 1)-1);
                            }
                        }
                        else
                        { //预览种植相关
                            if (fg || true)
                            { //高亮加成格子
                                int x, y;
                                HexTile t_tile;
                                foreach (var token in plant.fertilityAcquire)
                                {
                                    (x, y) = RotateCoord(token.x, token.y, rotateCnt);
                                    (x, y) = (q + x, r + y);
                                    t_tile = GetTile(x, y);
                                    if (t_tile) t_tile.requiredFertility = token.z;
                                    if (t_tile) t_tile.updateHighlight(5);
                                }
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
                            if (tile.Plant != null || !tile.unlocked) //不能种
                            {
                                tile.updateHighlight(3);
                            }
                        }

                    }
                    break;
                }
            }

        if (SideBar != null)
        {
            var sidebar = SideBar.GetComponent<Sidebar>();
            if (sidebar != null)
            {
                if (sidebar.GetCurrentPlantId() != CurrentPlantIndex + 1)//注意这里加一！
                {
                    SwitchPlant(sidebar.GetCurrentPlantId()-1);
                }
            }
        }
        ClearAllTempValue();
        foreach (var Tile in tiles.Values)
        {
            var tile = Tile.GetComponent<HexTile>();
            if (tile != null && tile.Plant != null)
            {
                var plant = tile.Plant.GetComponent<Plant>();
                if (plant != null && !tile.CheckPlantCanGrow(plant, plant.rotateCnt))
                {
                    plant.LooseHelth(Time.deltaTime); // 减少枯萎植物的生命值
                }
                if (plant.TimeToBearFruit > 0)
                {
                    if(plant.Grow(Time.deltaTime))// 植物生长
                    {
                        plant.timeGrown = 0;
                        var seed = plant.GenerateSeed();
                        Seed real_seed = null;
                        foreach (Transform child in seed.transform)
                        {
                            var c_seed = child.gameObject.GetComponent<Seed>();
                            if (c_seed != null)
                            {
                                real_seed = c_seed;
                                c_seed.SetSideBar(SideBar);
                                c_seed.SetInfoBar(InfoBar);
                            }
                        }
                        var _seed = seed.GetComponent<Seed>();
                        if (_seed != null)
                        {
                            real_seed = _seed;
                            _seed.SetSideBar(SideBar);
                            _seed.SetInfoBar(InfoBar);
                        }
                            
                        if (real_seed != null)
                        {
                            if(real_seed.plantId == 7)
                            {
                                real_seed.SetSideBar(SideBar);
                                real_seed.SetInfoBar(InfoBar);
                                real_seed.fruit_cnt = (int)tile.GetAllFertility() * 3;
                            }
                        }
                    }
                }
            }
        }

    }


    private void FixedUpdate()
    {
        
    }
}
