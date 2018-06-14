using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class HexMapCell : IAStarCell
{
    public HexCoordinates Coordinates;
    public HexMapChunk Chunk;
    public HexMapCell[] Neighbors = new HexMapCell[6];
    public int OffsetX;
    public int OffsetZ;
    public bool HasIncomingRiver;
    public bool HasOutgoingRiver;
    public HexDirection IncomingRiver;
    public HexDirection OutgoingRiver;
    public bool[] Roads = new bool[6];

    public event Action OnCellElevationChanged = delegate { };

    /// <summary>
    /// The count of active ships for this Path
    /// </summary>

    private bool isSpawnCell;

    private int terrainTypeIndex;
    private int elevation = int.MinValue;
    private int waterLevel;
    private int urbanLevel;
    private int farmLevel;
    private int plantLevel;
    private int specialIndex;
    private bool walled;

    public bool IsSpawnCell
    {
        get { return isSpawnCell; }
        set
        {
            if (isSpawnCell != value)
            {
                isSpawnCell = value;
                RefreshSelfOnly();
            }
        }
    }
    public bool IsUnderwater
    {
        get { return WaterLevel > Elevation; }
    }
    public int Elevation
    {
        get { return elevation; }
        set
        {
            if (elevation == value)
            {
                return;
            }
            elevation = value;
            OnCellElevationChanged.Invoke();
            ValidateRivers();

            for (int i = 0; i < Roads.Length; i++)
            {
                if (Roads[i] && GetElevationDifference((HexDirection)i) > 1)
                {
                    SetRoad(i, false);
                }
            }
            Refresh();
        }
    }
    public int WaterLevel
    {
        get { return waterLevel; }
        set
        {
            if (waterLevel == value)
            {
                return;
            }
            waterLevel = value;
            ValidateRivers();
            Refresh();
        }
    }
   
    public bool HasRiver
    {
        get { return HasIncomingRiver || HasOutgoingRiver; }
    }
    public bool HasRiverBeginOrEnd
    {
        get { return HasIncomingRiver != HasOutgoingRiver; }
    }
    public HexDirection RiverBeginOrEndDirection
    {
        get { return HasIncomingRiver ? IncomingRiver : OutgoingRiver; }
    }
    public bool HasRoads
    {
        get
        {
            for (int i = 0; i < Roads.Length; i++)
            {
                if (Roads[i])
                {
                    return true;
                }
            }
            return false;
        }
    }
    public int SpecialIndex
    {
        get
        {
            return specialIndex;
        }
        set
        {
            if (specialIndex != value && !HasRiver)
            {
                specialIndex = value;
                RemoveRoads();
                RefreshSelfOnly();
            }
        }
    }
    public bool IsSpecial
    {
        get
        {
            return SpecialIndex > 0;
        }
    }
    public int UrbanLevel
    {
        get { return urbanLevel; }
        set
        {
            if (urbanLevel != value)
            {
                urbanLevel = value;
                RefreshSelfOnly();
            }
        }
    }
    public int FarmLevel
    {
        get { return farmLevel; }
        set
        {
            if (farmLevel != value)
            {
                farmLevel = value;
                RefreshSelfOnly();
            }
        }
    }
    public int PlantLevel
    {
        get { return plantLevel; }
        set
        {
            if (plantLevel != value)
            {
                plantLevel = value;
                RefreshSelfOnly();
            }
        }
    }
    public bool Walled
    {
        get { return walled; }
        set
        {
            if (walled != value)
            {
                walled = value;
                Refresh();
            }
        }
    }
    public int TerrainTypeIndex
    {
        get { return terrainTypeIndex; }
        set
        {
            if (terrainTypeIndex != value)
            {
                terrainTypeIndex = value;
                Refresh();
            }
        }
    }
    public HexMapCell GetNeighbor(HexDirection direction)
    {
        return Neighbors[(int)direction];
    }
    public void SetNeighbor(HexDirection direction, HexMapCell cell)
    {
        Neighbors[(int)direction] = cell;
        cell.Neighbors[(int)direction.Opposite()] = this;
    }
    public HexEdgeType GetEdgeType(HexDirection direction)
    {
        return HexMetrics.GetEdgeType(
            Elevation, Neighbors[(int)direction].Elevation
        );
    }
    public HexEdgeType GetEdgeType(HexMapCell otherCell)
    {
        return HexMetrics.GetEdgeType(
            Elevation, otherCell.Elevation
        );
    }
    public bool HasRiverThroughEdge(HexDirection direction)
    {
        return
            HasIncomingRiver && IncomingRiver == direction ||
            HasOutgoingRiver && OutgoingRiver == direction;
    }
    public void RemoveIncomingRiver()
    {
        if (!HasIncomingRiver)
        {
            return;
        }
        HasIncomingRiver = false;
        RefreshSelfOnly();

        HexMapCell neighbor = GetNeighbor(IncomingRiver);
        neighbor.HasOutgoingRiver = false;
        neighbor.RefreshSelfOnly();
    }
    public void RemoveOutgoingRiver()
    {
        if (!HasOutgoingRiver)
        {
            return;
        }
        HasOutgoingRiver = false;
        RefreshSelfOnly();

        HexMapCell neighbor = GetNeighbor(OutgoingRiver);
        neighbor.HasIncomingRiver = false;
        neighbor.RefreshSelfOnly();
    }
    public void RemoveRiver()
    {
        RemoveOutgoingRiver();
        RemoveIncomingRiver();
    }
    public void SetOutgoingRiver(HexDirection direction)
    {
        if (HasOutgoingRiver && OutgoingRiver == direction)
        {
            return;
        }

        HexMapCell neighbor = GetNeighbor(direction);
        if (!IsValidRiverDestination(neighbor))
        {
            return;
        }

        RemoveOutgoingRiver();
        if (HasIncomingRiver && IncomingRiver == direction)
        {
            RemoveIncomingRiver();
        }
        HasOutgoingRiver = true;
        OutgoingRiver = direction;
        SpecialIndex = 0;

        neighbor.RemoveIncomingRiver();
        neighbor.HasIncomingRiver = true;
        neighbor.IncomingRiver = direction.Opposite();
        neighbor.SpecialIndex = 0;

        SetRoad((int)direction, false);
    }
    public bool HasRoadThroughEdge(HexDirection direction)
    {
        return HasRoads ? Roads[(int)direction] : false;
    }
    public void AddRoad(HexDirection direction)
    {
        if (
            !Roads[(int)direction] && !HasRiverThroughEdge(direction) &&
            !IsSpecial && !GetNeighbor(direction).IsSpecial &&
            GetElevationDifference(direction) <= 1
        )
        {
            SetRoad((int)direction, true);
        }
    }
    public void RemoveRoads()
    {
        for (int i = 0; i < Neighbors.Length; i++)
        {
            if (Roads[i])
            {
                SetRoad(i, false);
            }
        }
    }
    public int GetElevationDifference(HexDirection direction)
    {
        int difference = Elevation - GetNeighbor(direction).Elevation;
        return difference >= 0 ? difference : -difference;
    }
    bool IsValidRiverDestination(HexMapCell neighbor)
    {
        return (neighbor != null) && (
            Elevation >= neighbor.Elevation || WaterLevel == neighbor.Elevation
        );
    }
    void ValidateRivers()
    {
        if (
            HasOutgoingRiver &&
            !IsValidRiverDestination(GetNeighbor(OutgoingRiver))
        )
        {
            RemoveOutgoingRiver();
        }
        if (
            HasIncomingRiver &&
            !GetNeighbor(IncomingRiver).IsValidRiverDestination(this)
        )
        {
            RemoveIncomingRiver();
        }
    }
    void SetRoad(int index, bool state)
    {
        Roads[index] = state;
        Neighbors[index].Roads[(int)((HexDirection)index).Opposite()] = state;
        Neighbors[index].RefreshSelfOnly();
        RefreshSelfOnly();
    }
    void Refresh()
    {
        if (Chunk != null)
        {
            RefreshSelfOnly();
            for (int i = 0; i < Neighbors.Length; i++)
            {
                HexMapCell neighbor = Neighbors[i];
                if (neighbor != null && neighbor.Chunk != Chunk)
                {
                    neighbor.Chunk.Refresh();
                }
            }
        }
    }
    void RefreshSelfOnly()
    {
        Chunk.Refresh();
    }
    public void Save(BinaryWriter writer)
    {
        writer.Write((byte)terrainTypeIndex);
        writer.Write((byte)elevation);
        writer.Write((byte)waterLevel);
        writer.Write((byte)urbanLevel);
        writer.Write((byte)farmLevel);
        writer.Write((byte)plantLevel);
        writer.Write((byte)specialIndex);
        writer.Write(walled);

        if (HasIncomingRiver)
        {
            writer.Write((byte)(IncomingRiver + 128));
        }
        else
        {
            writer.Write((byte)0);
        }

        if (HasOutgoingRiver)
        {
            writer.Write((byte)(OutgoingRiver + 128));
        }
        else
        {
            writer.Write((byte)0);
        }

        int roadFlags = 0;
        for (int i = 0; i < Roads.Length; i++)
        {
            if (Roads[i])
            {
                roadFlags |= 1 << i;
            }
        }
        writer.Write((byte)roadFlags);
        writer.Write(isSpawnCell);
    }
    public void Load_V1(BinaryReader reader)
    {
        terrainTypeIndex = reader.ReadByte();
        elevation = reader.ReadByte();
        OnCellElevationChanged.Invoke();
        waterLevel = reader.ReadByte();
        urbanLevel = reader.ReadByte();
        farmLevel = reader.ReadByte();
        plantLevel = reader.ReadByte();
        specialIndex = reader.ReadByte();
        walled = reader.ReadBoolean();

        byte riverData = reader.ReadByte();
        if (riverData >= 128)
        {
            HasIncomingRiver = true;
            IncomingRiver = (HexDirection)(riverData - 128);
        }
        else
        {
            HasIncomingRiver = false;
        }

        riverData = reader.ReadByte();
        if (riverData >= 128)
        {
            HasOutgoingRiver = true;
            OutgoingRiver = (HexDirection)(riverData - 128);
        }
        else
        {
            HasOutgoingRiver = false;
        }

        int roadFlags = reader.ReadByte();
        for (int i = 0; i < Roads.Length; i++)
        {
            Roads[i] = (roadFlags & (1 << i)) != 0;
        }
    }
    public void Load_V2(BinaryReader reader)
    {
        isSpawnCell = reader.ReadBoolean();
    }
    public void Load(BinaryReader reader, int header)
    {
        Load_V1(reader);
        if (header > 1)
        {
            Load_V2(reader);
        }
    }

    #region IAStarCell
    public HexCoordinates AStarCoordinates { get { return Coordinates; } }
    public int AStarX { get { return Coordinates.X; } }
    public int AStarZ { get { return Coordinates.Z; } }

    public bool IsWalkable()
    {
        return waterLevel > 0;
    }

    public float MovementCost()
    {
        return 1.0f;
    }

    public IAStarCell GetAStarNeighbor(HexDirection direction)
    {
        return GetNeighbor(direction);
    }
    #endregion
}
