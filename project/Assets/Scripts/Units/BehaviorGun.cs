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
    // Use this for initialization
    void Start()
    {
        speedRotation = 20;
      
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance.ListShips != null && GameManager.Instance.ListShips.Any())
        {
            //Пока не особо работает
            var anyShip = GameManager.Instance.ListShips[0];
            var end = new Vector3(anyShip.Coordinates.X, 0f, anyShip.Coordinates.Z);
            end = HexMetrics.Perturb(end);
            StartCoroutine(ChangeAngle(end));
        }
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
            break;
        }
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

