using System;
using System.Collections;
using System.Collections.Generic;
using strange.extensions.signal.impl;
using UnityEngine;

[Serializable]
public class ItemContext
{
    public string PrefabName { get; set; }
    public Action ItemAction { get; set; }

    public ItemContext(string name, Action action)
    {
        PrefabName = name;
        ItemAction = action;
    }

    public enum ActionType
    {
        ACTION_NONE,
        ACTION_UPGRADE,
        ACTION_CREATE_SHIP,
        ACTION_CREATE_GUN,
        ACTION_CREATE_CASTLE,
        ACTION_REMOVE,
        ACTION_DUMMY,
        ACTION_RETURN
    }
}