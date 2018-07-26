using Signals;
using strange.extensions.command.impl;

namespace Commands
{
    public class InitGameCommand : Command
    {
        #region Public
        [Inject]
        public InitGameCompleteSignal InitGameComplete { get; private set; }

        public override void Execute()
        {
            InitGameComplete.Dispatch();
        }
        #endregion
    }
}
