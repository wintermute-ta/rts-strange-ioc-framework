using strange.extensions.mediation.impl;
using strange.extensions.pool.api;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BaseMediator : Mediator
{
    [Inject]
    public IPool<Shot> ShotPool { get; private set; }
    [Inject]
    public IGameManager GameManager { get; private set; }
    [Inject]
    public IUIManager UIManager { get; private set; }
    [Inject]
    public RotateMapCameraSignal RotateMapCamera { get; private set; }
    [Inject]
    public ZoomMapCameraSignal ZoomMapCamera { get; private set; }

    protected virtual void OnMediatorRegister(BaseUnitView view, IUnit unit)
    {
        view.AttachHealhBar(UIManager.CreateUI<SliderModel>(view.HealthBarCanvas.transform, SliderHandler.SliderType.BASE_SLIDER.ToString()));
        unit.OnAttack += OnUnitAttack;
        view.OnShotDestroy = OnShotDestroy;
        ZoomMapCamera.AddListener(view.HealthBarCanvas.UpdateCanvas);
        RotateMapCamera.AddListener(view.HealthBarCanvas.UpdateCanvas);
    }

    protected virtual void OnMediatorRemove(BaseUnitView view, IUnit unit)
    {
        view.RemoveHealthBar();
        unit.OnAttack -= OnUnitAttack;
        view.OnShotDestroy = null;
        ZoomMapCamera.RemoveListener(view.HealthBarCanvas.UpdateCanvas);
        RotateMapCamera.RemoveListener(view.HealthBarCanvas.UpdateCanvas);
    }

    private void OnUnitAttack(IUnit unit, IUnit target, float damage)
    {
        Shot shot = ShotPool.GetInstance();
        OnUnitViewAttack(GameManager.Get_UnitView(target.ID), shot);
    }

    protected virtual void OnUnitViewAttack(BaseUnitView target, Shot shot) { }

    private void OnShotDestroy(Shot shot)
    {
        ShotPool.ReturnInstance(shot);
    }
}
