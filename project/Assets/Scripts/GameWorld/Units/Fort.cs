using GameWorld.Settings;
using GameWorld.Weapons;

namespace GameWorld
{
    namespace Units
    {
        public class Fort : GroundUnit
        {
            [Inject(typeof(FortSettings))]
            public override IUnitSettings Settings { get; protected set; }
            [Inject(typeof(WeaponLargeCannon))]
            public override IWeapon Weapon { get; protected set; }

            public Fort() : base(UnitType.Fort)
            {
            }
        }
    }
}
