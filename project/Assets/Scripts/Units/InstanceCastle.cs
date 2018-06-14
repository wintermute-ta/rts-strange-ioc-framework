using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class InstanceCastle : Unit
{
    static UnitType[] _targetTypes = new UnitType[] { UnitType.Ship };

    public InstanceCastle() : base(UnitType.Castle, _targetTypes, GlobalContext.Get().GetInstance<IWeapon>(typeof(WeaponCastleCannon)))
    {
        HealthPoint = 1000f;
    }
}
