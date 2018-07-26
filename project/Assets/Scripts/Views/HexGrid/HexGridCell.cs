using UnityEngine;
using System.IO;
using System;
using UnityEngine.Assertions;
using GameWorld.HexMap;

namespace Views
{
    namespace HexGrid
    {
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

            public bool IsAttackRangeCell
            {
                get { return countIsAttackRangeCell > 0; }
                set
                {
                    if (value != IsAttackRangeCell)
                    {
                        bool oldValue = IsAttackRangeCell;
                        countIsAttackRangeCell += value ? 1 : -1;
                        Assert.IsTrue(countIsAttackRangeCell >= 0, "countIsAttackRangeCell < 0");
                        countIsAttackRangeCell = Mathf.Clamp(countIsAttackRangeCell, 0, int.MaxValue);
                        if (oldValue != IsAttackRangeCell)
                        {
                            chunk.Refresh();
                        }
                    }
                }
            }

            public bool IsPathCell
            {
                get { return countIsPathCell > 0; }
                set
                {
                    if (value != IsPathCell)
                    {
                        bool oldValue = IsPathCell;
                        countIsPathCell += value ? 1 : -1;
                        Assert.IsTrue(countIsPathCell >= 0, "countIsPathCell < 0");
                        countIsPathCell = Mathf.Clamp(countIsPathCell, 0, int.MaxValue);
                        if (oldValue != IsPathCell)
                        {
                            chunk.Refresh();
                        }
                    }
                }
            }

            public float StreamBedY
            {
                get
                {
                    return MapCell != null ? (MapCell.Elevation + HexMetrics.streamBedElevationOffset) * HexMetrics.elevationStep : 0.0f;
                }
            }

            public float RiverSurfaceY
            {
                get
                {
                    return MapCell != null ? (MapCell.Elevation + HexMetrics.waterElevationOffset) * HexMetrics.elevationStep : 0.0f;
                }
            }

            public float WaterSurfaceY
            {
                get
                {
                    return MapCell != null ? (MapCell.WaterLevel + HexMetrics.waterElevationOffset) * HexMetrics.elevationStep : 0.0f;
                }
            }

            private bool selected;
            private int countIsAttackRangeCell;
            private int countIsPathCell;

            void RefreshPosition(int elevation)
            {
                Vector3 position = transform.localPosition;
                position.y = elevation * HexMetrics.elevationStep;
                position.y += (HexMetrics.SampleNoise(position).y * 2f - 1f) * HexMetrics.elevationPerturbStrength;
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
    }
}
