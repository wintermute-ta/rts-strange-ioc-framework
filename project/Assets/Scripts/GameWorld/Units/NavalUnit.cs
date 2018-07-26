using GameWorld.HexMap;
using GameWorld.Settings;
using Signals.Units.NavalUnit;
using System.Collections.Generic;

namespace GameWorld
{
    namespace Units
    {
        public class NavalUnit : Unit, INavalUnit
        {
            static UnitType[] _targetTypes = new UnitType[] { UnitType.Fort, UnitType.GroundCannon };

            [Inject]
            public MoveToCell MoveToCellSignal { get; private set; }
            [Inject]
            public ReachTarget ReachTargetSignal { get; private set; }
            [Inject]
            public DestinationChanged DestinationChangedSignal { get; private set; }

            public IUnit DestinationUnit { get; private set; }
            public List<IAStarCell> Path { get; private set; }
            public override IUnitSettings Settings { get { return NavalSettings; } }
            public virtual INavalUnitSettings NavalSettings { get; protected set; }

            private bool isMove = false;
            private int indexPath;
            private float timeWait = 0f;

            public NavalUnit(UnitType type) : base(type, _targetTypes)
            {
                Path = new List<IAStarCell>();
            }

            public override void PostContruct()
            {
                base.PostContruct();

                DestroySignal.AddListener(OnDestroyUnit);
            }

            public override void UpdateAI(float deltaTime)
            {
                base.UpdateAI(deltaTime);

                IUnit newDestination = FindUnitOfType(PossibleTargets, UnitType.Fort);
                if (newDestination != DestinationUnit)
                {
                    Path.Clear();
                    DestinationUnit = newDestination;
                    DestinationChangedSignal.Dispatch(this, DestinationUnit);
                    if (Path.Count > 0)
                    {
                        BeginMoving(Path);
                    }
                }
                else if (Path != null && Path.Count > 0)
                {
                    timeWait += deltaTime;
                    if (timeWait >= NavalSettings.Speed)
                    {
                        timeWait = 0f;
                        if (isMove == false)
                        {
                            EndMovingToCell();
                        }
                    }
                }
            }

            private IUnit FindUnitOfType(List<IUnit> targets, UnitType type)
            {
                for (int i = 0; i < targets.Count; i++)
                {
                    if (targets[i].Type == type)
                    {
                        return targets[i];
                    }
                }
                return null;
            }

            public void BeginMoving(List<IAStarCell> pathTarget)
            {
                Path = pathTarget;
                indexPath = 0;
                IUnit unit = World.GetUnit(Path[indexPath].AStarCoordinates);
                if (unit != null || Path[indexPath + 1].IsBusy() == true)
                {
                    isMove = false;
                    return;
                }
                isMove = true;
                MoveToCellSignal.Dispatch(this, Path[indexPath].AStarCoordinates);
                isMove = false;

            }

            private void EndMovingToCell()
            {
                Coordinates = Path[indexPath].AStarCoordinates;
                if ((indexPath + 1) < Path.Count)
                {
                    indexPath++;
                    IUnit unit = World.GetUnit(Path[indexPath].AStarCoordinates);
                    if (unit != null || Path[indexPath].IsBusy() == true)
                    {
                        isMove = false;
                        indexPath--;
                        return;
                    }
                    World.Map.GetCell(Path[indexPath].AStarCoordinates).IsBusy = true;
                    World.Map.GetCell(Path[indexPath - 1].AStarCoordinates).IsBusy = false;
                    isMove = true;
                    MoveToCellSignal.Dispatch(this, Path[indexPath].AStarCoordinates);
                    isMove = false;
                }
                else
                {
                    ReachTargetSignal.Dispatch(this);
                }
            }

            public override void Restore()
            {
                base.Restore();
                isMove = true;
                OnDestroyUnit(DestinationUnit);
                DestroySignal.RemoveListener(OnDestroyUnit);
                World.Map.GetCell(Coordinates).IsBusy = false;
            }

            private void OnDestroyUnit(IUnit unit)
            {
                if (DestinationUnit != null)
                {
                    if (DestinationUnit.ID == unit.ID)
                    {
                        DestinationUnit = null;
                        Path.Clear();
                    }
                }
            }
        }
    }
}
