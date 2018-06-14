using System;
using System.Collections;
using System.Collections.Generic;
using strange.extensions.signal.impl;
using UnityEngine;

public class BaseUIModel : IBaseUI
{
    public Signal DesrtoySinal { get; set; }

    public BaseUIHandler Handler { get; set; }

    [Inject]
    public IUIManager UIManager { get; private set; }
    [Inject]
    public UIGlobalSignals UISignals { get; private set; }

    //If true - model will Dispatch global Signals, otherwise will not;
    private bool haveGlobalSignals;

    public virtual void Destroy()
    {
        if(Handler != null)
        {
            Handler.AppearSignal.RemoveListener(OnAppeared);
            Handler.DisappearSignal.RemoveListener(OnDisappeared);
            UIManager.ReturnToPool(Handler);
            Handler = null;
        }
        OnDestroy();
    }

    public virtual void OnDestroy()
    {
        if (haveGlobalSignals)
        {
            UISignals.CloseMenuSinal.Dispatch();
        }
    }

    public virtual void Open()
    {
        if (Handler != null)
        {
            if (haveGlobalSignals)
            {
                UISignals.OpenMenuSignal.Dispatch();
            }
            Handler.Open();
        }
    }

    public void Shutdown()
    {
        if (Handler != null)
        {
            Handler.Close();
        }      
    }

    public virtual void Init(GameObject window, bool globalSignals = false)
    {
        haveGlobalSignals = globalSignals;
        Handler = window.GetComponent<BaseUIHandler>();
        Handler.ReInit();
        Handler.AppearSignal.AddListener(OnAppeared);
        Handler.DisappearSignal.AddListener(OnDisappeared);
    }

    public virtual void OnDisappeared()
    {
        Destroy();
    }

    public virtual void OnAppeared() { }
    public virtual void OnCreate() { }
}
