using Core;
using GameWorld;
using GameWorld.HexMap;
using GameWorld.Units;
using Signals.GameManager;
using strange.extensions.pool.api;
using System.Collections.Generic;
using UnityEngine;
using Views.HexGrid;
using Views.Shots;
using Views.Units;
using System;
using GameWorld.Units.Components;
using Core.FSM;
using UI;
using System.Linq;

public class GameManager : IGameManager
{
    class DraggableItemContext : ItemContext
    {
        public int PathSectionIndex { get;  set; }
    }

    [Inject]
    public IGlobalContext Context { get; private set; }
    [Inject]
    public ICoreLogger Logger { get; private set; }
    [Inject]
    public IOverlayManager Overlays { get; private set; }
    [Inject]
    public IWorld World { get; private set; }
    [Inject]
    public IResourceManager ResourceManager { get; private set; }
    [Inject]
    public IUnitSchemeManager UnitSchemeManager { get; private set; }
    [Inject]
    public IFractionManager FractionManager { get; private set; }
    [Inject]
    public IGameManagerFSM GameManagerFSM { get; private set; }
    [Inject]
    public IUIManager UIManager { get; private set; }
    //[Inject]
    //public IUnitSettingsManager UnitSettingsManager { get; private set; }
    [Inject]
    public UI.UIGlobalSignals UISignals { get; private set; }
    [Inject]
    public ILifeCycle LifeCycle { get; private set; }
    [Inject]
    public IViewBinder ViewBinder { get; private set; }
    [Inject]
    public Signals.InitGameCompleteSignal InitGameCompleteSignal { get; private set; }
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
    public Signals.Camera.MoveEnd CameraEndMoveSignal { get; private set; }
    [Inject]
    public Signals.Camera.Pan CameraPanSignal { get; private set; }
    [Inject]
    public Signals.Camera.Rotate CameraRotateSignal { get; private set; }
    [Inject]
    public Signals.Camera.Zoom CameraZoomSignal { get; private set; }
    [Inject]
    public Signals.Units.Attack AttackSignal { get; private set; }
    [Inject]
    public Signals.Units.Weapon.ShotReachTarget ShotReachTargetSignal { get; private set; }
    [Inject]
    public Signals.Units.ProductionComplete ProductionCompleteSignal { get; private set; }
    [Inject]
    public UI.Manager.IContextMenuManager ContextMenuManager { get; private set; }
    [Inject]
    public SpawnUnitComplete SpawnUnitCompleteSignal { get; private set; }

    [Inject]
    public Signals.Camera.Lock CameraLockSignal { get; private set; }
    [Inject]
    public IHexGridUtility GridUtility { get; private set; }

    private Dictionary<int, BaseUnitView> listUnitViews;
    private Dictionary<int, BaseShotView> listShotViews;

    private IGameManagerFSMState statePlaceBase;
    private IGameManagerFSMState stateDefault;
    private IGameManagerFSMState statePause;
    private IGameManagerFSMState stateBuild;
    private IGameManagerFSMState stateProduction;
    private IGameManagerFSMState stateProductionSelectTarget;

#if UNITY_ANDROID && !UNITY_EDITOR
    private Vector2 fingerScreenOffset = new Vector2(0.0f, 0.25f * Screen.dpi); // offset up with 1/4 inches.
#elif UNITY_IOS && !UNITY_EDITOR
    // TODO: implement dpi receiving for iOS
    private Vector2 fingerScreenOffset = Vector2.zero;
#else
    private Vector2 fingerScreenOffset = Vector2.zero;
#endif

    public GameManager()
    {
        listUnitViews = new Dictionary<int, BaseUnitView>();
        listShotViews = new Dictionary<int, BaseShotView>();
    }

