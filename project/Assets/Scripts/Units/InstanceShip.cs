using System;
using System.Collections.Generic;

/// <summary>
/// Description of the instance ship
/// </summary>
public class InstanceShip : Unit
{
    static UnitType[] _targetTypes = new UnitType[] { UnitType.Castle, UnitType.Gun };

    public event Action<HexCoordinates> OnMoveToCell = delegate { };
    public event Action<InstanceShip> OnReachTarget = delegate { };
    public event Action<InstanceShip, IUnit> OnDestinationChanged = delegate { };

    public List<IAStarCell> Path { get; private set; }
    private int indexPath;
    private IUnit destination;

    public InstanceShip() : base(UnitType.Ship, _targetTypes, GlobalContext.Get().GetInstance<IWeapon>(typeof(WeaponShipCannon)))
    {
        HealthPoint = 200f;
        Path = new List<IAStarCell>();
    }

    public override void UpdateAI(float deltaTime)
    {
        base.UpdateAI(deltaTime);

        IUnit newDestination = FindUnitOfType(PossibleTargets, UnitType.Castle);
        if (newDestination != destination)
        {
            Path.Clear();
            if (destination != null)
            {
                destination.OnDestroy -= OnDestroyDestination;
            }
            destination = newDestination;
            if (destination != null)
            {
                destination.OnDestroy += OnDestroyDestination;
            }
            OnDestinationChanged.Invoke(this, destination);
            if (Path.Count > 0)
            {
                BeginMoving(Path);
            }
        }
    }

    private IUnit FindUnitOfType(List<IUnit> targets, UnitType type)
    {
        for (int i = 0; i < targets.Count; i++)
        {
            if (targets[i].Type == type)
            {
                return targets[i];
            }
        }
        return null;
    }

    public override void Restore()
    {
        base.Restore();

        OnDestroyDestination(destination);
    }

    public void BeginMoving(List<IAStarCell> pathTarget)
    {
        Path = pathTarget;
        indexPath = 0;
        OnMoveToCell.Invoke(Path[indexPath].AStarCoordinates);
    }

    public void EndMovingToCell()
    {
        Coordinates = Path[indexPath].AStarCoordinates;
        indexPath++;
        if (indexPath < Path.Count)
        {
            OnMoveToCell.Invoke(Path[indexPath].AStarCoordinates);
        }
        else
        {
            OnReachTarget.Invoke(this);
        }
    }

    private void OnDestroyDestination(IUnit unit)
    {
        destination.OnDestroy -= OnDestroyDestination;
        destination = null;
        Path.Clear();
    }
}
