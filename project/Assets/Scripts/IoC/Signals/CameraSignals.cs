using strange.extensions.signal.impl;
using UnityEngine;

namespace Signals
{
    namespace Camera
    {
        public class ValidatePosition : Signal { }
        public class Lock : Signal<bool> { }
        public class Pan : Signal { }
        public class Rotate : Signal { }
        public class Zoom : Signal { }
        public class Move : Signal<Vector3> { }
    }
}
