using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace GameWorld
{
    namespace HexMap
    {
        public class HexMap : IHexMap
        {
            public int CellCountX { get; private set; }
            public int CellCountZ { get; private set; }

            public event Action<HexMapChunk> OnDestroyChunk = delegate { };

            private HexMapChunk[] chunks;
            private HexMapCell[] cells;

            private int chunkCountX, chunkCountZ;

            public bool CreateMap(int x, int z)
            {
                if (
                    x <= 0 || x % HexMetrics.chunkSizeX != 0 ||
                    z <= 0 || z % HexMetrics.chunkSizeZ != 0
                )
                {
                    Debug.LogError("Unsupported map size.");
                    return false;
                }

                if (chunks != null)
                {
                    for (int i = 0; i < chunks.Length; i++)
                    {
                        OnDestroyChunk(chunks[i]);
                        chunks[i] = null;
                    }
                }

                CellCountX = x;
                CellCountZ = z;
                chunkCountX = CellCountX / HexMetrics.chunkSizeX;
                chunkCountZ = CellCountZ / HexMetrics.chunkSizeZ;
                CreateChunks();
                CreateCells();
                return true;
            }

            void CreateChunks()
            {
                chunks = new HexMapChunk[chunkCountX * chunkCountZ];

                for (int z = 0, i = 0; z < chunkCountZ; z++)
                {
                    for (int x = 0; x < chunkCountX; x++)
                    {
                        HexMapChunk chunk = chunks[i++] = new HexMapChunk(x, z, HexMetrics.chunkSizeX, HexMetrics.chunkSizeZ);
                    }
                }
            }

            void CreateCells()
            {
                cells = new HexMapCell[CellCountZ * CellCountX];

                for (int z = 0, i = 0; z < CellCountZ; z++)
                {
                    for (int x = 0; x < CellCountX; x++)
                    {
                        CreateCell(x, z, i++);
                    }
                }
            }

            void CreateCell(int x, int z, int i)
            {
                HexMapCell cell = cells[i] = new HexMapCell();
                cell.Coordinates = HexCoordinates.FromOffsetCoordinates(x, z);
                cell.OffsetX = x;
                cell.OffsetZ = z;

                if (x > 0)
                {
                    cell.SetNeighbor(HexDirection.W, cells[i - 1]);
                }
                if (z > 0)
                {
                    if ((z & 1) == 0)
                    {
                        cell.SetNeighbor(HexDirection.SE, cells[i - CellCountX]);
                        if (x > 0)
                        {
                            cell.SetNeighbor(HexDirection.SW, cells[i - CellCountX - 1]);
                        }
                    }
                    else {
                        cell.SetNeighbor(HexDirection.SW, cells[i - CellCountX]);
                        if (x < CellCountX - 1)
                        {
                            cell.SetNeighbor(HexDirection.SE, cells[i - CellCountX + 1]);
                        }
                    }
                }

                cell.Elevation = 0;

                AddCellToChunk(x, z, cell);
            }

            void AddCellToChunk(int x, int z, HexMapCell cell)
            {
                int chunkX = x / HexMetrics.chunkSizeX;
                int chunkZ = z / HexMetrics.chunkSizeZ;
                HexMapChunk chunk = chunks[chunkX + chunkZ * chunkCountX];

                int localX = x - chunkX * HexMetrics.chunkSizeX;
                int localZ = z - chunkZ * HexMetrics.chunkSizeZ;
                chunk.AddCell(localX + localZ * HexMetrics.chunkSizeX, cell);
            }

            public HexMapCell GetCell(HexCoordinates coordinates)
            {
                int z = coordinates.Z;
                if (z < 0 || z >= CellCountZ)
                {
                    return null;
                }
                int x = coordinates.X + z / 2;
                if (x < 0 || x >= CellCountX)
                {
                    return null;
                }
                return cells[x + z * CellCountX];
            }

            public HexMapChunk GetChunk(int index)
            {
                return chunks[index];
            }

            public HexMapChunk GetChunk(int x, int z)
            {
                if (z < 0 || z >= chunkCountZ)
                {
                    return null;
                }
                if (x < 0 || x >= chunkCountX)
                {
                    return null;
                }
                return GetChunk(x + z * chunkCountX);
            }

            public void LoadMap(byte[] data)
            {
                using (MemoryStream stream = new MemoryStream(data))
                {
                    using (BinaryReader reader = new BinaryReader(stream))
                    {
                        int header = reader.ReadInt32();
                        if (header <= 2)
                        {
                            Load(reader, header);
                        }
                        else
                        {
                            Debug.LogWarning("Unknown map format " + header);
                        }
                    }
                }
            }

            public bool LoadMap(string path)
            {
                //string path = string.Format("{0}/default.map", Application.persistentDataPath);
                if (!File.Exists(path))
                {
                    Debug.LogError("File does not exist " + path);
                    return false;
                }
                using (BinaryReader reader = new BinaryReader(File.OpenRead(path)))
                {
                    int header = reader.ReadInt32();
                    if (header <= 2)
                    {
                        Load(reader, header);
                    }
                    else
                    {
                        Debug.LogWarning("Unknown map format " + header);
                        return false;
                    }
                }
                return true;
            }

            public void SaveMap(string path)
            {
                using (BinaryWriter writer = new BinaryWriter(File.Open(path, FileMode.Create)))
                {
                    writer.Write(2);

                    writer.Write(CellCountX);
                    writer.Write(CellCountZ);

                    for (int i = 0; i < cells.Length; i++)
                    {
                        cells[i].Save(writer);
                    }
                }
            }

            public byte[] SaveMap()
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    using (BinaryWriter writer = new BinaryWriter(stream))
                    {
                        writer.Write(2);

                        writer.Write(CellCountX);
                        writer.Write(CellCountZ);

                        for (int i = 0; i < cells.Length; i++)
                        {
                            cells[i].Save(writer);
                        }
                    }
                    return stream.GetBuffer();
                }
            }

            private void Load(BinaryReader reader, int header)
            {
                int x = 20, z = 15;
                if (header >= 1)
                {
                    x = reader.ReadInt32();
                    z = reader.ReadInt32();
                }
                if (x != CellCountX || z != CellCountZ)
                {
                    if (!CreateMap(x, z))
                    {
                        return;
                    }
                }

                for (int i = 0; i < cells.Length; i++)
                {
                    cells[i].Load(reader, header);
                }
                for (int i = 0; i < chunks.Length; i++)
                {
                    chunks[i].Refresh();
                }
            }
        }
    }
}
