using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;
using strange.extensions.signal.impl;

[RequireComponent(typeof(RectTransform))]
public class RadialItemHandler : AnimatedUIHandler
{
    [SerializeField]
    private Image itemImg;

    [SerializeField]
    public ItemContext.ActionType actionType;
    public override string Name { get { return actionType.ToString();} }
    [SerializeField]
    private Sprite baseSprite;
    [SerializeField]
    private Sprite highlightedSprite;

    public bool IsHighlighted {
        get
        {
            return isHighlighted;
        }
        set
        {
            isHighlighted = value;
            UpdateItem();
        }
    }
    private bool isHighlighted = false;
    private int itemNumber = -1;

    public Signal<RadialItemHandler> SignalSelected = new Signal<RadialItemHandler>();

    public bool IsInteractable { get; set; }

    //public void Init(Sprite iconSprite)
    //{
    //    itemImg.overrideSprite = iconSprite;
    //}

    public void SetSize(Vector2 size)
    {
        ((RectTransform)transform).sizeDelta = size;
    }

    public override void Open()
    {
        itemImg.overrideSprite = baseSprite;
        base.Open();
    }

    private void UpdateItem()
    {
        if (isHighlighted)
        {
            itemImg.overrideSprite = highlightedSprite;
        }
        else
        {
            itemImg.overrideSprite = baseSprite;
        }
    }

    public override void OnAppear()
    {
        IsInteractable = true;
        if (isHighlighted)
        {
            SignalSelected.Dispatch(this);
        }
        base.OnAppear();
    }

    protected override void StartAppearing()
    {
        IsInteractable = false;
        base.StartAppearing();
    }

    protected override void StartDisappearing()
    {
        IsInteractable = false;
        IsHighlighted = false;
        base.StartDisappearing();
    }
}
