using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SliderHandler : BaseUIHandler {

    public override string Name
    {
        get
        {
            return type.ToString();
        }
    }
    [SerializeField]
    private SliderType type;
    [SerializeField]
    private RectTransform slider;

    private Camera mainCamera;
    private Canvas canvas;

    public void SetXAnchros(float min, float max)
    {
        slider.anchorMax = new Vector2(max, slider.anchorMax.y);
        slider.anchorMin = new Vector2(min, slider.anchorMin.y);
    }

    //void Update()
    //{
    //    //rotate canvas to camera
    //    canvas.transform.eulerAngles = new Vector3(
    //        mainCamera.transform.eulerAngles.x,
    //        mainCamera.transform.eulerAngles.y,
    //        transform.eulerAngles.z
    //        );
    //}

    //public override void Open()
    //{
    //    mainCamera = Camera.main;
    //    canvas = GetComponentInParent<Canvas>();
    //}

    public enum SliderType
    {
        BASE_SLIDER
    }
}
