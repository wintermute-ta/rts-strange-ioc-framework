using strange.extensions.pool.api;
using System.Collections;
using UnityEngine;
using strange.extensions.mediation.impl;
using strange.extensions.signal.impl;

namespace Views
{
    namespace Shots
    {
        public class BaseShotView : View, IPoolable
        {
            public int TargetUnitID { get; private set; }
            public Signal OnReachTarget = new Signal();
            public float Speed;

            public bool retain { get; private set; }

            private Vector3 targetPosition;

            public void Init(int targetId, Vector3 position, Vector3 targetVector)
            {
                gameObject.SetActive(true);

                transform.localPosition = position;
                TargetUnitID = targetId;
                AdjustTargetPosition(targetVector);
                StartCoroutine(CoMove());
            }

            public void AdjustTargetPosition(Vector3 targetPosition)
            {
                this.targetPosition = targetPosition + new Vector3(0.0f, 4.0f, 0.0f);
            }

            IEnumerator CoMove()
            {
                while (true)
                {
                    yield return null;
                    transform.localPosition = Vector3.MoveTowards(transform.localPosition, targetPosition, Time.deltaTime * Speed);
                    float distance = Vector3.Distance(transform.localPosition, targetPosition);
                    if (distance < 1f)
                    {
                        break;
                    }
                }
                OnReachTarget.Dispatch();
            }

            public void Release()
            {
                retain = false;
                Restore();
            }

            public void Restore()
            {
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;
                gameObject.SetActive(false);
            }

            public void Retain()
            {
                retain = true;
            }
        }
    }
}
