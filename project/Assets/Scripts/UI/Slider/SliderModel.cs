using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SliderModel : BaseUIModel {

    private float maxValue = 1.0f;
    private float minValue = 0.0f;
    private float currentValue = 0.0f;

    public SliderHandler Slider { get { return (SliderHandler)Handler; } }

    public void SetRange(float min, float max)
    {
        maxValue = max;
        minValue = min;
        UpdateSlider();
    }

    public void Init(float value)
    {
        Init(0.0f, value, value);
    }

    public void Init(float min, float max, float value)
    {
        currentValue = value;
        SetRange(min, max);
    }

    public void SetValue(float value)
    {
        //if (value > maxValue)
        //{
        //    value = maxValue;
        //}
        //else if (value < minValue)
        //{
        //    value = minValue;
        //}

        //currentValue = value;
        currentValue = Mathf.Clamp(value, minValue, maxValue);
        UpdateSlider();
    }

    private void UpdateSlider()
    {
        float maxAnchorX = ((currentValue - minValue) / maxValue);
        float minAnchorX = maxAnchorX - 1.0f;
        Slider.SetXAnchros(minAnchorX, maxAnchorX);
    }
}
