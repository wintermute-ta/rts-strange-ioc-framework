using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ResourceManager : IResourceManager
{
    private Dictionary<string, GameObject> prefabs;
    private Dictionary<string, Texture> textures;
    private Dictionary<string, TextAsset> maps;

    public ResourceManager()
    {
        prefabs = CreateResourceDictionary<GameObject>("Prefabs");
        textures = CreateResourceDictionary<Texture>("Textures");
        maps = CreateResourceDictionary<TextAsset>("Maps");
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

    public T[] GetComponentsFromPrefabs<T>() where T : Component
    {
        GameObject[] prefabArray = prefabs.Values.ToArray();
        List<T> targetPrefabs = new List<T>();
        if (prefabArray == null)
        {
            return targetPrefabs.ToArray();
        }

        for(int i = 0; i < prefabArray.Length; i++)
        {
            T component = prefabArray[i].GetComponent<T>();
            if (component != null)
            {
                targetPrefabs.Add(component);
            }
        }

        return targetPrefabs.ToArray();
    }

    public Texture GetTexture(string name)
    {
        return textures.ContainsKey(name) ? textures[name] : null;
    }

    public byte[] GetMap(string name)
    {
        return maps.ContainsKey(name) ? maps[name].bytes : null;
    }
}
