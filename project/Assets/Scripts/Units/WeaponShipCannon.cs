﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponShipCannon : Weapon
{
    public override int AttackRange { get { return 3; } }
    public override int RateOfFire { get { return 1; } }
    public override float Damage { get { return 1.0f; } }
}
