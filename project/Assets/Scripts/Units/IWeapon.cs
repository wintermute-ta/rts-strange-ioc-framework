using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IWeapon
{
    int AttackRange { get; }
    int RateOfFire { get; }
    float Damage { get; }
    List<IUnit> LockedTargets { get; }
    IUnit CurrentTarget { get; }
    Action<IUnit, float> OnFire { get; set; }
    Action<IUnit> OnChangeTarget { get; set; }

    void Update(float deltaTime, IUnit unit, List<IUnit> targets);
    void Cleanup();
}
