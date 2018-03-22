using UnityEngine;
using System.IO;
using System;

public class HexGridCell : MonoBehaviour
{
	public RectTransform uiRect;
	public HexGridChunk chunk;
    public HexMapCell MapCell { get; private set; }
    public HexGridCell[] Neighbors;

    public bool Selected
    {
        get { return selected; }
        set
        {
            if (selected != value)
            {
                selected = value;
                if (chunk != null)
                {
                    chunk.RefreshHighlight(selected, MapCell.Coordinates);
                }
            }
        }
    }

	public Vector3 Position { get { return transform.localPosition; } }

	public float StreamBedY
    {
		get
        {
            return MapCell != null ?
                (MapCell.Elevation + HexMetrics.streamBedElevationOffset) *
				HexMetrics.elevationStep : 0.0f;
		}
	}

	public float RiverSurfaceY
    {
		get
        {
			return MapCell != null ?
                (MapCell.Elevation + HexMetrics.waterElevationOffset) *
				HexMetrics.elevationStep : 0.0f;
		}
	}

	public float WaterSurfaceY
    {
		get
        {
			return MapCell != null ?
                (MapCell.WaterLevel + HexMetrics.waterElevationOffset) *
				HexMetrics.elevationStep : 0.0f;
		}
	}

    private bool selected;

    void RefreshPosition(int elevation)
    {
		Vector3 position = transform.localPosition;
		position.y = elevation * HexMetrics.elevationStep;
		position.y +=
			(HexMetrics.SampleNoise(position).y * 2f - 1f) *
			HexMetrics.elevationPerturbStrength;
		transform.localPosition = position;

		Vector3 uiPosition = uiRect.localPosition;
		uiPosition.z = -position.y;
		uiRect.localPosition = uiPosition;
	}

    void OnDrawGizmos()
    {
        //Vector3 center = chunk.transform.TransformPoint(HexMetrics.Perturb(Position));
        //Debug.DrawLine(center, center + new Vector3(0.0f, 10.0f, 0.0f));
    }

    public HexGridCell GetNeighbor(HexDirection direction)
    {
        return Neighbors[(int)direction];
    }

    public void SetNeighbor(HexDirection direction, HexGridCell cell)
    {
        Neighbors[(int)direction] = cell;
        cell.Neighbors[(int)direction.Opposite()] = this;
    }

    public void Init(HexMapCell cell, Vector3 position)
    {
        MapCell = cell;
        transform.localPosition = position;
        MapCell.OnCellElevationChanged += MapCell_OnCellElevationChanged;
    }

    public void RefreshPosition()
    {
        RefreshPosition(MapCell.Elevation);
    }

    private void MapCell_OnCellElevationChanged()
    {
        RefreshPosition();
    }
}