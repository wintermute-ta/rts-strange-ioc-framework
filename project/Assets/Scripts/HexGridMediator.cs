using strange.extensions.mediation.impl;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.EventSystems;

public class HexGridMediator : Mediator
{
    [Inject]
    public HexGridView GridView { get; private set; }
    [Inject]
    public IHexMap Map { get; private set; }
    [Inject]
    public CameraValidatePositionSignal ValidatePosition { get; private set; }
    [Inject]
    public IInputManager InputManager { get; private set; }
    [Inject]
    public ITouchDetector Touches { get; private set; }
    [Inject]
    public HexMapCreatedSignal HexMapCreated { get; private set; }
    [Inject]
    public HexGridCellSelectionSignal HexGridCellSelection { get; private set; }
    [Inject]
    public HexGridCellTouchSignal HexGridCellTouch { get; private set; }
    [Inject]
    public HexGridCellTouchHoldSignal HexGridCellTouchHold { get; private set; }
    [Inject]
    public HexGridInteractableSignal HexGridInteractable { get; private set; }
    [Inject]
    public HexGridChangeUIVisibleSignal HexGridChangeUIVisible { get; private set; }

    private HexGridCell selectedCell;

    public override void OnRegister()
    {
        Touches.PanningThreshold = new Vector2(Screen.width, Screen.height) * 0.001f;
        Touches.LongPressTimeout = 0.5f;
        Touches.OnLongPress.AddListener(OnLongPress);
        HexMapCreated.AddListener(OnHexMapCreated);
        GridView.OnInitialized.AddListener(OnInitialized);
        InputManager.OnPointerDown.AddListener(OnPointerDown);
        HexGridInteractable.AddListener(OnHexGridInteractable);
        HexGridChangeUIVisible.AddListener(OnHexGridChangeUIVisible);
    }

    //OnRemove() is like a destructor/OnDestroy. Use it to clean up.
    public override void OnRemove()
    {
        Touches.OnLongPress.RemoveListener(OnLongPress);
        HexMapCreated.RemoveListener(OnHexMapCreated);
        GridView.OnInitialized.RemoveListener(OnInitialized);
        InputManager.OnPointerDown.RemoveListener(OnPointerDown);
        HexGridInteractable.RemoveListener(OnHexGridInteractable);
        HexGridChangeUIVisible.RemoveListener(OnHexGridChangeUIVisible);
    }

    void Update()
    {
        Touches.Update(InputManager.Touches);
#if UNITY_STANDALONE || UNITY_EDITOR
        if (InputManager.MouseSupported)
        {
            if (!EventSystem.current.IsPointerOverGameObject() && GridView.Interactable)
            {
                UpdateCellSelection(Input.mousePosition);
            }
        }
#endif
    }

    void OnDrawGizmos()
    {
        Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        Debug.DrawRay(inputRay.origin, inputRay.direction * 1000f, Color.white);

    }

    private void UpdateCellSelection(Vector3 position)
    {
        HexGridCell currentCell = GetCellAtPosition(position);
        if (currentCell != null)
        {
            SelectCell(currentCell);
        }
        else
        {
            UnselectCell();
        }
    }

    private void SelectCell(HexGridCell cell)
    {
        bool isEqual = selectedCell != null ? cell.MapCell.Coordinates == selectedCell.MapCell.Coordinates : false;
        if (!isEqual)
        {
            UnselectCell();
            selectedCell = cell;
            selectedCell.Selected = true;
            HexGridCellSelection.Dispatch(selectedCell, true);
        }
    }

    private void UnselectCell()
    {
        if (selectedCell != null)
        {
            selectedCell.Selected = false;
            HexGridCellSelection.Dispatch(selectedCell, false);
            selectedCell = null;
        }
    }

    private HexGridCell GetCellAtPosition(Vector3 position)
    {
        Ray inputRay = Camera.main.ScreenPointToRay(position);
        RaycastHit hit;
        if (Physics.Raycast(inputRay, out hit))
        {
            return GridView.GetCell(HexMetrics.Unperturb(hit.point));
        }
        return null;
    }

    private void OnLongPress(ITouchData touch)
    {
        if (GridView.Interactable)
        {
            if (selectedCell != null)
            {
                HexGridCellTouchHold.Dispatch(selectedCell);
            }
        }
    }

    private void OnPointerDown(ITouchData touch)
    {
        if (GridView.Interactable)
        {
            UpdateCellSelection(touch.Position);
            if (selectedCell != null)
            {
                HexGridCellTouch.Dispatch(selectedCell);
            }
        }
    }

    private void OnHexMapCreated()
    {
        GridView.CreateGrid(Map);
    }

    private void OnInitialized()
    {
        ValidatePosition.Dispatch();
    }

    private void OnHexGridInteractable(bool enabled)
    {
        GridView.Interactable = enabled;
    }

    private void OnHexGridChangeUIVisible(bool visible)
    {
        GridView.ShowUI(visible);
    }
}
