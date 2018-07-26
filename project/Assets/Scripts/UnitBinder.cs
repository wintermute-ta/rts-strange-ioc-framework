using GameWorld.Units;
using strange.extensions.injector.api;
using strange.extensions.pool.api;
using strange.extensions.pool.impl;
using strange.framework.api;

public class UnitBinder : PoolBinder<IUnit, IUnit>, IUnitBinder
{
    [Inject]
    public IInjectionBinder InjectionBinder { get; private set; }

    public override IBinding GetRawBinding()
    {
        UnitBinding binding = new UnitBinding(resolver);
        InjectionBinder.injector.Inject(binding, false);
        return binding;
    }

    new public virtual IUnitBinding Bind<T>() where T : IUnit
    {
        // Bind unit type
        InjectionBinder.Bind<T>().To<T>();

        // Bind unit pool
        InjectionBinder.Bind<IPool<T>>().To<Pool<T>>().ToSingleton();

        return base.Bind<T>().To(InjectionBinder.GetInstance<IPool<T>>()) as IUnitBinding;
    }

    new public virtual IUnitBinding GetBinding<T>() where T : IUnit
    {
        return base.GetBinding<T>() as IUnitBinding;
    }

    new public virtual IUnitBinding GetBinding(object key)
    {
        return base.GetBinding(key) as IUnitBinding;
    }
}
