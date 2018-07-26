using GameWorld.HexMap;
using GameWorld.Units;
using System.Collections.Generic;

namespace GameWorld
{
    public interface IWorld
    {
        IHexMap Map { get; }

        void UpdateAI(float deltaTime);
        void GetUnitsOfTypes(UnitType[] types, int alliedFractionId, List<IUnit> result);
        IUnit SpawnUnit<T>(int id, int fraction, HexMapCell cell) where T : IUnit;
        //IUnit SpawnFort(int id, int fraction, HexMapCell cell);
        //IUnit SpawnShip(int id, int fraction, HexMapCell cell);
        //IUnit SpawnGroundCannon(int id, int fraction, HexMapCell cell);
        void FindPath(IAStarCell origin, IAStarCell goal, List<IAStarCell> result);
        IUnit GetUnit(HexCoordinates coordinates);
    }
}
