using GameWorld.Units;
using strange.framework.api;
using Views.Units;

public interface IUnitViewBinder : IPoolBinder<IUnit, BaseUnitView>
{
    IUnitViewBinding Bind<T>() where T : IUnit;
    IUnitViewBinding Bind(object value);
    IUnitViewBinding GetBinding<T>() where T : IUnit;
    IUnitViewBinding GetBinding(object key);
}
