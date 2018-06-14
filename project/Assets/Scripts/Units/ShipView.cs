using strange.extensions.mediation.impl;
using strange.extensions.pool.api;
using strange.extensions.signal.impl;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The description of the behavior instance ship
/// </summary>
public class ShipView : BaseUnitView, IUnitView<InstanceShip>
{
    public InstanceShip Unit { get; private set; }
    public Signal OnReachCell = new Signal();

    /// <summary>
    /// Speed move ship
    /// </summary>
    public float SpeedMove = 20f;
    /// <summary>
    /// Speed rotation ship
    /// </summary>
    public float SpeedRotation;

    private Rect rectObject;

    public void Init(InstanceShip unit, Vector3 position, Quaternion rotation)
    {
        Init(position, rotation, unit.HealthPoint);
        Unit = unit;

        rectObject = GUIRectWithObject(gameObject);
        SpeedMove = 20f;
        SpeedRotation = SpeedMove * 10f;
        Unit.OnHitDamage += Ship_OnHitDamage;
    }

    public void MoveTo(Vector3 end)
    {
        StartCoroutine(ChangeAngle(end));
    }

    private void Ship_OnHitDamage(IUnit unit, float damage)
    {
        UpdateHP(unit.HealthPoint);
    }

    /// <summary>
    /// Coroutine move ship between two cells
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    IEnumerator MoveBetweenCell(Vector3 vectorEnd)
    {
        float distance = Vector3.Distance(RemovePositionAxisY(transform.localPosition), vectorEnd);
        while (true)
        {
            yield return null;
            float step = Time.deltaTime * SpeedMove;
            // transform.right
            transform.localPosition = Vector3.MoveTowards(RemovePositionAxisY(transform.localPosition), vectorEnd, step);
            distance = Vector3.Distance(RemovePositionAxisY(transform.localPosition), vectorEnd);

            if (distance < 0.1f)
            {
                transform.localPosition = ReplacePositionAxisY(vectorEnd, transform.localPosition.y); //PKP: Move ship to end position
                OnReachCell.Dispatch();
                break;
            }
        }

    }

    /// <summary>
    /// Coroutine rotate ship
    /// </summary>
    /// <param name="VectorTo"></param>
    /// <returns></returns>
    IEnumerator ChangeAngle(Vector3 vectorEnd)
    {
        yield return null;
        Vector3 heading = vectorEnd - RemovePositionAxisY(transform.localPosition);
        Quaternion lookTarget = Quaternion.LookRotation(heading, RemovePositionAxisY(UnitTransform.up));

        while (true)
        {
            yield return null;
            float step = Time.deltaTime * SpeedRotation;
            Quaternion lookRotation = Quaternion.RotateTowards(RemoveRotationAxis(UnitTransform.localRotation), RemoveRotationAxis(lookTarget), step);
            UnitTransform.localRotation = lookRotation;

            float distance = Vector3.Distance(RemovePositionAxisY(transform.localPosition), vectorEnd);
            float angle = Quaternion.Angle(RemoveRotationAxis(UnitTransform.localRotation), RemoveRotationAxis(lookTarget));
            if (angle < 0.1f)
            {
                UnitTransform.localRotation = RemoveRotationAxis(lookTarget);
                StartCoroutine(MoveBetweenCell(vectorEnd));
                break;
            }
        }
    }

    private Rect GUIRectWithObject(GameObject go)
    {
        Vector3 cen = go.GetComponent<BoxCollider>().bounds.center;
        Vector3 ext = go.GetComponent<BoxCollider>().bounds.extents;
        Vector2[] extentPoints = new Vector2[8]
         {
               WorldToGUIPoint(new Vector3(cen.x-ext.x, cen.y-ext.y, cen.z-ext.z)),
               WorldToGUIPoint(new Vector3(cen.x+ext.x, cen.y-ext.y, cen.z-ext.z)),
               WorldToGUIPoint(new Vector3(cen.x-ext.x, cen.y-ext.y, cen.z+ext.z)),
               WorldToGUIPoint(new Vector3(cen.x+ext.x, cen.y-ext.y, cen.z+ext.z)),
               WorldToGUIPoint(new Vector3(cen.x-ext.x, cen.y+ext.y, cen.z-ext.z)),
               WorldToGUIPoint(new Vector3(cen.x+ext.x, cen.y+ext.y, cen.z-ext.z)),
               WorldToGUIPoint(new Vector3(cen.x-ext.x, cen.y+ext.y, cen.z+ext.z)),
               WorldToGUIPoint(new Vector3(cen.x+ext.x, cen.y+ext.y, cen.z+ext.z))
         };
        Vector2 min = extentPoints[0];
        Vector2 max = extentPoints[0];
        foreach (Vector2 v in extentPoints)
        {
            min = Vector2.Min(min, v);
            max = Vector2.Max(max, v);
        }
        return new Rect(min.x, min.y, max.x - min.x, max.y - min.y);
    }

    private Vector2 WorldToGUIPoint(Vector3 world)
    {
        Vector2 screenPoint = Camera.main.WorldToScreenPoint(world);
        screenPoint.y = (float)Screen.height - screenPoint.y;
        return screenPoint;
    }

    public override void Restore()
    {
        base.Restore();
        Unit.OnHitDamage -= Ship_OnHitDamage;
        //Unit = null;
    }
}
