using System;
using Core;
using Core.InstanceProviders;
using GameWorld;
using GameWorld.HexMap;
using GameWorld.Settings;
using GameWorld.Units;
using GameWorld.Weapons;
using Signals;
using strange.extensions.command.impl;
using strange.extensions.pool.api;
using strange.extensions.pool.impl;
using Views.HexGrid;
using Views.Shots;
using Views.Units;
using strange.extensions.mediation.impl;
using strange.extensions.mediation.api;

namespace Commands
{
    public class StartCommand : Command
    {
        #region Public
        //[Inject(ContextKeys.CONTEXT_VIEW)]
        //public GameObject ContextView { get; private set; }
        [Inject]
        public IResourceManager ResourceManager { get; private set; }
        [Inject]
        public IMediationBinder MediationBinder { get; private set; }

        public override void Execute()
        {
            // Mediation binding
            MediationBinder.Bind<HexMapCameraView>().To<HexMapCameraMediator>();
            MediationBinder.Bind<HexGridView>().To<HexGridMediator>();
            MediationBinder.Bind<HexMapEditorView>().To<HexMapEditorMediator>();

            injectionBinder.Bind<IUnitBinder>().To<UnitBinder>().ToSingleton();
            injectionBinder.Bind<IUnitViewBinder>().To<UnitViewBinder>().ToSingleton();
            IUnitBinder unitBinder = injectionBinder.GetInstance<IUnitBinder>();

            WeaponBinding<WeaponSmallCannon, SmallCannonSettings>();
            WeaponBinding<WeaponMediumCannon, MediumCannonSettings>();
            WeaponBinding<WeaponLargeCannon, LargeCannonSettings>();

            injectionBinder.Bind<IUnitSettings>().Bind<INavalUnitSettings>().To<ShipSettings>().ToName<ShipSettings>().ToSingleton();
            injectionBinder.Bind<IUnitSettings>().Bind<IGroundUnitSettings>().To<GroundCannonSettings>().ToName<GroundCannonSettings>().ToSingleton();
            injectionBinder.Bind<IUnitSettings>().Bind<IGroundUnitSettings>().To<FortSettings>().ToName<FortSettings>().ToSingleton();

            unitBinder.Bind<Ship>().ToView<ShipView, ShipMediator>("Ship");
            unitBinder.Bind<GroundCannon>().ToView<GroundCannonView, GroundCannonMediator>("GroundCannon01");
            unitBinder.Bind<Fort>().ToView<FortView, FortMediator>("Fort");

            PrefabBinding<RoundShotView, RoundShotMediator>("RoundShot");

            injectionBinder.Bind<IWorld>().To<World>().ToSingleton();
            injectionBinder.Bind<IHexMap>().To<HexMap>().ToSingleton();
            injectionBinder.Bind<IPool<SaveLoadItem>>().To<Pool<SaveLoadItem>>().ToSingleton();
            injectionBinder.Bind<IGameManager>().To<GameManager>().ToSingleton();
            injectionBinder.Bind<IHexGridUtility>().To<HexGridUtility>().ToSingleton();

            IPool<SaveLoadItem> poolSaveLoadItem = injectionBinder.GetInstance<IPool<SaveLoadItem>>();
            poolSaveLoadItem.instanceProvider = new PrefabInstanceProvider(ResourceManager.GetPrefab("SaveLoadItem"));
            poolSaveLoadItem.inflationType = PoolInflationType.INCREMENT;

            commandBinder.Bind<Signals.Settings.Changed>();
            commandBinder.Bind<Signals.Camera.ValidatePosition>();
            commandBinder.Bind<Signals.Camera.Lock>();
            commandBinder.Bind<Signals.Camera.Zoom>();
            commandBinder.Bind<Signals.Camera.Rotate>();
            commandBinder.Bind<Signals.Camera.Pan>();
            commandBinder.Bind<Signals.Camera.Move>();
            commandBinder.Bind<Signals.HexMapCreatedSignal>();
            commandBinder.Bind<Signals.HexGrid.CellSelection>();
            commandBinder.Bind<Signals.HexGrid.CellTouch>();
            commandBinder.Bind<Signals.HexGrid.CellTouchHold>();
            commandBinder.Bind<Signals.HexGrid.Interactable>();
            commandBinder.Bind<Signals.HexGrid.ChangeUIVisible>();
            commandBinder.Bind<Signals.HexGrid.CellTouchTap>();
            commandBinder.Bind<Signals.Units.Weapon.Fire>();
            commandBinder.Bind<Signals.Units.Weapon.ChangeTarget>();
            commandBinder.Bind<Signals.Units.Weapon.ShotReachTarget>();
            commandBinder.Bind<Signals.Units.Attack>();
            commandBinder.Bind<Signals.Units.ChangeTarget>();
            commandBinder.Bind<Signals.Units.Destroy>();
            commandBinder.Bind<Signals.Units.HitDamage>();
            commandBinder.Bind<Signals.Units.NavalUnit.MoveToCell>();
            commandBinder.Bind<Signals.Units.NavalUnit.ReachTarget>();
            commandBinder.Bind<Signals.Units.NavalUnit.DestinationChanged>();
            commandBinder.Bind<Signals.Units.NavalUnit.ViewPositionChanged>();
            commandBinder.Bind<InitGameSignal>().InSequence().To<InitialInstanceCommand>().To<LoadDefaultMapCommand>().To<InitGameCommand>().Once();
            injectionBinder.GetInstance<InitGameSignal>().Dispatch();
        }

        private void WeaponBinding<TWeapon, TSettings>() where TWeapon : Weapon where TSettings : WeaponSettings
        {
            // Bind settings
            injectionBinder.Bind<IWeaponSettings>().To<TSettings>().ToName<TSettings>().ToSingleton();

            // Bind weapon type
            injectionBinder.Bind<IWeapon>().To<TWeapon>().ToName<TWeapon>();
        }

        private void PrefabBinding<TView, TMediator>(string prefab) where TView : View where TMediator : Mediator
        {
            // Mediation
            MediationBinder.Bind<TView>().To<TMediator>();

            // Bind view pool
            injectionBinder.Bind<IPool<TView>>().To<Pool<TView>>().ToSingleton();

            // Set view pool parameters
            IPool<TView> poolUnit = injectionBinder.GetInstance<IPool<TView>>();
            poolUnit.instanceProvider = new PrefabInstanceProvider(ResourceManager.GetPrefab(prefab));
            poolUnit.inflationType = PoolInflationType.INCREMENT;
        }
        #endregion
    }
}
