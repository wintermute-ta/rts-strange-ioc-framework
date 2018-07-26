using GameWorld.Settings;

namespace GameWorld
{
    namespace Weapons
    {
        public class WeaponSmallCannon : Weapon
        {
            [Inject(typeof(SmallCannonSettings))]
            public override IWeaponSettings Settings { get; protected set; }
        }
    }
}
