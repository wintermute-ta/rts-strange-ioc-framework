﻿using GameWorld.HexMap;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Views
{
    namespace HexGrid
    {
        public class HexGridChunk : MonoBehaviour
        {
            public HexMesh terrain, rivers, roads, water, waterShore, estuaries, highlights;

            public HexFeatureManager features;

            HexGridCell[] cells;
            HexMapChunk chunk;

            Canvas gridCanvas;

            static Color color1 = new Color(1f, 0f, 0f);
            static Color color2 = new Color(0f, 1f, 0f);
            static Color color3 = new Color(0f, 0f, 1f);

            void Awake()
            {
                gridCanvas = GetComponentInChildren<Canvas>();

                cells = new HexGridCell[HexMetrics.chunkSizeX * HexMetrics.chunkSizeZ];
                ShowUI(false);
            }

            private void Chunk_OnChanged()
            {
                Refresh();
            }

            public void Init(HexMapChunk mapChunk, Transform parent)
            {
                chunk = mapChunk;
                transform.SetParent(parent);
                chunk.OnChanged += Chunk_OnChanged;
            }

            public void AddCell(int index, HexGridCell cell)
            {
                cells[index] = cell;
                cell.chunk = this;
                cell.transform.SetParent(transform, false);
                cell.uiRect.SetParent(gridCanvas.transform, false);
            }

            public void Refresh()
            {
                enabled = true;
            }

            public void ShowUI(bool visible)
            {
                gridCanvas.gameObject.SetActive(visible);
            }

            void LateUpdate()
            {
                Triangulate();
                enabled = false;
            }

            public void RefreshHighlight(bool highlighted, HexCoordinates coordinates)
            {
                if (highlighted)
                {
                    terrain.SetHighlightCoord(coordinates, 1.0f);
                }
                else
                {
                    terrain.SetHighlightCoord(coordinates, 0.0f);
                }
            }

            public void Triangulate()
            {
                terrain.Clear();
                rivers.Clear();
                roads.Clear();
                water.Clear();
                waterShore.Clear();
                estuaries.Clear();
                features.Clear();
                highlights.Clear();
                for (int i = 0; i < cells.Length; i++)
                {
                    Triangulate(cells[i]);
                }
                terrain.Apply();
                rivers.Apply();
                roads.Apply();
                water.Apply();
                waterShore.Apply();
                estuaries.Apply();
                features.Apply();
                highlights.Apply();
            }

            void Triangulate(HexGridCell cell)
            {
                for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
                {
                    Triangulate(d, cell);
                }
                if (!cell.MapCell.IsUnderwater)
                {
                    if (!cell.MapCell.HasRiver && !cell.MapCell.HasRoads)
                    {
                        features.AddFeature(cell.MapCell, cell.Position);
                    }
                    if (cell.MapCell.IsSpecial)
                    {
                        features.AddSpecialFeature(cell.MapCell, cell.Position);
                    }
                }
            }

            void Triangulate(HexDirection direction, HexGridCell cell)
            {
                Vector3 center = cell.Position;
                EdgeVertices e = new EdgeVertices(center + HexMetrics.GetFirstSolidCorner(direction), center + HexMetrics.GetSecondSolidCorner(direction));

                if (cell.MapCell.HasRiver)
                {
                    if (cell.MapCell.HasRiverThroughEdge(direction))
                    {
                        e.v3.y = cell.StreamBedY;
                        if (cell.MapCell.HasRiverBeginOrEnd)
                        {
                            TriangulateWithRiverBeginOrEnd(direction, cell, center, e);
                        }
                        else
                        {
                            TriangulateWithRiver(direction, cell, center, e);
                        }
                    }
                    else
                    {
                        TriangulateAdjacentToRiver(direction, cell.MapCell, center, e);
                    }
                }
                else
                {
                    TriangulateWithoutRiver(direction, cell.MapCell, center, e);

                    if (!cell.MapCell.IsUnderwater && !cell.MapCell.HasRoadThroughEdge(direction))
                    {
                        features.AddFeature(cell.MapCell, (center + e.v1 + e.v5) * (1f / 3f));
                    }
                }

                if (direction <= HexDirection.SE)
                {
                    TriangulateConnection(direction, cell, e);
                }

                if (cell.MapCell.IsUnderwater)
                {
                    TriangulateWater(direction, cell, center);
                }

                if (cell.MapCell.IsSpawnCell || cell.IsPathCell || cell.IsAttackRangeCell)
                {
                    TriangulateHighlight(direction, cell, center);
                }
            }

            private void TriangulateHighlight(HexDirection direction, HexGridCell cell, Vector3 center)
            {
                Vector3 c1 = center + HexMetrics.GetFirstSolidCorner(direction);
                Vector3 c2 = center + HexMetrics.GetSecondSolidCorner(direction);

                Color color = cell.MapCell.IsSpawnCell ? Color.red : Color.black;
                color = cell.IsPathCell ? Color.Lerp(color, Color.cyan, 0.5f) : color;
                color = cell.IsAttackRangeCell ? Color.Lerp(color, Color.yellow, 0.5f) : color;
                highlights.AddTriangle(center, c1, c2);
                highlights.AddTriangleColor(new Color(color.r, color.g, color.b, 0.0f), new Color(color.r, color.g, color.b, 1.0f), new Color(color.r, color.g, color.b, 1.0f));
            }

            void TriangulateWater(HexDirection direction, HexGridCell cell, Vector3 center)
            {
                center.y = cell.WaterSurfaceY;

                HexGridCell neighbor = cell.GetNeighbor(direction);
                if (neighbor != null && !neighbor.MapCell.IsUnderwater)
                {
                    TriangulateWaterShore(direction, cell, neighbor, center);
                }
                else
                {
                    TriangulateOpenWater(direction, cell.MapCell, neighbor != null ? neighbor.MapCell : null, center);
                }
            }

            void TriangulateOpenWater(HexDirection direction, HexMapCell cell, HexMapCell neighbor, Vector3 center)
            {
                Vector3 c1 = center + HexMetrics.GetFirstWaterCorner(direction);
                Vector3 c2 = center + HexMetrics.GetSecondWaterCorner(direction);

                water.AddTriangle(center, c1, c2);

                if (direction <= HexDirection.SE && neighbor != null)
                {
                    Vector3 bridge = HexMetrics.GetWaterBridge(direction);
                    Vector3 e1 = c1 + bridge;
                    Vector3 e2 = c2 + bridge;

                    water.AddQuad(c1, c2, e1, e2);

                    if (direction <= HexDirection.E)
                    {
                        HexMapCell nextNeighbor = cell.GetNeighbor(direction.Next());
                        if (nextNeighbor == null || !nextNeighbor.IsUnderwater)
                        {
                            return;
                        }
                        water.AddTriangle(c2, e2, c2 + HexMetrics.GetWaterBridge(direction.Next()));
                    }
                }
            }

            void TriangulateWaterShore(HexDirection direction, HexGridCell cell, HexGridCell neighbor, Vector3 center)
            {
                EdgeVertices e1 = new EdgeVertices(
                    center + HexMetrics.GetFirstWaterCorner(direction),
                    center + HexMetrics.GetSecondWaterCorner(direction)
                );
                water.AddTriangle(center, e1.v1, e1.v2);
                water.AddTriangle(center, e1.v2, e1.v3);
                water.AddTriangle(center, e1.v3, e1.v4);
                water.AddTriangle(center, e1.v4, e1.v5);

                Vector3 center2 = neighbor.Position;
                center2.y = center.y;
                EdgeVertices e2 = new EdgeVertices(center2 + HexMetrics.GetSecondSolidCorner(direction.Opposite()), center2 + HexMetrics.GetFirstSolidCorner(direction.Opposite())
                );

                if (cell.MapCell.HasRiverThroughEdge(direction))
                {
                    TriangulateEstuary(e1, e2, cell.MapCell.IncomingRiver == direction);
                }
                else
                {
                    waterShore.AddQuad(e1.v1, e1.v2, e2.v1, e2.v2);
                    waterShore.AddQuad(e1.v2, e1.v3, e2.v2, e2.v3);
                    waterShore.AddQuad(e1.v3, e1.v4, e2.v3, e2.v4);
                    waterShore.AddQuad(e1.v4, e1.v5, e2.v4, e2.v5);
                    waterShore.AddQuadUV(0f, 0f, 0f, 1f);
                    waterShore.AddQuadUV(0f, 0f, 0f, 1f);
                    waterShore.AddQuadUV(0f, 0f, 0f, 1f);
                    waterShore.AddQuadUV(0f, 0f, 0f, 1f);
                }

                HexGridCell nextNeighbor = cell.GetNeighbor(direction.Next());
                if (nextNeighbor != null)
                {
                    Vector3 v3 = nextNeighbor.Position + (nextNeighbor.MapCell.IsUnderwater ? HexMetrics.GetFirstWaterCorner(direction.Previous()) : HexMetrics.GetFirstSolidCorner(direction.Previous()));
                    v3.y = center.y;
                    waterShore.AddTriangle(e1.v5, e2.v5, v3);
                    waterShore.AddTriangleUV(Vector2.zero, new Vector2(0f, 1f), new Vector2(0f, nextNeighbor.MapCell.IsUnderwater ? 0f : 1f));
                }
            }

            void TriangulateEstuary(EdgeVertices e1, EdgeVertices e2, bool incomingRiver)
            {
                waterShore.AddTriangle(e2.v1, e1.v2, e1.v1);
                waterShore.AddTriangle(e2.v5, e1.v5, e1.v4);
                waterShore.AddTriangleUV(new Vector2(0f, 1f), Vector2.zero, Vector2.zero);
                waterShore.AddTriangleUV(new Vector2(0f, 1f), Vector2.zero, Vector2.zero);

                estuaries.AddQuad(e2.v1, e1.v2, e2.v2, e1.v3);
                estuaries.AddTriangle(e1.v3, e2.v2, e2.v4);
                estuaries.AddQuad(e1.v3, e1.v4, e2.v4, e2.v5);

                estuaries.AddQuadUV(new Vector2(0f, 1f), Vector2.zero, Vector2.one, Vector2.zero);
                estuaries.AddTriangleUV(Vector2.zero, Vector2.one, Vector2.one);
                estuaries.AddQuadUV(Vector2.zero, Vector2.zero, Vector2.one, new Vector2(0f, 1f));

                if (incomingRiver)
                {
                    estuaries.AddQuadUV2(new Vector2(1.5f, 1f), new Vector2(0.7f, 1.15f), new Vector2(1f, 0.8f), new Vector2(0.5f, 1.1f));
                    estuaries.AddTriangleUV2(new Vector2(0.5f, 1.1f), new Vector2(1f, 0.8f), new Vector2(0f, 0.8f));
                    estuaries.AddQuadUV2(new Vector2(0.5f, 1.1f), new Vector2(0.3f, 1.15f), new Vector2(0f, 0.8f), new Vector2(-0.5f, 1f));
                }
                else
                {
                    estuaries.AddQuadUV2(new Vector2(-0.5f, -0.2f), new Vector2(0.3f, -0.35f), Vector2.zero, new Vector2(0.5f, -0.3f));
                    estuaries.AddTriangleUV2(new Vector2(0.5f, -0.3f), Vector2.zero, new Vector2(1f, 0f));
                    estuaries.AddQuadUV2(new Vector2(0.5f, -0.3f), new Vector2(0.7f, -0.35f), new Vector2(1f, 0f), new Vector2(1.5f, -0.2f));
                }
            }

            void TriangulateWithoutRiver(HexDirection direction, HexMapCell cell, Vector3 center, EdgeVertices e)
            {
                TriangulateEdgeFan(center, e, cell.TerrainTypeIndex);

                if (cell.HasRoads)
                {
                    Vector2 interpolators = chunk.GetRoadInterpolators(direction, cell);
                    TriangulateRoad(center, Vector3.Lerp(center, e.v1, interpolators.x), Vector3.Lerp(center, e.v5, interpolators.y), e, cell.HasRoadThroughEdge(direction));
                }
            }

            void TriangulateAdjacentToRiver(HexDirection direction, HexMapCell cell, Vector3 center, EdgeVertices e)
            {
                if (cell.HasRoads)
                {
                    TriangulateRoadAdjacentToRiver(direction, cell, center, e);
                }

                if (cell.HasRiverThroughEdge(direction.Next()))
                {
                    if (cell.HasRiverThroughEdge(direction.Previous()))
                    {
                        center += HexMetrics.GetSolidEdgeMiddle(direction) * (HexMetrics.innerToOuter * 0.5f);
                    }
                    else if (cell.HasRiverThroughEdge(direction.Previous2()))
                    {
                        center += HexMetrics.GetFirstSolidCorner(direction) * 0.25f;
                    }
                }
                else if (cell.HasRiverThroughEdge(direction.Previous()) && cell.HasRiverThroughEdge(direction.Next2()))
                {
                    center += HexMetrics.GetSecondSolidCorner(direction) * 0.25f;
                }

                EdgeVertices m = new EdgeVertices(Vector3.Lerp(center, e.v1, 0.5f), Vector3.Lerp(center, e.v5, 0.5f));

                TriangulateEdgeStrip(m, color1, cell.TerrainTypeIndex, e, color1, cell.TerrainTypeIndex);
                TriangulateEdgeFan(center, m, cell.TerrainTypeIndex);

                if (!cell.IsUnderwater && !cell.HasRoadThroughEdge(direction))
                {
                    features.AddFeature(cell, (center + e.v1 + e.v5) * (1f / 3f));
                }
            }

            void TriangulateRoadAdjacentToRiver(HexDirection direction, HexMapCell cell, Vector3 center, EdgeVertices e)
            {
                bool hasRoadThroughEdge = cell.HasRoadThroughEdge(direction);
                bool previousHasRiver = cell.HasRiverThroughEdge(direction.Previous());
                bool nextHasRiver = cell.HasRiverThroughEdge(direction.Next());
                Vector2 interpolators = chunk.GetRoadInterpolators(direction, cell);
                Vector3 roadCenter = center;

                if (cell.HasRiverBeginOrEnd)
                {
                    roadCenter += HexMetrics.GetSolidEdgeMiddle(cell.RiverBeginOrEndDirection.Opposite()) * (1f / 3f);
                }
                else if (cell.IncomingRiver == cell.OutgoingRiver.Opposite())
                {
                    Vector3 corner;
                    if (previousHasRiver)
                    {
                        if (!hasRoadThroughEdge && !cell.HasRoadThroughEdge(direction.Next()))
                        {
                            return;
                        }
                        corner = HexMetrics.GetSecondSolidCorner(direction);
                    }
                    else
                    {
                        if (!hasRoadThroughEdge && !cell.HasRoadThroughEdge(direction.Previous()))
                        {
                            return;
                        }
                        corner = HexMetrics.GetFirstSolidCorner(direction);
                    }
                    roadCenter += corner * 0.5f;
                    if (cell.IncomingRiver == direction.Next() && (cell.HasRoadThroughEdge(direction.Next2()) || cell.HasRoadThroughEdge(direction.Opposite())))
                    {
                        features.AddBridge(roadCenter, center - corner * 0.5f);
                    }
                    center += corner * 0.25f;
                }
                else if (cell.IncomingRiver == cell.OutgoingRiver.Previous())
                {
                    roadCenter -= HexMetrics.GetSecondCorner(cell.IncomingRiver) * 0.2f;
                }
                else if (cell.IncomingRiver == cell.OutgoingRiver.Next())
                {
                    roadCenter -= HexMetrics.GetFirstCorner(cell.IncomingRiver) * 0.2f;
                }
                else if (previousHasRiver && nextHasRiver)
                {
                    if (!hasRoadThroughEdge)
                    {
                        return;
                    }
                    Vector3 offset = HexMetrics.GetSolidEdgeMiddle(direction) * HexMetrics.innerToOuter;
                    roadCenter += offset * 0.7f;
                    center += offset * 0.5f;
                }
                else
                {
                    HexDirection middle;
                    if (previousHasRiver)
                    {
                        middle = direction.Next();
                    }
                    else if (nextHasRiver)
                    {
                        middle = direction.Previous();
                    }
                    else
                    {
                        middle = direction;
                    }
                    if (!cell.HasRoadThroughEdge(middle) && !cell.HasRoadThroughEdge(middle.Previous()) && !cell.HasRoadThroughEdge(middle.Next()))
                    {
                        return;
                    }
                    Vector3 offset = HexMetrics.GetSolidEdgeMiddle(middle);
                    roadCenter += offset * 0.25f;
                    if (direction == middle && cell.HasRoadThroughEdge(direction.Opposite()))
                    {
                        features.AddBridge(roadCenter, center - offset * (HexMetrics.innerToOuter * 0.7f));
                    }
                }

                Vector3 mL = Vector3.Lerp(roadCenter, e.v1, interpolators.x);
                Vector3 mR = Vector3.Lerp(roadCenter, e.v5, interpolators.y);
                TriangulateRoad(roadCenter, mL, mR, e, hasRoadThroughEdge);
                if (previousHasRiver)
                {
                    TriangulateRoadEdge(roadCenter, center, mL);
                }
                if (nextHasRiver)
                {
                    TriangulateRoadEdge(roadCenter, mR, center);
                }
            }

            void TriangulateWithRiverBeginOrEnd(HexDirection direction, HexGridCell cell, Vector3 center, EdgeVertices e)
            {
                EdgeVertices m = new EdgeVertices(Vector3.Lerp(center, e.v1, 0.5f), Vector3.Lerp(center, e.v5, 0.5f));
                m.v3.y = e.v3.y;

                TriangulateEdgeStrip(m, color1, cell.MapCell.TerrainTypeIndex, e, color1, cell.MapCell.TerrainTypeIndex);
                TriangulateEdgeFan(center, m, cell.MapCell.TerrainTypeIndex);

                if (!cell.MapCell.IsUnderwater)
                {
                    bool reversed = cell.MapCell.HasIncomingRiver;
                    TriangulateRiverQuad(m.v2, m.v4, e.v2, e.v4, cell.RiverSurfaceY, 0.6f, reversed);
                    center.y = m.v2.y = m.v4.y = cell.RiverSurfaceY;
                    rivers.AddTriangle(center, m.v2, m.v4);
                    if (reversed)
                    {
                        rivers.AddTriangleUV(new Vector2(0.5f, 0.4f), new Vector2(1f, 0.2f), new Vector2(0f, 0.2f));
                    }
                    else
                    {
                        rivers.AddTriangleUV(new Vector2(0.5f, 0.4f), new Vector2(0f, 0.6f), new Vector2(1f, 0.6f));
                    }
                }
            }

            void TriangulateWithRiver(HexDirection direction, HexGridCell cell, Vector3 center, EdgeVertices e)
            {
                Vector3 centerL, centerR;
                if (cell.MapCell.HasRiverThroughEdge(direction.Opposite()))
                {
                    centerL = center + HexMetrics.GetFirstSolidCorner(direction.Previous()) * 0.25f;
                    centerR = center + HexMetrics.GetSecondSolidCorner(direction.Next()) * 0.25f;
                }
                else if (cell.MapCell.HasRiverThroughEdge(direction.Next()))
                {
                    centerL = center;
                    centerR = Vector3.Lerp(center, e.v5, 2f / 3f);
                }
                else if (cell.MapCell.HasRiverThroughEdge(direction.Previous()))
                {
                    centerL = Vector3.Lerp(center, e.v1, 2f / 3f);
                    centerR = center;
                }
                else if (cell.MapCell.HasRiverThroughEdge(direction.Next2()))
                {
                    centerL = center;
                    centerR = center + HexMetrics.GetSolidEdgeMiddle(direction.Next()) * (0.5f * HexMetrics.innerToOuter);
                }
                else
                {
                    centerL = center + HexMetrics.GetSolidEdgeMiddle(direction.Previous()) * (0.5f * HexMetrics.innerToOuter);
                    centerR = center;
                }
                center = Vector3.Lerp(centerL, centerR, 0.5f);

                EdgeVertices m = new EdgeVertices(Vector3.Lerp(centerL, e.v1, 0.5f), Vector3.Lerp(centerR, e.v5, 0.5f), 1f / 6f);
                m.v3.y = center.y = e.v3.y;

                TriangulateEdgeStrip(m, color1, cell.MapCell.TerrainTypeIndex, e, color1, cell.MapCell.TerrainTypeIndex);

                terrain.AddTriangle(centerL, m.v1, m.v2);
                terrain.AddQuad(centerL, center, m.v2, m.v3);
                terrain.AddQuad(center, centerR, m.v3, m.v4);
                terrain.AddTriangle(centerR, m.v4, m.v5);

                terrain.AddTriangleColor(color1);
                terrain.AddQuadColor(color1);
                terrain.AddQuadColor(color1);
                terrain.AddTriangleColor(color1);

                Vector3 types;
                types.x = types.y = types.z = cell.MapCell.TerrainTypeIndex;
                terrain.AddTriangleTerrainTypes(types);
                terrain.AddQuadTerrainTypes(types);
                terrain.AddQuadTerrainTypes(types);
                terrain.AddTriangleTerrainTypes(types);

                if (!cell.MapCell.IsUnderwater)
                {
                    bool reversed = cell.MapCell.IncomingRiver == direction;
                    TriangulateRiverQuad(centerL, centerR, m.v2, m.v4, cell.RiverSurfaceY, 0.4f, reversed);
                    TriangulateRiverQuad(m.v2, m.v4, e.v2, e.v4, cell.RiverSurfaceY, 0.6f, reversed);
                }
            }

            void TriangulateConnection(HexDirection direction, HexGridCell cell, EdgeVertices e1)
            {
                HexGridCell neighbor = cell.GetNeighbor(direction);
                if (neighbor == null)
                {
                    return;
                }

                Vector3 bridge = HexMetrics.GetBridge(direction);
                bridge.y = neighbor.Position.y - cell.Position.y;
                EdgeVertices e2 = new EdgeVertices(e1.v1 + bridge, e1.v5 + bridge);

                bool hasRiver = cell.MapCell.HasRiverThroughEdge(direction);
                bool hasRoad = cell.MapCell.HasRoadThroughEdge(direction);

                if (hasRiver)
                {
                    e2.v3.y = neighbor.StreamBedY;

                    if (!cell.MapCell.IsUnderwater)
                    {
                        if (!neighbor.MapCell.IsUnderwater)
                        {
                            TriangulateRiverQuad(e1.v2, e1.v4, e2.v2, e2.v4, cell.RiverSurfaceY, neighbor.RiverSurfaceY, 0.8f, cell.MapCell.HasIncomingRiver && cell.MapCell.IncomingRiver == direction);
                        }
                        else if (cell.MapCell.Elevation > neighbor.MapCell.WaterLevel)
                        {
                            TriangulateWaterfallInWater(e1.v2, e1.v4, e2.v2, e2.v4, cell.RiverSurfaceY, neighbor.RiverSurfaceY, neighbor.WaterSurfaceY);
                        }
                    }
                    else if (!neighbor.MapCell.IsUnderwater && neighbor.MapCell.Elevation > cell.MapCell.WaterLevel)
                    {
                        TriangulateWaterfallInWater(e2.v4, e2.v2, e1.v4, e1.v2, neighbor.RiverSurfaceY, cell.RiverSurfaceY, cell.WaterSurfaceY);
                    }
                }

                if (cell.MapCell.GetEdgeType(direction) == HexEdgeType.Slope)
                {
                    TriangulateEdgeTerraces(e1, cell.MapCell, e2, neighbor.MapCell, hasRoad);
                }
                else
                {
                    TriangulateEdgeStrip(e1, color1, cell.MapCell.TerrainTypeIndex, e2, color2, neighbor.MapCell.TerrainTypeIndex, hasRoad);
                }

                features.AddWall(e1, cell.MapCell, e2, neighbor.MapCell, hasRiver, hasRoad);

                HexGridCell nextNeighbor = cell.GetNeighbor(direction.Next());
                if (direction <= HexDirection.E && nextNeighbor != null)
                {
                    Vector3 v5 = e1.v5 + HexMetrics.GetBridge(direction.Next());
                    v5.y = nextNeighbor.Position.y;

                    if (cell.MapCell.Elevation <= neighbor.MapCell.Elevation)
                    {
                        if (cell.MapCell.Elevation <= nextNeighbor.MapCell.Elevation)
                        {
                            TriangulateCorner(e1.v5, cell.MapCell, e2.v5, neighbor.MapCell, v5, nextNeighbor.MapCell);
                        }
                        else
                        {
                            TriangulateCorner(v5, nextNeighbor.MapCell, e1.v5, cell.MapCell, e2.v5, neighbor.MapCell);
                        }
                    }
                    else if (neighbor.MapCell.Elevation <= nextNeighbor.MapCell.Elevation)
                    {
                        TriangulateCorner(e2.v5, neighbor.MapCell, v5, nextNeighbor.MapCell, e1.v5, cell.MapCell);
                    }
                    else
                    {
                        TriangulateCorner(v5, nextNeighbor.MapCell, e1.v5, cell.MapCell, e2.v5, neighbor.MapCell);
                    }
                }
            }

            void TriangulateWaterfallInWater(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, float y1, float y2, float waterY)
            {
                v1.y = v2.y = y1;
                v3.y = v4.y = y2;
                v1 = HexMetrics.Perturb(v1);
                v2 = HexMetrics.Perturb(v2);
                v3 = HexMetrics.Perturb(v3);
                v4 = HexMetrics.Perturb(v4);
                float t = (waterY - y2) / (y1 - y2);
                v3 = Vector3.Lerp(v3, v1, t);
                v4 = Vector3.Lerp(v4, v2, t);
                rivers.AddQuadUnperturbed(v1, v2, v3, v4);
                rivers.AddQuadUV(0f, 1f, 0.8f, 1f);
            }

            void TriangulateCorner(Vector3 bottom, HexMapCell bottomCell, Vector3 left, HexMapCell leftCell, Vector3 right, HexMapCell rightCell)
            {
                HexEdgeType leftEdgeType = bottomCell.GetEdgeType(leftCell);
                HexEdgeType rightEdgeType = bottomCell.GetEdgeType(rightCell);

                if (leftEdgeType == HexEdgeType.Slope)
                {
                    if (rightEdgeType == HexEdgeType.Slope)
                    {
                        TriangulateCornerTerraces(bottom, bottomCell, left, leftCell, right, rightCell);
                    }
                    else if (rightEdgeType == HexEdgeType.Flat)
                    {
                        TriangulateCornerTerraces(left, leftCell, right, rightCell, bottom, bottomCell);
                    }
                    else
                    {
                        TriangulateCornerTerracesCliff(bottom, bottomCell, left, leftCell, right, rightCell);
                    }
                }
                else if (rightEdgeType == HexEdgeType.Slope)
                {
                    if (leftEdgeType == HexEdgeType.Flat)
                    {
                        TriangulateCornerTerraces(right, rightCell, bottom, bottomCell, left, leftCell);
                    }
                    else
                    {
                        TriangulateCornerCliffTerraces(bottom, bottomCell, left, leftCell, right, rightCell);
                    }
                }
                else if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope)
                {
                    if (leftCell.Elevation < rightCell.Elevation)
                    {
                        TriangulateCornerCliffTerraces(right, rightCell, bottom, bottomCell, left, leftCell);
                    }
                    else
                    {
                        TriangulateCornerTerracesCliff(left, leftCell, right, rightCell, bottom, bottomCell);
                    }
                }
                else
                {
                    terrain.AddTriangle(bottom, left, right);
                    terrain.AddTriangleColor(color1, color2, color3);
                    Vector3 types;
                    types.x = bottomCell.TerrainTypeIndex;
                    types.y = leftCell.TerrainTypeIndex;
                    types.z = rightCell.TerrainTypeIndex;
                    terrain.AddTriangleTerrainTypes(types);
                }

                features.AddWall(bottom, bottomCell, left, leftCell, right, rightCell);
            }

            void TriangulateEdgeTerraces(EdgeVertices begin, HexMapCell beginCell, EdgeVertices end, HexMapCell endCell, bool hasRoad)
            {
                EdgeVertices e2 = EdgeVertices.TerraceLerp(begin, end, 1);
                Color c2 = HexMetrics.TerraceLerp(color1, color2, 1);
                float t1 = beginCell.TerrainTypeIndex;
                float t2 = endCell.TerrainTypeIndex;

                TriangulateEdgeStrip(begin, color1, t1, e2, c2, t2, hasRoad);

                for (int i = 2; i < HexMetrics.terraceSteps; i++)
                {
                    EdgeVertices e1 = e2;
                    Color c1 = c2;
                    e2 = EdgeVertices.TerraceLerp(begin, end, i);
                    c2 = HexMetrics.TerraceLerp(color1, color2, i);
                    TriangulateEdgeStrip(e1, c1, t1, e2, c2, t2, hasRoad);
                }

                TriangulateEdgeStrip(e2, c2, t1, end, color2, t2, hasRoad);
            }

            void TriangulateCornerTerraces(Vector3 begin, HexMapCell beginCell, Vector3 left, HexMapCell leftCell, Vector3 right, HexMapCell rightCell)
            {
                Vector3 v3 = HexMetrics.TerraceLerp(begin, left, 1);
                Vector3 v4 = HexMetrics.TerraceLerp(begin, right, 1);
                Color c3 = HexMetrics.TerraceLerp(color1, color2, 1);
                Color c4 = HexMetrics.TerraceLerp(color1, color3, 1);
                Vector3 types;
                types.x = beginCell.TerrainTypeIndex;
                types.y = leftCell.TerrainTypeIndex;
                types.z = rightCell.TerrainTypeIndex;

                terrain.AddTriangle(begin, v3, v4);
                terrain.AddTriangleColor(color1, c3, c4);
                terrain.AddTriangleTerrainTypes(types);

                for (int i = 2; i < HexMetrics.terraceSteps; i++)
                {
                    Vector3 v1 = v3;
                    Vector3 v2 = v4;
                    Color c1 = c3;
                    Color c2 = c4;
                    v3 = HexMetrics.TerraceLerp(begin, left, i);
                    v4 = HexMetrics.TerraceLerp(begin, right, i);
                    c3 = HexMetrics.TerraceLerp(color1, color2, i);
                    c4 = HexMetrics.TerraceLerp(color1, color3, i);
                    terrain.AddQuad(v1, v2, v3, v4);
                    terrain.AddQuadColor(c1, c2, c3, c4);
                    terrain.AddQuadTerrainTypes(types);
                }

                terrain.AddQuad(v3, v4, left, right);
                terrain.AddQuadColor(c3, c4, color2, color3);
                terrain.AddQuadTerrainTypes(types);
            }

            void TriangulateCornerTerracesCliff(Vector3 begin, HexMapCell beginCell, Vector3 left, HexMapCell leftCell, Vector3 right, HexMapCell rightCell)
            {
                float b = 1f / (rightCell.Elevation - beginCell.Elevation);
                if (b < 0)
                {
                    b = -b;
                }
                Vector3 boundary = Vector3.Lerp(HexMetrics.Perturb(begin), HexMetrics.Perturb(right), b);
                Color boundaryColor = Color.Lerp(color1, color3, b);
                Vector3 types;
                types.x = beginCell.TerrainTypeIndex;
                types.y = leftCell.TerrainTypeIndex;
                types.z = rightCell.TerrainTypeIndex;

                TriangulateBoundaryTriangle(begin, color1, left, color2, boundary, boundaryColor, types);

                if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope)
                {
                    TriangulateBoundaryTriangle(left, color2, right, color3, boundary, boundaryColor, types);
                }
                else
                {
                    terrain.AddTriangleUnperturbed(HexMetrics.Perturb(left), HexMetrics.Perturb(right), boundary);
                    terrain.AddTriangleColor(color2, color3, boundaryColor);
                    terrain.AddTriangleTerrainTypes(types);
                }
            }

            void TriangulateCornerCliffTerraces(Vector3 begin, HexMapCell beginCell, Vector3 left, HexMapCell leftCell, Vector3 right, HexMapCell rightCell)
            {
                float b = 1f / (leftCell.Elevation - beginCell.Elevation);
                if (b < 0)
                {
                    b = -b;
                }
                Vector3 boundary = Vector3.Lerp(HexMetrics.Perturb(begin), HexMetrics.Perturb(left), b);
                Color boundaryColor = Color.Lerp(color1, color2, b);
                Vector3 types;
                types.x = beginCell.TerrainTypeIndex;
                types.y = leftCell.TerrainTypeIndex;
                types.z = rightCell.TerrainTypeIndex;

                TriangulateBoundaryTriangle(right, color3, begin, color1, boundary, boundaryColor, types);

                if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope)
                {
                    TriangulateBoundaryTriangle(left, color2, right, color3, boundary, boundaryColor, types);
                }
                else
                {
                    terrain.AddTriangleUnperturbed(HexMetrics.Perturb(left), HexMetrics.Perturb(right), boundary);
                    terrain.AddTriangleColor(color2, color3, boundaryColor);
                    terrain.AddTriangleTerrainTypes(types);
                }
            }

            void TriangulateBoundaryTriangle(Vector3 begin, Color beginColor, Vector3 left, Color leftColor, Vector3 boundary, Color boundaryColor, Vector3 types)
            {
                Vector3 v2 = HexMetrics.Perturb(HexMetrics.TerraceLerp(begin, left, 1));
                Color c2 = HexMetrics.TerraceLerp(beginColor, leftColor, 1);

                terrain.AddTriangleUnperturbed(HexMetrics.Perturb(begin), v2, boundary);
                terrain.AddTriangleColor(beginColor, c2, boundaryColor);
                terrain.AddTriangleTerrainTypes(types);

                for (int i = 2; i < HexMetrics.terraceSteps; i++)
                {
                    Vector3 v1 = v2;
                    Color c1 = c2;
                    v2 = HexMetrics.Perturb(HexMetrics.TerraceLerp(begin, left, i));
                    c2 = HexMetrics.TerraceLerp(beginColor, leftColor, i);
                    terrain.AddTriangleUnperturbed(v1, v2, boundary);
                    terrain.AddTriangleColor(c1, c2, boundaryColor);
                    terrain.AddTriangleTerrainTypes(types);
                }

                terrain.AddTriangleUnperturbed(v2, HexMetrics.Perturb(left), boundary);
                terrain.AddTriangleColor(c2, leftColor, boundaryColor);
                terrain.AddTriangleTerrainTypes(types);
            }

            void TriangulateEdgeFan(Vector3 center, EdgeVertices edge, float type)
            {
                terrain.AddTriangle(center, edge.v1, edge.v5);
                // PKP: Reduced hex vertex count to 6
                //terrain.AddTriangle(center, edge.v1, edge.v2);
                //terrain.AddTriangle(center, edge.v2, edge.v3);
                //terrain.AddTriangle(center, edge.v3, edge.v4);
                //terrain.AddTriangle(center, edge.v4, edge.v5);

                terrain.AddTriangleColor(color1);
                // PKP: Reduced hex vertex count to 6
                //terrain.AddTriangleColor(color1);
                //terrain.AddTriangleColor(color1);
                //terrain.AddTriangleColor(color1);

                Vector3 types;
                types.x = types.y = types.z = type;
                terrain.AddTriangleTerrainTypes(types);
                // PKP: Reduced hex vertex count to 6
                //terrain.AddTriangleTerrainTypes(types);
                //terrain.AddTriangleTerrainTypes(types);
                //terrain.AddTriangleTerrainTypes(types);
            }

            void TriangulateEdgeStrip(EdgeVertices e1, Color c1, float type1, EdgeVertices e2, Color c2, float type2, bool hasRoad = false)
            {
                // PKP: Reduced edge vertex count to 4
                terrain.AddQuad(e1.v1, e1.v5, e2.v1, e2.v5);
                //terrain.AddQuad(e1.v1, e1.v2, e2.v1, e2.v2);
                //terrain.AddQuad(e1.v2, e1.v3, e2.v2, e2.v3);
                //terrain.AddQuad(e1.v3, e1.v4, e2.v3, e2.v4);
                //terrain.AddQuad(e1.v4, e1.v5, e2.v4, e2.v5);

                terrain.AddQuadColor(c1, c2);
                // PKP: Reduced edge vertex count to 4
                //terrain.AddQuadColor(c1, c2);
                //terrain.AddQuadColor(c1, c2);
                //terrain.AddQuadColor(c1, c2);

                Vector3 types;
                types.x = types.z = type1;
                types.y = type2;
                terrain.AddQuadTerrainTypes(types);
                // PKP: Reduced edge vertex count to 4
                //terrain.AddQuadTerrainTypes(types);
                //terrain.AddQuadTerrainTypes(types);
                //terrain.AddQuadTerrainTypes(types);

                if (hasRoad)
                {
                    TriangulateRoadSegment(e1.v2, e1.v3, e1.v4, e2.v2, e2.v3, e2.v4);
                }
            }

            void TriangulateRiverQuad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, float y, float v, bool reversed)
            {
                TriangulateRiverQuad(v1, v2, v3, v4, y, y, v, reversed);
            }

            void TriangulateRiverQuad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, float y1, float y2, float v, bool reversed)
            {
                v1.y = v2.y = y1;
                v3.y = v4.y = y2;
                rivers.AddQuad(v1, v2, v3, v4);
                if (reversed)
                {
                    rivers.AddQuadUV(1f, 0f, 0.8f - v, 0.6f - v);
                }
                else
                {
                    rivers.AddQuadUV(0f, 1f, v, v + 0.2f);
                }
            }

            void TriangulateRoad(Vector3 center, Vector3 mL, Vector3 mR, EdgeVertices e, bool hasRoadThroughCellEdge)
            {
                if (hasRoadThroughCellEdge)
                {
                    Vector3 mC = Vector3.Lerp(mL, mR, 0.5f);
                    TriangulateRoadSegment(mL, mC, mR, e.v2, e.v3, e.v4);
                    roads.AddTriangle(center, mL, mC);
                    roads.AddTriangle(center, mC, mR);
                    roads.AddTriangleUV(new Vector2(1f, 0f), new Vector2(0f, 0f), new Vector2(1f, 0f));
                    roads.AddTriangleUV(new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(0f, 0f));
                }
                else
                {
                    TriangulateRoadEdge(center, mL, mR);
                }
            }

            void TriangulateRoadEdge(Vector3 center, Vector3 mL, Vector3 mR)
            {
                roads.AddTriangle(center, mL, mR);
                roads.AddTriangleUV(new Vector2(1f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f));
            }

            void TriangulateRoadSegment(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Vector3 v5, Vector3 v6)
            {
                roads.AddQuad(v1, v2, v4, v5);
                roads.AddQuad(v2, v3, v5, v6);
                roads.AddQuadUV(0f, 1f, 0f, 0f);
                roads.AddQuadUV(1f, 0f, 0f, 0f);
            }
        }
    }
}
