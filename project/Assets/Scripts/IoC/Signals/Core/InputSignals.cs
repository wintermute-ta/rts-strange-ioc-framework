using Core;
using strange.extensions.signal.impl;

namespace Signals
{
    public class TouchSignal : Signal<ITouchData> { }
    public class PointerDown : TouchSignal { }
    public class PointerUp : TouchSignal { }
}
