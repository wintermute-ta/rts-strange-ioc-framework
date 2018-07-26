using Core;
using GameWorld.HexMap;
using Signals;
using strange.extensions.command.impl;
using UnityEngine;
using Views.HexGrid;
using Views.Settings;

namespace Commands
{
    public class InitialInstanceCommand : Command
    {
        #region Public
        [Inject]
        public IGlobalContext Context { get; set; }
        [Inject]
        public IGameManager GameManager { get; private set; }
        [Inject]
        public IResourceManager ResourceManager { get; private set; }
        [Inject]
        public IHexMap Map { get; private set; }
        [Inject]
        public IHexGridUtility GridUtility { get; private set; }
        [Inject]
        public HexMapCreatedSignal HexMapCreated { get; private set; }

        public override void Execute()
        {
            InstanceMainCamera();
            GridUtility.CreateInstance();
            HexMapCameraView cameraView = Context.InstancePrefabView<HexMapCameraView>(ResourceManager.GetPrefab("HexMapCamera"));
            Map.CreateMap(GridUtility.CellCountX, GridUtility.CellCountZ);
            HexMapCreated.Dispatch();
            InstanceUI();
            InstanceSettingsEditor();
        }

        private void InstanceSettingsEditor()
        {
            // Settings editor
            GameObject settings = new GameObject("Settings");
            Context.InstanceComponent<UnitSettingsView>(settings);
            Context.InstanceComponent<WeaponSettingsView>(settings);
        }

        private void InstanceMainCamera()
        {
            Context.InstancePrefab<Camera>(ResourceManager.GetPrefab("MainCamera"));
        }

        private void InstanceUI()
        {
            Transform transformUI = GameObject.Instantiate(ResourceManager.GetPrefab("UI")).transform;
            Transform transformUIEditor = GameObject.Instantiate(ResourceManager.GetPrefab("UIEditor")).transform;
            if (transformUIEditor != null)
            {
                transformUIEditor.SetParent(transformUI, true);

                //Transform transformHexMapEditor = transformUIEditor.FindChild("HexMapEditor");
                //if (transformHexMapEditor != null)
                //{
                //    globalContext.InstanceComponent<HexMapEditorView>(transformHexMapEditor.gameObject);
                //}
            }
        }
        #endregion
    }
}
