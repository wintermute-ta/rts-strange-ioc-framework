using strange.extensions.mediation.impl;
using strange.extensions.pool.api;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseUnitView : View, IPoolable
{
    public bool retain { get; private set; }
    public Action<Shot> OnShotDestroy = delegate { };

    public WorldSpaceCanvasHandler HealthBarCanvas;
    public Transform UnitTransform;
    private SliderModel healthBar;
    private WeaponScript weapon;
    private Camera mainCamera;
    private float initialHP;

    protected override void Awake()
    {
        base.Awake();
        mainCamera = Camera.main;
        weapon = GetComponent<WeaponScript>();
        weapon.OnShotDestroy = Weapon_OnShotDestroy;
        retain = false;
    }

    void Update()
    {
        //rotate canvas to camera

    }

    protected void Init(Vector3 position, Quaternion rotation, float initialHP)
    {
        this.initialHP = initialHP;

        transform.localPosition = position;
        transform.localRotation = rotation;
        gameObject.SetActive(true);
        HealthBarCanvas.UpdateCanvas();
    }

    public virtual void AttackTarget(BaseUnitView target, Shot shot)
    {
        weapon.Attack(shot, target.transform.position);
    }

    public void AttachHealhBar(SliderModel healthBar)
    {
        this.healthBar = healthBar;
        ResetHP();
        this.healthBar.Open();
    }

    public void RemoveHealthBar()
    {
        healthBar.Shutdown();
        healthBar = null;
    }

    public void UpdateHP(float hp)
    {
        healthBar.SetValue(hp);
    }

    private void Weapon_OnShotDestroy(Shot shot)
    {
        OnShotDestroy.Invoke(shot);
    }

    protected void ResetHP()
    {
        if (healthBar != null)
        {
            healthBar.Init(initialHP);
        }
    }

    protected Quaternion RemoveRotationAxis(Quaternion rotation)
    {
        Vector3 euler = rotation.eulerAngles;
        euler.x = 0f;
        euler.z = 0f;
        return Quaternion.Euler(euler);
    }

    protected Vector3 RemovePositionAxisY(Vector3 position)
    {
        return ReplacePositionAxisY(position, 0.0f);
    }

    protected Vector3 ReplacePositionAxisY(Vector3 position, float value)
    {
        position.y = value;
        return position;
    }

    public virtual void Restore()
    {
        ResetHP();
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        gameObject.SetActive(false);
    }

    public void Retain()
    {
        retain = true;
    }

    public void Release()
    {
        retain = false;
        Restore();
    }
}
