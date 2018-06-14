using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A special hexgrid-provider
public class HexGridUtility : IHexGridUtility
{
    [Inject]
    public IResourceManager ResourceManager { get; private set; }

    private HexGridView view;

    public HexGridView CreateInstance()
    {
        return view = GlobalContext.Get().InstancePrefabView<HexGridView>(ResourceManager.GetPrefab("HexGrid"));
    }

    public int CellCountX { get { return view.CellCountX; } }
    public int CellCountZ { get { return view.CellCountX; } }
    public HexGridCell GetCell(HexCoordinates coordinates)
    {
        return view.GetCell(coordinates);
    }

    public void UpdateAttackRangeCoordinates(HexCoordinates center, int range, bool visible)
    {
        view.UpdateAttackRangeCoordinates(center, range, visible);
    }
}
