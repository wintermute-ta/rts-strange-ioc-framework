using strange.extensions.command.impl;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitialInstanceCommand : Command
{
    #region Public
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
        HexMapCameraView cameraView = GlobalContext.Get().InstancePrefabView<HexMapCameraView>(ResourceManager.GetPrefab("HexMapCamera"));
        Map.CreateMap(GridUtility.CellCountX, GridUtility.CellCountZ);
        HexMapCreated.Dispatch();
        InstanceUI();
    }

    private void InstanceMainCamera()
    {
        GlobalContext.Get().InstancePrefab<Camera>(ResourceManager.GetPrefab("MainCamera"));
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
            //    GlobalContext.Get().InstanceComponent<HexMapEditorView>(transformHexMapEditor.gameObject);
            //}
        }
    }
    #endregion
}
