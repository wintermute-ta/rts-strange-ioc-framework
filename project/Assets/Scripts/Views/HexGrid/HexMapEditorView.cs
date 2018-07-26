using UnityEngine;
using strange.extensions.mediation.impl;
using strange.extensions.signal.impl;

namespace Views
{
    namespace HexGrid
    {
        public class HexMapEditorView : View
        {
            public Signal<int, int> OnCreateMap = new Signal<int, int>();
            public Signal<string> OnLoadMap = new Signal<string>();
            public Signal<string> OnSaveMap = new Signal<string>();
            public Signal OnOpenPopup = new Signal();
            public Signal OnClosePopup = new Signal();
            public Signal<float> OnChangeBrushSize = new Signal<float>();
            public Signal<bool> OnChangeUIVisible = new Signal<bool>();

            public NewMapMenu newMapMenu;
            public SaveLoadMenu saveLoadMenu;

            public bool InEditMode { get; private set; }
            private GameObject panelLeft;
            private GameObject panelRight;
            public int ActiveElevation { get; private set; }
            public int ActiveWaterLevel { get; private set; }

            public int ActiveUrbanLevel { get; private set; }
            public int ActiveFarmLevel { get; private set; }
            public int ActivePlantLevel { get; private set; }
            public int ActiveSpecialIndex { get; private set; }

            public int ActiveTerrainTypeIndex { get; private set; }

            public bool ApplyElevation { get; private set; }
            public bool ApplyWaterLevel { get; private set; }

            public bool ApplyUrbanLevel { get; private set; }
            public bool ApplyFarmLevel { get; private set; }
            public bool ApplyPlantLevel { get; private set; }
            public bool ApplySpecialIndex { get; private set; }

            public enum OptionalToggle
            {
                Ignore, Yes, No
            }

            public OptionalToggle RiverMode { get; private set; }
            public OptionalToggle RoadMode { get; private set; }
            public OptionalToggle WalledMode { get; private set; }
            public OptionalToggle SpawnZoneMode { get; private set; }

            public void SetTerrainTypeIndex(int index)
            {
                ActiveTerrainTypeIndex = index;
            }

            public void SetApplyElevation(bool toggle)
            {
                ApplyElevation = toggle;
            }

            public void SetElevation(float elevation)
            {
                ActiveElevation = (int)elevation;
            }

            public void SetApplyWaterLevel(bool toggle)
            {
                ApplyWaterLevel = toggle;
            }

            public void SetWaterLevel(float level)
            {
                ActiveWaterLevel = (int)level;
            }

            public void SetApplyUrbanLevel(bool toggle)
            {
                ApplyUrbanLevel = toggle;
            }

            public void SetUrbanLevel(float level)
            {
                ActiveUrbanLevel = (int)level;
            }

            public void SetApplyFarmLevel(bool toggle)
            {
                ApplyFarmLevel = toggle;
            }

            public void SetFarmLevel(float level)
            {
                ActiveFarmLevel = (int)level;
            }

            public void SetApplyPlantLevel(bool toggle)
            {
                ApplyPlantLevel = toggle;
            }

            public void SetPlantLevel(float level)
            {
                ActivePlantLevel = (int)level;
            }

            public void SetApplySpecialIndex(bool toggle)
            {
                ApplySpecialIndex = toggle;
            }

            public void SetSpecialIndex(float index)
            {
                ActiveSpecialIndex = (int)index;
            }

            public void SetBrushSize(float size)
            {
                OnChangeBrushSize.Dispatch(size);
            }

            public void SetRiverMode(int mode)
            {
                RiverMode = (OptionalToggle)mode;
            }

            public void SetRoadMode(int mode)
            {
                RoadMode = (OptionalToggle)mode;
            }

            public void SetWalledMode(int mode)
            {
                WalledMode = (OptionalToggle)mode;
            }

            public void SetSpawnZoneMode(int mode)
            {
                SpawnZoneMode = (OptionalToggle)mode;
            }

            public void ShowUI(bool visible)
            {
                OnChangeUIVisible.Dispatch(visible);
            }

            protected override void Awake()
            {
                panelLeft = transform.FindChild("Left Panel").gameObject;
                panelRight = transform.FindChild("Right Panel").gameObject;

                newMapMenu.OnOpen += Menu_OnOpen;
                newMapMenu.OnClose += Menu_OnClose;
                newMapMenu.OnCreateMap += NewMapMenu_OnCreateMap;

                saveLoadMenu.OnLoad += SaveLoadMenu_OnLoad;
                saveLoadMenu.OnSave += SaveLoadMenu_OnSave;
                saveLoadMenu.OnOpen += Menu_OnOpen;
                saveLoadMenu.OnClose += Menu_OnClose;

                ApplyElevation = true;
                ApplyWaterLevel = true;

                OnEditModeChanged();
            }

            private void NewMapMenu_OnCreateMap(int x, int z)
            {
                OnCreateMap.Dispatch(x, z);
            }

            private void SaveLoadMenu_OnLoad(string path)
            {
                OnLoadMap.Dispatch(path);
            }

            private void SaveLoadMenu_OnSave(string path)
            {
                OnSaveMap.Dispatch(path);
            }

            private void Menu_OnOpen()
            {
                OnOpenPopup.Dispatch();
            }

            private void Menu_OnClose()
            {
                OnClosePopup.Dispatch();
            }

            private void OnEditModeChanged()
            {
                if (panelLeft != null)
                {
                    panelLeft.SetActive(InEditMode);
                }
                if (panelRight != null)
                {
                    panelRight.SetActive(InEditMode);
                }
            }

            public void SwitchEditMode()
            {
                InEditMode = !InEditMode;

                OnEditModeChanged();
            }
        }
    }
}
