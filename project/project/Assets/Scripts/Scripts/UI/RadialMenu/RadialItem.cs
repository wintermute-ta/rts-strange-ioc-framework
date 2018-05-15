using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class RadialItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private Image backgroundImg;
    [SerializeField]
    private Image iconImg;

    [SerializeField]
    private int itemNumber = -1;
    [SerializeField]
    private Color backgroundColor = Color.white;
    [SerializeField]
    private Color highlightedColor = Color.blue;

    private Action<int> onSelected;

    public void Init(Action<int> action, int num, Sprite iconSprite)
    {
        Init(action, num);
        iconImg.overrideSprite = iconSprite;
    }

    public void Init(Action<int> action, int num)
    {
        onSelected = action;
        itemNumber = num;
    }

    public void SetSize(Vector2 size)
    {
        ((RectTransform)transform).sizeDelta = size;
    }

    public void SetAnchors(Vector2 max, Vector2 min)
    {
        RectTransform rectTransform = ((RectTransform)transform);
        rectTransform.anchorMax = max;
        rectTransform.anchorMin = min;
    }


    public void OnPointerEnter(PointerEventData eventData)
    {
        backgroundImg.color = highlightedColor;
        onSelected(itemNumber);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        backgroundImg.color = backgroundColor;
        onSelected(-1);
    }
}
