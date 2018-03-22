using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IUnit
{
    /// <summary>
    /// Position object
    /// </summary>
    HexCoordinates Coordinates { get; set; }
    /// <summary>
    /// Attack range is measured in count cells
    /// </summary>
    int AttackRange { get; set; }
    /// <summary>
    /// HP object
    /// </summary>
    float HealthPoint { get; set; }
    /// <summary>
    /// Damage per second
    /// </summary>
    float DamagePerSecond { get; set; }
}
