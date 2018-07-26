using GameWorld.HexMap;
using GameWorld.Settings;
using GameWorld.Units;
using Signals.Units.Weapon;
using System;
using System.Collections.Generic;

namespace GameWorld
{
    namespace Weapons
    {
        public abstract class Weapon : IWeapon
        {
            [Inject]
            public Fire FireSignal { get; private set; }
            [Inject]
            public ChangeTarget ChangeTargetSignal { get; private set; }
            [Inject]
            public Signals.Units.Destroy DestroySignal { get; private set; }

            public int UnitID { get; set; }
            public virtual IWeaponSettings Settings { get; protected set; }
            public List<IUnit> LockedTargets { get; private set; }
            public IUnit CurrentTarget { get { return LockedTargets.Count > 0 ? LockedTargets[0] : null; } }

            private float shotDelay;

            public Weapon()
            {
                LockedTargets = new List<IUnit>();
                shotDelay = 0.0f;
            }

            [PostConstruct]
            public void PostContruct()
            {
                DestroySignal.AddListener(OnDestroyUnit);
            }

            public virtual void Cleanup()
            {
                LockedTargets.Clear();
            }

            public virtual void Update(float deltaTime, IUnit unit, List<IUnit> targets)
            {
                UpdateTargets(unit.Coordinates, targets);
                if (CurrentTarget != null)
                {
                    UpdateFire(deltaTime, CurrentTarget);
                }
                else
                {
                    shotDelay = 0.0f;
                }
            }

            protected virtual void UpdateFire(float deltaTime, IUnit target)
            {
                shotDelay += deltaTime;
                if (shotDelay >= (1.0 / Settings.RateOfFire))
                {
                    shotDelay = 0.0f;
                    FireSignal.Dispatch(this, target, Settings.Damage);
                }
            }

            protected virtual void LockTarget(IUnit target)
            {
                bool retargeting = CurrentTarget == null;
                LockedTargets.Add(target);
                if (retargeting)
                {
                    ChangeTargetSignal.Dispatch(this, CurrentTarget);
                }
            }

            protected virtual void UnlockTarget(IUnit target)
            {
                bool retargeting = target.ID == CurrentTarget.ID;
                if (retargeting)
                {
                    ChangeTargetSignal.Dispatch(this, null);
                }
                LockedTargets.Remove(target);
                if (retargeting && (CurrentTarget != null))
                {
                    ChangeTargetSignal.Dispatch(this, CurrentTarget);
                }
            }

            private void OnDestroyUnit(IUnit unit)
            {
                if (IsTargetLocked(unit))
                {
                    UnlockTarget(unit);
                }
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
                    if (!unit.InDestruction)
                    {
                        int distance = CalculatingDistanceBetweenObjects(coordinates, unit.Coordinates);
                        if (distance <= Settings.AttackRange)
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
    }
}
