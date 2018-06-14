using strange.extensions.mediation.impl;
using strange.extensions.pool.api;
using strange.extensions.signal.impl;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

/// <summary>
/// The description of the behavior instance gun
/// </summary>
public class GunView : BaseUnitView, IUnitView<InstanceGun>
{
    public InstanceGun Unit { get; private set; }

    /// <summary>
    /// Speed rotation gun
    /// </summary>
    private float speedRotation;
    private Coroutine coroutineTracking = null;

    public void Init(InstanceGun unit, Vector3 position, Quaternion rotation)
    {
        Init(position, rotation, unit.HealthPoint);
        Unit = unit;
        speedRotation = 360.0f;
        Unit.OnHitDamage += Gun_OnHitDamage;
    }

    private void Gun_OnHitDamage(IUnit unit, float damage)
    {
        UpdateHP(unit.HealthPoint);
    }

    public void TrackTarget(BaseUnitView target)
    {
        coroutineTracking = StartCoroutine(ChangeAngle(target));
    }

    public void StopTracking()
    {
        if (coroutineTracking != null)
        {
            StopCoroutine(coroutineTracking);
            coroutineTracking = null;
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
            Vector3 heading = target.transform.position - new Vector3(transform.localPosition.x, 0f, transform.localPosition.z);
            Quaternion lookTarget = Quaternion.LookRotation(heading, new Vector3(UnitTransform.up.x, 0f, UnitTransform.up.z));

            float step = Time.deltaTime * speedRotation;
            Quaternion lookRotation = Quaternion.RotateTowards(RemoveRotationAxis(UnitTransform.localRotation), RemoveRotationAxis(lookTarget), step);
            UnitTransform.localRotation = lookRotation;

            float angle = Quaternion.Angle(RemoveRotationAxis(UnitTransform.localRotation), RemoveRotationAxis(lookTarget));
            if (angle < 0.1f)
            {
                UnitTransform.localRotation = RemoveRotationAxis(lookTarget);
                //break;
            }
        }
    }

    public override void Restore()
    {
        base.Restore();
        Unit.OnHitDamage -= Gun_OnHitDamage;
        //Unit = null;
    }
}

