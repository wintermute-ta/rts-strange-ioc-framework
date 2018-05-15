using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : SingletonMonoBehaviour<ResourceManager>
{
    private Dictionary<string, GameObject> prefabs;
    private Dictionary<string, Texture> textures;

    internal override void Awake()
    {
        base.Awake();
        prefabs = CreateResourceDictionary<GameObject>("Prefabs");
        textures = CreateResourceDictionary<Texture>("Textures");
    }

    private Dictionary<string, T> CreateResourceDictionary<T>(string type) where T : UnityEngine.Object
    {
        Dictionary<string, T> resources = new Dictionary<string, T>();
        T[] loaded = Resources.LoadAll<T>(string.Format("{0}/", type));
        for (int i = 0; i < loaded.Length; i++)
        {
            resources.Add(loaded[i].name, loaded[i]);
        }
        return resources;
    }

    public GameObject GetPrefab(string name)
    {
        return prefabs.ContainsKey(name) ? prefabs[name] : null;
    }

    public T GetPrefabComponent<T>(string name) where T : Component
    {
        return prefabs.ContainsKey(name) ? prefabs[name].GetComponent<T>() : null;
    }

    public Texture GetTexture(string name)
    {
        return textures.ContainsKey(name) ? textures[name] : null;
    }
}
