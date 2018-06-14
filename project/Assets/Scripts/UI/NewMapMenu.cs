using System;
using UnityEngine;

public class NewMapMenu : MonoBehaviour
{
    public event Action<int, int> OnCreateMap = delegate { };
    public event Action OnClose = delegate { };
    public event Action OnOpen = delegate { };

    public void Open()
    {
        gameObject.SetActive(true);
        OnOpen.Invoke();
    }

    public void Close()
    {
        gameObject.SetActive(false);
        OnClose.Invoke();
    }

    public void CreateSmallMap()
    {
        CreateMap(20, 15);
    }

    public void CreateMediumMap()
    {
        CreateMap(40, 30);
    }

    public void CreateLargeMap()
    {
        CreateMap(80, 60);
    }

    void CreateMap(int x, int z)
    {
        OnCreateMap.Invoke(x, z);
        Close();
    }
}