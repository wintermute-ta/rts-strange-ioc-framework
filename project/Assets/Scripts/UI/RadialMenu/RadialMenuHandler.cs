using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using strange.extensions.signal.impl;
using Core;

[RequireComponent(typeof(RectTransform), typeof(AspectRatioFitter))]
public class RadialMenuHandler : BaseUIHandler
{
    [SerializeField]
    [Range(0.0f, 360.0f)]
    private float baseAngle = 360.0f;
    [SerializeField]
    [Range(-360.0f, 360.0f)]
    private float baseShiftAngle = 0.0f;

    [SerializeField]
    [Tooltip("Item size(square) in percent from screen size")]
    private float size = 0.25f;
    [SerializeField]
    [Tooltip("Distance from center in percents")]
    [Range(0.0f, 1.0f)]
    private float baseRadius = 0.6f;
    [SerializeField]
    [Tooltip("Create new item only when previous  has been appeared")]
    private bool _isOneByOne = true;
    public bool IsOneByOne { get { return _isOneByOne; } }

    public Signal<int> ItemSelectedSignal = new Signal<int>();
    private List<RadialItemHandler> items;
    private int hightlitedItem = -1;
    private float stepAngle = 0.0f;

    private float endAngle = 0.0f;
    private float shiftAngle = 0.0f;

    public override void Open()
    {
        RectTransform rectTransform = ((RectTransform)transform);
        rectTransform.anchorMax = new Vector2(rectTransform.anchorMax.x, size);
        rectTransform.anchorMin= new Vector2(rectTransform.anchorMin.x, 0.0f);
        hightlitedItem = -1;
        base.Open();
    }
    /// <summary>
    /// Calculate Items positions in Handler space
    /// </summary>
    /// <param name="radialItems"> items</param>
    /// <param name="angle"> angle of arc of circle(270f to fill all circle)</param>
    /// <param name="shift"> define start angle</param>
    public void SetItems(List<RadialItemHandler> radialItems, float angle, float shift)
    {
        float step = angle / (radialItems.Count - 1);
        stepAngle = step;
        endAngle = angle;
        shiftAngle = shift;
        float radius = baseRadius;
        // to radians
        step *= (float)(Mathf.PI / 180.0d);

        float shiftRad = Mathf.Deg2Rad * shift;
        for (int i = 0; i < radialItems.Count; i++)
        {
            RadialItemHandler item = radialItems[i];
            Vector2 position = new Vector2(Mathf.Sin(shiftRad + step * i) * radius + 0.5f, Mathf.Cos(shiftRad + step * i) * radius + 0.5f);
            item.SetTarget(new Vector2(position.x + size, position.y + size), new Vector2(position.x - size, position.y - size));
            item.CalculateStartAnchors();
        }
        items = radialItems;
    }
    /// <summary>
    /// Calculate Items positions in Handler space with static params(sets in editor)
    /// </summary>
    /// <param name="radialItems"></param>
    public void SetItems(List<RadialItemHandler> radialItems)
    {
        SetItems(radialItems, baseAngle, baseShiftAngle);
    }

    //calulate pointer position in percents (UI space);
    private Vector2 PointerToUI(Vector2 pointer)
    {
        RectTransform rectTransform = (RectTransform)transform;
        Vector2 position = Vector2.zero;
        position.x = (pointer.x - (rectTransform.position.x - rectTransform.rect.size.x/2.0f)) / rectTransform.rect.size.x;
        position.y = (pointer.y - (rectTransform.position.y - rectTransform.rect.size.y/2.0f)) / rectTransform.rect.size.y;
        return position;
    }

    private float GetAngle(Vector2 to)
    {
        float angle = Mathf.Atan2(to.x - 0.5f, to.y - 0.5f) * Mathf.Rad2Deg;
        //to 360 deg
        if(angle < 0.0f)
        {
            angle += 360.0f; 
        }
        return angle;
    }

    private int GetItemOnPointer(ITouchData data)
    {
        //if return -1 if there are no items
        if(items == null || items.Count <= 0)
        {
            return -1;
        }
        Vector2 position = PointerToUI(data.Position);
        float angle = GetAngle(position) - shiftAngle;
        float step = stepAngle;
        //add shift
        angle += step / 2.0f;
        if(angle > 360.0f)
        {
            angle -= 360.0f;
        }
        int num = (int)Mathf.Floor((angle)/step);
        if(num >= items.Count || !IsPointerOnItem(position,num))
        {
            return -1;
        }
        return num;
    }

    private bool IsPointerOnItem(Vector2 pos, int num)
    {
        RectTransform item = (RectTransform)items[num].transform;
        if(!items[num].IsInteractable)
        {
            return false;
        }
        if (pos.x <= item.anchorMax.x && pos.x >= item.anchorMin.x && pos.y <= item.anchorMax.y && pos.y >= item.anchorMin.y)
        {
            return true;
        }


        return false;
    }

    public void UpdateItems(ITouchData data)
    {
        int num = GetItemOnPointer(data);
        if (num == hightlitedItem)
        {
            return;
        }

        if (hightlitedItem != -1)
        {
            items[hightlitedItem].IsHighlighted = false;
        }

        if (num != -1)
        {
            items[num].IsHighlighted = true;
        }

        hightlitedItem = num;
        ItemSelectedSignal.Dispatch(num);
    }


    //public float CalculateRadius(float step, float itemSize,float startAngle,float endAngle)
    //{
    //    //if (step >= 90.0f)
    //    //{
    //    //    return baseRadius;
    //    //}

    //    step *= Mathf.Deg2Rad;
    //    startAngle *= Mathf.Deg2Rad;
    //    endAngle *= Mathf.Deg2Rad;

    //    RectTransform rectTransform = ((RectTransform)transform);
    //    float radius = Mathf.Sqrt(itemSize * itemSize * 2.0f) / Mathf.Sin(step / 2.0f);
    //    //Check pos
    //    Vector2 startPos = new Vector2(Mathf.Sin(startAngle) * radius + 0.5f, Mathf.Cos(startAngle) * radius + 0.5f);
    //    startPos -= new Vector2(0.5f, 0.5f);
    //    startPos *= rectTransform.rect.size.x;
    //    startPos += (Vector2) transform.position;
    //    Debug.Log(startPos + " size (" + Display.main.renderingWidth + "," + Display.main.renderingHeight + ")");
    //    //
    //    return radius;
    //}
}