    [PostConstruct]
    public void PostConstruct()
    {
        InitGameCompleteSignal.AddOnce(OnInitGameComplete);
        HexGridCellTouchSignal.AddListener(OnHexGridCellTouch);
        HexGridCellTouchHoldSignal.AddListener(OnHexGridCellTouchHold);
        HexGridCellTouchTapSignal.AddListener(OnHexGridCellTap);
        LifeCycle.OnBackPressed.AddListener(OnBackPressed);
        LifeCycle.OnUpdate.AddListener(OnUpdate);
        LifeCycle.OnFixedUpdate.AddListener(OnFixedUpdate);
        AttackSignal.AddListener(OnUnitAttack);
        ShotReachTargetSignal.AddListener(OnShotReachTarget);
        SpawnUnitCompleteSignal.AddListener(OnSpawnUnitComplete);
        ProductionCompleteSignal.AddListener(OnProductionComplete);
        UISignals.PauseSignal.AddListener(OnPause);

        statePlaceBase = GameManagerFSM.AddState("PlaceBase", OnEnterPlaceBase, OnExitPlaceBase);
        stateDefault = GameManagerFSM.AddState("Default", OnEnterDefaultSelectCell, null);
        statePause = GameManagerFSM.AddState("Pause", OnEnterPause, OnExitPause);
        stateBuild = GameManagerFSM.AddState("Build", OnEnterBuild, OnExitBuild);
        stateProduction = GameManagerFSM.AddState("Production", OnEnterProduction, OnExitProduction);
        stateProductionSelectTarget = GameManagerFSM.AddState("ProductionTarget", OnEnterProductionSelectTarget, OnExitProductionSelectTarget);

        // Events
        statePlaceBase.OnBackPressed.AddListener(OnDefaultBackPressed);
        stateDefault.OnCellSelected.AddListener(OnDefaultSelectCell);
        stateDefault.OnBackPressed.AddListener(OnDefaultBackPressed);
        statePause.OnBackPressed.AddListener(OnPauseBackPressed);
        stateBuild.OnCellSelected.AddListener(OnDefaultSelectCell);
        stateBuild.OnBackPressed.AddListener(OnBuildBackPressed);
        stateProduction.OnCellSelected.AddListener(OnDefaultSelectCell);
        stateProduction.OnBackPressed.AddListener(OnBuildBackPressed);
        stateProductionSelectTarget.OnCellSelected.AddListener(OnProductionSelectTarget);
        stateProductionSelectTarget.OnBackPressed.AddListener(OnProductionSelectTargetBackPressed);

        // Transitions
        statePlaceBase.AddTransition(stateDefault);
        stateDefault.AddTransition(stateProduction);
        stateDefault.AddTransition(stateBuild);
        stateDefault.AddTransition(statePause);
        statePause.AddTransition(stateDefault);
        stateProduction.AddTransition(stateDefault);
        stateProduction.AddTransition(stateProduction);
        stateProduction.AddTransition(stateBuild);
        stateProduction.AddTransition(stateProductionSelectTarget);
        stateProductionSelectTarget.AddTransition(stateProduction);
        stateBuild.AddTransition(stateDefault);
        stateBuild.AddTransition(stateBuild);
        stateBuild.AddTransition(stateProduction);
    }

    public void Init()
    {
        GameManagerFSM.ChangeState(statePlaceBase);

        // Debug
        UIManager.CreateUI<HexMapEditorView>(UIManager.Parent, "HexMapEditor").Open();
        UIManager.CreateUI<TopMenuView>(UIManager.Parent, "TopMenu").Open();
    }

    public BaseUnitView Get_UnitView(int id)
    {
        return listUnitViews[id] ?? null;
    }

    private void OnInitGameComplete()
    {
        Init();
    }

    private BaseShotView Get_ShotView(int id)
    {
        return listShotViews[id] ?? null;
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
        GameManagerFSM.CellSelected(cell);
    }

    private void OnEnterPlaceBase()
    {
        for (int x = 0; x < World.Map.CellCountX; x++)
        {
            for (int z = 0; z < World.Map.CellCountZ; z++)
            {
                HexCoordinates coordinates = HexCoordinates.FromOffsetCoordinates(x, z);
                HexMapCell cell = World.Map.GetCell(coordinates);
                if (cell != null)
                {
                    if (cell.SpawnType != HexMapCell.SpawnCellType.None)
                    {
                        IFraction fraction = FractionManager.FindFraction(cell.SpawnType);
                        if (fraction != null)
                        {
                            GridUtility.ChangeCellFractionHighlight(coordinates, fraction.Type, true);
                            cell.ChangeFractionControl(fraction.Type, true);
                        }
                    }
                }
            }
        }

        // Two draggable bases for hot seat
        // Remove second base for multipleyer
        UnitScheme unitScheme = UnitSchemeManager.FindUnitScheme("UnitFort");
        List<UI.ItemContext> context = new List<UI.ItemContext>();
        context.Add(new UI.ItemContext(() => { OnBeginDraggingUnit(FractionType.Local, unitScheme, OnSpawnDraggingUnitSingleton); }, ResourceManager.GetSprite(unitScheme.IconName), ResourceManager.GetSprite(unitScheme.IconHighlightName)));
        context.Add(new UI.ItemContext(() => { OnBeginDraggingUnit(FractionType.Remote, unitScheme, OnSpawnDraggingUnitSingleton); }, ResourceManager.GetSprite(unitScheme.IconName), ResourceManager.GetSprite(unitScheme.IconHighlightName)));
        ContextMenuManager.UpdateUnits(context, true);

        List<UI.ItemContext> buttonContext = new List<UI.ItemContext>();
        buttonContext.Add(new UI.ItemContext(OnConfirmPlaceBase, ResourceManager.GetSprite("Accept"), ResourceManager.GetSprite("Accept_Gray")));
        ContextMenuManager.UpdateButtons(buttonContext);
    }

    private void OnBeginDraggingUnit(FractionType fraction, UnitScheme scheme, Action<HexGridCell> onPlaceDraggingUnit)
    {
        UISignals.DragSignal.AddListener(OnDraggingUnit);
        UISignals.DragEndSignal.AddOnce(OnEndDraggingUnit);

        GameManagerFSM.DragSelectedCell = null;
        GameManagerFSM.DragUnitScheme = scheme;
        GameManagerFSM.DragUnitFraction = fraction;
        GameManagerFSM.DragUnitView = ViewBinder.GetInstance<BaseUnitView>(scheme.ViewPrefab);
        GameManagerFSM.DragUnitView.Init(null, Vector3.zero, FractionManager[FractionType.None].Color);
        GameManagerFSM.DragUnitView.gameObject.SetActive(false); // Prevent to show model outside map
        GameManagerFSM.DragUnitView.ViewState = UnitViewStates.Highlight;
        GameManagerFSM.OnPlaceDraggingUnit = onPlaceDraggingUnit;
        SetInteractable(false);
    }

