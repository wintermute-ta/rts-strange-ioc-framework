using GameWorld.Units;
using GameWorld.Weapons;
using strange.extensions.signal.impl;

namespace Signals
{
    namespace Units
    {
        namespace Weapon
        {
            public class Fire : Signal<IWeapon, IUnit, float> { }
            public class ChangeTarget : Signal<IWeapon, IUnit> { }
            public class ShotReachTarget : Signal<int> { }
        }
    }
}