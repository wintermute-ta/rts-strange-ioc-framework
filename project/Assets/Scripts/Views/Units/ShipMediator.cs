using UnityEngine;
using Signals.Units.NavalUnit;
using GameWorld.Units;
using GameWorld.HexMap;
using Views.HexGrid;

namespace Views
{
    namespace Units
    {
        public class ShipMediator : BaseUnitMediator<ShipView>
        {
            [Inject]
            public IHexGridUtility GridUtility { get; private set; }
            [Inject]
            public MoveToCell MoveToCellSignal { get; private set; }
            [Inject]
            public ViewPositionChanged ViewPositionChangedSignal { get; private set; }

            public override void OnRegister()
            {
                base.OnRegister();
                View.OnChangePosition.AddListener(OnChangePosition);
                MoveToCellSignal.AddListener(OnMoveToCell);
            }

            //OnRemove() is like a destructor/OnDestroy. Use it to clean up.
            public override void OnRemove()
            {
                base.OnRemove();
                View.OnChangePosition.RemoveListener(OnChangePosition);
                MoveToCellSignal.RemoveListener(OnMoveToCell);
            }

            private void OnChangePosition(Vector3 position)
            {
                ViewPositionChangedSignal.Dispatch(View.Unit, position);
            }

            protected override void OnUnitChangeTarget(IUnit unit, IUnit target)
            {
                if (IsViewUnit(unit))
                {
                    if (target != null)
                    {
                        Debug.LogFormat("Unit ({0}:{1}) start tracking target ({2}:{3})", unit.Type, unit.ID, target.Type, target.ID);
                    }
                    else
                    {
                        Debug.LogFormat("Unit ({0}:{1}) stop tracking his target", unit.Type, unit.ID);
                    }
                }
            }

            private void OnMoveToCell(INavalUnit unit, HexCoordinates destination)
            {
                if (IsViewUnit(unit))
                {
                    HexGridCell cell = GridUtility.GetCell(unit.Coordinates);
                    if (cell != null)
                    {
                        cell.IsPathCell = false;
                    }

                    HexGridCell _offsetEnd = GridUtility.GetCell(destination);
                    Vector3 end = new Vector3(_offsetEnd.Position.x, 0f, _offsetEnd.Position.z);
                    end = HexMetrics.Perturb(end);
                    View.MoveTo(end);
                }
            }
        }
    }
}