    private void OnDraggingUnit(ITouchData touch)
    {
        // Place base template
        if (GameManagerFSM.DragUnitView != null)
        {
            Vector2 touchPosition = touch.Position;
            touchPosition += fingerScreenOffset;
            //Logger.LogFormat("Dragging touch: {0}, dpi = {1}", touch.Position, Screen.dpi);

            HexGridCell cell = GridUtility.GetCellAtScreenPoint(touchPosition);
            if (cell != null)
            {
                if (!GameManagerFSM.DragUnitView.gameObject.activeSelf)
                {
                    GameManagerFSM.DragUnitView.gameObject.SetActive(true);
                }

                bool isGround = !cell.MapCell.IsWater();
                bool isBuildable = GameManagerFSM.DragUnitScheme.Type == UnitType.Vehicle ? !isGround : isGround;
                bool inRange = cell.MapCell.IsFractionControl(GameManagerFSM.DragUnitFraction);

                // Allow water base
                if (GameManagerFSM.DragUnitScheme.Type == UnitType.Base)
                {
                    isBuildable = true;
                }

                if (isBuildable)
                {
                    isBuildable = World.FindUnit(cell.MapCell.Coordinates) == null;
                }

                GameManagerFSM.DragSelectedCell = cell;
                GameManagerFSM.DragUnitView.PlaceAt(HexMetrics.Perturb(cell.Position), SelectDefaultUnitDirection(GameManagerFSM.DragUnitFraction));
                GameManagerFSM.DragUnitView.HighlightColor = isBuildable && inRange ? new Color(0.0f, 1.0f, 0.0f, 0.5f) : new Color(1.0f, 0.0f, 0.0f, 0.5f);
            }
        }
    }

    private void OnEndDraggingUnit(ITouchData touch)
    {
        UISignals.DragSignal.RemoveListener(OnDraggingUnit);

        if (GameManagerFSM.DragUnitView != null)
        {
            ViewBinder.ReturnInstance(GameManagerFSM.DragUnitScheme.ViewPrefab, GameManagerFSM.DragUnitView);
            GameManagerFSM.DragUnitView.gameObject.SetActive(false);
            GameManagerFSM.DragUnitView = null;
        }

        // Check cell for local base placement
        Vector2 touchPosition = touch.Position;
        touchPosition += fingerScreenOffset;
        HexGridCell cell = GridUtility.GetCellAtScreenPoint(touchPosition);
        if (cell == null)
        {
            cell = GameManagerFSM.DragSelectedCell;
        }
        if (cell != null)
        {
            if (cell.MapCell.IsFractionControl(GameManagerFSM.DragUnitFraction))
            {
                bool isGround = !cell.MapCell.IsWater();
               
                bool isBuildable = GameManagerFSM.DragUnitScheme.Type == UnitType.Vehicle ? !isGround : isGround;
                // Allow water base
                if (GameManagerFSM.DragUnitScheme.Type == UnitType.Base)
                {
                    isBuildable = true;
                }
                if (isBuildable)
                {
                    if (GameManagerFSM.OnPlaceDraggingUnit != null)
                    {
                        GameManagerFSM.OnPlaceDraggingUnit.Invoke(cell);
                    }
                }
            }
        }

        GameManagerFSM.DragSelectedCell = null;
        GameManagerFSM.DragUnitScheme = null;
        GameManagerFSM.DragUnitFraction = FractionType.None;
        GameManagerFSM.OnPlaceDraggingUnit = null;
        SetInteractable(true);
    }

    private void OnSpawnDraggingUnitSingleton(HexGridCell cell)
    {
        List<IUnit> units = new List<IUnit>();
        World.FindAllUnits(GameManagerFSM.DragUnitFraction, units);
        if (units.Count > 0)
        {
            // Just reposition
            units[0].PlaceAt(cell.MapCell.Coordinates, SelectDefaultUnitDirection(GameManagerFSM.DragUnitFraction));
        }
        else
        {
            SpawnUnit(GameManagerFSM.DragUnitFraction, GameManagerFSM.DragUnitScheme.Name, cell, false);
        }
    }

    private void OnSpawnDraggingUnit(HexGridCell cell)
    {
        if (World.FindUnit(cell.MapCell.Coordinates) == null)
        {
            SpawnUnit(GameManagerFSM.DragUnitFraction, GameManagerFSM.DragUnitScheme.Name, cell, false);
        }
    }

    private void OnConfirmPlaceBase()
    {
        IUnit localBase = World.FindUnit((unit) => { return unit.Fraction == FractionType.Local; });
        IUnit remoteBase = World.FindUnit((unit) => { return unit.Fraction == FractionType.Remote; });

        if ((localBase != null) && (remoteBase != null))
        {
            World.ChangeUnitFractionControl(localBase, true);
            World.ChangeUnitFractionControl(remoteBase, true);

            List<IAStarCell> path = new List<IAStarCell>();
            HexMapCell cellLocalBase = World.Map.GetCell(localBase.Coordinates);
            HexMapCell cellRemoteBase = World.Map.GetCell(remoteBase.Coordinates);
            World.FindAirPath(cellLocalBase, cellRemoteBase, path);
            if (path.Count > 0)
            {
                FractionManager[localBase.Fraction].DefaultUnitDirection = cellLocalBase.Coordinates.DirectionTo(path[1].Coordinates);
                FractionManager[remoteBase.Fraction].DefaultUnitDirection = cellRemoteBase.Coordinates.DirectionTo(path[path.Count - 2].Coordinates);
            }
            GameManagerFSM.ChangeState(stateDefault);
        }
    }

