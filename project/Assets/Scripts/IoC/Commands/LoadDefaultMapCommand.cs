using Core;
using GameWorld.HexMap;
using Signals;
using strange.extensions.command.impl;

namespace Commands
{
    public class LoadDefaultMapCommand : Command
    {
        #region Public
        [Inject]
        public IResourceManager ResourceManager { get; private set; }
        [Inject]
        public ICoroutineCreator Coroutines { get; private set; }
        [Inject]
        public IHexMap Map { get; private set; }
        [Inject]
        public HexMapCreatedSignal HexMapCreated { get; private set; }

        public override void Execute()
        {
            Retain();
            Coroutines.DelayedAction(OnLoadMap);
        }

        private void OnLoadMap()
        {
            Map.LoadMap(ResourceManager.GetMap("default.map"));
            HexMapCreated.Dispatch();
            Release();
        }
        #endregion
    }
}
