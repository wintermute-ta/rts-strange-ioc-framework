using Core;
using GameWorld;
using GameWorld.HexMap;
using GameWorld.Units;
using strange.extensions.pool.api;
using System.Collections.Generic;
using UnityEngine;
using Views.HexGrid;
using Views.Shots;
using Views.Units;

public class GameManager : IGameManager
{
    const int ID_ATTACK_FRACTION = 1;
    const int ID_DEFEND_FRACTION = 2;

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
    public IUnitViewBinder UnitViewBinder { get; private set; }
    [Inject]
    public IPool<RoundShotView> RoundShotPool { get; private set; }
    [Inject]
    public Signals.HexGrid.CellTouch HexGridCellTouchSignal { get; private set; }
    [Inject]
    public Signals.HexGrid.CellTouchHold HexGridCellTouchHoldSignal { get; private set; }
    [Inject]
    public Signals.HexGrid.CellTouchTap HexGridCellTouchTapSignal { get; private set; }
    [Inject]
    public Signals.HexGrid.Interactable HexGridInteractableSignal { get; private set; }
    [Inject]
    public Signals.Camera.Move CameraMoveSignal { get; private set; }
    [Inject]
    public Signals.Units.Attack AttackSignal { get; private set; }
    [Inject]
    public Signals.Units.NavalUnit.DestinationChanged DestinationChangedSignal { get; private set; }
    [Inject]
    public Signals.Units.Weapon.ShotReachTarget ShotReachTargetSignal { get; private set; }

    [Inject]
    public Signals.Camera.Lock CameraLockSignal { get; private set; }
    [Inject]
    public IHexGridUtility GridUtility { get; private set; }

    private Dictionary<int, BaseUnitView> listUnitViews;
    private Dictionary<int, BaseShotView> listShotViews;

    public BaseUnitView Get_UnitView(int id)
    {
        return listUnitViews[id] ?? null;
    }

    public BaseShotView Get_ShotView(int id)
    {
        return listShotViews[id] ?? null;
    }

    public GameManager()
    {
        listUnitViews = new Dictionary<int, BaseUnitView>();
        listShotViews = new Dictionary<int, BaseShotView>();
    }

    [PostConstruct]
    public void PostConstruct()
    {
        HexGridCellTouchSignal.AddListener(OnHexGridCellTouch);
        HexGridCellTouchHoldSignal.AddListener(OnHexGridCellTouchHold);
        HexGridCellTouchTapSignal.AddListener(OnHexGridCellTap);
        LifeCycle.OnBackPressed.AddListener(OnBackPressed);
        LifeCycle.OnUpdate.AddListener(OnUpdate);
        LifeCycle.OnFixedUpdate.AddListener(OnFixedUpdate);
        DestinationChangedSignal.AddListener(OnNavalUnitDestinationChanged);
        AttackSignal.AddListener(OnUnitAttack);
        ShotReachTargetSignal.AddListener(OnShotReachTarget);
    }

    public void Init()
    {
    }

    private void OnHexGridCellTouchHold(HexGridCell cell)
    {
        //OpenRadialMenu(cell);
    }

    private void OnHexGridCellTouch(HexGridCell cell)
    {
        //OpenRadialMenu(cell);
    }

    private void OnHexGridCellTap(HexGridCell cell)
    {
        OpenRadialMenu(cell);
    }

