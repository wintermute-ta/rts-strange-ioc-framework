using strange.extensions.injector.api;
using strange.extensions.mediation.impl;
using strange.framework.api;
using strange.framework.impl;
using Views.Units;

public class UnitBinding : Binding, IUnitBinding
{
    [Inject]
    public IInjectionBinder InjectionBinder { get; private set; }
    [Inject]
    public IUnitViewBinder ViewBinder { get; private set; }

    public UnitBinding(Binder.BindingResolver resolver) : base(resolver)
    {
        keyConstraint = BindingConstraintType.ONE;
        valueConstraint = BindingConstraintType.ONE;
    }

    public IUnitBinding ToView<TView, TMediator>(string prefab) where TView : BaseUnitView where TMediator : Mediator
    {
        ViewBinder.Bind(key).ToView<TView, TMediator>(prefab);

        return this;
    }
}
