using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using strange.extensions.context.impl;

public class RootContextView : ContextView
{
	#region Unity
	// Use this for initialization after deserialization
	void Awake()
	{
		// Create your context here.
		context = new GlobalContext(this, strange.extensions.context.api.ContextStartupFlags.AUTOMATIC);
		//context.Start();
	}
	#endregion
}
