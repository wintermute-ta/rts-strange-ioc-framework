using strange.extensions.mediation.impl;
using strange.extensions.pool.api;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : IGameManager
{
    const int ID_ATTACK_FRACTION = 1;
    const int ID_DEFEND_FRACTION = 2;

    [Inject]
    public IHexMap Map { get; private set; }
    [Inject]
    public IWorld World { get; private set; }
    [Inject]
    public IResourceManager ResourceManager { get; private set; }
    [Inject]
    public IUIManager UIManager { get; private set; }
    [Inject]
    public UIGlobalSignals UISignals { get; private set; }
    [Inject]
    public ILifeCycle LifeCycle { get; private set; }
    [Inject]
    public IPool<ShipView> ShipViewPool { get; private set; }
    [Inject]
    public IPool<GunView> GunViewPool { get; private set; }
    [Inject]
    public IPool<CastleView> CastleViewPool { get; private set; }
    [Inject]
    public HexGridCellTouchSignal HexGridCellTouch { get; private set; }
    [Inject]
    public HexGridCellTouchHoldSignal HexGridCellTouchHold { get; private set; }
    [Inject]
    public HexGridInteractableSignal HexGridInteractable { get; private set; }
    [Inject]
    public LockMapCameraSignal LockMapCamera { get; private set; }
    [Inject]
    public IHexGridUtility GridUtility { get; private set; }
    [Inject]
    public ShipDestroySignal ShipDestroy { get; private set; }

    private Dictionary<int, BaseUnitView> listViews;

    public BaseUnitView Get_UnitView(int id)
    {
        return listViews[id] ?? null;
    }

    public GameManager()
    {
        listViews = new Dictionary<int, BaseUnitView>();
    }

    [PostConstruct]
    public void PostConstruct()
    {
        HexGridCellTouch.AddListener(OnHexGridCellTouch);
        HexGridCellTouchHold.AddListener(OnHexGridCellTouchHold);
        LifeCycle.OnBackPressed.AddListener(OnBackPressed);
        LifeCycle.OnUpdate.AddListener(OnUpdate);
        LifeCycle.OnFixedUpdate.AddListener(OnFixedUpdate);
        ShipDestroy.AddListener(OnShipDestroy);
    }

    public void Init()
    {
    }

    private void OnHexGridCellTouchHold(HexGridCell cell)
    {
        OpenRadialMenu(cell);
    }

    private void OnHexGridCellTouch(HexGridCell cell)
    {
        //OpenRadialMenu(cell);
    }

    private void OpenRadialMenu(HexGridCell cell)
    {
        IUnit unit = World.GetUnit(cell.MapCell.Coordinates);
        if (cell.MapCell.IsSpawnCell || (unit != null))
        {
            GlobalContext globalContext = GlobalContext.Get();
            List<ItemContext> context = new List<ItemContext>();
            if (cell.MapCell.IsWalkable())
            {
                if (cell.MapCell.IsSpawnCell)
                {
                    context.Add(new ItemContext(ItemContext.ActionType.ACTION_CREATE_SHIP.ToString(), () => { CreateShip(cell); }));
                }
            }
            else
            {
                if (cell.MapCell.IsSpawnCell)
                {
                    context.Add(new ItemContext(ItemContext.ActionType.ACTION_CREATE_GUN.ToString(), () => { CreateGun(cell); }));
                    context.Add(new ItemContext(ItemContext.ActionType.ACTION_CREATE_CASTLE.ToString(), () => { CreateCastle(cell); }));
                }
                if (unit != null)
                {
                    context.Add(new ItemContext(ItemContext.ActionType.ACTION_REMOVE.ToString(), () => { RemoveGroundInstance(unit); }));
                }
            }
            context.Add(new ItemContext(ItemContext.ActionType.ACTION_RETURN.ToString(), DummyFunction));
            RadialMenuModel model = UIManager.CreateUI<RadialMenuModel>(UIManager.Parent, "RadialMenuScreen", Camera.main.WorldToScreenPoint(HexMetrics.Perturb(cell.Position)), true);
            UISignals.CloseMenuSinal.AddOnce(delegate { SetInteractable(true); });
            UISignals.OpenMenuSignal.AddOnce(delegate { SetInteractable(false); });
            model.SetContext(context);
            model.Open();
        }
    }

    private void OnUpdate(float time)
    {
    }

    private void OnFixedUpdate(float time)
    {
        World.UpdateAI(time);
    }

    private void OnBackPressed()
    {
        Application.Quit();
    }

    public void SetInteractable(bool enabled)
    {
        HexGridInteractable.Dispatch(enabled);
        LockMapCamera.Dispatch(!enabled);
    }

    public void DummyFunction()
    {
        Debug.Log("DummyFunction");
    }

    public void CreateShip(HexGridCell cell)
    {
        SpawnShip(cell);
    }

    public void CreateGun(HexGridCell cell)
    {
        SpawnGun(cell);
    }

    public void CreateCastle(HexGridCell cell)
    {
        SpawnCastle(cell);
    }

    public void RemoveGroundInstance(IUnit unit)
    {
        unit.DestroyUnit();
    }

    private void Ship_OnDestroyed(IUnit unit)
    {
        unit.OnDestroyed -= Ship_OnDestroyed;
        BaseUnitView view = Get_UnitView(unit.ID);
        ShipViewPool.ReturnInstance(view);
        listViews.Remove(unit.ID);
        //GameObject.Destroy(view.gameObject);
    }

    private void Gun_OnDestroyed(IUnit unit)
    {
        unit.OnDestroyed -= Gun_OnDestroyed;
        BaseUnitView view = Get_UnitView(unit.ID);
        GunViewPool.ReturnInstance(view);
        listViews.Remove(unit.ID);
        //GameObject.Destroy(view.gameObject);
    }

    private void Castle_OnDestroyed(IUnit unit)
    {
        unit.OnDestroyed -= Castle_OnDestroyed;
        BaseUnitView view = Get_UnitView(unit.ID);
        CastleViewPool.ReturnInstance(view);
        listViews.Remove(unit.ID);
        //GameObject.Destroy(view.gameObject);
    }

    public CastleView SpawnCastle(HexGridCell cell)
    {
        Vector3 position = HexMetrics.Perturb(cell.Position);
        position = cell.chunk.transform.TransformPoint(position);
        CastleView view = CastleViewPool.GetInstance();
        InstanceCastle unit = World.SpawnCastle(view.GetInstanceID(), ID_DEFEND_FRACTION, cell.MapCell.Coordinates);
        cell.MapCell.IsSpawnCell = false;
        unit.OnDestroyed += Castle_OnDestroyed;
        view.Init(unit, position, Quaternion.identity);
        GridUtility.UpdateAttackRangeCoordinates(cell.MapCell.Coordinates, unit.AttackRange, true);
        listViews.Add(unit.ID, view);

        return view;
    }

    public ShipView SpawnShip(HexGridCell cell)
    {
        Vector3 position = HexMetrics.Perturb(cell.Position);
        position = cell.chunk.transform.TransformPoint(position);
        ShipView view = ShipViewPool.GetInstance();
        InstanceShip unit = World.SpawnShip(view.GetInstanceID(), ID_ATTACK_FRACTION, cell.MapCell.Coordinates);
        unit.OnDestroyed += Ship_OnDestroyed;
        unit.OnDestinationChanged += Ship_OnDestinationChanged;
        view.Init(unit, position, Quaternion.AngleAxis(90.0f, Vector3.up));
        listViews.Add(unit.ID, view);

        return view;
    }

    public GunView SpawnGun(HexGridCell cell)
    {
        Vector3 position = HexMetrics.Perturb(cell.Position);
        position = cell.chunk.transform.TransformPoint(position);
        GunView view = GunViewPool.GetInstance();
        InstanceGun unit = World.SpawnGun(view.GetInstanceID(), ID_DEFEND_FRACTION, cell.MapCell.Coordinates);
        cell.MapCell.IsSpawnCell = false;
        unit.OnDestroyed += Gun_OnDestroyed;
        view.Init(unit, position, Quaternion.identity);
        GridUtility.UpdateAttackRangeCoordinates(cell.MapCell.Coordinates, unit.AttackRange, true);
        unit.ID = view.GetInstanceID();
        listViews.Add(unit.ID, view);

        return view;
    }

    private void Ship_OnDestinationChanged(InstanceShip ship, IUnit destination)
    {
        if (destination != null)
        {
            // Let's go!
            HexMapCell cellOrigin = Map.GetCell(ship.Coordinates);
            HexMapCell cellDestination = Map.GetCell(destination.Coordinates);
            World.FindPath(cellOrigin, cellDestination, ship.Path);
            for (int i = 0; i < ship.Path.Count; i++)
            {
                HexGridCell cell = GridUtility.GetCell(ship.Path[i].AStarCoordinates);
                if (cell != null)
                {
                    cell.IsPathCell = true;
                }
            }
        }
    }

    private void OnShipDestroy(InstanceShip ship)
    {
        for (int i = 0; i < ship.Path.Count; i++)
        {
            HexGridCell cell = GridUtility.GetCell(ship.Path[i].AStarCoordinates);
            if (cell != null)
            {
                cell.IsPathCell = false;
            }
        }
    }
}
