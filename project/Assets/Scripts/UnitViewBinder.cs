using GameWorld.Units;
using strange.extensions.injector.api;
using strange.extensions.pool.api;
using strange.framework.api;
using Views.Units;

public class UnitViewBinder : PoolBinder<IUnit, BaseUnitView>, IUnitViewBinder
{
    [Inject]
    public IInjectionBinder InjectionBinder { get; private set; }

    public override IBinding GetRawBinding()
    {
        IBinding binding = new UnitViewBinding(resolver);
        InjectionBinder.injector.Inject(binding, false);
        return binding;
    }

    public override void ReturnInstance(BaseUnitView instance)
    {
        IPool pool = GetPool(instance.Unit.GetType());
        if (pool != null)
        {
            pool.ReturnInstance(instance);
        }
    }

    new public virtual IUnitViewBinding Bind<T>() where T : IUnit
    {
        return base.Bind<T>() as IUnitViewBinding;
    }

    new public virtual IUnitViewBinding Bind(object value)
    {
        return base.Bind(value) as IUnitViewBinding;
    }

    new public virtual IUnitViewBinding GetBinding<T>() where T : IUnit
    {
        return base.GetBinding<T>() as IUnitViewBinding;
    }

    new public virtual IUnitViewBinding GetBinding(object key)
    {
        return base.GetBinding(key) as IUnitViewBinding;
    }
}
