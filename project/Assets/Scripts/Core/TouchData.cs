using strange.extensions.pool.api;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchData : ITouchData
{
    #region ITouchData
    public int Id { get; private set; }
    public TouchPhase Phase { get; private set; }
    public Vector2 Position { get; private set; }
    public Vector2 DeltaPosition { get; private set; }
    public Vector2 SmoothDeltaPosition { get; private set; }
    public float BeginTime { get; private set; }

    public void Init(int id, TouchPhase phase, Vector2 position, Vector2 deltaPosition, Vector2 smoothDeltaPosition, float time)
    {
        Id = id;
        Phase = phase;
        Position = position;
        DeltaPosition = deltaPosition;
        SmoothDeltaPosition = smoothDeltaPosition;
        BeginTime = time;
    }

    public void Init(Touch touch, float sensetivity)
    {
        Init(touch.fingerId, touch.phase, touch.position, touch.deltaPosition, touch.deltaPosition * sensetivity, Time.time);
        //Init(touch.fingerId, touch.phase, touch.position, touch.deltaPosition, Time.time);
    }

    public void Init(ITouchData touch, float time)
    {
        Init(touch.Id, touch.Phase, touch.Position,  touch.DeltaPosition, touch.SmoothDeltaPosition, time);
    }

    public void Update(ITouchData touch, float beginTime)
    {
        if (Id == touch.Id)
        {
            Init(touch, beginTime);
        }
    }
    #endregion
}
