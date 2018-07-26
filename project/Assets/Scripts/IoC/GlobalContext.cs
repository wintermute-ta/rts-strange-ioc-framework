using UnityEngine;
using strange.extensions.context.api;
using strange.extensions.pool.api;
using strange.extensions.pool.impl;
using strange.extensions.mediation.impl;
using strange.extensions.mediation.api;
using Core;
using Core.InstanceProviders;
using Signals;
using Commands;
using System;

public class GlobalContext : SignalsContext<StartSignal, StartCommand>, IGlobalContext
{
    [Obsolete("Static GlobalContext has been deprecated. Use injections instead GlobalContext.Get().")]
    public static GlobalContext Get() { return staticContext; }
    private static GlobalContext staticContext;

    #region Constructors
    public GlobalContext(MonoBehaviour contextView) : base(contextView) { }
    public GlobalContext(MonoBehaviour contextView, ContextStartupFlags flags) : base(contextView, flags) { }
    #endregion

    #region Public
    /// <summary>
    /// </summary>
    public T GetInstance<T>()
    {
        return injectionBinder.GetInstance<T>();
    }

    public T GetInstance<T>(object name)
    {
        return injectionBinder.GetInstance<T>(name);
    }

    public IMediationBinder MediationBinder { get { return mediationBinder; } }
    /// <summary>
    /// Use this method to create an instance of a type T with injections
    /// </summary>
    public T Instance<T>() where T : class, new() //use this method instead new
    {
        T instance = new T();
        if (instance != null)
        {
            injectionBinder.injector.Inject(instance);
        }
        return instance;
    }

    /// <summary>
    /// Use this method to create an instance of a type T with injections
    /// </summary>
    public T InstanceComponent<T>(GameObject objectForAddComponent) where T : Component, new() //use this method instead new
    {
        T instance = objectForAddComponent.GetComponent<T>();
        if (instance == null)
        {
            instance = objectForAddComponent.AddComponent<T>();
        }
        injectionBinder.injector.Inject(instance);
        return instance;
    }

    public T InstanceScriptableObject<T>() where T : ScriptableObject //use this method instead new
    {
        T instance = ScriptableObject.CreateInstance<T>();
        injectionBinder.injector.Inject(instance);
        return instance;
    }

    public T InstancePrefab<T>(GameObject prefab, Transform parent = null) where T : Component, new() //use this method instead new
    {
        GameObject objInstance = GameObject.Instantiate(prefab, parent);
        return InstanceComponent<T>(objInstance);
    }

    public T InstancePrefabView<T>(GameObject prefab, Transform parent = null) where T : View, new() //use this method instead new
    {
        GameObject objInstance = GameObject.Instantiate(prefab, parent);
        return objInstance.GetComponent<T>();
    }
    #endregion

    #region Protected
    // Adds MonoBehaviour as a component to GlobalContext view's gameObject for further binding
    private T InstanceComponent<T>() where T : Component, new() //use this method instead new
    {
        GameObject gameObject = GetContextView() as GameObject;
        if (gameObject == null)
            return null;
        T instance = gameObject.GetComponent<T>();
        if (instance == null)
        {
            instance = gameObject.AddComponent<T>();
        }

        return instance;
    }

    protected override void addCoreComponents()
    {
        base.addCoreComponents();

        // Add your core bindings here.
        injectionBinder.Bind<IGlobalContext>().ToValue(this).ToSingleton();
    }

    // Use this for initialization
    protected override void mapBindings()
    {
        base.mapBindings();

        // Add your project specific bindings here.
        staticContext = this;

        // Mediation binding

        // Injection binding
        injectionBinder.Bind<IDebugConsole>().ToValue(InstanceComponent<DebugConsole>()).ToSingleton();
        injectionBinder.Bind<ICoroutineCreator>().ToValue(InstanceComponent<CoroutineCreator>()).ToSingleton();
        injectionBinder.Bind<ILifeCycle>().ToValue(InstanceComponent<LifeCycle>()).ToSingleton();
        injectionBinder.Bind<IFPSCounter>().ToValue(InstanceComponent<FPSCounter>()).ToSingleton();
        injectionBinder.Bind<TouchData>().To<TouchData>();
        injectionBinder.Bind<IPool<TouchData>>().To<Pool<TouchData>>().ToSingleton();
        injectionBinder.Bind<ITouchDetector>().To<TouchDetector>();
        injectionBinder.Bind<IInputManager>().ToValue(InstanceComponent<InputManager>()).ToSingleton();
        injectionBinder.Bind<IResourceManager>().To<ResourceManager>().ToSingleton();

        injectionBinder.Bind<UIGlobalSignals>().ToValue(Instance<UIGlobalSignals>()).ToSingleton();
        injectionBinder.Bind<IUIManager>().ToValue(Instance<UIManager>()).ToSingleton();
        injectionBinder.Bind<IPool<GameObject>>().To<Pool<GameObject>>();

        injectionBinder.Bind<GameObjectInstanceProvider>().To<GameObjectInstanceProvider>();

        // Command binding
        commandBinder.Bind<InitGameCompleteSignal>().Once();
    }

    protected override void postBindings()
    {
        base.postBindings();

        injectionBinder.injector.Inject(injectionBinder.GetInstance<IInputManager>());
    }
    #endregion
}
