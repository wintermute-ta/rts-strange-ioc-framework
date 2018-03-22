using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Description of the instance gun
/// </summary>
public class InstanceGun : IUnit
{
    public HexCoordinates Coordinates { get; set; }
    public int AttackRange { get; set; }
    public float HealthPoint { get; set; }
    public float DamagePerSecond { get; set; }

    public InstanceGun(HexCoordinates coordinates)
    {
        Coordinates = coordinates;
        AttackRange = 3;
        HealthPoint = 10;
    }

}
