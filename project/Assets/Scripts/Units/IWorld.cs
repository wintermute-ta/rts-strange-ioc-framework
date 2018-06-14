using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IWorld
{
    void UpdateAI(float deltaTime);
    void GetUnitsOfTypes(UnitType[] types, int alliedFractionId, List<IUnit> result);
    InstanceCastle SpawnCastle(int id, int fraction, HexCoordinates coordinates);
    InstanceShip SpawnShip(int id, int fraction, HexCoordinates coordinates);
    InstanceGun SpawnGun(int id, int fraction, HexCoordinates coordinates);
    void FindPath(IAStarCell origin, IAStarCell goal, List<IAStarCell> result);
    IUnit GetUnit(HexCoordinates coordinates);
}
