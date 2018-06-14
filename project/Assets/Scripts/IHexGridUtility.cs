using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHexGridUtility
{
    HexGridView CreateInstance();
    int CellCountX { get; }
    int CellCountZ { get; }
    HexGridCell GetCell(HexCoordinates coordinates);
    void UpdateAttackRangeCoordinates(HexCoordinates center, int range, bool visible);
}