    private void OnExitPlaceBase()
    {
        for (int x = 0; x < World.Map.CellCountX; x++)
        {
            for (int z = 0; z < World.Map.CellCountZ; z++)
            {
                HexCoordinates coordinates = HexCoordinates.FromOffsetCoordinates(x, z);
                HexMapCell cell = World.Map.GetCell(coordinates);
                if (cell != null)
                {
                    if (cell.SpawnType != HexMapCell.SpawnCellType.None)
                    {
                        IFraction fraction = FractionManager.FindFraction(cell.SpawnType);
                        if (fraction != null)
                        {
                            GridUtility.ChangeCellFractionHighlight(coordinates, fraction.Type, false);
                            cell.ChangeFractionControl(fraction.Type, false);
                        }
                    }
                }
            }
        }

        CloseBottomPanel();
    }

    private void CloseBottomPanel()
    {
        ContextMenuManager.CloseUnitContainer();
        ContextMenuManager.CloseButtonContainer();
    }

    private void OnEnterDefaultSelectCell()
    {
        CloseBottomPanel();
    }

    #region PauseState

    private void OnPause(bool bPause)
    {
        if(bPause)
        {
            GameManagerFSM.PushState(statePause);
        }
        else
        {
            GameManagerFSM.PopState();
        }
    }

    private void OnEnterPause()
    {
        SetInteractable(false);
    }

    private void OnExitPause()
    {
        SetInteractable(true);
    }

    private void OnPauseBackPressed()
    {
        UIManager.OnBackPressed();
    }

    #endregion

    private void OnEnterBuild()
    {
        // Open build bottom panel
        if (GameManagerFSM.SelectedUnit != null)
        {
            GridUtility.ChangeFractionHighlight(GameManagerFSM.SelectedUnit.Fraction, true);

            List<UnitScheme> unitSchemes = new List<UnitScheme>();
            UnitSchemeManager.FindUnitSchemes(GameManagerFSM.SelectedUnit.ProductionComponent.Settings.ProductionType, unitSchemes);
            List<UI.ItemContext> context = new List<UI.ItemContext>();
            for (int i = 0; i < unitSchemes.Count; i++)
            {
                UnitScheme unitScheme = unitSchemes[i];
                context.Add(new UI.ItemContext(() => { OnBeginDraggingUnit(GameManagerFSM.SelectedUnit.Fraction, unitScheme, OnSpawnDraggingUnit); },
                    ResourceManager.GetSprite(unitScheme.IconName), ResourceManager.GetSprite(unitScheme.IconHighlightName)));
            }
            ContextMenuManager.UpdateUnits(context, true);
            ContextMenuManager.CloseButtonContainer();
        }
    }

    private void OnExitBuild()
    {
        GridUtility.ChangeFractionHighlight(GameManagerFSM.SelectedUnit.Fraction, false);
        CloseBottomPanel();
    }

    private void OnEnterProduction()
    {
        // Open production bottom panel
        if (GameManagerFSM.SelectedUnit != null)
        {
            IProductionComponent production = GameManagerFSM.SelectedUnit.ProductionComponent;
            ChangePathHighlight(GameManagerFSM.SelectedUnit.ProductionComponent.TargetPath, true);

            List<UI.ItemContext> context = new List<UI.ItemContext>();
            List<UnitScheme> list = new List<UnitScheme>();
            UnitSchemeManager.FindUnitSchemes(production.Settings.ProductionType, list);
            for (int i = 0; i < list.Count; i++)
            {
                UnitScheme unitScheme = list[i];
                context.Add(new UI.ItemContext(() => { OnProductionUnit(production, unitScheme); },
                    ResourceManager.GetSprite(unitScheme.IconName), ResourceManager.GetSprite(unitScheme.IconHighlightName)));
            }
            ContextMenuManager.UpdateUnits(context, false);

            List<UI.ItemContext> buttonContext = new List<UI.ItemContext>();
            buttonContext.Add(new UI.ItemContext(() => { GameManagerFSM.ChangeState(stateProductionSelectTarget); }, ResourceManager.GetSprite("Target"), ResourceManager.GetSprite("Target_Gray")));
            ContextMenuManager.UpdateButtons(buttonContext);
        }
    }

    private void OnProductionUnit(IProductionComponent production, UnitScheme unitScheme)
    {
        production.AddToQueue(unitScheme);
    }

    private void OnExitProduction()
    {
        ChangePathHighlight(GameManagerFSM.SelectedUnit.ProductionComponent.TargetPath, false);
    }

