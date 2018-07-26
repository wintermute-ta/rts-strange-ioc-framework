using strange.extensions.pool.api;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using GameWorld.HexMap;
using GameWorld.Units;

namespace GameWorld
{
    public class World : IWorld
    {
        [Inject]
        public IHexMap Map { get; private set; }
        [Inject]
        public Signals.Units.Attack AttackSignal { get; private set; }
        [Inject]
        public Signals.Units.NavalUnit.ReachTarget ReachTargetSignal { get; private set; }

        [Inject]
        public IUnitBinder UnitBinder { get; private set; }

        private List<IUnit> units;

        public World()
        {
            units = new List<IUnit>();
        }

        [PostConstruct]
        public void PostConstruct()
        {
            AttackSignal.AddListener(OnUnitAttack);
            ReachTargetSignal.AddListener(OnUnitReachTarget);
        }

        public void UpdateAI(float deltaTime)
        {
            for (int i = 0; i < units.Count; i++)
            {
                units[i].UpdateAI(deltaTime);
            }

            for (int i = units.Count; i > 0; i--)
            {
                IUnit unit = units[i - 1];
                if (unit.InDestruction)
                {
                    unit.OnDestroy.Dispatch(unit);
                    units.RemoveAt(i - 1);
                }
            }
        }

        ///return units define types
        public void GetUnitsOfTypes(UnitType[] types, int alliedFractionId, List<IUnit> result)
        {
            if (result != null)
            {
                result.Clear();
                for (int i = 0; i < types.Length; i++)
                {
                    result.AddRange(units.Where(x => { return x.Type.Equals(types[i]) && x.IsEnemyOf(alliedFractionId); }));
                }
            }
        }

        public IUnit SpawnUnit<T>(int id, int fraction, HexMapCell cell) where T : IUnit
        {
            IUnit unit = UnitBinder.GetInstance<T>();
            unit.Init(id, fraction, cell.Coordinates);
            unit.OnDestroy.AddOnce(Unit_OnDestroyed);
            if (unit.Settings.HideSpawnCell)
            {
                cell.IsSpawnCell = false;
                unit.OnDestroy.AddOnce(OnRestoreUnitSpawnCell);
            }
            units.Add(unit);

            return unit;
        }

        private void Unit_OnDestroyed(IUnit unit)
        {
            UnitBinder.ReturnInstance(unit);
        }

        public void FindPath(IAStarCell origin, IAStarCell goal, List<IAStarCell> result)
        {
            if (result != null)
            {
                result.Clear();
                AStar.FindPath(result, origin, goal, false);
            }
        }

        public IUnit GetUnit(HexCoordinates coordinates)
        {
            for (int i = 0; i < units.Count; i++)
            {
                if (units[i].Coordinates == coordinates)
                {
                    return units[i];
                }
            }
            return null;
        }

        private void OnUnitAttack(IUnit unit, IUnit target, float damage)
        {
            target.HitDamage(damage);
        }

        private void OnUnitReachTarget(INavalUnit unit)
        {
            // Just destroy ship now.
            //ship.DestroyUnit();
        }

        private void OnRestoreUnitSpawnCell(IUnit unit)
        {
            HexMapCell cell = Map.GetCell(unit.Coordinates);
            if (cell != null)
            {
                cell.IsSpawnCell = true;
            }
        }
    }
}
