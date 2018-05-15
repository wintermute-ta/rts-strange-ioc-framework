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

    /// <summary>
    /// TRUE when the gun ends the turn in the direction of the ship
    /// until then FALSE
    /// </summary>
    private bool isCanAttack;
    void Start()
    {
        speedRotation = 20;
        Weapon = GetComponent<WeaponScript>();
        isCanAttack = false;
    }
    void Update()
    {
        var result = IsAvaliableEnemy();
    }
    public void Init(HexGrid hexGrid, InstanceGun gun)
    {
        this.gun = gun;
        gun = new InstanceGun(new HexCoordinates(8, 3));
    }

    /// <summary>
    /// Change andgle gun in the direction of the ship 
    /// </summary>
    /// <param name="VectorTo"></param>
    /// <returns></returns>
    IEnumerator ChangeAngle(Vector3 VectorTo)
    {
        Vector3 _heading = VectorTo - new Vector3(transform.localPosition.x, 0f, transform.localPosition.z);
        Quaternion _lookTarget = Quaternion.LookRotation(_heading, new Vector3(transform.up.x, 0f, transform.up.z));
        isCanAttack = false;
        while (true)
        {
            yield return null;
          
            float _step = Time.deltaTime * speedRotation;
            Quaternion _lookRotation = Quaternion.RotateTowards(RemoveRotationAxis(transform.localRotation), RemoveRotationAxis(_lookTarget), _step);
            transform.localRotation = _lookRotation;

            float _angle = Quaternion.Angle(RemoveRotationAxis(transform.localRotation), RemoveRotationAxis(_lookTarget));
            if (_angle < 0.1f)
            {
                transform.localRotation = RemoveRotationAxis(_lookTarget);
                break;
            }
            isCanAttack = true;
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
                   
                    StartCoroutine(ChangeAngle(_bs.transform.position));
                   // if (isCanAttack)
                    Weapon.Attack(true, _bs.transform.position);
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

