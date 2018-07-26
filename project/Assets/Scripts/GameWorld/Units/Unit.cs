using GameWorld.HexMap;
using GameWorld.Settings;
using GameWorld.Weapons;
using Signals.Units;
using strange.extensions.pool.api;
using System.Collections.Generic;
using UnityEngine;

namespace GameWorld
{
    namespace Units
    {
        public class Unit : IUnit, IPoolable
        {
            [Inject]
            public IWorld World { get; private set; }
            [Inject]
            public Signals.Units.Weapon.Fire WeaponFireSignal { get; private set; }
            [Inject]
            public Signals.Units.Weapon.ChangeTarget WeaponChangeTargetSignal { get; private set; }
            [Inject]
            public Signals.Settings.Changed SettingsChangedSignal { get; private set; }
            [Inject]
            public HitDamage HitDamageSignal { get; private set; }
            [Inject]
            public Destroy DestroySignal { get; private set; }
            [Inject]
            public Attack AttackSignal { get; private set; }
            [Inject]
            public ChangeTarget ChangeTargetSignal { get; private set; }

            public int ID { get; set; }
            public int FractionId { get; set; }
            public UnitType Type { get; private set; }
            public HexCoordinates Coordinates { get; set; }
            public int AttackRange { get { return Weapon != null ? Weapon.Settings.AttackRange : 0; } }
            public float HealthPoint { get; set; }
            public Destroy OnDestroy { get; private set; }
            public List<IUnit> PossibleTargets { get; protected set; }
            public bool InDestruction { get; protected set; }
            public virtual IUnitSettings Settings { get; protected set; }
            public virtual IWeapon Weapon { get; protected set; }

            protected UnitType[] TargetTypes { get; private set; }

            public bool retain { get; private set; }
            public Unit(UnitType type, UnitType[] targetTypes)
            {
                Type = type;
                TargetTypes = targetTypes;
                PossibleTargets = new List<IUnit>();
                OnDestroy = new Destroy();
                retain = false;
                InDestruction = false;
            }

            [PostConstruct]
            public virtual void PostContruct()
            {
                if (Settings != null)
                {
                    HealthPoint = Settings.HealthPoint;
                }
            }

            public virtual void Init(int id, int fraction, HexCoordinates coordinates)
            {
                ID = id;
                FractionId = fraction;
                Coordinates = coordinates;
                InDestruction = false;

                if (Weapon != null)
                {
                    Weapon.UnitID = id;
                }
                WeaponFireSignal.AddListener(OnWeaponFire);
                WeaponChangeTargetSignal.AddListener(OnWeaponChangeTarget);
                SettingsChangedSignal.AddListener(OnSettingsChanged);
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
                HitDamageSignal.Dispatch(this, damage);
                if (HealthPoint == 0)
                {
                    DestroyUnit();
                }
            }

            public virtual void DestroyUnit()
            {
                InDestruction = true;
                DestroySignal.Dispatch(this);
            }

            public virtual void UpdateAI(float deltaTime)
            {
                World.GetUnitsOfTypes(TargetTypes, FractionId, PossibleTargets);
                if (PossibleTargets.Count > 0)
                {
                    if (Weapon != null)
                    {
                        Weapon.Update(deltaTime, this, PossibleTargets);
                    }
                }
            }

            private void OnWeaponFire(IWeapon weapon, IUnit target, float damage)
            {
                if (weapon.UnitID == ID)
                {
                    AttackSignal.Dispatch(this, target, damage);
                }
            }

            private void OnWeaponChangeTarget(IWeapon weapon, IUnit target)
            {
                if (weapon.UnitID == ID)
                {
                    ChangeTargetSignal.Dispatch(this, target);
                }
            }

            private void OnSettingsChanged()
            {
                HealthPoint = Settings.HealthPoint;
            }

            public virtual void Restore()
            {
                InDestruction = false;
                WeaponFireSignal.RemoveListener(OnWeaponFire);
                WeaponChangeTargetSignal.RemoveListener(OnWeaponChangeTarget);
                if (Weapon != null)
                {
                    Weapon.Cleanup();
                }
                if (Settings != null)
                {
                    HealthPoint = Settings.HealthPoint;
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
    }
}
