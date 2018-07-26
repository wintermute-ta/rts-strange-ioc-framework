using GameWorld.Settings;
using UnityEngine;

namespace Views
{
    namespace Settings
    {
        public class UnitSettingsView : MonoBehaviour
        {
            [Inject(typeof(ShipSettings))]
            public INavalUnitSettings ShipSettings { get; private set; }
            [Inject(typeof(GroundCannonSettings))]
            public IGroundUnitSettings GroundCannonSettings { get; private set; }
            [Inject(typeof(FortSettings))]
            public IGroundUnitSettings FortSettings { get; private set; }
        }
    }
}
