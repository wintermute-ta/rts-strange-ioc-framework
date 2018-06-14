using System.Collections;
using System.Collections.Generic;
using strange.extensions.signal.impl;
using UnityEngine;

public class UIGlobalSignals {
    public Signal OpenMenuSignal { get { return _openMenuSignal; } }
    public Signal CloseMenuSinal { get{ return _closeMenuSignal; } }
    public Signal<BaseUIHandler> ReturnToPool { get { return _returnToPool; } }
    private Signal _openMenuSignal = new Signal();
    private Signal _closeMenuSignal = new Signal();
    private Signal<BaseUIHandler> _returnToPool = new Signal<BaseUIHandler>();
}
