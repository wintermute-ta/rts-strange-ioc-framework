using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameWorld
{
    namespace HexMap
    {
        public interface IAStarCell
        {
            int AStarX { get; }
            int AStarZ { get; }
            HexCoordinates AStarCoordinates { get; }
            //bool IsPathCell { get; set; }

            bool IsWalkable();
            bool IsBusy();
            float MovementCost();
            IAStarCell GetAStarNeighbor(HexDirection direction);
        }
    }
}
