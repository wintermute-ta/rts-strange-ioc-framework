using GameWorld.HexMap;
using System.Collections.Generic;

namespace GameWorld
{
    namespace Units
    {
        public interface INavalUnit : IUnit
        {
            IUnit DestinationUnit { get; }
            List<IAStarCell> Path { get; }
        }
    }
}
