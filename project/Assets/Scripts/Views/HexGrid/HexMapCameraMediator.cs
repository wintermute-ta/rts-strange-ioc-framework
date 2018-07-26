using strange.extensions.mediation.impl;
using UnityEngine;
using Signals.Camera;
using GameWorld.HexMap;
using Core;

namespace Views
{
    namespace HexGrid
    {
        public class HexMapCameraMediator : Mediator
        {
            [Inject]
            public HexMapCameraView CameraView { get; private set; }
            [Inject]
            public IHexMap Map { get; private set; }
            [Inject]
            public ValidatePosition ValidatePositionSignal { get; private set; }
            [Inject]
            public Lock LockSignal { get; private set; }
            [Inject]
            public Zoom ZoomSignal { get; private set; }
            [Inject]
            public Rotate RotateSignal { get; private set; }
            [Inject]
            public Pan PanSignal { get; private set; }
            [Inject]
            public IInputManager InputManager { get; private set; }
            [Inject]
            public ITouchDetector Touches { get; private set; }
            [Inject]
            public Move CameraMoveSignal { get; private set; }

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
                        ZoomSignal.Dispatch();
                    }
                    if (Input.GetMouseButton(2))
                    {
                        float rotationDelta = Input.GetAxis("Mouse X");
                        if (rotationDelta != 0f)
                        {
                            CameraView.AdjustRotation(rotationDelta);
                            RotateSignal.Dispatch();
                        }
                    }
                }
#endif
            }

            public override void OnRegister()
            {
                ValidatePositionSignal.AddListener(OnValidatePosition);
                LockSignal.AddListener(OnLockMapCamera);
                Touches.OnPan.AddListener(OnPan);
                Touches.OnZoom.AddListener(OnZoom);
                Touches.OnRotate.AddListener(OnRotate);
                CameraMoveSignal.AddListener(OnMove);
                Touches.PanningThreshold = new Vector2(Screen.width, Screen.height) * 0.001f;
            }

            //OnRemove() is like a destructor/OnDestroy. Use it to clean up.
            public override void OnRemove()
            {
                ValidatePositionSignal.RemoveListener(OnValidatePosition);
                LockSignal.RemoveListener(OnLockMapCamera);
                Touches.OnPan.RemoveListener(OnPan);
                Touches.OnZoom.RemoveListener(OnZoom);
                Touches.OnRotate.RemoveListener(OnRotate);
                CameraMoveSignal.RemoveListener(OnMove);
            }

            private void OnPan(ITouchData touch)
            {
                if (!locked)
                {
                    //if (Math.Abs(touch.SmoothDeltaPosition.x) > 0.0f || Math.Abs(touch.SmoothDeltaPosition.y) > 0.0f)
                    {
                        CameraView.AdjustPosition(-touch.SmoothDeltaPosition.x, -touch.SmoothDeltaPosition.y, Map.CellCountX, Map.CellCountZ);
                        PanSignal.Dispatch();
                    }
                }
            }

            private void OnZoom(float zoom)
            {
                if (!locked)
                {
                    CameraView.AdjustZoom(zoom * 0.01f);
                    ZoomSignal.Dispatch();
                }
            }

            private void OnRotate(float rotation)
            {
                if (!locked)
                {
                    CameraView.AdjustRotation(rotation);
                    RotateSignal.Dispatch();
                }
            }

            private void OnValidatePosition()
            {
                CameraView.AdjustPosition(0f, 0f, Map.CellCountX, Map.CellCountZ);
            }

            private void OnMove(Vector3 position)
            {
                CameraView.SetPosition(position);
            }

            private void OnLockMapCamera(bool locked)
            {
                this.locked = locked;
            }
        }
    }
}
