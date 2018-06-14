using strange.extensions.pool.api;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class World : IWorld
{
    [Inject]
    public IHexMap Map { get; private set; }
    [Inject]
    public IPool<InstanceShip> InstanceShipPool { get; private set; }
    [Inject]
    public IPool<InstanceGun> InstanceGunPool { get; private set; }
    [Inject]
    public IPool<InstanceCastle> InstanceCastlePool { get; private set; }

    private List<IUnit> units;

    public World()
    {
        units = new List<IUnit>();
    }

    [PostConstruct]
    public void PostConstruct()
    {
    }

    public void UpdateAI(float deltaTime)
    {
        for (int i = 0; i < units.Count; i++)
        {
            units[i].UpdateAI(deltaTime);
        }

        for (int i = units.Count; i > 0; i--)
        {
            if (units[i - 1].Destroyed)
            {
                units[i - 1].PerformDestruction();
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

    public InstanceCastle SpawnCastle(int id, int fraction, HexCoordinates coordinates)
    {
        InstanceCastle unit = InstanceCastlePool.GetInstance();
        unit.FractionId = fraction;
        unit.Coordinates = coordinates;
        unit.OnDestroyed += Castle_OnDestroyed;
        unit.OnDestroyed += Unit_OnDestroyed;
        unit.OnAttack += Unit_OnAttack;
        unit.ID = id;
        units.Add(unit);

        return unit;
    }

    public InstanceShip SpawnShip(int id, int fraction, HexCoordinates coordinates)
    {
        InstanceShip unit = InstanceShipPool.GetInstance();
        unit.FractionId = fraction;
        unit.Coordinates = coordinates;
        unit.OnReachTarget += Ship_OnReachTarget; // for ship destruction by game design
        unit.OnDestroyed += Ship_OnDestroyed;
        unit.OnDestroyed += Unit_OnDestroyed;
        unit.OnAttack += Unit_OnAttack;
        unit.ID = id;
        units.Add(unit);

        return unit;
    }

    public InstanceGun SpawnGun(int id, int fraction, HexCoordinates coordinates)
    {
        InstanceGun unit = InstanceGunPool.GetInstance();
        unit.FractionId = fraction;
        unit.Coordinates = coordinates;
        unit.OnDestroyed += Gun_OnDestroyed;
        unit.OnDestroyed += Unit_OnDestroyed;
        unit.OnAttack += Unit_OnAttack;
        unit.ID = id;
        units.Add(unit);

        return unit;
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

    private void Unit_OnAttack(IUnit attack, IUnit target, float damage)
    {
        target.HitDamage(damage);
    }

    private void Unit_OnDestroyed(IUnit unit)
    {
        unit.OnDestroyed -= Unit_OnDestroyed;
        units.Remove(unit);
    }

    private void Ship_OnDestroyed(IUnit unit)
    {
        unit.OnDestroyed -= Ship_OnDestroyed;
        InstanceShipPool.ReturnInstance(unit);
    }

    private void Gun_OnDestroyed(IUnit unit)
    {
        unit.OnDestroyed -= Gun_OnDestroyed;
        InstanceGunPool.ReturnInstance(unit);
    }

    private void Castle_OnDestroyed(IUnit unit)
    {
        unit.OnDestroyed -= Castle_OnDestroyed;
        InstanceCastlePool.ReturnInstance(unit);
    }

    private void Ship_OnReachTarget(InstanceShip ship)
    {
        // Just destroy ship now.
        ship.DestroyUnit();
    }
}
