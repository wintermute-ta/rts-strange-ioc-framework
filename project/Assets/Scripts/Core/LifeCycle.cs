using strange.extensions.signal.impl;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LifeCycle : MonoBehaviour, ILifeCycle
{
    public Signal OnBackPressed { get; private set; }
    public Signal<float> OnUpdate { get; private set; }
    public Signal<float> OnFixedUpdate { get; private set; }

    #region Unity
    // Use this for initialization after deserialization
    void Awake()
	{
        OnBackPressed = new Signal();
        OnUpdate = new Signal<float>();
        OnFixedUpdate = new Signal<float>();
    }

	// Update is called once per frame
	void Update()
	{
        OnUpdate.Dispatch(Time.deltaTime);
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnBackPressed.Dispatch();
        }
    }

    void FixedUpdate()
    {
        OnFixedUpdate.Dispatch(Time.fixedDeltaTime);
    }
    #endregion
}
