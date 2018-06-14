using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponGroundCannon : Weapon
{
    public override int AttackRange { get { return 3; } }
    public override int RateOfFire { get { return 1; } }
    public override float Damage { get { return 5.0f; } }
}
