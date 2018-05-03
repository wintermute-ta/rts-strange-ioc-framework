using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.EventSystems;
using System;
using System.Collections;

public class HexGrid : MonoBehaviour {

	public int cellCountX = 20, cellCountZ = 15;

	public HexGridCell cellPrefab;
	public Text cellLabelPrefab;
	public HexGridChunk chunkPrefab;

	public Texture2D noiseSource;

	public int seed;

    public HexGridCell selectedCell;

    public bool Interactable { get; set; }

    //	public Color[] colors;

    HexGridChunk[] chunks;
    HexGridCell[] cells;
    HexMap hexMap;

	int chunkCountX, chunkCountZ;

	void Awake()
    {
		HexMetrics.noiseSource = noiseSource;
		HexMetrics.InitializeHashGrid(seed);
        //		HexMetrics.colors = colors;
        StartCoroutine(CoInitCamera());
        //CreateGrid(cellCountX, cellCountZ);
        //if (TryLoadDefaultMap())
        //{
        //    StartCoroutine(CoInitCamera());
        //}
	}

    public void CreateGrid(HexMap hexMap)
    {
        this.hexMap = hexMap;

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

    void CreateChunks(HexMap hexMap) {
		chunks = new HexGridChunk[chunkCountX * chunkCountZ];

		for (int z = 0, i = 0; z < chunkCountZ; z++) {
			for (int x = 0; x < chunkCountX; x++) {
				HexGridChunk chunk = chunks[i++] = Instantiate(chunkPrefab);
                chunk.Init(hexMap.GetChunk(x, z), transform);
			}
		}
	}

	void CreateCells(HexMap hexMap) {
		cells = new HexGridCell[cellCountZ * cellCountX];

		for (int z = 0, i = 0; z < hexMap.CellCountZ; z++) {
			for (int x = 0; x < hexMap.CellCountX; x++) {
				CreateCell(hexMap.GetCell(HexCoordinates.FromOffsetCoordinates(x, z)), i++);
			}
		}
	}

    void Update()
    {
        if (!EventSystem.current.IsPointerOverGameObject() && Interactable)
        {
            HandleInput();
        }
    }

    void HandleInput()
    {
        Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(inputRay, out hit))
        {
            HexGridCell currentCell = GetCell(HexMetrics.Unperturb(hit.point));
            if (currentCell != null)
            {
                if (selectedCell != null)
                {
                    if (currentCell.MapCell.Coordinates != selectedCell.MapCell.Coordinates)
                    {
                        selectedCell.Selected = false;
                    }
                }
                currentCell.Selected = true;
                selectedCell = currentCell;
            }
        }
        else
        {
            if (selectedCell != null)
            {
                selectedCell.Selected = false;
                selectedCell = null;
            }
        }
    }

    void OnDrawGizmos()
    {
        Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        Debug.DrawRay(inputRay.origin, inputRay.direction, Color.red);
    }

    void OnEnable () {
		if (!HexMetrics.noiseSource) {
			HexMetrics.noiseSource = noiseSource;
			HexMetrics.InitializeHashGrid(seed);
//			HexMetrics.colors = colors;
		}
	}

	public HexGridCell GetCell(Vector3 position) {
		position = transform.InverseTransformPoint(position);
		HexCoordinates coordinates = HexCoordinates.FromPosition(position);
        return GetCell(coordinates);
	}

    public HexGridCell GetCell(HexCoordinates coordinates)
    {
        int index = coordinates.X + coordinates.Z * cellCountX + coordinates.Z / 2;
        return (index >= 0) && (index < cells.Length) ? cells[index] : null;
    }

    public void ShowUI (bool visible) {
		for (int i = 0; i < chunks.Length; i++) {
			chunks[i].ShowUI(visible);
		}
	}

	void CreateCell (HexMapCell mapCell, int i) {
		Vector3 position;
		position.x = (mapCell.OffsetX + mapCell.OffsetZ * 0.5f - mapCell.OffsetZ / 2) * (HexMetrics.innerRadius * 2f);
		position.y = 0f;
		position.z = mapCell.OffsetZ * (HexMetrics.outerRadius * 1.5f);

        HexGridCell cell = cells[i] = Instantiate<HexGridCell>(cellPrefab);
        cell.Init(mapCell, position);

		if (mapCell.OffsetX > 0) {
			cell.SetNeighbor(HexDirection.W, cells[i - 1]);
		}
		if (mapCell.OffsetZ > 0) {
			if ((mapCell.OffsetZ & 1) == 0) {
				cell.SetNeighbor(HexDirection.SE, cells[i - cellCountX]);
				if (mapCell.OffsetX > 0) {
					cell.SetNeighbor(HexDirection.SW, cells[i - cellCountX - 1]);
				}
			}
			else {
				cell.SetNeighbor(HexDirection.SW, cells[i - cellCountX]);
				if (mapCell.OffsetX < cellCountX - 1) {
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

	void AddCellToChunk(HexGridCell cell) {
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
        HexMapCamera.ValidatePosition();
    }
}