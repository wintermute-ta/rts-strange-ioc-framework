using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Description of the instance gun
/// </summary>
public class InstanceGun : IUnit
{
    public int ID { get; set; }
    public HexCoordinates Coordinates { get; set; }
    public int AttackRange { get; set; }
    public float HealthPoint { get; set; }
    public float DamagePerSecond { get; set; }
    public List<HexCoordinates> AttackCoordinates { get; set; }

    public InstanceGun(HexCoordinates coordinates)
    {
        Coordinates = coordinates;
        AttackRange = 3;
        HealthPoint = 10;
        AttackCoordinates = new List<HexCoordinates>();

        #region AttackCoordinates update. For gun list AttackCoordinates update one time.
        var hexMapCell = GameManager.Instance.Map.GetCell(coordinates);
        var neighbors = new List<HexCoordinates>();
        var iteration = AttackRange;
        while (iteration != 0)
        {
            if (!neighbors.Any())
            {
                //1st iteration. 
                foreach (var cell in hexMapCell.Neighbors)
                {
                    AttackCoordinates.Add(cell.Coordinates);
                    neighbors.Add(cell.Coordinates);
                }
            }
            else
            {
                //more then 1st iteration.
                var neighbors2 = new List<HexCoordinates>();
                foreach (var cell in neighbors)
                {
                    hexMapCell = GameManager.Instance.Map.GetCell(cell);
                    foreach (var c in hexMapCell.Neighbors)
                    {
                        neighbors2.Add(c.Coordinates);
                    }
                }  
                //get distinct elements
                neighbors2 = neighbors2.Distinct().ToList();
                AttackCoordinates.AddRange(neighbors2);
                neighbors = neighbors2;
            }
            iteration--;
           
        }
        //get distinct elements
        AttackCoordinates = AttackCoordinates.Distinct().ToList();
        #endregion
    }

}
