using strange.extensions.mediation.impl;
using strange.extensions.pool.api;
using strange.extensions.signal.impl;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CastleView : BaseUnitView, IUnitView<InstanceCastle>
{
    public InstanceCastle Unit { get; private set; }

    public void Init(InstanceCastle unit, Vector3 position, Quaternion rotation)
    {
        Init(position, rotation, unit.HealthPoint);
        Unit = unit;
        Unit.OnHitDamage += Castle_OnHitDamage;
    }

    private void Castle_OnHitDamage(IUnit unit, float damage)
    {
        UpdateHP(unit.HealthPoint);
    }

    public override void Restore()
    {
        base.Restore();
        Unit.OnHitDamage -= Castle_OnHitDamage;
        //Unit = null;
    }
}
