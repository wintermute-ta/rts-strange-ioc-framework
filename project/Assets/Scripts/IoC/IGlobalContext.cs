using strange.extensions.mediation.api;
using strange.extensions.mediation.impl;
using UnityEngine;

public interface IGlobalContext
{
    T GetInstance<T>();
    T GetInstance<T>(object name);
    IMediationBinder MediationBinder { get; }
    T Instance<T>() where T : class, new();
    T InstanceComponent<T>(GameObject objectForAddComponent) where T : Component, new();
    T InstanceScriptableObject<T>() where T : ScriptableObject;
    T InstancePrefab<T>(GameObject prefab, Transform parent = null) where T : Component, new();
    T InstancePrefabView<T>(GameObject prefab, Transform parent = null) where T : View, new();
}
