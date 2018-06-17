using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UI;
using System;

namespace Core
{
    public interface IUIManager
    {
        Transform Parent { get; }
        ITouchDetector TouchDetector { get; }
        bool IsAnyOpenedMenus { get; }

        BaseUIView CreateHandler(Transform parent, string name);
        BaseUIView CreateHandler(Transform parent, string name, Vector2 position);
        T CreateUI<T>(Transform parent, string name) where T : BaseUIView, new();
        T CreateUI<T>(Transform parent, string name, Vector2 position) where T : BaseUIView, new();
        void CloseUI(BaseUIView[] views);
        void ReturnToPool(BaseUIView handler);

        void OnMenuOpened(BaseUIView view);
        void OnMenuClosed(BaseUIView view);
        void OnBackPressed();
    }
}
