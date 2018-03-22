using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Description of the instance ship
/// </summary>
public class InstanceShip : IUnit
{
    public HexCoordinates Coordinates { get; set; }
    public int AttackRange { get; set; }
    public float HealthPoint { get; set; }
    public float DamagePerSecond { get; set; }

    private List<IAStarCell> path;
    private int indexPath;

    public InstanceShip(HexCoordinates coordinates)
    {
        Coordinates = coordinates;
        AttackRange = 1;
        HealthPoint = 5;
    }

    public event Action<HexCoordinates> OnMoveToCell = delegate { };
    public event Action<InstanceShip> OnReachTarget = delegate { };



    public void BeginMoving(List<IAStarCell> pathTarget)
    {
        path = pathTarget;
        indexPath = 0;
        OnMoveToCell.Invoke(path[indexPath].AStarCoordinates);
    }

    public void EndMovingToCell()
    {
        Coordinates = path[indexPath].AStarCoordinates;
        indexPath++;
        if (indexPath < path.Count)
        {
            OnMoveToCell.Invoke(path[indexPath].AStarCoordinates);
        }
        else
        {
            OnReachTarget.Invoke(this);
        }
    }
}
