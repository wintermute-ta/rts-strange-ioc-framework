using GameWorld.Settings;
using GameWorld.Weapons;

namespace GameWorld
{
    namespace Units
    {
        /// <summary>
        /// Description of the instance gun
        /// </summary>
        public class GroundCannon : GroundUnit
        {
            [Inject(typeof(GroundCannonSettings))]
            public override IUnitSettings Settings { get; protected set; }
            [Inject(typeof(WeaponMediumCannon))]
            public override IWeapon Weapon { get; protected set; }

            public GroundCannon() : base(UnitType.GroundCannon)
            {
            }
        }
    }
}
