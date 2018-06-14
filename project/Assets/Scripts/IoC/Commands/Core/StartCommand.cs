using strange.extensions.command.impl;
using strange.extensions.context.api;
using strange.extensions.pool.api;
using strange.extensions.pool.impl;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartCommand : Command
{
    #region Public
    //[Inject(ContextKeys.CONTEXT_VIEW)]
    //public GameObject ContextView { get; private set; }
    [Inject]
    public IResourceManager ResourceManager { get; private set; }

    public override void Execute()
    {
        // Mediation binding
        GlobalContext.Get().MediationBinder.Bind<HexMapCameraView>().To<HexMapCameraMediator>();
        GlobalContext.Get().MediationBinder.Bind<HexGridView>().To<HexGridMediator>();
        GlobalContext.Get().MediationBinder.Bind<HexMapEditorView>().To<HexMapEditorMediator>();
        GlobalContext.Get().MediationBinder.Bind<ShipView>().To<ShipMediator>();
        GlobalContext.Get().MediationBinder.Bind<GunView>().To<GunMediator>();
        GlobalContext.Get().MediationBinder.Bind<CastleView>().To<CastleMediator>();

        injectionBinder.Bind<IWorld>().To<World>().ToSingleton();
        injectionBinder.Bind<InstanceShip>().To<InstanceShip>();
        injectionBinder.Bind<InstanceGun>().To<InstanceGun>();
        injectionBinder.Bind<InstanceCastle>().To<InstanceCastle>();
        injectionBinder.Bind<IWeapon>().To<WeaponShipCannon>().ToName<WeaponShipCannon>();
        injectionBinder.Bind<IWeapon>().To<WeaponGroundCannon>().ToName<WeaponGroundCannon>();
        injectionBinder.Bind<IWeapon>().To<WeaponCastleCannon>().ToName<WeaponCastleCannon>();
        injectionBinder.Bind<IHexMap>().To<HexMap>().ToSingleton();
        injectionBinder.Bind<IPool<SaveLoadItem>>().To<Pool<SaveLoadItem>>().ToSingleton();
        injectionBinder.Bind<IPool<InstanceShip>>().To<Pool<InstanceShip>>().ToSingleton();
        injectionBinder.Bind<IPool<InstanceGun>>().To<Pool<InstanceGun>>().ToSingleton();
        injectionBinder.Bind<IPool<InstanceCastle>>().To<Pool<InstanceCastle>>().ToSingleton();
        injectionBinder.Bind<IPool<ShipView>>().To<Pool<ShipView>>().ToSingleton();
        injectionBinder.Bind<IPool<GunView>>().To<Pool<GunView>>().ToSingleton();
        injectionBinder.Bind<IPool<CastleView>>().To<Pool<CastleView>>().ToSingleton();
        injectionBinder.Bind<IPool<Shot>>().To<Pool<Shot>>().ToSingleton();
        injectionBinder.Bind<IGameManager>().To<GameManager>().ToSingleton();
        injectionBinder.Bind<IHexGridUtility>().To<HexGridUtility>().ToSingleton();

        IPool<SaveLoadItem> poolSaveLoadItem = injectionBinder.GetInstance<IPool<SaveLoadItem>>();
        poolSaveLoadItem.instanceProvider = new PrefabInstanceProvider(ResourceManager.GetPrefab("SaveLoadItem"));
        poolSaveLoadItem.inflationType = PoolInflationType.INCREMENT;

        IPool<ShipView> poolShipView = injectionBinder.GetInstance<IPool<ShipView>>();
        poolShipView.instanceProvider = new PrefabInstanceProvider(ResourceManager.GetPrefab("Unit"));
        poolShipView.inflationType = PoolInflationType.INCREMENT;

        IPool<GunView> poolGunView = injectionBinder.GetInstance<IPool<GunView>>();
        poolGunView.instanceProvider = new PrefabInstanceProvider(ResourceManager.GetPrefab("cannon_upgrade_01"));
        poolGunView.inflationType = PoolInflationType.INCREMENT;

        IPool<CastleView> poolCastleView = injectionBinder.GetInstance<IPool<CastleView>>();
        poolCastleView.instanceProvider = new PrefabInstanceProvider(ResourceManager.GetPrefab("Watchtower_Castle"));
        poolCastleView.inflationType = PoolInflationType.INCREMENT;

        IPool<Shot> poolShot = injectionBinder.GetInstance<IPool<Shot>>();
        poolShot.instanceProvider = new PrefabInstanceProvider(ResourceManager.GetPrefab("Bullet"));
        poolShot.inflationType = PoolInflationType.INCREMENT;

        commandBinder.Bind<CameraValidatePositionSignal>();
        commandBinder.Bind<LockMapCameraSignal>();
        commandBinder.Bind<ZoomMapCameraSignal>();
        commandBinder.Bind<RotateMapCameraSignal>();
        commandBinder.Bind<PanMapCameraSignal>();
        commandBinder.Bind<HexMapCreatedSignal>();
        commandBinder.Bind<HexGridCellSelectionSignal>();
        commandBinder.Bind<HexGridCellTouchSignal>();
        commandBinder.Bind<HexGridCellTouchHoldSignal>();
        commandBinder.Bind<HexGridInteractableSignal>();
        commandBinder.Bind<HexGridChangeUIVisibleSignal>();
        commandBinder.Bind<ShipDestroySignal>();
        commandBinder.Bind<InitGameSignal>().InSequence().To<InitialInstanceCommand>().To<LoadDefaultMapCommand>().To<InitGameCommand>().Once();
        injectionBinder.GetInstance<InitGameSignal>().Dispatch();
    }
    #endregion
}
