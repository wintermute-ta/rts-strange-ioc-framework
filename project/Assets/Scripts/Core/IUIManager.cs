using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IUIManager {

    Transform Parent { get; }

    GameObject CreateWindow(Transform parent, string prefabName);
    GameObject CreateWindow(Transform parent, string prefabName, Vector2 position);
    T CreateHandler<T>(Transform parent, string prefab) where T : BaseUIHandler, new();
    T CreateHandler<T>(Transform parent, string prefab, Vector2 position) where T : BaseUIHandler, new();
    T CreateUI<T>(Transform parent, string prefab, bool globalSignals = false) where T : BaseUIModel, new();
    T CreateUI<T>(Transform parent, string prefab, Vector2 position, bool globalSignals = false) where T : BaseUIModel, new();
    GameObject GetFromPool(string name);
    void ReturnToPool(BaseUIHandler handler);
}
