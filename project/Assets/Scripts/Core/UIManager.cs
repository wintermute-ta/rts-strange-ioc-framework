using System.Collections.Generic;
using strange.extensions.pool.api;
using UnityEngine;
using Core.InstanceProviders;

namespace Core
{
    public class UIManager : IUIManager
    {
        [Inject]
        public IResourceManager ResourceManager { get; private set; }
        [Inject]
        public UIGlobalSignals UISignals { get; private set; }

        private Dictionary<string, IPool<GameObject>> uiPool = new Dictionary<string, IPool<GameObject>>();
        private Dictionary<string, GameObject> uiPrefabs = new Dictionary<string, GameObject>();

        public string PrefabName { get { return "Canvas"; } }
        public Transform Parent
        {
            get
            {
                if (canvas == null)
                {
                    canvas = GetFromPool(PrefabName).transform;
                }
                return canvas;
            }
        }
        private Transform canvas;

        #region CreateWindow
        public GameObject CreateWindow(Transform parent, string prefabName)
        {
            GameObject window = GetFromPool(prefabName);
            window.transform.SetParent(parent, false);
            return window;
        }

        public GameObject CreateWindow(Transform parent, string prefabName, Vector2 position)
        {
            GameObject window = GetFromPool(prefabName);
            window.transform.SetParent(parent, false);
            window.transform.position = position;
            return window;
        }
        #endregion

        #region CreateHandler
        public T CreateHandler<T>(Transform parent, string prefab) where T : BaseUIHandler, new()
        {
            GameObject window = CreateWindow(parent, prefab);
            T handler = GlobalContext.Get().InstanceComponent<T>(window);
            return handler;
        }

        public T CreateHandler<T>(Transform parent, string prefab, Vector2 position) where T : BaseUIHandler, new()
        {
            GameObject window = CreateWindow(parent, prefab, position);
            T handler = GlobalContext.Get().InstanceComponent<T>(window);
            return handler;
        }
        #endregion

        #region CreateUI
        public T CreateUI<T>(Transform parent, string prefab, bool globalSignals = false) where T : BaseUIModel, new()
        {
            T model = GlobalContext.Get().Instance<T>();
            GameObject window = CreateWindow(parent, prefab);
            model.Init(window, globalSignals);
            return model;
        }

        public T CreateUI<T>(Transform parent, string prefab, Vector2 position, bool globalSignals = false) where T : BaseUIModel, new()
        {
            T model = GlobalContext.Get().Instance<T>();
            GameObject window = CreateWindow(parent, prefab, position);
            model.Init(window, globalSignals);
            return model;
        }
        #endregion

        #region PostConstruct
        [PostConstruct]
        public void PostConstuct()
        {
            uiPrefabs = CreatePrefabsDictionary<BaseUIHandler>();
            UISignals.ReturnToPool.AddListener(ReturnToPool);
        }

        private Dictionary<string, GameObject> CreatePrefabsDictionary<T>() where T : BaseUIHandler
        {
            Dictionary<string, GameObject> dictionary = new Dictionary<string, GameObject>();
            T[] prefabsArray = ResourceManager.GetComponentsFromPrefabs<T>();

            if (prefabsArray != null)
            {
                for (int i = 0; i < prefabsArray.Length; i++)
                {
                    if (!dictionary.ContainsKey(prefabsArray[i].Name))
                    {
                        dictionary.Add(prefabsArray[i].Name, prefabsArray[i].gameObject);
                    }
                }
            }

            return dictionary;
        }
        #endregion

        #region Pool
        public GameObject GetFromPool(string name)
        {
            IPool<GameObject> targetPool;
            if (uiPool.TryGetValue(name, out targetPool))
            {
                return targetPool.GetInstance();
            }
            else
            {
                targetPool = GlobalContext.Get().GetInstance<IPool<GameObject>>();
                GameObjectInstanceProvider provider = GlobalContext.Get().GetInstance<GameObjectInstanceProvider>();
                GameObject prefab;
                if (!uiPrefabs.TryGetValue(name, out prefab))
                {
                    Debug.LogError("Prefab with name " + name + " doesn't exist");
                    return null;
                }
                provider.Prefab = prefab;
                targetPool.instanceProvider = provider;
                targetPool.inflationType = PoolInflationType.INCREMENT;
                targetPool.poolType = typeof(GameObject);
                uiPool.Add(name, targetPool);
                return targetPool.GetInstance();
            }
        }

        public void ReturnToPool(BaseUIHandler handler)
        {
            if (handler == null)
            {
                return;
            }

            IPool<GameObject> targetPool;
            if (uiPool.TryGetValue(handler.Name, out targetPool))
            {
                handler.gameObject.SetActive(false);
                targetPool.ReturnInstance(handler.gameObject);
            }
            else
            {
                GameObject.Destroy(handler.gameObject);
            }
        }
        #endregion
    }
}
