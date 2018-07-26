using GameWorld.Settings;
using GameWorld.Units;
using System.Collections.Generic;

namespace GameWorld
{
    namespace Weapons
    {
        public interface IWeapon
        {
            int UnitID { get; set; }
            IWeaponSettings Settings { get; }
            List<IUnit> LockedTargets { get; }
            IUnit CurrentTarget { get; }

            void Update(float deltaTime, IUnit unit, List<IUnit> targets);
            void Cleanup();
        }
    }
}
