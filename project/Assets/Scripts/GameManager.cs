using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : SingletonMonoBehaviour<GameManager>
{
    public HexMap Map { get; private set; }
    public event Action<HexMapCell> OnSpawnShip = delegate { };
    public event Action<InstanceShip> OnDestroyShip = delegate { };
    public event Action<InstanceGun> OnDestroyGun = delegate { };
    private List<InstanceShip> ships;
    private List<InstanceGun> guns;
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

    internal override void Awake()
    {
        base.Awake();

        Map = new HexMap();
        ships = new List<InstanceShip>();
        guns = new List<InstanceGun>();
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

    private void Ship_OnReachTarget(InstanceShip ship)
    {
        ships.Remove(ship);
        OnDestroyShip.Invoke(ship);
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
