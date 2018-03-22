using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
	#region Unity
	// Use this for initialization after deserialization
	void Awake()
	{
		
	}
	
	// Use this for initialization after first OnEnable
	void Start()
	{
        GetComponent<Renderer>().sharedMaterial.SetVector("_Point", transform.position/*new Vector4(1.0f, 0.0f, 0.0f, 1.0f)*/);
        GetComponent<Renderer>().sharedMaterial.SetFloat("_DistanceNear", 10.0f);
        GetComponent<Renderer>().sharedMaterial.SetColor("_ColorNear", new Color(1.0f, 0.0f, 0.0f));
        GetComponent<Renderer>().sharedMaterial.SetColor("_ColorFar", new Color(1.0f, 1.0f, 1.0f));
    }
	
	// Update is called once per frame
	void Update()
	{
		
	}
	#endregion
}
