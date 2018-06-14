using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAStarCell
{
    int AStarX { get; }
    int AStarZ { get; }
    HexCoordinates AStarCoordinates { get; }
    //bool IsPathCell { get; set; }

    bool IsWalkable();
    float MovementCost();
    IAStarCell GetAStarNeighbor(HexDirection direction);
}
