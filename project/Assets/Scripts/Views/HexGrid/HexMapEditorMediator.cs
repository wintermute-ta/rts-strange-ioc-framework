using strange.extensions.mediation.impl;
using UnityEngine;
using UnityEngine.EventSystems;
using Signals.Camera;
using GameWorld.HexMap;
using Signals;

namespace Views
{
    namespace HexGrid
    {
        public class HexMapEditorMediator : Mediator
        {
            [Inject]
            public HexMapEditorView Editor { get; private set; }
            [Inject]
            public IHexMap Map { get; private set; }
            [Inject]
            public ValidatePosition ValidatePositionSignal { get; private set; }
            [Inject]
            public Lock LockMapCameraSignal { get; private set; }
            [Inject]
            public HexMapCreatedSignal HexMapCreated { get; private set; }
            [Inject]
            public Signals.HexGrid.ChangeUIVisible HexGridChangeUIVisibleSignal { get; private set; }

            private bool isDrag;
            private HexDirection dragDirection;
            private HexGridCell previousCell;
            private int brushSize;

            public override void OnRegister()
            {
                Editor.OnCreateMap.AddListener(OnCreateMap);
                Editor.OnLoadMap.AddListener(OnLoadMap);
                Editor.OnSaveMap.AddListener(OnSaveMap);
                Editor.OnOpenPopup.AddListener(OnOpenPopup);
                Editor.OnClosePopup.AddListener(OnClosePopup);
                Editor.OnChangeBrushSize.AddListener(OnChangeBrushSize);
                Editor.OnChangeUIVisible.AddListener(OnChangeUIVisible);
            }

            //OnRemove() is like a destructor/OnDestroy. Use it to clean up.
            public override void OnRemove()
            {
                Editor.OnCreateMap.RemoveListener(OnCreateMap);
                Editor.OnLoadMap.RemoveListener(OnLoadMap);
                Editor.OnSaveMap.RemoveListener(OnSaveMap);
                Editor.OnOpenPopup.RemoveListener(OnOpenPopup);
                Editor.OnClosePopup.RemoveListener(OnClosePopup);
                Editor.OnChangeBrushSize.RemoveListener(OnChangeBrushSize);
                Editor.OnChangeUIVisible.RemoveListener(OnChangeUIVisible);
            }

            void Update()
            {
                if (Editor.InEditMode)
                {
                    if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject())
                    {
                        HandleInput(Input.mousePosition);
                    }
                    else
                    {
                        previousCell = null;
                    }
                }
            }

            void HandleInput(Vector3 position)
            {
                //TODO: decomposite
                //Ray inputRay = Camera.main.ScreenPointToRay(position);
                //RaycastHit hit;
                //if (Physics.Raycast(inputRay, out hit))
                //{
                //    HexGridCell currentCell = Grid.GetCell(hit.point);
                //    if (previousCell && previousCell != currentCell)
                //    {
                //        ValidateDrag(currentCell);
                //    }
                //    else
                //    {
                //        isDrag = false;
                //    }
                //    EditCells(currentCell.MapCell);
                //    previousCell = currentCell;
                //}
                //else
                //{
                //    previousCell = null;
                //}
            }

            void ValidateDrag(HexGridCell currentCell)
            {
                for (dragDirection = HexDirection.NE; dragDirection <= HexDirection.NW; dragDirection++)
                {
                    if (previousCell.GetNeighbor(dragDirection) == currentCell)
                    {
                        isDrag = true;
                        return;
                    }
                }
                isDrag = false;
            }

            void EditCells(HexMapCell center)
            {
                int centerX = center.Coordinates.X;
                int centerZ = center.Coordinates.Z;

                for (int r = 0, z = centerZ - brushSize; z <= centerZ; z++, r++)
                {
                    for (int x = centerX - r; x <= centerX + brushSize; x++)
                    {
                        EditCell(Map.GetCell(new HexCoordinates(x, z)));
                    }
                }
                for (int r = 0, z = centerZ + brushSize; z > centerZ; z--, r++)
                {
                    for (int x = centerX - brushSize; x <= centerX + r; x++)
                    {
                        EditCell(Map.GetCell(new HexCoordinates(x, z)));
                    }
                }
            }

            void EditCell(HexMapCell cell)
            {
                if (cell != null)
                {
                    if (Editor.ActiveTerrainTypeIndex >= 0)
                    {
                        cell.TerrainTypeIndex = Editor.ActiveTerrainTypeIndex;
                    }
                    if (Editor.ApplyElevation)
                    {
                        cell.Elevation = Editor.ActiveElevation;
                    }
                    if (Editor.ApplyWaterLevel)
                    {
                        cell.WaterLevel = Editor.ActiveWaterLevel;
                    }
                    if (Editor.ApplySpecialIndex)
                    {
                        cell.SpecialIndex = Editor.ActiveSpecialIndex;
                    }
                    if (Editor.ApplyUrbanLevel)
                    {
                        cell.UrbanLevel = Editor.ActiveUrbanLevel;
                    }
                    if (Editor.ApplyFarmLevel)
                    {
                        cell.FarmLevel = Editor.ActiveFarmLevel;
                    }
                    if (Editor.ApplyPlantLevel)
                    {
                        cell.PlantLevel = Editor.ActivePlantLevel;
                    }
                    if (Editor.RiverMode == HexMapEditorView.OptionalToggle.No)
                    {
                        cell.RemoveRiver();
                    }
                    if (Editor.RoadMode == HexMapEditorView.OptionalToggle.No)
                    {
                        cell.RemoveRoads();
                    }
                    if (Editor.WalledMode != HexMapEditorView.OptionalToggle.Ignore)
                    {
                        cell.Walled = Editor.WalledMode == HexMapEditorView.OptionalToggle.Yes;
                    }
                    if (Editor.SpawnZoneMode != HexMapEditorView.OptionalToggle.Ignore)
                    {
                        cell.IsSpawnCell = Editor.SpawnZoneMode == HexMapEditorView.OptionalToggle.Yes;
                    }
                    if (isDrag)
                    {
                        HexMapCell otherCell = cell.GetNeighbor(dragDirection.Opposite());
                        if (otherCell != null)
                        {
                            if (Editor.RiverMode == HexMapEditorView.OptionalToggle.Yes)
                            {
                                otherCell.SetOutgoingRiver(dragDirection);
                            }
                            if (Editor.RoadMode == HexMapEditorView.OptionalToggle.Yes)
                            {
                                otherCell.AddRoad(dragDirection);
                            }
                        }
                    }
                }
            }

            private void OnCreateMap(int x, int z)
            {
                Map.CreateMap(x, z);
                HexMapCreated.Dispatch();
                ValidatePositionSignal.Dispatch();
            }

            private void OnLoadMap(string path)
            {
                bool isLoaded = Map.LoadMap(path);
                if (isLoaded)
                {
                    ValidatePositionSignal.Dispatch();
                }
            }

            private void OnSaveMap(string path)
            {
                Map.SaveMap(path);
            }

            private void OnOpenPopup()
            {
                LockMapCameraSignal.Dispatch(true);
            }

            private void OnClosePopup()
            {
                LockMapCameraSignal.Dispatch(false);
            }

            private void OnChangeBrushSize(float size)
            {
                brushSize = (int)size;
            }

            private void OnChangeUIVisible(bool visible)
            {
                HexGridChangeUIVisibleSignal.Dispatch(visible);
            }
        }
    }
}
