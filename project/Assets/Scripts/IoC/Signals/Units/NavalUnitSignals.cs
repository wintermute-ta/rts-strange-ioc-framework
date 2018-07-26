using GameWorld.HexMap;
using GameWorld.Units;
using strange.extensions.signal.impl;
using UnityEngine;

namespace Signals
{
    namespace Units
    {
        namespace NavalUnit
        {
            public class MoveToCell : Signal<INavalUnit, HexCoordinates> { };
            public class ReachTarget : Signal<INavalUnit> { };
            public class DestinationChanged : Signal<INavalUnit, IUnit> { }
            public class ViewPositionChanged : Signal<IUnit, Vector3> { };
        }
    }
}