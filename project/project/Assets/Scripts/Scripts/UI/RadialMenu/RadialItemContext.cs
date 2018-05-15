using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RadialItemContext
{
    public ActionType type;
    public Action action;

    public RadialItemContext(ActionType actionType, Action callback)
    {
        type = actionType;
        action = callback;
    }

    //If you'll add new action type, you also should create RadialItem prefab with name "RadialItem_ActionType" like RadialItem_ACTION_UPGRADE in "Assets\Resources\Prefabs\UI\RadialMenu"
    public enum ActionType
    {
        ACTION_UPGRADE,
        ACTION_CREATE_SHIP,
        ACTION_CREATE_GUN,
        ACTION_DUMMY,
        ACTION_RETURN
    }
}