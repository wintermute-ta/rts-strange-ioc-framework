using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IUnit
{
    /// <summary>
    /// ID for View
    /// </summary>
    int ID { get; set; }
    /// <summary>
    /// Id for for fraction relations
    /// </summary>
    int FractionId { get; set; }
    /// <summary>
    /// Type of unit
    /// </summary>
    UnitType Type { get; }
    /// <summary>
    /// Position object
    /// </summary>
    HexCoordinates Coordinates { get; set; }
    /// <summary>
    /// Attack range is measured in count cells
    /// </summary>
    int AttackRange { get; }
    /// <summary>
    /// HP object
    /// </summary>
    float HealthPoint { get; }
    bool Destroyed { get; }

    event Action<IUnit, float> OnHitDamage;
    event Action<IUnit> OnDestroy;
    event Action<IUnit, IUnit, float> OnAttack;
    event Action<IUnit, IUnit> OnChangeTarget;
    Action<IUnit> OnDestroyed { get; set; }

    void HitDamage(float damage);
    void DestroyUnit(); // Mark unit for delayed destruction
    void PerformDestruction(); // Completely destroy unit (don't call it from logic!!!!)
    void UpdateAI(float deltaTime);
    bool IsEnemy(IUnit unit);
    bool IsEnemyOf(int fractionId);

    //int CalculatingDistanceBetweenObjects(HexCoordinates targetCoordinates);
}
