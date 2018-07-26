using GameWorld.Settings;

namespace GameWorld
{
    namespace Weapons
    {
        public class WeaponMediumCannon : Weapon
        {
            [Inject(typeof(MediumCannonSettings))]
            public override IWeaponSettings Settings { get; protected set; }
        }
    }
}
