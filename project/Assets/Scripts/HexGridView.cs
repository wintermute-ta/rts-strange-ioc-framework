using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using strange.extensions.mediation.impl;
using strange.extensions.signal.impl;

public class HexGridView : View
{
    public Signal OnInitialized = new Signal();
    [SerializeField]
    private int cellCountX = 20, cellCountZ = 15;
    public int CellCountX { get { return cellCountX; } private set { cellCountX = value; } }
    public int CellCountZ { get { return cellCountZ; } private set { cellCountZ = value; } }

    public HexGridCell cellPrefab;
    public Text cellLabelPrefab;
    public HexGridChunk chunkPrefab;

    public Texture2D noiseSource;

    public int seed;

    public bool Interactable { get; set; }

    HexGridChunk[] chunks;
    HexGridCell[] cells;

    int chunkCountX, chunkCountZ;

    protected override void Awake()
    {
        base.Awake();
        HexMetrics.noiseSource = noiseSource;
        HexMetrics.InitializeHashGrid(seed);
        StartCoroutine(CoInitCamera());
    }

    public void CreateGrid(IHexMap hexMap)
    {
        hexMap.OnDestroyChunk += HexMap_OnDestroyChunk;
        if (chunks != null)
        {
            for (int i = 0; i < chunks.Length; i++)
            {
                DestroyChunk(i);
            }
        }

        cellCountX = hexMap.CellCountX;
        cellCountZ = hexMap.CellCountZ;
        chunkCountX = cellCountX / HexMetrics.chunkSizeX;
        chunkCountZ = cellCountZ / HexMetrics.chunkSizeZ;
        CreateChunks(hexMap);
        CreateCells(hexMap);

        Interactable = true;
    }

    private void HexMap_OnDestroyChunk(HexMapChunk chunk)
    {
        int index = chunk.X + chunk.Z * chunkCountX;
        DestroyChunk(index);
    }

    private void DestroyChunk(int index)
    {
        Destroy(chunks[index].gameObject);
        chunks[index] = null;
    }

    void CreateChunks(IHexMap hexMap)
    {
        chunks = new HexGridChunk[chunkCountX * chunkCountZ];

        for (int z = 0, i = 0; z < chunkCountZ; z++)
        {
            for (int x = 0; x < chunkCountX; x++)
            {
                HexGridChunk chunk = chunks[i++] = Instantiate(chunkPrefab);
                chunk.Init(hexMap.GetChunk(x, z), transform);
            }
        }
    }

    void CreateCells(IHexMap hexMap)
    {
        cells = new HexGridCell[cellCountZ * cellCountX];

        for (int z = 0, i = 0; z < hexMap.CellCountZ; z++)
        {
            for (int x = 0; x < hexMap.CellCountX; x++)
            {
                CreateCell(hexMap.GetCell(HexCoordinates.FromOffsetCoordinates(x, z)), i++);
            }
        }
    }

    void OnDrawGizmos()
    {
        Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        Debug.DrawRay(inputRay.origin, inputRay.direction, Color.red);
    }

    void OnEnable()
    {
        if (!HexMetrics.noiseSource)
        {
            HexMetrics.noiseSource = noiseSource;
            HexMetrics.InitializeHashGrid(seed);
        }
    }

    public void UpdateAttackRangeCoordinates(HexCoordinates center, int range, bool visible)
    {
        for (int dx = -range + center.X; dx <= range + center.X; dx++)
        {
            for (int dy = -range + center.Y; dy <= range + center.Y; dy++)
            {
                for (int dz = -range + center.Z; dz <= range + center.Z; dz++)
                {
                    if (dx + dy + dz == 0)
                    {
                        HexGridCell cell = GetCell(dx, dz);
                        if (cell != null)
                        {
                            if (cell.MapCell.IsWalkable())
                            {
                                cell.IsAttackRangeCell = visible;
                            }
                        }
                    }
                }
            }
        }
    }

    public HexGridCell GetCell(Vector3 position)
    {
        position = transform.InverseTransformPoint(position);
        HexCoordinates coordinates = HexCoordinates.FromPosition(position);
        return GetCell(coordinates);
    }

    public HexGridCell GetCell(HexCoordinates coordinates)
    {
        return GetCell(coordinates.X, coordinates.Z);
    }

    public HexGridCell GetCell(int x, int z)
    {
        if (z < 0 || z >= CellCountZ)
        {
            return null;
        }
        int tempX = x + z / 2;
        if (tempX < 0 || tempX >= CellCountX)
        {
            return null;
        }
        int index = x + z * cellCountX + z / 2;
        return (index >= 0) && (index < cells.Length) ? cells[index] : null;
    }

    public void ShowUI(bool visible)
    {
        for (int i = 0; i < chunks.Length; i++)
        {
            chunks[i].ShowUI(visible);
        }
    }

    void CreateCell(HexMapCell mapCell, int i)
    {
        Vector3 position;
        position.x = (mapCell.OffsetX + mapCell.OffsetZ * 0.5f - mapCell.OffsetZ / 2) * (HexMetrics.innerRadius * 2f);
        position.y = 0f;
        position.z = mapCell.OffsetZ * (HexMetrics.outerRadius * 1.5f);

        HexGridCell cell = cells[i] = Instantiate<HexGridCell>(cellPrefab);
        cell.Init(mapCell, position);

        if (mapCell.OffsetX > 0)
        {
            cell.SetNeighbor(HexDirection.W, cells[i - 1]);
        }
        if (mapCell.OffsetZ > 0)
        {
            if ((mapCell.OffsetZ & 1) == 0)
            {
                cell.SetNeighbor(HexDirection.SE, cells[i - cellCountX]);
                if (mapCell.OffsetX > 0)
                {
                    cell.SetNeighbor(HexDirection.SW, cells[i - cellCountX - 1]);
                }
            }
            else {
                cell.SetNeighbor(HexDirection.SW, cells[i - cellCountX]);
                if (mapCell.OffsetX < cellCountX - 1)
                {
                    cell.SetNeighbor(HexDirection.SE, cells[i - cellCountX + 1]);
                }
            }
        }

        Text label = Instantiate<Text>(cellLabelPrefab);
        label.rectTransform.anchoredPosition =
            new Vector2(position.x, position.z);
        label.text = cell.MapCell.Coordinates.ToStringOnSeparateLines();
        cell.uiRect = label.rectTransform;

        AddCellToChunk(cell);

        cell.RefreshPosition();
    }

    void AddCellToChunk(HexGridCell cell)
    {
        int chunkX = cell.MapCell.OffsetX / HexMetrics.chunkSizeX;
        int chunkZ = cell.MapCell.OffsetZ / HexMetrics.chunkSizeZ;
        HexGridChunk chunk = chunks[chunkX + chunkZ * chunkCountX];

        int localX = cell.MapCell.OffsetX - chunkX * HexMetrics.chunkSizeX;
        int localZ = cell.MapCell.OffsetZ - chunkZ * HexMetrics.chunkSizeZ;
        chunk.AddCell(localX + localZ * HexMetrics.chunkSizeX, cell);
    }

    private IEnumerator CoInitCamera()
    {
        yield return new WaitForEndOfFrame();
        OnInitialized.Dispatch();
    }
}