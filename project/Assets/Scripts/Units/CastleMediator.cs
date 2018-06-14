using strange.extensions.mediation.impl;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CastleMediator : BaseMediator
{
    [Inject]
    public CastleView CastleView { get; private set; }
    [Inject]
    public IHexGridUtility GridUtility { get; private set; }

    public override void OnRegister()
    {
        OnMediatorRegister(CastleView, CastleView.Unit);
        CastleView.Unit.OnDestroy += OnUnitDestroy;
        CastleView.Unit.OnChangeTarget += OnUnitChangeTarget;
    }

    //OnRemove() is like a destructor/OnDestroy. Use it to clean up.
    public override void OnRemove()
    {
        OnMediatorRemove(CastleView, CastleView.Unit);
        CastleView.Unit.OnDestroy -= OnUnitDestroy;
        CastleView.Unit.OnChangeTarget -= OnUnitChangeTarget;
    }

    protected override void OnUnitViewAttack(BaseUnitView target, Shot shot)
    {
        CastleView.AttackTarget(target, shot);
    }

    private void OnUnitChangeTarget(IUnit unit, IUnit target)
    {
        //if (target != null)
        //{
        //    CastleView.TrackTarget(GameManager.Get_UnitView(target.ID));
        //}
        //else
        //{
        //    CastleView.StopTracking();
        //}
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
