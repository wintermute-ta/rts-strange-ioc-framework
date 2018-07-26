using GameWorld.Units;

public interface IUnitBinder : IPoolBinder<IUnit, IUnit>
{
    IUnitBinding Bind<T>() where T : IUnit;
    IUnitBinding GetBinding<T>() where T : IUnit;
    IUnitBinding GetBinding(object key);
}
