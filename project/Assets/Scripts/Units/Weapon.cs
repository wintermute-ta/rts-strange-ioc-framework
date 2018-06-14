using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Weapon : IWeapon
{
    public abstract int AttackRange { get; }
    public abstract int RateOfFire { get; }
    public abstract float Damage { get; }
    public List<IUnit> LockedTargets { get; private set; }
    public IUnit CurrentTarget { get { return LockedTargets.Count > 0 ? LockedTargets[0] : null; } }
    public Action<IUnit, float> OnFire { get; set; }
    public Action<IUnit> OnChangeTarget { get; set; }

    private float shotDelay;

    public Weapon()
    {
        LockedTargets = new List<IUnit>();
        shotDelay = 0.0f;
        OnFire = delegate { };
        OnChangeTarget = delegate { };
    }

    public virtual void Cleanup()
    {
        LockedTargets.Clear();
    }

    public virtual void Update(float deltaTime, IUnit unit, List<IUnit> targets)
    {
        IUnit lastTarget = CurrentTarget;
        UpdateTargets(unit.Coordinates, targets);
        IUnit target = CurrentTarget;
        if (CheckTargetChanged(lastTarget, target))
        {
            OnChangeTarget.Invoke(target);
        }
        if (target != null)
        {
            UpdateFire(deltaTime, target);
        }
        else
        {
            shotDelay = 0.0f;
        }
    }

    private bool CheckTargetChanged(IUnit lastTarget, IUnit target)
    {
        return lastTarget != target;
    }

    protected virtual void UpdateFire(float deltaTime, IUnit target)
    {
        shotDelay += deltaTime;
        if (shotDelay >= (1.0 / RateOfFire))
        {
            shotDelay = 0.0f;
            OnFire.Invoke(target, Damage);
        }
    }

    protected virtual void LockTarget(IUnit target)
    {
        LockedTargets.Add(target);
        target.OnDestroy += Target_OnDestroy;
    }

    protected virtual void UnlockTarget(IUnit target)
    {
        LockedTargets.Remove(target);
        target.OnDestroy -= Target_OnDestroy;
    }

    private void Target_OnDestroy(IUnit unit)
    {
        UnlockTarget(unit);
    }

    protected virtual bool IsTargetLocked(IUnit target)
    {
        return LockedTargets.Contains(target);
    }

    protected virtual void UpdateTargets(HexCoordinates coordinates, List<IUnit> targets)
    {
        for (int i = 0; i < targets.Count; i++)
        {
            IUnit unit = targets[i];
            if (!unit.Destroyed)
            {
                int distance = CalculatingDistanceBetweenObjects(coordinates, unit.Coordinates);
                if (distance <= AttackRange)
                {
                    if (!IsTargetLocked(unit))
                    {
                        LockTarget(unit);
                    }
                }
                else
                {
                    if (IsTargetLocked(unit))
                    {
                        UnlockTarget(unit);
                    }
                }
            }
        }
    }

    ///Calculating distance between gun (Coordinates) and ship (ship).
    ///Coordinates gun are obtained from in global variable "Coordinates"
    /// </summary>
    /// <param name="targetCoordinates">Hex coordinates for target</param>
    /// <returns></returns>
    protected virtual int CalculatingDistanceBetweenObjects(HexCoordinates coordinates, HexCoordinates targetCoordinates)
    {
        int _result = (Math.Abs(coordinates.X - targetCoordinates.X) +
             Math.Abs(coordinates.X + coordinates.Z - targetCoordinates.X - targetCoordinates.Z) +
             Math.Abs(coordinates.Z - targetCoordinates.Z)) / 2;
        return _result;
    }
}
