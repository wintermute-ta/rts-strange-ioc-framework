using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using strange.extensions.signal.impl;

public class AnimatedUIHandler : BaseUIHandler {

    public enum AppearType
    {
        FROM_CENTER,
        FROM_LEFT,
        FROM_RIGHT,
        FROM_TOP,
        FROM_BOTTOM
    }

    [SerializeField]
    [Tooltip("If true UI will appearing from zero size to target, else UI will have static size")]
    private bool fromZeroSize;
    [SerializeField]
    private AppearType appearType = AppearType.FROM_CENTER;
    [SerializeField]
    private float animationTime = 1.0f;
    private float currentTime = 0.0f;
    private Vector2 targetAnchorsMax = Vector2.zero;
    private Vector2 targetAnchorsMin = Vector2.zero;
    private RectTransform rectTransform;

    //Start anchors sets Only by AppearType
    private Vector2 startAnchorsMax = Vector2.zero;
    private Vector2 startAnchorsMin = Vector2.zero;
    private int animationDirection = 0;


    #region AnimatedUIHandler
    public void SetTarget(Vector2 max, Vector2 min)
    {
        targetAnchorsMax = max;
        targetAnchorsMin = min;
    }

    public void SetAppearType(AppearType type)
    {
        appearType = type;
    }

    private void OnAnimationMove()
    {
        currentTime += Time.deltaTime * animationDirection;

        if (animationDirection > 0)
        {
            //Direct animation
            if (currentTime >= animationTime)
            {
                rectTransform.anchorMax = targetAnchorsMax;
                rectTransform.anchorMin = targetAnchorsMin;
                animationDirection = 0;
                OnAppear();
            }
        }
        else
        {
            //Inverse animation
            if (currentTime <= 0.0f)
            {
                rectTransform.anchorMax = startAnchorsMax;
                rectTransform.anchorMin = startAnchorsMin;
                animationDirection = 0;
                OnDisappear();
            }
        }

        float t = 1.0f / animationTime * currentTime;
        rectTransform.anchorMax = Vector2.Lerp(startAnchorsMax, targetAnchorsMax, t);
        rectTransform.anchorMin = Vector2.Lerp(startAnchorsMin, targetAnchorsMin, t);
    }

    public void CalculateStartAnchors()
    {
        Vector2 startPosition;
        Vector2 anchorsShift = new Vector2(targetAnchorsMax.x - targetAnchorsMin.x, targetAnchorsMax.y - targetAnchorsMin.y);
        anchorsShift /= 2.0f;

        switch (appearType)
        {
            case AppearType.FROM_BOTTOM:
                startPosition = new Vector2(0.5f, 0.0f - anchorsShift.y);
                break;
            case AppearType.FROM_CENTER:
                startPosition = new Vector2(0.5f, 0.5f);
                break;
            case AppearType.FROM_LEFT:
                startPosition = new Vector2(0.0f - anchorsShift.x, 0.5f);
                break;
            case AppearType.FROM_RIGHT:
                startPosition = new Vector2(1.0f + anchorsShift.x, 0.5f);
                break;
            case AppearType.FROM_TOP:
                startPosition = new Vector2(0.5f, 1.0f + anchorsShift.y);
                break;
            default:
                startPosition = Vector2.zero;
                break;
        }

        if(fromZeroSize)
        {
            startAnchorsMin = startAnchorsMax = startPosition;
        }
        else
        {
            startAnchorsMin = startPosition - anchorsShift;
            startAnchorsMax = startPosition + anchorsShift;
        }
    }

    #endregion

    #region Unity
    void Update()
    {
        if(animationDirection != 0)
        {
            OnAnimationMove();
        }
    }
    #endregion

    #region override

    protected override void StartAppearing()
    {
        animationDirection = 1;
    }

    protected override void StartDisappearing()
    {
        animationDirection = -1;
    }

    public override void Open()
    {
        rectTransform = (RectTransform)transform;
        base.Open();
    }

    public override void OnDisappear()
    {
        rectTransform.anchorMax = startAnchorsMax;
        rectTransform.anchorMin = startAnchorsMin;
        base.OnDisappear();
    }

    public override void ReInit()
    {
        currentTime = 0;
        animationDirection = 0;
    }
    #endregion

}
