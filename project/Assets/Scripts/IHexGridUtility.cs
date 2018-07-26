using GameWorld.HexMap;
using Views.HexGrid;

public interface IHexGridUtility
{
    HexGridView CreateInstance();
    int CellCountX { get; }
    int CellCountZ { get; }
    HexGridCell GetCell(HexCoordinates coordinates);
    void UpdateAttackRangeCoordinates(HexCoordinates center, int range, bool visible);
}
