using strange.extensions.mediation.impl;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GunMediator : BaseMediator
{
    [Inject]
    public GunView GunView { get; private set; }
    [Inject]
    public IHexGridUtility GridUtility { get; private set; }

    public override void OnRegister()
    {
        OnMediatorRegister(GunView, GunView.Unit);
        GunView.Unit.OnDestroy += OnUnitDestroy;
        GunView.Unit.OnChangeTarget += OnUnitChangeTarget;
    }

    //OnRemove() is like a destructor/OnDestroy. Use it to clean up.
    public override void OnRemove()
    {
        OnMediatorRemove(GunView, GunView.Unit);
        GunView.Unit.OnDestroy -= OnUnitDestroy;
        GunView.Unit.OnChangeTarget -= OnUnitChangeTarget;
    }

    protected override void OnUnitViewAttack(BaseUnitView target, Shot shot)
    {
        GunView.AttackTarget(target, shot);
    }

    private void OnUnitChangeTarget(IUnit unit, IUnit target)
    {
        if (target != null)
        {
            GunView.TrackTarget(GameManager.Get_UnitView(target.ID));
        }
        else
        {
            GunView.StopTracking();
        }
    }

    private void OnUnitDestroy(IUnit unit)
    {
        HexGridCell cell = GridUtility.GetCell(unit.Coordinates);
        if (cell != null)
        {
            cell.MapCell.IsSpawnCell = true;
            GridUtility.UpdateAttackRangeCoordinates(cell.MapCell.Coordinates, unit.AttackRange, false);
        }
    }
}
