using GameWorld.Units;
using strange.extensions.pool.api;
using Views.Units;

public interface IGameManager
{
    BaseUnitView Get_UnitView(int id);
}