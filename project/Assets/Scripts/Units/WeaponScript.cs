using strange.extensions.signal.impl;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;

/// <summary>
/// Script for Attack and logic bullet
/// </summary>
public class WeaponScript : MonoBehaviour
{
    public Action<Shot> OnShotDestroy = delegate { };

    public float Speed;
	// Use this for initialization
	void Start ()
    {
        Speed = 100f;
    }
	
	// Update is called once per frame
	void Update ()
    {
       
    }

    public void Attack(Shot shot, Vector3 targetVector)
    {
        shot.transform.localPosition = transform.position;

        float targetY = targetVector.y + 4f;
        targetVector = new Vector3(targetVector.x, targetY, targetVector.z);
        StartCoroutine(MoveBullet(shot, targetVector));
    }

    IEnumerator MoveBullet(Shot shot, Vector3 targetVector)
    {
        float distance = Vector3.Distance(shot.transform.localPosition, targetVector);
        while (true)
        {
            yield return null;
            float step = Time.deltaTime * Speed;
            shot.transform.localPosition = Vector3.MoveTowards(shot.transform.localPosition, targetVector, step);
            distance = Vector3.Distance(shot.transform.localPosition, targetVector);
            if(distance < 1f || distance > 50f)
            {
                OnShotDestroy.Invoke(shot);
                //Destroy(shotTransform.gameObject);
                break;
            }
        }
    }
}
