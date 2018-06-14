using System.Collections.Generic;

public interface IInstanceShip : IUnit
{
    List<IAStarCell> Path { get; }
    void BeginMoving(List<IAStarCell> pathTarget);
}
