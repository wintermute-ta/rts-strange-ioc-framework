using GameWorld.Units;
using strange.extensions.mediation.impl;
using strange.extensions.pool.api;
using strange.framework.api;
using Views.Units;

public interface IUnitBinding
{
    IUnitBinding ToView<TView, TMediator>(string prefab) where TView : BaseUnitView where TMediator : Mediator;
}
