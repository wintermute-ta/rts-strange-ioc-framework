using UnityEngine;
using strange.extensions.context.api;
using strange.extensions.context.impl;
using strange.extensions.signal.impl;
using strange.extensions.command.api;
using strange.extensions.command.impl;

public class SignalsContext<TSignal, TCommand> : MVCSContext where TSignal : Signal where TCommand : Command
{
    #region Constructors
    public SignalsContext(MonoBehaviour contextView) : base(contextView) { }
    public SignalsContext(MonoBehaviour contextView, ContextStartupFlags flags) : base(contextView, flags) { }
	#endregion
	
	#region Public
	public override IContext Start()
	{
        base.Start();
        TSignal startSignal = injectionBinder.GetInstance<TSignal>();
        startSignal.Dispatch();
        return this;
    }
    #endregion

    #region Protected
    protected override void addCoreComponents()
	{
		base.addCoreComponents();

        injectionBinder.Unbind<ICommandBinder>();
        injectionBinder.Bind<ICommandBinder>().To<SignalCommandBinder>().ToSingleton();
    }

    protected override void mapBindings()
	{
		base.mapBindings();
        commandBinder.Bind<TSignal>().To<TCommand>().Once();

        //implicitBinder.ScanForAnnotatedClasses(new string[] { "" });
    }
    #endregion
}
