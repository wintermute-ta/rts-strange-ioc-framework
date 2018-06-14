using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IFPSCounter
{
    bool ShowFPS { get; set; }
    float UpdateTimeout { get; set; }
    Color TextColor { get; set; }
    TextAnchor Alignment { get; set; }
    int FontSize { get; set; }
    string Format { get; set; }
}
