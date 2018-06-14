using System.Collections;
using System.Collections.Generic;
using strange.extensions.mediation.impl;
using strange.extensions.signal.impl;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class BaseUIHandler : View {

    public bool IsAppeared { get; set; }
    public virtual string Name { get { return gameObject.name; } }

    public Signal AppearSignal = new Signal();
    public Signal DisappearSignal = new Signal();

    public virtual void Init() { }
    public virtual void ReInit() { }

    public virtual void Open()
    {
        gameObject.SetActive(true);
        StartAppearing();
    }

    public virtual void Close()
    {
        StartDisappearing();
    }

    protected virtual void StartAppearing()
    {
        OnAppear();
    }

    protected virtual void StartDisappearing()
    {
        OnDisappear();
    }

    public virtual void OnAppear()
    {
        IsAppeared = true;
        AppearSignal.Dispatch();
    }

    public virtual void OnDisappear()
    {
        IsAppeared = false;
        DisappearSignal.Dispatch();
    }
}
