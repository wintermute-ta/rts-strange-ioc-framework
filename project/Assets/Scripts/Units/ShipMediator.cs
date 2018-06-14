using strange.extensions.mediation.impl;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using strange.extensions.pool.api;

public class ShipMediator : BaseMediator
{
    [Inject]
    public ShipView ShipView { get; private set; }
    [Inject]
    public IHexGridUtility GridUtility { get; private set; }
    [Inject]
    public ShipDestroySignal ShipDestroy { get; private set; }

    public override void OnRegister()
    {
        OnMediatorRegister(ShipView, ShipView.Unit);
        ShipView.Unit.OnMoveToCell += OnMoveToCell;
        ShipView.Unit.OnDestroy += OnUnitDestroy;
        ShipView.OnReachCell.AddListener(OnReachCell);
    }

    //OnRemove() is like a destructor/OnDestroy. Use it to clean up.
    public override void OnRemove()
    {
        OnMediatorRemove(ShipView, ShipView.Unit);
        ShipView.Unit.OnMoveToCell -= OnMoveToCell;
        ShipView.Unit.OnDestroy -= OnUnitDestroy;
        ShipView.OnReachCell.RemoveListener(OnReachCell);
    }

    protected override void OnUnitViewAttack(BaseUnitView target, Shot shot)
    {
        ShipView.AttackTarget(target, shot);
    }

    private void OnMoveToCell(HexCoordinates coordinates)
    {
        HexGridCell offsetEnd = GridUtility.GetCell(coordinates);
        Vector3 end = new Vector3(offsetEnd.Position.x, 0f, offsetEnd.Position.z);
        end = HexMetrics.Perturb(end);
        ShipView.MoveTo(end);
    }

    private void OnReachCell()
    {
        //When ship go out cell, clear flag cell "IsPathCell"
        HexGridCell cell = GridUtility.GetCell(ShipView.Unit.Coordinates);
        if (cell != null)
        {
            cell.IsPathCell = false;
        }
        ShipView.Unit.EndMovingToCell();
    }

    private void OnUnitDestroy(IUnit unit)
    {
        ShipDestroy.Dispatch(ShipView.Unit);
    }
}