    private void OnEnterProductionSelectTarget()
    {
        // Open production select target bottom panel
        if (GameManagerFSM.SelectedUnit != null)
        {
            IProductionComponent production = GameManagerFSM.SelectedUnit.ProductionComponent;
            IAStarCell unitCell = World.Map.GetCell(GameManagerFSM.SelectedUnit.Coordinates);
            GameManagerFSM.SelectedPathSections.Fill(unitCell, production.TargetPath, production.TargetPathSections);
            for (int i = 0; i < GameManagerFSM.SelectedPathSections.Count - 1; i++)
            {
                List<IAStarCell> section = GameManagerFSM.SelectedPathSections[i];
                DraggableItemView view = InstanceDraggablePoint(section[section.Count - 1], i);
                GameManagerFSM.DraggablePoints.Add(view);
            }
            CameraEndMoveSignal.AddListener(OnCameraEndMoveUpdateUI);
            CameraPanSignal.AddListener(OnCameraChangeUpdateUI);
            CameraRotateSignal.AddListener(OnCameraChangeUpdateUI);
            CameraZoomSignal.AddListener(OnCameraChangeUpdateUI);

            ChangePathHighlight(GameManagerFSM.SelectedPathSections, true);

            List<UI.ItemContext> context = new List<UI.ItemContext>();
            ContextMenuManager.UpdateUnits(context, false);

            List<UI.ItemContext> buttonContext = new List<UI.ItemContext>();
            buttonContext.Add(new UI.ItemContext(ConfirmProductionTarget, ResourceManager.GetSprite("Accept"), ResourceManager.GetSprite("Accept_Gray")));
            buttonContext.Add(new UI.ItemContext(CancelProductionTarget, ResourceManager.GetSprite("Cancel"), ResourceManager.GetSprite("Cancel_Gray")));
            ContextMenuManager.UpdateButtons(buttonContext);
        }
    }

    private void OnExitProductionSelectTarget()
    {
        ChangePathHighlight(GameManagerFSM.SelectedPathSections, false);
        GameManagerFSM.SelectedPathSections.Clear();
        for (int i = 0; i < GameManagerFSM.DraggablePoints.Count; i++)
        {
            DraggableItemView view = GameManagerFSM.DraggablePoints[i];
            RemoveDraggablePoint(view);
        }
        GameManagerFSM.DraggablePoints.Clear();

        CameraEndMoveSignal.RemoveListener(OnCameraEndMoveUpdateUI);
        CameraPanSignal.RemoveListener(OnCameraChangeUpdateUI);
        CameraRotateSignal.RemoveListener(OnCameraChangeUpdateUI);
        CameraZoomSignal.RemoveListener(OnCameraChangeUpdateUI);
    }

    private void ConfirmProductionTarget()
    {
        GameManagerFSM.SelectedUnit.ProductionComponent.ChangeTargetPath(GameManagerFSM.SelectedPathSections);
        GameManagerFSM.ChangeState(stateProduction);
    }

    private void CancelProductionTarget()
    {
        GameManagerFSM.ChangeState(stateProduction);
    }

    private void ChangePathSectionHighlight(List<IAStarCell> section, bool isAuto, bool visible)
    {
        if (section.Count > 0)
        {
            int skipLastCell = isAuto ? 1 : 0;
            for (int i = 1; i < (section.Count - skipLastCell); i++)
            {
                GridUtility.ChangeCellHighlight(section[i].Coordinates, isAuto ? HexGridCell.HighlightType.AutoPath : HexGridCell.HighlightType.PlayerPath, visible);
            }
            if (!isAuto)
            {
                GridUtility.ChangeCellHighlight(section[section.Count - 1].Coordinates, HexGridCell.HighlightType.PathPoint, visible);
            }
        }
    }

    private void ChangePathHighlight(PathSections sections, bool visible)
    {
        if (sections.Count > 0)
        {
            for (int i = 0; i < sections.Count - 1; i++)
            {
                ChangePathSectionHighlight(sections[i], false, visible);
            }
            //if (sections.Count > 1)
            {
                ChangePathSectionHighlight(sections[sections.Count - 1], true, visible);
            }
        }
    }

    private void ChangePathHighlight(List<IAStarCell> path, bool visible)
    {
        for (int i = 0; i < path.Count - 1; i++)
        {
            GridUtility.ChangeCellHighlight(path[i].Coordinates, HexGridCell.HighlightType.PlayerPath, visible);
        }
    }

    private void OnCameraEndMoveUpdateUI()
    {
        OnCameraChangeUpdateUI(Camera.main);
    }

    private void OnCameraChangeUpdateUI(Camera camera)
    {
        for (int i = 0; i < GameManagerFSM.DraggablePoints.Count; i++)
        {
            DraggableItemView view = GameManagerFSM.DraggablePoints[i];
            DraggableItemContext context = (DraggableItemContext)view.Context;
            List<IAStarCell> section = GameManagerFSM.SelectedPathSections[context.PathSectionIndex];
            IAStarCell cell = section[section.Count - 1];
            view.UpdatePosition(HexMetrics.Perturb(HexMetrics.Position(cell.Coordinates, cell.Elevation)), camera);
        }
    }

    private void OnDefaultSelectCell(HexGridCell cell)
    {
        MoveToCell(cell);
        //OpenRadialMenu(cell);
    }

