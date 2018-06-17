using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    public interface ITouchData
    {
        int Id { get; }
        TouchPhase Phase { get; }
        Vector2 Position { get; }
        Vector2 DeltaPosition { get; }
        Vector2 SmoothDeltaPosition { get; }
        float Radius { get; }
        float RadiusVariance { get; }
        float BeginTime { get; }

        void Init(int id, TouchPhase phase, Vector2 position, Vector2 deltaPosition, Vector2 smoothDeltaPosition, float radius, float radiusVariance, float time);
        void Init(Touch touch, float sensetivity);
        void Init(ITouchData touch, float time);
        void Update(ITouchData touch, float beginTime);
    }
}
