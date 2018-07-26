using GameWorld.Settings;
using GameWorld.Weapons;

namespace GameWorld
{
    namespace Units
    {
        /// <summary>
        /// Description of the instance ship
        /// </summary>
        public class Ship : NavalUnit
        {
            [Inject(typeof(ShipSettings))]
            public override INavalUnitSettings NavalSettings { get; protected set; }
            [Inject(typeof(WeaponSmallCannon))]
            public override IWeapon Weapon { get; protected set; }

            public Ship() : base(UnitType.Ship) { }
        }
    }
}
