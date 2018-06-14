using strange.extensions.pool.api;
using strange.extensions.signal.impl;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchDetector : ITouchDetector
{
    [Inject]
    public IPool<TouchData> TouchPool { get; private set; }

    public Signal<ITouchData> OnTouch { get; private set; }
    public Signal<ITouchData> OnTap { get; private set; }
    public Signal<ITouchData> OnLongPress { get; private set; }
    public Signal<ITouchData> OnPan { get; private set; }
    public Signal<float> OnZoom { get; private set; }
    public Signal<float> OnRotate { get; private set; }

    public float LongPressTimeout { get; set; }
    public Vector2 PanningThreshold { get; set; }
    public float ZoomingThreshold { get; set; }
    public float RotatingThreshold { get; set; }
    public string DebugTag { get; set; }

    private TouchDetectorState state;
    private List<ITouchData> activeTouches;
    private bool singleLongPress;

    public TouchDetector()
    {
        state = TouchDetectorState.Wait;
        activeTouches = new List<ITouchData>();
        OnTouch = new Signal<ITouchData>();
        OnTap = new Signal<ITouchData>();
        OnLongPress = new Signal<ITouchData>();
        OnPan = new Signal<ITouchData>();
        OnZoom = new Signal<float>();
        OnRotate = new Signal<float>();
        LongPressTimeout = 1.0f;
        PanningThreshold = Vector2.one;
        ZoomingThreshold = 0.1f;
        RotatingThreshold = 0.1f;
        singleLongPress = false;
    }

    public void Update(List<ITouchData> touches)
    {
        UpdateActiveTouchList(touches);

        switch (state)
        {
            case TouchDetectorState.Wait:
                {
                    UpdateWait();
                    break;
                }
            case TouchDetectorState.SingleTouch:
                {
                    UpdateSingleTouch();
                    break;
                }
            case TouchDetectorState.SinglePan:
                {
                    UpdateSinglePan();
                    break;
                }
            case TouchDetectorState.MultiTouch:
                {
                    UpdateMultiTouch();
                    break;
                }
            case TouchDetectorState.MultiTouchZoomRotate:
                {
                    UpdateMultiTouchZoomRotate();
                    break;
                }
        }
    }

    private void UpdateWait()
    {
        UpdateWaitState();
    }

    private void UpdateWaitState()
    {
        if (activeTouches.Count > 0)
        {
            ChangeState(activeTouches.Count > 1 ? TouchDetectorState.MultiTouch : TouchDetectorState.SingleTouch);
        }
    }

    private void UpdateSingleTouch()
    {
        UpdateSingleTouchState();

        if (state == TouchDetectorState.SingleTouch)
        {
            ITouchData activeTouch = activeTouches[0];
            switch (activeTouch.Phase)
            {
                case TouchPhase.Canceled:
                case TouchPhase.Ended:
                    {
                        OnTap.Dispatch(activeTouch);
                        RemoveStoredTouch(activeTouch);
                        break;
                    }
                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    {
                        if (Mathf.Abs(activeTouch.DeltaPosition.x) >= PanningThreshold.x || Mathf.Abs(activeTouch.DeltaPosition.y) >= PanningThreshold.y)
                        {
                            ChangeState(TouchDetectorState.SinglePan);
                            OnPan.Dispatch(activeTouch);
                        }
                        if (!singleLongPress)
                        {
                            if ((Time.time - activeTouch.BeginTime) >= LongPressTimeout)
                            {
                                singleLongPress = true;
                                OnLongPress.Dispatch(activeTouch);
                            }
                        }
                        break;
                    }
            }
        }
    }

    private void UpdateSingleTouchState()
    {
        if (activeTouches.Count != 1)
        {
            ChangeState(activeTouches.Count > 1 ? TouchDetectorState.MultiTouch : TouchDetectorState.Wait);
        }
    }

    private void UpdateSinglePan()
    {
        UpdateSingleTouchState();

        if (state == TouchDetectorState.SinglePan)
        {
            ITouchData activeTouch = activeTouches[0];
            switch (activeTouch.Phase)
            {
                case TouchPhase.Canceled:
                case TouchPhase.Ended:
                    {
                        RemoveStoredTouch(activeTouch);
                        break;
                    }
                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    {
                        if (Mathf.Abs(activeTouch.DeltaPosition.x) > 0.0f || Mathf.Abs(activeTouch.DeltaPosition.y) > 0.0f)
                        {
                            OnPan.Dispatch(activeTouch);
                        }
                        if (!singleLongPress)
                        {
                            if ((Time.time - activeTouch.BeginTime) >= LongPressTimeout)
                            {
                                singleLongPress = true;
                                OnLongPress.Dispatch(activeTouch);
                            }
                        }
                        break;
                    }
            }
        }
    }

    private void UpdateMultiTouch()
    {
        UpdateMultiTouchState();

        if (state == TouchDetectorState.MultiTouch)
        {
            // Check Zoom and Rotate
            ITouchData touchA = activeTouches[0];
            ITouchData touchB = activeTouches[1];

            if ((touchA.Phase == TouchPhase.Moved) || (touchB.Phase == TouchPhase.Moved))
            {
                // Zoom
                float currentDistance = Vector2.Distance(touchA.Position, touchB.Position);
                float prevDistance = Vector2.Distance(touchA.Position - touchA.DeltaPosition, touchB.Position - touchB.DeltaPosition);
                float delta = currentDistance - prevDistance;

                // Rotation
                float currentAngle = Angle(touchA.Position, touchB.Position);
                float prevAngle = Angle(touchA.Position - touchA.DeltaPosition, touchB.Position - touchB.DeltaPosition);
                float deltaAngle = Mathf.DeltaAngle(prevAngle, currentAngle);

                if ((Mathf.Abs(delta) >= ZoomingThreshold) || Mathf.Abs(deltaAngle) >= RotatingThreshold)
                {
                    ChangeState(TouchDetectorState.MultiTouchZoomRotate);
                    OnZoom.Dispatch(currentDistance - prevDistance);
                    OnRotate.Dispatch(deltaAngle);
                }
            }
            if ((touchB.Phase == TouchPhase.Canceled) || (touchB.Phase == TouchPhase.Ended))
            {
                RemoveStoredTouch(touchB);
            }
            if ((touchA.Phase == TouchPhase.Canceled) || (touchA.Phase == TouchPhase.Ended))
            {
                RemoveStoredTouch(touchA);
            }
        }
    }

    private void UpdateMultiTouchZoomRotate()
    {
        UpdateMultiTouchState();

        if (state == TouchDetectorState.MultiTouchZoomRotate)
        {
            // Check Zoom and Rotate
            ITouchData touchA = activeTouches[0];
            ITouchData touchB = activeTouches[1];

            if ((touchA.Phase == TouchPhase.Moved) || (touchB.Phase == TouchPhase.Moved))
            {
                // Zoom
                float currentDistance = Vector2.Distance(touchA.Position, touchB.Position);
                float prevDistance = Vector2.Distance(touchA.Position - touchA.DeltaPosition, touchB.Position - touchB.DeltaPosition);
                float delta = currentDistance - prevDistance;
                OnZoom.Dispatch(currentDistance - prevDistance);

                // Rotation
                float currentAngle = Angle(touchA.Position, touchB.Position);
                float prevAngle = Angle(touchA.Position - touchA.DeltaPosition, touchB.Position - touchB.DeltaPosition);
                float deltaAngle = Mathf.DeltaAngle(prevAngle, currentAngle);
                OnRotate.Dispatch(deltaAngle);
            }
            if ((touchB.Phase == TouchPhase.Canceled) || (touchB.Phase == TouchPhase.Ended))
            {
                RemoveStoredTouch(touchB);
            }
            if ((touchA.Phase == TouchPhase.Canceled) || (touchA.Phase == TouchPhase.Ended))
            {
                RemoveStoredTouch(touchA);
            }
        }
    }

    private void UpdateMultiTouchState()
    {
        if (activeTouches.Count < 2)
        {
            ChangeState(activeTouches.Count > 0 ? TouchDetectorState.SingleTouch : TouchDetectorState.Wait);
        }
    }

    private float Angle(Vector2 positionA, Vector2 positionB)
    {
        Vector2 from = positionB - positionA;
        Vector2 to = new Vector2(1.0f, 0.0f);

        float result = Vector2.Angle(from, to);
        Vector3 cross = Vector3.Cross(from, to);

        if (cross.z > 0)
        {
            result = 360.0f - result;
        }

        return result;
    }

    private void UpdateActiveTouchList(List<ITouchData> touches)
    {
        for (int i = activeTouches.Count; i > 0; i--)
        {
            ITouchData activeTouch = activeTouches[i - 1];
            if (FindTouch(touches, activeTouch.Id) == null)
            {
                // We lost our touch.
                RemoveStoredTouch(activeTouch);
            }
        }
        for (int i = 0; i < touches.Count; i++)
        {
            ITouchData touch = touches[i];
            ITouchData activeTouch = FindTouch(activeTouches, touch.Id);
            if (activeTouch == null)
            {
                if (touch.Phase == TouchPhase.Began)
                {
                    StoreTouch(touch);
                    OnTouch.Dispatch(touch);
                }
                else
                {
                    // Unknown touch without Began phase
                }
            }
            else
            {
                if (touch.Phase != TouchPhase.Began)
                {
                    activeTouch.Update(touch, activeTouch.BeginTime);
                }
                else
                {
                    // Duplicated touch with Began phase
                }
            }
        }
    }

    private void StoreTouch(ITouchData touch)
    {
        TouchData instance = TouchPool.GetInstance();
        instance.Init(touch, touch.BeginTime);
        activeTouches.Add(instance);
    }

    private void RemoveStoredTouch(ITouchData touch)
    {
        TouchPool.ReturnInstance(touch);
        activeTouches.Remove(touch);
    }

    private ITouchData FindTouch(List<ITouchData> touches, int id)
    {
        for (int i = 0; i < touches.Count; i++)
        {
            ITouchData touch = touches[i];
            if (touch.Id == id)
            {
                return touch;
            }
        }
        return null;
    }

    private void ChangeState(TouchDetectorState newState)
    {
        if (newState != state)
        {
            Debug.LogFormat("{2}TouchDetector state changed: {0} -> {1}", state, newState, string.IsNullOrEmpty(DebugTag) ? string.Empty : DebugTag);
            OnTransitionFromState(state);
            state = newState;
            OnTransitionToState(newState);
        }
    }

    private void OnTransitionFromState(TouchDetectorState state)
    {
        switch (state)
        {
            case TouchDetectorState.SinglePan:
            case TouchDetectorState.SingleTouch:
                {
                    singleLongPress = false;
                    break;
                }
        }
    }

    private void OnTransitionToState(TouchDetectorState state)
    {
    }
}
