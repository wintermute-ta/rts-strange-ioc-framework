using strange.extensions.mediation.impl;
using strange.extensions.pool.api;
using Views.Units;

public interface IUnitViewBinding
{
    IUnitViewBinding ToView<TView, TMediator>(string prefab) where TView : BaseUnitView where TMediator : Mediator;
}
