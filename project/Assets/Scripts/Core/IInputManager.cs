using strange.extensions.signal.impl;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInputManager
{
    TouchSignal OnPointerDown { get; }
    TouchSignal OnPointerUp { get; }
    List<ITouchData> Touches { get; }
    float TouchSensetivity { get; set; }
    bool MouseSupported { get; }
    Vector3 MousePosition { get; }
}