    private void MoveToCell(HexGridCell cell)
    {
        Logger.LogFormat("Tap to cell {0}", cell.MapCell.Coordinates);
        IUnit unit = World.FindUnit(cell.MapCell.Coordinates);
        if (unit != null)
        {
            OnSelectUnit(unit);
            return;
        }
        else
        {
            World.ChangeUnitSelection(false);
            GameManagerFSM.ChangeState(stateDefault);
            ContextMenuManager.CloseUnitContainer();
            //else
            //{
            //    OnOpenWorldContext();
            //}
        }
        //if (cell.MapCell.SpawnCell != FractionType.None || (unit != null))
        //{
        //    SetInteractable(false);
        //    //CameraEndMoveSignal.AddOnce(() => { OpenRadialMenu(cell); });
        //    //CameraMoveSignal.Dispatch(HexMetrics.Perturb(cell.Position));

        //    List<UI.ItemContext> context = new List<UI.ItemContext>();
        //    ContextMenuManager.UpdateUnits(context, false);

        //    OpenRadialMenu(cell);
        //}
    }

    private void OnSelectUnit(IUnit unit)
    {
        World.ChangeUnitSelection(false);
        unit.Selected = true;
        if (unit.ProductionComponent != null)
        {
            IGameManagerFSMState state = null;
            switch (unit.ProductionComponent.Settings.ProductionType)
            {
                case UnitType.Building:
                    state = stateBuild;
                    break;
                case UnitType.Vehicle:
                    state = stateProduction;
                    break;
            }
            if (state != null)
            {
                GameManagerFSM.ChangeState(state, () => { GameManagerFSM.SelectedUnit = unit; });
            }
        }
        else
        {
            // Refresh state
            GameManagerFSM.ChangeState(GameManagerFSM.State);
        }
    }

    //private void OnOpenFactoryContext(IFactoryUnit factoryUnit)
    //{
    //    //HardCode typeof & context will change;
    //    List<UI.ItemContext> context = new List<UI.ItemContext>();

    //    List<String> contextNames = UnitSettingsManager.GetUnitNames();

    //    for(int i = 0; i < contextNames.Count; i++)
    //    {
    //        //Spike for delegates;
    //        int j = i;
    //        context.Add(new UI.ItemContext(() => { factoryUnit.AddToQueue(typeof(Ship),UnitSettingsManager.GetUnitSettings<Ship,IUnitSettings>(contextNames[j])); }
    //    , ResourceManager.GetSprite("cruise"), ResourceManager.GetSprite("white_circle")));
    //    }
    //    ContextMenuManager.UpdateUnits(context, false);
    //    List<UI.ItemContext> buttonContext = new List<UI.ItemContext>();
    //    buttonContext.Add(new UI.ItemContext(null, ResourceManager.GetSprite("bullet"), ResourceManager.GetSprite("white_circle")));
    //    buttonContext.Add(new UI.ItemContext(null, ResourceManager.GetSprite("bullet"), ResourceManager.GetSprite("white_circle")));
    //    ContextMenuManager.UpdateButtons(buttonContext);
    //}

    //private void OnProductionSelectTarget(HexGridCell cell)
    //{
    //    if (GameManagerFSM.SelectedUnit != null)
    //    {
    //        ChangePathCells(selectedTargetPath, false);
    //        HexMapCell cellBegin = World.Map.GetCell(GameManagerFSM.SelectedUnit.Coordinates);
    //        World.FindWaterPath(cellBegin, cell.MapCell, selectedTargetPath);
    //        if (selectedTargetPath.Count > 0)
    //        {
    //            if (!selectedTargetPath[selectedTargetPath.Count - 1].IsWater())
    //            {
    //                selectedTargetPath.RemoveAt(selectedTargetPath.Count - 1);
    //            }
    //        }
    //        ChangePathCells(selectedTargetPath, true);
    //    }
    //}

    private void OnProductionSelectTarget(HexGridCell cell)
    {
        if (GameManagerFSM.SelectedUnit != null)
        {
            if (GameManagerFSM.SelectedPathSections.IndexOf(cell.MapCell) == -1)
            {
                ChangePathHighlight(GameManagerFSM.SelectedPathSections, false);

                IAStarCell cellEnemyBase = null;
                if (GameManagerFSM.SelectedPathSections.Count > 0)
                {
                    cellEnemyBase = GameManagerFSM.SelectedPathSections.Last[GameManagerFSM.SelectedPathSections.Last.Count - 1];
                    // Remove last section (path to enemy base).
                    GameManagerFSM.SelectedPathSections.RemoveLast();
                }

                IUnit enemyBase = World.FindUnit((unit) => { return unit.IsEnemy(GameManagerFSM.SelectedUnit) && unit.Settings.Type == UnitType.Base; });
                if (enemyBase != null)
                {
                    cellEnemyBase = World.Map.GetCell(enemyBase.Coordinates);
                }
                List<IAStarCell> lastSection = GameManagerFSM.SelectedPathSections.Last;
                IAStarCell cellLast = lastSection != null ? lastSection[lastSection.Count - 1] : World.Map.GetCell(GameManagerFSM.SelectedUnit.Coordinates);
                IAStarCell cellSelected = cell.MapCell;

                if ((cellLast != null) && (cellSelected != null))
                {
                    List<IAStarCell> path = new List<IAStarCell>();
                    World.FindPath(cellLast, cellSelected, path, CheckCellWalkable);
                    if (path.Count > 0)
                    {
                        GameManagerFSM.SelectedPathSections.Add(path);

                        DraggableItemView view = InstanceDraggablePoint(cell.MapCell, GameManagerFSM.SelectedPathSections.Count - 1);
                        GameManagerFSM.DraggablePoints.Add(view);
                    }
                }
                if ((cellSelected != null) && (cellEnemyBase != null))
                {
                    List<IAStarCell> path = new List<IAStarCell>();
                    World.FindPath(cellSelected, cellEnemyBase, path, CheckCellWalkable);
                    if (path.Count > 0)
                    {
                        //path.RemoveAt(path.Count - 1);
                        GameManagerFSM.SelectedPathSections.Add(path);
                    }
                }
                ChangePathHighlight(GameManagerFSM.SelectedPathSections, true);
            }
        }
    }

