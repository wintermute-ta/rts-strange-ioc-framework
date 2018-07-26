using System;
using UnityEngine;

namespace Views
{
    namespace HexGrid
    {
        [Serializable]
        public struct HexFeatureCollection
        {
            public Transform[] prefabs;

            public Transform Pick(float choice)
            {
                return prefabs[(int)(choice * prefabs.Length)];
            }
        }
    }
}
