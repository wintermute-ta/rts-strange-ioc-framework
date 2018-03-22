using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The description of the behavior instance ship
/// </summary>
public class BehaviorShip : MonoBehaviour
{
    /// <summary>
    /// Скорость движения корабля.
    /// </summary>
    public float speedMove;
    /// <summary>
    /// Скорость поворота корабля.
    /// </summary>
    public float speedRotation;

    private InstanceShip ship;
    private HexGrid hexGrid;
    // Use this for initialization
    void Start()
    {
        speedMove = 20f;
        speedRotation = speedMove * 10f;
        //Устанавливаем начальный угол для корабля.
        transform.Rotate(Vector3.up, 90);
    }
	
	// Update is called once per frame
	void Update()
    {
       
    }

    public void Init(HexGrid hexGrid, InstanceShip ship)
    {
        this.ship = ship;
        this.hexGrid = hexGrid;
        ship.OnReachTarget += Ship_OnReachTarget;
        ship.OnMoveToCell += Ship_OnMoveToCell;

        HexMapCell cellOrigin = GameManager.Instance.Map.GetCell(ship.Coordinates);
        HexMapCell cellDestination = GameManager.Instance.Map.GetCell(new HexCoordinates(16, 6));
        List<IAStarCell> path = FindPath(cellOrigin, cellDestination);
        for (int i = 0; i < path.Count; i++)
        {
            HexMapCell cell = path[i] as HexMapCell;
            cell.IsPathCell = true;
        }
        ship.BeginMoving(path);

        //Передаем -1, т.к. инкремент происходит перед получением элемента. Чтобы получился 0 на первом шаге, передаем -1.
        //StartCoroutine(ChangeEndCell(-1));
    }

    private void Ship_OnReachTarget(InstanceShip ship)
    {
        this.ship.OnMoveToCell -= Ship_OnMoveToCell;
        this.ship.OnReachTarget -= Ship_OnReachTarget;
        Destroy(gameObject);
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
        AStar pathFinder = new AStar();
        pathFinder.FindPath(origin, goal, false);
        return pathFinder.CellsFromPath();
    }

    /// <summary>
    /// Движение объекта между ячейками.
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
            float step = Time.deltaTime * speedMove;
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

    //void OnDrawGizmos()
    //{
    //    Debug.DrawLine(transform.position, end, Color.red);
    //}

        /*
    /// <summary>
    /// Корутин получения следующей ячейки для двивания.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    IEnumerator ChangeEndCell(int index)
    {
       
        if (index < (path.Count - 1))
        {
            yield return null;
         
            index++;
            HexGridCell offsetEnd = GameManager.Instance.Map.GetCell(path[index].AStarCoordinates);
            var end = new Vector3(offsetEnd.Position.x, 0f, offsetEnd.Position.z);
            end = HexMetrics.Perturb(end);
            StartCoroutine(ChangeAngle(end));
            StartCoroutine(MoveBetweenCell(index));
        }
        else
        {
            StopCoroutine(ChangeEndCell(path.Count));
            Destroy(gameObject);
        }
    }
    */
    /// <summary>
    /// Корутин поворота корабля.
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
            float step = Time.deltaTime * speedRotation;
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