    private DraggableItemView InstanceDraggablePoint(IAStarCell cell, int pathSectionIndex)
    {
        Vector3 worldPosition = HexMetrics.Perturb(HexMetrics.Position(cell.Coordinates, cell.Elevation));
        DraggableItemView view = UIManager.CreateUI<DraggableItemView>(UIManager.Parent, "DraggableItem");
        view.Init(new DraggableItemContext() { PathSectionIndex = pathSectionIndex, BaseSprite = ResourceManager.GetSprite("Cancel_Gray"), HighlightedSprite = ResourceManager.GetSprite("Accept"), DisableSprite = ResourceManager.GetSprite("Cancel") });
        view.Open();
        view.OnDragAction += OnDragPoint;
        view.OnEndDragAction += OnEndDragPoint;
        view.UpdatePosition(worldPosition);
        return view;
    }

    private void RemoveDraggablePoint(DraggableItemView view)
    {
        view.OnDragAction -= OnDragPoint;
        view.OnEndDragAction -= OnEndDragPoint;
        view.Close();
    }

    private void OnDragPoint(DraggableItemView item, Vector2 screenPosition)
    {
        DraggableItemContext itemContext = (DraggableItemContext)item.Context;

        HexGridCell cell = GridUtility.GetCellAtScreenPoint(screenPosition);
        if (cell != null)
        {
            List<IAStarCell> section = GameManagerFSM.SelectedPathSections[itemContext.PathSectionIndex];
            IAStarCell pointCell = section[section.Count - 1];

            // Check dragging to new cell
            if (cell.MapCell.Coordinates != pointCell.Coordinates)
            {

                // PathSectionIndex < GameManagerFSM.SelectedPathSections.Count always.
                List<IAStarCell> nextSection = GameManagerFSM.SelectedPathSections[itemContext.PathSectionIndex + 1];
                IAStarCell prevCell = section[0];
                IAStarCell nextCell = nextSection[nextSection.Count - 1];
                List<IAStarCell> newSection = new List<IAStarCell>();

                if ((cell.MapCell.Coordinates == prevCell.Coordinates) || (cell.MapCell.Coordinates == nextCell.Coordinates))
                {
                    // Cell is next or previous cell
                    item.State = ItemView.ItemState.DISABLED;
                }
                else
                {
                    item.State = ItemView.ItemState.HIGHLIGHTED;
                }

                ChangePathHighlight(GameManagerFSM.SelectedPathSections, false);

                World.FindPath(prevCell, cell.MapCell, newSection, CheckCellWalkable);
                GameManagerFSM.SelectedPathSections.ReplaceAt(itemContext.PathSectionIndex, newSection);
                World.FindPath(cell.MapCell, nextCell, newSection, CheckCellWalkable);
                GameManagerFSM.SelectedPathSections.ReplaceAt(itemContext.PathSectionIndex + 1, newSection);

                ChangePathHighlight(GameManagerFSM.SelectedPathSections, true);
            }
            item.ScreenPosition = screenPosition;
        }
    }

    private void OnEndDragPoint(DraggableItemView item, Vector2 screenPosition)
    {
        DraggableItemContext itemContext = (DraggableItemContext)item.Context;

        ChangePathHighlight(GameManagerFSM.SelectedPathSections, false);
        // Refresh sections
        for (int i = GameManagerFSM.SelectedPathSections.Count; i > 0; i--)
        {
            int index = i - 1;
            List<IAStarCell> section = GameManagerFSM.SelectedPathSections[index];
            if (section.Count < 2)
            {
                GameManagerFSM.SelectedPathSections.RemoveAt(index);
            }
        }
        ChangePathHighlight(GameManagerFSM.SelectedPathSections, true);

        // Refresh drag points
        for (int i = 0; i < GameManagerFSM.DraggablePoints.Count; i++)
        {
            RemoveDraggablePoint(GameManagerFSM.DraggablePoints[i]);
        }
        GameManagerFSM.DraggablePoints.Clear();
        for (int i = 0; i < GameManagerFSM.SelectedPathSections.Count - 1; i++)
        {
            List<IAStarCell> section = GameManagerFSM.SelectedPathSections[i];
            DraggableItemView view = InstanceDraggablePoint(section[section.Count - 1], i);
            GameManagerFSM.DraggablePoints.Add(view);
        }
    }

    private void OnDefaultBackPressed()
    {
        UIManager.CreateUI<PauseMenuView>(UIManager.Parent, "PauseMenu").Open();
    }

    private void OnBuildBackPressed()
    {
        UIManager.CreateUI<PauseMenuView>(UIManager.Parent, "PauseMenu").Open();
    }

    private void OnProductionSelectTargetBackPressed()
    {
        CancelProductionTarget();
    }

    private void OnUpdate(float time)
    {
        World.Update(time);
    }

    private void OnFixedUpdate(float time)
    {
        World.UpdateAI(time);
    }