    private void OpenRadialMenu(HexGridCell cell)
    {
        IUnit unit = World.GetUnit(cell.MapCell.Coordinates);
        if (cell.MapCell.IsSpawnCell || (unit != null))
        {
            CameraMoveSignal.Dispatch(HexMetrics.Perturb(cell.Position));
            List<ItemContext> context = new List<ItemContext>();
            if (cell.MapCell.IsWalkable())
            {
                if (cell.MapCell.IsSpawnCell)
                {
                    context.Add(new ItemContext(ItemContext.ActionType.ACTION_CREATE_SHIP.ToString(), () => { SpawnUnit<Ship>(ID_ATTACK_FRACTION, cell); }));
                }
            }
            else
            {
                if (cell.MapCell.IsSpawnCell)
                {
                    context.Add(new ItemContext(ItemContext.ActionType.ACTION_CREATE_GUN.ToString(), () => { SpawnUnit<GroundCannon>(ID_DEFEND_FRACTION, cell); }));
                    context.Add(new ItemContext(ItemContext.ActionType.ACTION_CREATE_CASTLE.ToString(), () => { SpawnUnit<Fort>(ID_DEFEND_FRACTION, cell); }));
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
        HexGridInteractableSignal.Dispatch(enabled);
        CameraLockSignal.Dispatch(!enabled);
    }

    public void DummyFunction()
    {
        Debug.Log("DummyFunction");
    }

    public void RemoveGroundInstance(IUnit unit)
    {
        unit.DestroyUnit();
    }

    public BaseUnitView SpawnUnit<T>(int fraction, HexGridCell cell) where T : IUnit
    {
        IUnit unitInThisCell = World.GetUnit(cell.MapCell.Coordinates);
        if (unitInThisCell == null)
        {
            BaseUnitView view = UnitViewBinder.GetInstance<T>();
            IUnit unit = World.SpawnUnit<T>(view.GetInstanceID(), fraction, cell.MapCell);
            
            Vector3 position = HexMetrics.Perturb(cell.Position);
            position = cell.chunk.transform.TransformPoint(position);
            unit.OnDestroy.AddOnce(Unit_OnDestroyed);
            unit.OnDestroy.AddOnce(OnRemoveDestroyedUnitView);
            view.Init(unit, position);
            if (view.AddAttackRange)
            {
                unit.OnDestroy.AddOnce(OnRemoveUnitAttackRange);
                GridUtility.UpdateAttackRangeCoordinates(cell.MapCell.Coordinates, unit.AttackRange, true);
            }
            listUnitViews.Add(unit.ID, view);
            return view;
        }
        return null;
    }

    private void Unit_OnDestroyed(IUnit unit)
    {
        BaseUnitView view = Get_UnitView(unit.ID);
        UnitViewBinder.ReturnInstance(view);
    }

    public BaseShotView SpawnShot(IUnit unit, IUnit target)
    {
        HexGridCell cell = GridUtility.GetCell(unit.Coordinates);
        if (cell != null)
        {
            Vector3 position = HexMetrics.Perturb(cell.Position);
            position = cell.chunk.transform.TransformPoint(position);
            BaseUnitView viewUnit = Get_UnitView(unit.ID);
            BaseUnitView viewTarget = Get_UnitView(target.ID);
            if ((viewUnit != null) && (viewTarget != null))
            {
                BaseShotView view = RoundShotPool.GetInstance();
                view.Init(target.ID, viewUnit.transform.position, viewTarget.transform.position);
                listShotViews.Add(view.GetInstanceID(), view);
                return view;
            }
        }
        return null;
    }

    private void OnNavalUnitDestinationChanged(INavalUnit unit, IUnit unitDestination)
    {
        if (unitDestination != null)
        {
            HexMapCell cellOrigin = World.Map.GetCell(unit.Coordinates);
            HexMapCell cellDestination = World.Map.GetCell(unitDestination.Coordinates);
            World.FindPath(cellOrigin, cellDestination, unit.Path);
            for (int i = 0; i < unit.Path.Count; i++)
            {
                HexGridCell cell = GridUtility.GetCell(unit.Path[i].AStarCoordinates);
                if (cell != null)
                {
                    cell.IsPathCell = true;
                }
            }
        }
    }

    private void OnRemoveDestroyedUnitView(IUnit unit)
    {
        listUnitViews.Remove(unit.ID);
    }

    private void OnShotReachTarget(int id)
    {
        BaseShotView view = Get_ShotView(id);
        listShotViews.Remove(id);
        RoundShotPool.ReturnInstance(view);
    }

    private void OnRemoveUnitAttackRange(IUnit unit)
    {
        HexMapCell cell = World.Map.GetCell(unit.Coordinates);
        if (cell != null)
        {
            GridUtility.UpdateAttackRangeCoordinates(cell.Coordinates, unit.AttackRange, false);
        }
    }

    private void OnUnitAttack(IUnit unit, IUnit target, float damage)
    {
        //Debug.LogFormat("Unit ({0}:{1}) attack to ({2}:{3})", unit.Type, unit.ID, target.Type, target.ID);
        SpawnShot(unit, target);
    }
}