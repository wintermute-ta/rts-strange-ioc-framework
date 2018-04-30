using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// The description of the behavior instance gun
/// </summary>
public class BehaviorGun : MonoBehaviour
{
    /// <summary>
    /// Speed rotation gun
    /// </summary>
    private float speedRotation;

    private InstanceGun gun;
    private WeaponScript Weapon;
    // Use this for initialization
    void Start()
    {
        speedRotation = 100;
        Weapon = GetComponent<WeaponScript>();
    }

    // Update is called once per frame
    void Update()
    {
        var result = IsAvaliableEnemy();
    }
    public void Init(HexGrid hexGrid, InstanceGun gun)
    {
        this.gun = gun;
        gun = new InstanceGun(new HexCoordinates(8, 3));
    }

    IEnumerator ChangeAngle(Vector3 VectorTo)
    {
        yield return null;
        Vector3 heading = VectorTo - new Vector3(transform.localPosition.x, 0f, transform.localPosition.z);
        Quaternion lookTarget = Quaternion.LookRotation(heading, new Vector3(transform.up.x, 0f, transform.up.z));

        while (true)
        {
            yield return null;
            float step = Time.deltaTime * speedRotation;
            Quaternion lookRotation = Quaternion.RotateTowards(RemoveRotationAxis(transform.localRotation), RemoveRotationAxis(lookTarget), step);
            transform.localRotation = lookRotation;

            float angle = Quaternion.Angle(RemoveRotationAxis(transform.localRotation), RemoveRotationAxis(lookTarget));
            if (angle < 0.1f)
            {
                transform.localRotation = RemoveRotationAxis(lookTarget);
                break;
            }
            StopCoroutine(ChangeAngle(VectorTo));
        }
    }

    /// <summary>
    /// Check enemy in range attack.
    /// </summary>
    private bool IsAvaliableEnemy()
    {
        if (GameManager.Instance.ListShips != null && GameManager.Instance.ListShips.Any())
        {
            bool _firstShipDetected = false;
            foreach (InstanceShip ship in GameManager.Instance.ListShips)
            {
                if (_firstShipDetected)
                    return true;

                if (gun.AttackCoordinates.Contains(ship.Coordinates))
                {
                    _firstShipDetected = true;
                    BehaviorShip _bs = GameManager.Instance.Get_BehaviorShip(ship.ID);
                    Weapon.Attack(true, _bs.transform.position);
                    var end = new Vector3(ship.Coordinates.X, 0f, ship.Coordinates.Z);
                    end = HexMetrics.Perturb(end);
                    StartCoroutine(ChangeAngle(end));
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }
        return false;
    }

    private Quaternion RemoveRotationAxis(Quaternion targetRotation)
    {
        Vector3 euler = targetRotation.eulerAngles;
        euler.x = 0f;
        euler.z = 0f;
        return Quaternion.Euler(euler);
    }
    private Vector3 RemovePositionAxis(Vector3 targetRotation)
    {
        Vector3 vector = targetRotation;
        vector.y = 0f;
        return vector;
    }
}

