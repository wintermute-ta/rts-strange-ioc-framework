using GameWorld.Units;
using strange.extensions.mediation.impl;
using strange.extensions.pool.api;
using UnityEngine;

namespace Views
{
    namespace Units
    {
        public class BaseUnitView : View, IPoolable
        {
            public IUnit Unit { get; private set; }
            public bool retain { get; private set; }

            public WorldSpaceCanvasHandler HealthBarCanvas
            {
                get
                {
                    if (_healthBarCanvas == null)
                    {
                        _healthBarCanvas = GetComponentInChildren<WorldSpaceCanvasHandler>(true);
                    }
                    return _healthBarCanvas;
                }
            }

            public virtual bool AddAttackRange { get { return false; } }

            [SerializeField]
            private WorldSpaceCanvasHandler _healthBarCanvas;
            public Transform TurretTransform;
            private SliderModel healthBar;
            private Camera mainCamera;
            private float initialHP;

            protected override void Awake()
            {
                base.Awake();
                mainCamera = Camera.main;
                retain = false;
            }

            public virtual void Init(IUnit unit, Vector3 position)
            {
                Unit = unit;
                initialHP = unit.HealthPoint;

                transform.localPosition = position;
                transform.localRotation = Quaternion.identity;
                gameObject.SetActive(true);
                HealthBarCanvas.UpdateCanvas();
            }

            public void AttachHealhBar(SliderModel healthBar)
            {
                this.healthBar = healthBar;
                ResetHP();
                this.healthBar.Open();
            }

            public void RemoveHealthBar()
            {
                healthBar.Shutdown();
                healthBar = null;
            }

            public void UpdateHP(float hp)
            {
                healthBar.SetValue(hp);
            }

            protected void ResetHP()
            {
                if (healthBar != null)
                {
                    healthBar.Init(initialHP);
                }
            }

            protected Quaternion RemoveRotationAxis(Quaternion rotation)
            {
                Vector3 euler = rotation.eulerAngles;
                euler.x = 0f;
                euler.z = 0f;
                return Quaternion.Euler(euler);
            }

            protected Vector3 RemovePositionAxisY(Vector3 position)
            {
                return ReplacePositionAxisY(position, 0.0f);
            }

            protected Vector3 ReplacePositionAxisY(Vector3 position, float value)
            {
                position.y = value;
                return position;
            }

            public virtual void Restore()
            {
                ResetHP();
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;
                gameObject.SetActive(false);
            }

            public void Retain()
            {
                retain = true;
            }

            public void Release()
            {
                retain = false;
                Restore();
            }
        }
    }
}
