using strange.extensions.signal.impl;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ILifeCycle
{
    Signal OnBackPressed { get; }
    Signal<float> OnUpdate { get; }
    Signal<float> OnFixedUpdate { get; }
}
