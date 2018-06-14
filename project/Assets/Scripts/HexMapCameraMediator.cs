using strange.extensions.mediation.impl;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class HexMapCameraMediator : Mediator
{
    [Inject]
    public HexMapCameraView CameraView { get; private set; }
    [Inject]
    public IHexMap Map { get; private set; }
    [Inject]
    public CameraValidatePositionSignal ValidatePosition { get; private set; }
    [Inject]
    public LockMapCameraSignal LockMapCamera { get; private set; }
    [Inject]
    public ZoomMapCameraSignal ZoomMapCamera { get; private set; }
    [Inject]
    public RotateMapCameraSignal RotateMapCamera { get; private set; }
    [Inject]
    public PanMapCameraSignal PanMapCamera { get; private set; }
    [Inject]
    public IInputManager InputManager { get; private set; }
    [Inject]
    public ITouchDetector Touches { get; private set; }

    private bool locked = false;

    void Update()
    {
        Touches.Update(InputManager.Touches);
#if UNITY_STANDALONE || UNITY_EDITOR
        //Mouse emulation
        if (InputManager.MouseSupported)
        {
            float zoomDelta = Input.GetAxis("Mouse ScrollWheel");
            if (zoomDelta != 0f)
            {
                CameraView.AdjustZoom(zoomDelta);
                ZoomMapCamera.Dispatch();
            }
            if (Input.GetMouseButton(2))
            {
                float rotationDelta = Input.GetAxis("Mouse X");
                if (rotationDelta != 0f)
                {
                    CameraView.AdjustRotation(rotationDelta);
                    RotateMapCamera.Dispatch();
                }
            }
        }
#endif
    }

    public override void OnRegister()
    {
        ValidatePosition.AddListener(OnValidatePosition);
        LockMapCamera.AddListener(OnLockMapCamera);
        Touches.OnPan.AddListener(OnPan);
        Touches.OnZoom.AddListener(OnZoom);
        Touches.OnRotate.AddListener(OnRotate);
        Touches.PanningThreshold = new Vector2(Screen.width, Screen.height) * 0.001f;
    }

    //OnRemove() is like a destructor/OnDestroy. Use it to clean up.
    public override void OnRemove()
    {
        ValidatePosition.RemoveListener(OnValidatePosition);
        LockMapCamera.RemoveListener(OnLockMapCamera);
        Touches.OnPan.RemoveListener(OnPan);
        Touches.OnZoom.RemoveListener(OnZoom);
        Touches.OnRotate.RemoveListener(OnRotate);
    }

    private void OnPan(ITouchData touch)
    {
        if (!locked)
        {
            //if (Math.Abs(touch.SmoothDeltaPosition.x) > 0.0f || Math.Abs(touch.SmoothDeltaPosition.y) > 0.0f)
            {
                CameraView.AdjustPosition(-touch.SmoothDeltaPosition.x, -touch.SmoothDeltaPosition.y, Map.CellCountX, Map.CellCountZ);
                PanMapCamera.Dispatch();
            }
        }
    }

    private void OnZoom(float zoom)
    {
        if (!locked)
        {
            CameraView.AdjustZoom(zoom * 0.01f);
            ZoomMapCamera.Dispatch();
        }
    }

    private void OnRotate(float rotation)
    {
        if (!locked)
        {
            CameraView.AdjustRotation(rotation);
            RotateMapCamera.Dispatch();
        }
    }

    private void OnValidatePosition()
    {
        CameraView.AdjustPosition(0f, 0f, Map.CellCountX, Map.CellCountZ);
    }

    private void OnLockMapCamera(bool locked)
    {
        this.locked = locked;
    }
}
