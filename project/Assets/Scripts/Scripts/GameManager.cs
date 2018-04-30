using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : SingletonMonoBehaviour<GameManager>
{
    public HexMap Map { get; private set; }
    public event Action<HexMapCell> OnSpawnShip = delegate { };
    public event Action<BehaviorShip> OnDestroyShip = delegate { };
    public event Action<InstanceGun> OnDestroyGun = delegate { };
    private List<InstanceShip> ships;
    private List<InstanceGun> guns;
    private Dictionary<int, BehaviorShip> behaviorShips;
    /// <summary>
    /// Return list ships on the map
    /// </summary>
    public List<InstanceShip> ListShips
    {
        get
        {
            return ships;
        }
    }
    /// <summary>
    /// Return behavorShip by id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public BehaviorShip Get_BehaviorShip(int id)
    {
        return behaviorShips[id] ?? null;
    }
    internal override void Awake()
    {
        base.Awake();

        Map = new HexMap();
        ships = new List<InstanceShip>();
        guns = new List<InstanceGun>();
        behaviorShips = new Dictionary<int, BehaviorShip>();
    }

    internal override void OnDestroy()
    {
        base.OnDestroy();
    }

    // Use this for initialization
    void Start()
    {
		
	}
	
	// Update is called once per frame
	void Update()
    {
		
	}

    public InstanceShip SpawnShip(HexCoordinates coordinates)
    {
        InstanceShip ship = new InstanceShip(coordinates);
        ship.OnReachTarget += Ship_OnReachTarget;
        ships.Add(ship);
      
        return ship;
    }
    public void AddBehavioralShipsInList(int id, BehaviorShip bs)
    {
        behaviorShips.Add(id, bs);
    }
    private void RemoveBehavioralShipsInList(int id)
    {
        behaviorShips.Remove(id);
    }

    public void Ship_OnReachTarget(InstanceShip ship)
    {
        ships.Remove(ship);
        var bs = behaviorShips[ship.ID];
        RemoveBehavioralShipsInList(ship.ID);
        OnDestroyShip.Invoke(bs);
    }
    public InstanceGun SpawnGun(HexCoordinates coordinates)
    {
        InstanceGun gun = new InstanceGun(coordinates);
       // gun.OnReachTarget += Gun_OnReachTarget;
        guns.Add(gun);
        return gun;
    }

    private void Gun_OnReachTarget(InstanceGun gun)
    {
        guns.Remove(gun);
        OnDestroyGun.Invoke(gun);
    }
}
