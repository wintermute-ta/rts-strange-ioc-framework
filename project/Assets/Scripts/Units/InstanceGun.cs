using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Description of the instance gun
/// </summary>
public class InstanceGun : Unit
{
    static UnitType[] _targetTypes = new UnitType[] { UnitType.Ship };

    public InstanceGun() : base(UnitType.Gun, _targetTypes, GlobalContext.Get().GetInstance<IWeapon>(typeof(WeaponGroundCannon)))
    {
        HealthPoint = 50f;
    }
}
