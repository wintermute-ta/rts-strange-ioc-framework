using strange.extensions.pool.api;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Shot : MonoBehaviour, IPoolable
{
    /// <summary>
    /// The value of the damage of the bullets from the gun that created it
    /// </summary>
    public float Damage;

    public bool retain { get; private set; }

    public void Init()
    {
        gameObject.SetActive(true);
    }

    public void Release()
    {
        retain = false;
        Restore();
    }

    public void Restore()
    {
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        gameObject.SetActive(false);
    }

    public void Retain()
    {
        retain = true;
    }

    // Use this for initialization
    void Start ()
    {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
		
	}
}
