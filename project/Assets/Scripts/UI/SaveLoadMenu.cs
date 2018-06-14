using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using strange.extensions.mediation.impl;
using strange.extensions.pool.api;

public class SaveLoadMenu : MonoBehaviour
{
    [Inject]
    public IPool<SaveLoadItem> pool { get; private set; }

    public Text menuLabel, actionButtonLabel;
    public InputField nameInput;

    public event Action<string> OnSave = delegate { };
    public event Action<string> OnLoad = delegate { };
    public event Action<string> OnDelete = delegate { };
    public event Action OnClose = delegate { };
    public event Action OnOpen = delegate { };

    private RectTransform listContent;
    private bool saveMode;

    public void Open(bool saveMode)
    {
        this.saveMode = saveMode;
        if (saveMode)
        {
            menuLabel.text = "Save Map";
            actionButtonLabel.text = "Save";
        }
        else
        {
            menuLabel.text = "Load Map";
            actionButtonLabel.text = "Load";
        }
        FillList();
        gameObject.SetActive(true);
        OnOpen.Invoke();
    }

    public void Close()
    {
        gameObject.SetActive(false);
        OnClose.Invoke();
    }

    public void Action()
    {
        string path = GetSelectedPath();
        if (path == null)
        {
            return;
        }
        if (saveMode)
        {
            OnSave(path);
        }
        else {
            OnLoad(path);
        }
        Close();
    }

    public void SelectItem(string name)
    {
        nameInput.text = name;
    }

    public void Delete()
    {
        string path = GetSelectedPath();
        if (path == null)
        {
            return;
        }
        if (File.Exists(path))
        {
            File.Delete(path);
        }
        nameInput.text = "";
        FillList();
    }

    void FillList()
    {
        for (int i = listContent.childCount; i > 0; i--)
        {
            SaveLoadItem item = listContent.GetChild(i - 1).GetComponent<SaveLoadItem>();
            item.OnSelected -= Item_OnSelected;
            item.transform.SetParent(null, false);
            pool.ReturnInstance(item);
            //Destroy(listContent.GetChild(i).gameObject);
        }
        string[] paths = Directory.GetFiles(Application.persistentDataPath, "*.map.bytes");
        Array.Sort(paths);
        for (int i = 0; i < paths.Length; i++)
        {
            SaveLoadItem item = pool.GetInstance();
            item.gameObject.SetActive(true);
            item.OnSelected += Item_OnSelected;
            item.MapName = Path.GetFileNameWithoutExtension(paths[i]);
            item.transform.SetParent(listContent, false);
        }
    }

    private void Item_OnSelected(string mapName)
    {
        SelectItem(mapName);
    }

    string GetSelectedPath()
    {
        string mapName = nameInput.text;
        if (mapName.Length == 0)
        {
            return null;
        }
        return Path.Combine(Application.persistentDataPath, mapName + ".map.bytes");
    }
}