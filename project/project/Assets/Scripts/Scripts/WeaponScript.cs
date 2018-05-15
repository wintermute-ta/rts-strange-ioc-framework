using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Script for Attack and logic bullet
/// </summary>
public class WeaponScript : MonoBehaviour
{

    public Transform ShotPrefab;
    public float ShootingRate;
    public float Speed;

    private float shootCooldown;
	// Use this for initialization
	void Start ()
    {
        Speed = 100f;
        ShootingRate = 0.75f;
        shootCooldown = 0f;
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (shootCooldown > 0)
        {
            shootCooldown -= Time.deltaTime;
            return;
        }
    }
    private void FixedUpdate()
    {
       
    }
    public void Attack(bool isEnemy, Vector3 targetVector)
    {
       if(CanAttack)
        {
            shootCooldown = ShootingRate;
            Transform _shotTransform = Instantiate(ShotPrefab) as Transform;
            _shotTransform.transform.localPosition = transform.position;
            targetVector = new Vector3(targetVector.x, transform.localPosition.y, targetVector.z);
            StartCoroutine(MoveBullet(_shotTransform, targetVector));
        }
    }
    public bool CanAttack
    {
        get
        {
            return shootCooldown <= 0f;
        }
    }

    IEnumerator MoveBullet( Transform shotTransform, Vector3 targetVector)
    {
        float _distance = Vector3.Distance(shotTransform.transform.localPosition, targetVector);
        while (true)
        {
            yield return null;
            float step = Time.deltaTime * Speed;
            shotTransform.transform.localPosition = Vector3.MoveTowards(shotTransform.transform.localPosition, targetVector, step);
            _distance = Vector3.Distance(shotTransform.transform.localPosition, targetVector);
            if(_distance < 1f || _distance > 50f)
            {
                Destroy(shotTransform.gameObject);
                break;
            }
        }
     
    

    }
}
