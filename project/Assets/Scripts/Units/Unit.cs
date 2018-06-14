using strange.extensions.pool.api;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : IUnit, IPoolable
{
    [Inject]
    public IWorld World { get; private set; }

    public int ID { get; set; }
    public int FractionId { get; set; }
    public UnitType Type { get; private set; }
    public HexCoordinates Coordinates { get; set; }
    public int AttackRange { get { return weapon != null ? weapon.AttackRange : 0; } }
    public float HealthPoint { get; protected set; }
    public Action<IUnit> OnDestroyed { get; set; }
    public List<IUnit> PossibleTargets { get; protected set; }
    public bool Destroyed { get; protected set; }

    public event Action<IUnit, float> OnHitDamage = delegate { };
    public event Action<IUnit> OnDestroy = delegate { };
    public event Action<IUnit, IUnit, float> OnAttack = delegate { };
    public event Action<IUnit, IUnit> OnChangeTarget = delegate { };

    protected UnitType[] TargetTypes { get; private set; }
    protected IWeapon weapon { get; set; }

    public bool retain { get; private set; }
    public Unit(UnitType type, UnitType[] targetTypes) : this(type, targetTypes, null) { }
    public Unit(UnitType type, UnitType[] targetTypes, IWeapon weapon)
    {
        Type = type;
        TargetTypes = targetTypes;
        PossibleTargets = new List<IUnit>();
        OnDestroyed = delegate { };
        this.weapon = weapon;
        if (this.weapon != null)
        {
            this.weapon.OnFire = OnWeaponFire;
            this.weapon.OnChangeTarget = OnWeaponChangeTarget;
        }
        retain = false;
        Destroyed = false;
    }

    public virtual bool IsEnemy(IUnit unit)
    {
        return IsEnemyOf(unit.FractionId);
    }

    public virtual bool IsEnemyOf(int fractionId)
    {
        return fractionId != FractionId;
    }

    public virtual void HitDamage(float damage)
    {
        HealthPoint -= Mathf.Min(damage, HealthPoint);
        OnHitDamage.Invoke(this, damage);
        if (HealthPoint == 0)
        {
            DestroyUnit();
        }
    }

    public virtual void DestroyUnit()
    {
        OnDestroy.Invoke(this);
        Destroyed = true;
    }

    public void PerformDestruction()
    {
        OnDestroyed.Invoke(this);
    }

    public virtual void UpdateAI(float deltaTime)
    {
        World.GetUnitsOfTypes(TargetTypes, FractionId, PossibleTargets);
        if (PossibleTargets.Count > 0)
        {
            if (weapon != null)
            {
                weapon.Update(deltaTime, this, PossibleTargets);
            }
        }
    }

    private void OnWeaponFire(IUnit target, float damage)
    {
        OnAttack.Invoke(this, target, damage);
    }

    private void OnWeaponChangeTarget(IUnit target)
    {
        OnChangeTarget.Invoke(this, target);
    }

    public virtual void Restore()
    {
        Destroyed = false;
        if (weapon != null)
        {
            weapon.Cleanup();
        }
    }

    public void Retain()
    {
        retain = true;
    }

    public void Release()
    {
        retain = false;
        Restore();
    }
}
