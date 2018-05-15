using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The description of the behavior instance ship
/// </summary>
public class BehaviorShip : MonoBehaviour
{
    /// <summary>
    /// Speed move ship
    /// </summary>
    public float SpeedMove;
    /// <summary>
    /// Speed rotation ship
    /// </summary>
    public float SpeedRotation;

    private InstanceShip ship;
    private HexGrid hexGrid;

    void Start()
    {
        SpeedMove = 20f;
        SpeedRotation = SpeedMove * 10f;
        //Set start angle rotation for ship
        transform.Rotate(Vector3.up, 90);
    }
	void Update()
    {
       
    }
    private void OnTriggerEnter(Collider other)
    {
        ship.HealthPoint -= 1;
        if (ship.HealthPoint == 0)
            GameManager.Instance.Ship_OnReachTarget(ship);
    }

    public void Init(HexGrid hexGrid, InstanceShip ship)
    {
        this.ship = ship;
        this.hexGrid = hexGrid;
        ship.OnReachTarget += Ship_OnReachTarget;
        ship.OnMoveToCell += Ship_OnMoveToCell;
        GameManager.Instance.OnDestroyShip += Instance_OnDestroyShip;
        HexMapCell cellOrigin = GameManager.Instance.Map.GetCell(ship.Coordinates);
        HexMapCell cellDestination = GameManager.Instance.Map.GetCell(new HexCoordinates(16, 6));
        List<IAStarCell> path = FindPath(cellOrigin, cellDestination);
        for (int i = 0; i < path.Count; i++)
        {
            HexMapCell cell = path[i] as HexMapCell;
            cell.IsPathCell = true;
        }
        ship.BeginMoving(path);
    } 

    private void Ship_OnReachTarget(InstanceShip ship)
    {
        this.ship.OnMoveToCell -= Ship_OnMoveToCell;
        this.ship.OnReachTarget -= Ship_OnReachTarget;
        GameManager.Instance.OnDestroyShip -= Instance_OnDestroyShip;
    }

    private void Instance_OnDestroyShip(BehaviorShip obj)
    {
        if (obj != null)
            Destroy(obj.gameObject);
    }

    private void Ship_OnMoveToCell(HexCoordinates targetCoordinates)
    {
        HexGridCell offsetEnd = hexGrid.GetCell(targetCoordinates);
        var end = new Vector3(offsetEnd.Position.x, 0f, offsetEnd.Position.z);
        end = HexMetrics.Perturb(end);
        StartCoroutine(ChangeAngle(end));
        StartCoroutine(MoveBetweenCell(targetCoordinates));
    }

    protected List<IAStarCell> FindPath(IAStarCell origin, IAStarCell goal)
    {
        return AStar.FindPath(origin, goal, false);
    }

    /// <summary>
    /// Coroutine move ship between two cells
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    IEnumerator MoveBetweenCell(HexCoordinates targetCoordinates)
    {
        HexGridCell offsetEnd = hexGrid.GetCell(targetCoordinates);
        Vector3 end = new Vector3(offsetEnd.Position.x, 0f, offsetEnd.Position.z);
        end = HexMetrics.Perturb(end);
   
        float distance = Vector3.Distance(RemovePositionAxis(transform.localPosition), end);
        while (true)
        {
            yield return null;
            float step = Time.deltaTime * SpeedMove;
            //step = 1;
            gameObject.transform.localPosition = Vector3.MoveTowards(RemovePositionAxis(transform.localPosition), end, step);
            distance = Vector3.Distance(RemovePositionAxis(transform.localPosition), end);
            if (distance < 5f)
            {
                ship.EndMovingToCell();
                break;
            }
        }

    }

    /// <summary>
    /// Coroutine rotate ship
    /// </summary>
    /// <param name="VectorTo"></param>
    /// <returns></returns>
    IEnumerator ChangeAngle(Vector3 VectorTo)
    {
        yield return null;
        Vector3 heading = VectorTo - new Vector3(transform.localPosition.x, 0f, transform.localPosition.z);
        Quaternion lookTarget = Quaternion.LookRotation(heading, new Vector3(transform.up.x, 0f, transform.up.z));

        while (true)
        {
            yield return null;
            float step = Time.deltaTime * SpeedRotation;
            Quaternion lookRotation = Quaternion.RotateTowards(RemoveRotationAxis(transform.localRotation), RemoveRotationAxis(lookTarget), step);
            transform.localRotation = lookRotation;

            float angle = Quaternion.Angle(RemoveRotationAxis(transform.localRotation), RemoveRotationAxis(lookTarget));
            if (angle < 0.1f)
            {
                transform.localRotation = RemoveRotationAxis(lookTarget);
                break;
            }
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