    private void OnBackPressed()
    {
        GameManagerFSM.BackPressed();
    }

    private void SetInteractable(bool enabled)
    {
        HexGridInteractableSignal.Dispatch(enabled);
        CameraLockSignal.Dispatch(!enabled);
    }

    private void DummyFunction()
    {
        Logger.Log("DummyFunction");
    }

    private void RemoveGroundInstance(IUnit unit)
    {
        unit.KillUnit();
    }

    private void SpawnUnit(FractionType fraction, string unitTypeName, HexGridCell cell, bool isCheckExistUnit = true)
    {
        if (isCheckExistUnit == true)
        {
            if (World.FindUnit(cell.MapCell.Coordinates) == null)
            {
                DispatchSpawnUnit(fraction, unitTypeName, cell.MapCell.Coordinates, SelectDefaultUnitDirection(fraction));
            }
        }
        else
        {
            DispatchSpawnUnit(fraction, unitTypeName, cell.MapCell.Coordinates, SelectDefaultUnitDirection(fraction));
        }
    }

    private BaseShotView SpawnShot(IUnit unit, IUnit target)
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
                BaseShotView view = ViewBinder.GetInstance<BaseShotView>(typeof(RoundShotView));
                view.Init(target.ID, viewUnit.transform.position, viewTarget.transform.position);
                listShotViews.Add(view.GetInstanceID(), view);
                return view;
            }
        }
        return null;
    }

    private void DispatchSpawnUnit(FractionType fraction, string schemeName, HexCoordinates coordinates, HexDirection direction)
    {
        SpawnUnit signal = Context.GetInstance<SpawnUnit>();
        //signal.Dispatch(new SpawnUnitArguments(unitType, fraction, settings, coordinates, direction));
        signal.Dispatch(fraction, schemeName, coordinates, direction);
    }

    private void DispatchSpawnUnit(IUnit factory, string schemeName, HexCoordinates coordinates, HexDirection direction)
    {
        ProductionSpawnUnit signal = Context.GetInstance<ProductionSpawnUnit>();
        signal.Dispatch(factory, schemeName, coordinates, direction);
    }

    private void OnSpawnUnitComplete(IUnit unit, BaseUnitView view)
    {
        unit.OnDeath.AddOnce(Unit_OnDestroyed);
        unit.OnDeath.AddOnce(OnRemoveDestroyedUnitView);
//        if (view.AddAttackRange)
//        {
//            unit.OnDeath.AddOnce(OnRemoveUnitAttackRange);
//            GridUtility.UpdateAttackRangeCoordinates(unit.Coordinates, unit.WeaponComponent.MaxAttackRange, true);
//        }
        listUnitViews.Add(unit.ID, view);

        OnSelectUnit(unit);
    }

    private void OnProductionComplete(IUnit factory, UnitScheme scheme, HexCoordinates coordinates)
    {
        DispatchSpawnUnit(factory, scheme.Name, coordinates, SelectDefaultUnitDirection(factory.Fraction));
    }

    private HexDirection SelectDefaultUnitDirection(FractionType fraction)
    {
        return FractionManager[fraction].DefaultUnitDirection;
    }

    private void Unit_OnDestroyed(IUnit unit)
    {
        BaseUnitView view = Get_UnitView(unit.ID);
        UnitScheme scheme = UnitSchemeManager.FindUnitScheme(unit.Settings.Scheme);
        ViewBinder.ReturnInstance(scheme.ViewPrefab, view);
    }

    private void OnRemoveDestroyedUnitView(IUnit unit)
    {
        listUnitViews.Remove(unit.ID);
    }

    private void OnShotReachTarget(int id)
    {
        BaseShotView view = Get_ShotView(id);
        listShotViews.Remove(id);
        ViewBinder.ReturnInstance(view.GetType(), view);
    }

    private void OnRemoveUnitAttackRange(IUnit unit)
    {
        HexMapCell cell = World.Map.GetCell(unit.Coordinates);
        if (cell != null)
        {
            UpdateAttackRangeCoordinates(cell.Coordinates, unit.WeaponComponent.MaxAttackRange, false);
        }
    }

    private void OnUnitAttack(IUnit unit, IUnit target, float damage)
    {
        //Logger.LogFormat("Unit ({0}:{1}) attack to ({2}:{3})", unit.Type, unit.ID, target.Type, target.ID);
        SpawnShot(unit, target);
    }

    private bool CheckCellWalkable(IAStarCell cell)
    {
        if (cell.IsWater())
        {
            // Exclude bases.
            IUnit notBase = World.FindUnit((unit) => { return (unit.Coordinates == cell.Coordinates) && (unit.Settings.Type == UnitType.Base); });
            return notBase == null;
        }
        return false;
    }

    private void UpdateAttackRangeCoordinates(HexCoordinates center, int range, bool visible)
    {
        for (int dx = -range + center.X; dx <= range + center.X; dx++)
        {
            for (int dy = -range + center.Y; dy <= range + center.Y; dy++)
            {
                for (int dz = -range + center.Z; dz <= range + center.Z; dz++)
                {
                    if (dx + dy + dz == 0)
                    {
                        GridUtility.ChangeCellHighlight(new HexCoordinates(dx, dz), HexGridCell.HighlightType.AttackRange, visible);
                    }
                }
            }
        }
    }
}