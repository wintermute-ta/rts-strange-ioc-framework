using strange.extensions.pool.api;
using System;
using UnityEngine;
using UnityEngine.UI;

public class SaveLoadItem : MonoBehaviour, IPoolable
{
	//public SaveLoadMenu menu;

	public string MapName
    {
		get { return mapName; }
		set
        {
			mapName = value;
			transform.GetChild(0).GetComponent<Text>().text = value;
		}
	}

    void Awake()
    {
        retain = false;
    }

    public bool retain { get; private set; }

    public event Action<string> OnSelected = delegate { };

	private string mapName;

	public void Select()
    {
        OnSelected.Invoke(mapName);
	}

    public void Restore()
    {
        OnReturnToPool();
    }

    public void Retain()
    {
        retain = true;
    }

    public void Release()
    {
        OnReturnToPool();
    }

    private void OnReturnToPool()
    {
        gameObject.SetActive(false);
    }
}