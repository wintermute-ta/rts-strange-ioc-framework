using GameWorld.Units;
using UnityEngine;

namespace Views
{
    namespace Units
    {
        public class GroundCannonMediator : BaseUnitMediator<GroundCannonView>
        {
            protected override void OnUnitChangeTarget(IUnit unit, IUnit target)
            {
                if (IsViewUnit(unit))
                {
                    if (target != null)
                    {
                        Debug.LogFormat("Unit ({0}:{1}) start tracking target ({2}:{3})", unit.Type, unit.ID, target.Type, target.ID);
                        View.TrackTarget(GameManager.Get_UnitView(target.ID));
                    }
                    else
                    {
                        Debug.LogFormat("Unit ({0}:{1}) stop tracking his target", unit.Type, unit.ID);
                        View.StopTracking();
                    }
                }
            }
        }
    }
}
