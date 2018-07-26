using GameWorld.Units;
using System.Collections;
using UnityEngine;

namespace Views
{
    namespace Units
    {
        /// <summary>
        /// The description of the behavior instance gun
        /// </summary>
        public class GroundCannonView : BaseUnitView
        {
            /// <summary>
            /// Speed rotation gun
            /// </summary>
            private float speedRotation;
            private Coroutine coroutineTracking = null;
            private Coroutine coroutineResetAngle = null;

            public override bool AddAttackRange { get { return true; } }

            public override void Init(IUnit unit, Vector3 position)
            {
                base.Init(unit, position);

                speedRotation = 360.0f;
                coroutineResetAngle = StartCoroutine(ResetAngleToDefault());
            }

            public void TrackTarget(BaseUnitView target)
            {
                if (coroutineResetAngle != null)
                {
                    StopCoroutine(coroutineResetAngle);
                    coroutineResetAngle = null;
                }
                coroutineTracking = StartCoroutine(ChangeAngle(target));
            }

            public void StopTracking()
            {
                if (coroutineTracking != null)
                {
                    StopCoroutine(coroutineTracking);
                    coroutineTracking = null;
                    coroutineResetAngle = StartCoroutine(ResetAngleToDefault());
                }
            }

            /// <summary>
            /// Change andgle gun in the direction of the ship 
            /// </summary>
            /// <param name="VectorTo"></param>
            /// <returns></returns>
            IEnumerator ChangeAngle(BaseUnitView target)
            {
                while (true)
                {
                    yield return null;
                    Vector3 heading = target.transform.localPosition - new Vector3(transform.localPosition.x, 0f, transform.localPosition.z);
                    Quaternion lookTarget = Quaternion.LookRotation(heading, new Vector3(TurretTransform.up.x, 0f, TurretTransform.up.z));

                    float step = Time.deltaTime * speedRotation;
                    Quaternion lookRotation = Quaternion.RotateTowards(RemoveRotationAxis(TurretTransform.localRotation), RemoveRotationAxis(lookTarget), step);
                    TurretTransform.localRotation = lookRotation;

                    float angle = Quaternion.Angle(RemoveRotationAxis(TurretTransform.localRotation), RemoveRotationAxis(lookTarget));
                    if (angle < 0.1f)
                    {
                        TurretTransform.localRotation = RemoveRotationAxis(lookTarget);
                    }
                }
            }

            IEnumerator ResetAngleToDefault()
            {
                while (true)
                {
                    yield return null;
                    Vector3 heading = transform.localPosition + Vector3.left - new Vector3(transform.localPosition.x, 0f, transform.localPosition.z);
                    Quaternion lookTarget = Quaternion.LookRotation(heading, new Vector3(TurretTransform.up.x, 0f, TurretTransform.up.z));

                    float step = Time.deltaTime * speedRotation;
                    Quaternion lookRotation = Quaternion.RotateTowards(RemoveRotationAxis(TurretTransform.localRotation), RemoveRotationAxis(lookTarget), step);
                    TurretTransform.localRotation = lookRotation;

                    float angle = Quaternion.Angle(RemoveRotationAxis(TurretTransform.localRotation), RemoveRotationAxis(lookTarget));
                    if (angle < 0.1f)
                    {
                        TurretTransform.localRotation = RemoveRotationAxis(lookTarget);
                        coroutineResetAngle = null;
                        break;
                    }
                }
            }
        }
    }
}
