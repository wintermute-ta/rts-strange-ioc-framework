using System;
using System.Collections;
using System.Collections.Generic;
using strange.framework.api;
using UnityEngine;

public class GameObjectInstanceProvider : IInstanceProvider
{
    [Inject]
    public IResourceManager ResourceManager { get; set; }

    public string PrefabName
    {
        set
        {
            prefab = ResourceManager.GetPrefab(value);
            if (prefab == null)
            {
                Debug.LogError("Prefab with name " + value + " doesn't exist");
            }
        }
    }

    public GameObject Prefab
    {
        set
        {
            prefab = value;
        }
    }


    private GameObject prefab;

    T IInstanceProvider.GetInstance<T>()
    {
        throw new NotImplementedException();
    }

    object IInstanceProvider.GetInstance(Type key)
    {
        GameObject gameObject = GameObject.Instantiate(prefab);
        gameObject.name = prefab.name;
        return gameObject;
    }


}
