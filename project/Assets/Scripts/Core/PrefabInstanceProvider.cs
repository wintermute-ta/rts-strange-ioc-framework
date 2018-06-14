using strange.framework.api;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PrefabInstanceProvider : IInstanceProvider
{
    private GameObject prefab;
    public PrefabInstanceProvider(GameObject prefab)
    {
        this.prefab = prefab;
    }

    public object GetInstance(Type key)
    {
        return GameObject.Instantiate(prefab).GetComponent(key);
    }

    public T GetInstance<T>()
    {
        object instance = GetInstance(typeof(T));
        T retv = (T)instance;
        return retv;
    }
}
