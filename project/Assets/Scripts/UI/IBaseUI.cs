using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBaseUI {

    void Init(GameObject window, bool globalSignals = false);
    void Open();
    void Shutdown();

    void Destroy(); //destroy this view
    void OnCreate();
    void OnDestroy();

    void OnAppeared();
    void OnDisappeared();
}
