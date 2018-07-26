using System.Collections;
using System.Collections.Generic;
using strange.extensions.mediation.impl;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas))]
public class WorldSpaceCanvasHandler : View
{
    [SerializeField]
    private Canvas canvas;
    private Camera cachedCamera;

    protected override void Awake()
    {
        if(canvas == null)
        {
            Debug.LogWarning("You should set canvas by editor in WorldSpaceCanvasHandler in gameObject " + gameObject.name);
            canvas = this.GetComponent<Canvas>();
        }
        cachedCamera = Camera.main;
    }

    public void UpdateCanvas()
    {
        canvas.transform.eulerAngles = new Vector3(
            cachedCamera.transform.eulerAngles.x,
            cachedCamera.transform.eulerAngles.y,
            transform.eulerAngles.z
        );
    }

    private void LateUpdate()
    {
        UpdateCanvas();
    }
}
