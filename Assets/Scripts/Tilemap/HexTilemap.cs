using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

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

    /// 六边形的长轴为z轴
    static public readonly Vector3 UnitQ = new Vector3(Mathf.Sqrt(3) * 0.5f, 0, 0.5f);
    static public readonly Vector3 UnitR = new Vector3(0, 0, -1);
    static public readonly Vector3 UnitS = new Vector3(-Mathf.Sqrt(3) * 0.5f, 0, 0.5f);
    static public Vector3 CoordToPosition(int q, int r, float cellSize) => (q * UnitQ + r * UnitR - (q + r) * UnitS) * cellSize;
    static public int CoordDistance(int q1, int r1, int q2 = 0, int r2 = 0) => (math.abs(q1 - q2) + math.abs(q1 + r1 - q2 - r2) + math.abs(r1 - r2)) / 2;
    public Vector3 CoordToPosition(int q, int r) => CoordToPosition(q, r, CellSize);

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
    }

    void Update()
    {
        if (AutoFillTile && !Input.GetKey(KeyCode.D))
            DoAutoFillTile();
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
    }
}
