using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IUnit
{
    /// <summary>
    /// ID for BehaviorShip(Gun)
    /// </summary>
    int ID { get; set; }
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
    /// <summary>
    /// List coordinates for attack (calculated from AttackRange)
    /// </summary>
    List<HexCoordinates> AttackCoordinates { get; set; }
}
