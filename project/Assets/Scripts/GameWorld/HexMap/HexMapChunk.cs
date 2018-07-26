using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameWorld
{
    namespace HexMap
    {
        public class HexMapChunk
        {
            public int X;
            public int Z;
            public event Action OnChanged = delegate { };

            HexMapCell[] cells;

            public HexMapChunk(int x, int z, int chunkSizeX, int chunkSizeZ)
            {
                cells = new HexMapCell[chunkSizeX * chunkSizeZ];
                X = x;
                Z = z;
            }

            public void AddCell(int index, HexMapCell cell)
            {
                cells[index] = cell;
                cell.Chunk = this;
            }

            public Vector2 GetRoadInterpolators(HexDirection direction, HexMapCell cell)
            {
                Vector2 interpolators;
                if (cell.HasRoadThroughEdge(direction))
                {
                    interpolators.x = interpolators.y = 0.5f;
                }
                else
                {
                    interpolators.x = cell.HasRoadThroughEdge(direction.Previous()) ? 0.5f : 0.25f;
                    interpolators.y = cell.HasRoadThroughEdge(direction.Next()) ? 0.5f : 0.25f;
                }
                return interpolators;
            }

            public void Refresh()
            {
                OnChanged.Invoke();
            }
        }
    }
}