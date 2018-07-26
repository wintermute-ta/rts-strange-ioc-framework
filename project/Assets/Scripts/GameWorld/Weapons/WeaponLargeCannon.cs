using GameWorld.Settings;

namespace GameWorld
{
    namespace Weapons
    {
        public class WeaponLargeCannon : Weapon
        {
            [Inject(typeof(LargeCannonSettings))]
            public override IWeaponSettings Settings { get; protected set; }
        }
    }
}
