using Core;
using Core.InstanceProviders;
using strange.extensions.injector.api;
using strange.extensions.mediation.api;
using strange.extensions.mediation.impl;
using strange.extensions.pool.api;
using strange.extensions.pool.impl;
using strange.framework.api;
using strange.framework.impl;
using Views.Units;

public class UnitViewBinding : Binding, IUnitViewBinding
{
    [Inject]
    public IInjectionBinder InjectionBinder { get; private set; }
    [Inject]
    public IMediationBinder MediationBinder { get; private set; }
    [Inject]
    public IResourceManager ResourceManager { get; private set; }

    public UnitViewBinding(Binder.BindingResolver resolver) : base(resolver)
    {
        keyConstraint = BindingConstraintType.ONE;
        valueConstraint = BindingConstraintType.ONE;
    }

    public IUnitViewBinding ToView<TView, TMediator>(string prefab) where TView : BaseUnitView where TMediator : Mediator
    {
        // Mediation
        MediationBinder.Bind<TView>().To<TMediator>();

        // Bind view pool
        InjectionBinder.Bind<IPool<TView>>().To<Pool<TView>>().ToSingleton();

        // Set view pool parameters
        IPool<TView> poolUnit = InjectionBinder.GetInstance<IPool<TView>>();
        poolUnit.instanceProvider = new PrefabInstanceProvider(ResourceManager.GetPrefab(prefab));
        poolUnit.inflationType = PoolInflationType.INCREMENT;

        return base.To(poolUnit) as IUnitViewBinding;
    }
}
