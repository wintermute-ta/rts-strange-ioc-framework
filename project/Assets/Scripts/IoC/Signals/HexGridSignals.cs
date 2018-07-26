using strange.extensions.signal.impl;
using Views.HexGrid;

namespace Signals
{
    namespace HexGrid
    {
        public class CellSelection : Signal<HexGridCell, bool> { }
        public class CellTouchHold : Signal<HexGridCell> { }
        public class CellTouchTap : Signal<HexGridCell> { }
        public class CellTouch : Signal<HexGridCell> { }
        public class ChangeUIVisible : Signal<bool> { }
        public class Interactable : Signal<bool> { }
    }
}
