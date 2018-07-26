using GameWorld.Units;
using strange.extensions.signal.impl;

namespace Signals
{
    namespace Units
    {
        public class HitDamage : Signal<IUnit, float> { };
        public class Destroy : Signal<IUnit> { };
        public class Attack : Signal<IUnit, IUnit, float> { }
        public class ChangeTarget : Signal<IUnit, IUnit> { }
    }
}