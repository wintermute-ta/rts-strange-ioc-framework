using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
    private bool isMouseDown;
    private Transform transformUI;
    private HexGrid hexGrid;
    private HexMapEditor mapEditor;

    void Awake()
    {
        //transformUI = GameObject.FindGameObjectWithTag("UI").transform;
    }

	// Use this for initialization
	void Start()
    {
        HexGrid prefabGrid = ResourceManager.Instance.GetPrefabComponent<HexGrid>("HexGrid");
        InitMap(prefabGrid.cellCountX, prefabGrid.cellCountZ);
        hexGrid = InstanceHexGrid(prefabGrid, GameManager.Instance.Map);
        InstanceUI(hexGrid, GameManager.Instance.Map);
        InstanceDefaultLight();
        InstanceMainCamera();
        InstanceHexMapCamera(hexGrid);
	}

    private void InstanceMainCamera()
    {
        Instantiate(ResourceManager.Instance.GetPrefab("MainCamera"));
    }

    private void InstanceDefaultLight()
    {
        Instantiate(ResourceManager.Instance.GetPrefab("DefaultLight"));
    }

    private bool InitMap(int cellCountX, int cellCountZ)
    {
        GameManager.Instance.Map.CreateMap(cellCountX, cellCountZ);
        return GameManager.Instance.Map.TryLoadDefaultMap();
    }

    private HexGrid InstanceHexGrid(HexGrid prefab, HexMap hexMap)
    {
        HexGrid grid = Instantiate(prefab);
        grid.CreateGrid(hexMap);
        return grid;
    }

    private void InstanceUI(HexGrid grid, HexMap map)
    {
        transformUI = Instantiate(ResourceManager.Instance.GetPrefab("UI")).transform;
        GameObject uiEditor = Instantiate(ResourceManager.Instance.GetPrefab("UIEditor"));
        if (uiEditor != null)
        {
            Transform transformHexMapEditor = uiEditor.transform.FindChild("HexMapEditor");
            mapEditor = transformHexMapEditor != null ? transformHexMapEditor.GetComponent<HexMapEditor>() : null;
            if (mapEditor != null)
            {
                mapEditor.transform.SetParent(transformUI, true);
                mapEditor.hexMap = map;
                mapEditor.hexGrid = grid;
                mapEditor.newMapMenu.hexGrid = grid;
                mapEditor.saveLoadMenu.hexGrid = grid;
            }
        }
    }

    private void InstanceHexMapCamera(HexGrid grid)
    {
        HexMapCamera hexMapCamera = Instantiate(ResourceManager.Instance.GetPrefabComponent<HexMapCamera>("HexMapCamera"));
        hexMapCamera.grid = grid;
    }

    // Update is called once per frame
    void Update()
    {

    }

    void LateUpdate()
    {
        if (!isMouseDown)
        {
            isMouseDown = Input.GetMouseButton(0);
        }
        else
        {
            if (!Input.GetMouseButton(0))
            {
                isMouseDown = false;

                if (hexGrid != null)
                {
                    if (hexGrid.selectedCell != null)
                    {
                        if (hexGrid.selectedCell.MapCell.IsSpawnCell)
                        {
                            if (!hexGrid.selectedCell.MapCell.IsWalkable())
                            {    //Spawn for Gun
                                InstanceGun gun = GameManager.Instance.SpawnGun(hexGrid.selectedCell.MapCell.Coordinates);
                                SpawnGun(gun, hexGrid.selectedCell);
                                hexGrid.selectedCell.MapCell.IsSpawnCell = false;
                            }
                            else
                            {  //Spawn for Ship
                                InstanceShip ship = GameManager.Instance.SpawnShip(hexGrid.selectedCell.MapCell.Coordinates);
                                SpawnShip(ship, hexGrid.selectedCell);
                            }
                        }
                    }
                }
            }
        }
    }

    public BehaviorShip SpawnShip(InstanceShip ship, HexGridCell cell)
    {
        Vector3 position = HexMetrics.Perturb(cell.Position);
        position = cell.chunk.transform.TransformPoint(position);
        BehaviorShip unit = Instantiate(ResourceManager.Instance.GetPrefabComponent<BehaviorShip>("Unit"), position, Quaternion.identity);
        unit.Init(hexGrid, ship);

        ship.ID = unit.GetInstanceID();
        GameManager.Instance.AddBehavioralShipsInList(unit.GetInstanceID(), unit);
        GameManager.Instance.OnDestroyShip += Instance_OnDestroyShip;
        return unit;
    }

    private void Instance_OnDestroyShip(BehaviorShip obj)
    {
        if(obj != null)
            Destroy(obj.gameObject);
    }

    public BehaviorGun SpawnGun(InstanceGun gun, HexGridCell cell)
    {
        Vector3 position = HexMetrics.Perturb(cell.Position);
        position = cell.chunk.transform.TransformPoint(position);
        BehaviorGun unit = Instantiate(ResourceManager.Instance.GetPrefabComponent<BehaviorGun>("cannon_upgrade_01"), position, Quaternion.identity);
        unit.Init(hexGrid, gun);
        return unit;
    }
}
