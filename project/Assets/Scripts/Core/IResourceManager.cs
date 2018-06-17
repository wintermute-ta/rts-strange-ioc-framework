using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    public interface IResourceManager
    {
        GameObject GetPrefab(string name);
        T GetPrefabComponent<T>(string name) where T : Component;
        T[] GetComponentsFromPrefabs<T>() where T : Component;
        Texture GetTexture(string name);
        Material GetMaterial(string name);
        byte[] GetMap(string name);
        Sprite GetSprite(string name);
        AudioClip GetAudioClip(string name);
    }
}
