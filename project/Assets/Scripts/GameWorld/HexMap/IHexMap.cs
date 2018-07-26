using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace GameWorld
{
    namespace HexMap
    {
        public interface IHexMap
        {
            int CellCountX { get; }
            int CellCountZ { get; }
            event Action<HexMapChunk> OnDestroyChunk;

            bool CreateMap(int x, int z);
            HexMapCell GetCell(HexCoordinates coordinates);
            HexMapChunk GetChunk(int index);
            HexMapChunk GetChunk(int x, int z);
            void LoadMap(byte[] data);
            bool LoadMap(string path);
            void SaveMap(string path);
            byte[] SaveMap();
        }
    }
}