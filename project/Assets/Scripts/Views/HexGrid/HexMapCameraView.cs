using GameWorld.HexMap;
using strange.extensions.mediation.impl;
using UnityEngine;

namespace Views
{
    namespace HexGrid
    {
        public class HexMapCameraView : View
        {
            public float stickMinZoom, stickMaxZoom;
            public float swivelMinZoom, swivelMaxZoom;
            public float moveSpeedMinZoom, moveSpeedMaxZoom;
            public float rotationSpeed;

            Transform swivel, stick;
            float zoom = 1f;
            float rotationAngle;

            public bool Locked
            {
                set
                {
                    enabled = !value;
                }
            }

            protected override void Awake()
            {
                base.Awake();

                swivel = transform.GetChild(0);
                stick = swivel.GetChild(0);
            }

            protected override void Start()
            {
                base.Start();

                Camera mainCamera = Camera.main;
                mainCamera.transform.SetParent(stick, false);
                mainCamera.transform.localPosition = new Vector3(0.0f, 0.0f, stickMaxZoom);
                mainCamera.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
            }

            public void AdjustZoom(float delta)
            {
                zoom = Mathf.Clamp01(zoom + delta);

                float distance = Mathf.Lerp(stickMinZoom, stickMaxZoom, zoom);
                stick.localPosition = new Vector3(0f, 0f, distance);

                float angle = Mathf.Lerp(swivelMinZoom, swivelMaxZoom, zoom);
                swivel.localRotation = Quaternion.Euler(angle, 0f, 0f);
            }

            public void AdjustRotation(float delta)
            {
                rotationAngle += delta * rotationSpeed * Time.deltaTime;
                if (rotationAngle < 0f)
                {
                    rotationAngle += 360f;
                }
                else if (rotationAngle >= 360f)
                {
                    rotationAngle -= 360f;
                }
                transform.localRotation = Quaternion.Euler(0f, rotationAngle, 0f);
            }

            public void AdjustPosition(float xDelta, float zDelta, int cellCountX, int cellCountZ)
            {
                Vector3 direction = transform.localRotation * new Vector3(xDelta, 0f, zDelta).normalized;
                float damping = Mathf.Max(Mathf.Abs(xDelta), Mathf.Abs(zDelta));
                float distance = Mathf.Lerp(moveSpeedMinZoom, moveSpeedMaxZoom, zoom) * damping * Time.deltaTime;

                Vector3 position = transform.localPosition;
                position += direction * distance;
                transform.localPosition = ClampPosition(position, cellCountX, cellCountZ);
            }

            public void SetPosition(Vector3 position)
            {
                transform.position = new Vector3(position.x, transform.position.y, position.z);
            }

            Vector3 ClampPosition(Vector3 position, int cellCountX, int cellCountZ)
            {
                float xMax = (cellCountX - 0.5f) * (2f * HexMetrics.innerRadius);
                position.x = Mathf.Clamp(position.x, 0f, xMax);

                float zMax = (cellCountZ - 1) * (1.5f * HexMetrics.outerRadius);
                position.z = Mathf.Clamp(position.z, 0f, zMax);

                return position;
            }
        }
    }
}
