using GameWorld.Units;
using strange.extensions.signal.impl;
using System.Collections;
using UnityEngine;

namespace Views
{
    namespace Units
    {
        /// <summary>
        /// The description of the behavior instance ship
        /// </summary>
        public class ShipView : BaseUnitView
        {
            public Signal<Vector3> OnChangePosition = new Signal<Vector3>();
            public Signal OnReachCell = new Signal();

            /// <summary>
            /// Speed move ship
            /// </summary>
            private float speedMove;
            /// <summary>
            /// Speed rotation ship
            /// </summary>
            private float speedRotation;

            public override void Init(IUnit unit, Vector3 position)
            {
                base.Init(unit, position);

                transform.localRotation = Quaternion.AngleAxis(90.0f, Vector3.up);
                speedMove = 20f;
                speedRotation = speedMove * 10f;
            }

            public void MoveTo(Vector3 end)
            {
                StartCoroutine(ChangeAngle(end));
            }

            /// <summary>
            /// Coroutine move ship between two cells
            /// </summary>
            /// <param name="index"></param>
            /// <returns></returns>
            IEnumerator MoveBetweenCell(Vector3 vectorEnd)
            {
                //float distance = Vector3.Distance(RemovePositionAxisY(transform.localPosition), vectorEnd);
                while (true)
                {
                    yield return null;
                    float step = Time.deltaTime * speedMove;
                    transform.localPosition = Vector3.MoveTowards(RemovePositionAxisY(transform.localPosition), vectorEnd, step);
                    OnChangePosition.Dispatch(transform.localPosition);
                    float distance = Vector3.Distance(RemovePositionAxisY(transform.localPosition), vectorEnd);
                    if (distance < 0.1f)
                    {
                        transform.localPosition = ReplacePositionAxisY(vectorEnd, transform.localPosition.y); //PKP: Move ship to end position
                        OnChangePosition.Dispatch(transform.localPosition);
                        break;
                    }
                }
                OnReachCell.Dispatch();
            }

            /// <summary>
            /// Coroutine rotate ship
            /// </summary>
            /// <param name="VectorTo"></param>
            /// <returns></returns>
            IEnumerator ChangeAngle(Vector3 vectorEnd)
            {
                yield return null;
                Vector3 heading = vectorEnd - RemovePositionAxisY(transform.localPosition);
                Quaternion lookTarget = Quaternion.LookRotation(heading, transform.up);

                while (true)
                {
                    yield return null;
                    float step = Time.deltaTime * speedRotation;
                    Quaternion lookRotation = Quaternion.RotateTowards(RemoveRotationAxis(transform.localRotation), RemoveRotationAxis(lookTarget), step);
                    transform.localRotation = lookRotation;

                    float distance = Vector3.Distance(RemovePositionAxisY(transform.localPosition), vectorEnd);
                    float angle = Quaternion.Angle(RemoveRotationAxis(transform.localRotation), RemoveRotationAxis(lookTarget));
                    if (angle < 0.1f)
                    {
                        transform.localRotation = RemoveRotationAxis(lookTarget);
                        break;
                    }
                }
                StartCoroutine(MoveBetweenCell(vectorEnd));
            }
        }
    }
}
