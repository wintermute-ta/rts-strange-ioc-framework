using GameWorld.Settings;
using UnityEngine;

namespace Views
{
    namespace Settings
    {
        public class WeaponSettingsView : MonoBehaviour
        {
            [Inject(typeof(SmallCannonSettings))]
            public IWeaponSettings SmallCannonSettings { get; private set; }
            [Inject(typeof(MediumCannonSettings))]
            public IWeaponSettings MediumCannonSettings { get; private set; }
            [Inject(typeof(LargeCannonSettings))]
            public IWeaponSettings LargeCannonSettings { get; private set; }
        }
    }
}
