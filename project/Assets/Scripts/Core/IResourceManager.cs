using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IResourceManager
{
    GameObject GetPrefab(string name);
    T GetPrefabComponent<T>(string name) where T : Component;
    T[] GetComponentsFromPrefabs<T>() where T : Component;
    Texture GetTexture(string name);
    byte[] GetMap(string name);
}
