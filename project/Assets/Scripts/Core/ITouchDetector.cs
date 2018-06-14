using strange.extensions.signal.impl;
using System.Collections.Generic;
using UnityEngine;

public interface ITouchDetector
{
    Signal<ITouchData> OnTouch { get; }
    Signal<ITouchData> OnTap { get; }
    Signal<ITouchData> OnLongPress { get; }
    Signal<ITouchData> OnPan { get; }
    Signal<float> OnZoom { get; }
    Signal<float> OnRotate { get; }
    float LongPressTimeout { get; set; }
    Vector2 PanningThreshold { get; set; }
    float ZoomingThreshold { get; set; }
    float RotatingThreshold { get; set; }
    string DebugTag { get; set; }

    void Update(List<ITouchData> touches);
}
