using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSCounter : MonoBehaviour, IFPSCounter
{
    public bool ShowFPS
    {
        get { return showFPS; }
        set
        {
            if (value != showFPS)
            {
                showFPS = value;
                OnShowFPSChanged();
            }
        }
    }
    public float UpdateTimeout { get; set; }
    public Color TextColor { get; set; }
    public TextAnchor Alignment { get; set; }
    public int FontSize { get; set; }
    public string Format { get; set; }

    private bool showFPS;
    private float fps;
    private GUIStyle style = new GUIStyle();
    private Rect rect;
    private Coroutine coroutineUpdate = null;

    #region Unity
    // Use this for initialization after deserialization
    void Awake()
	{
        UpdateTimeout = 0.5f;
        ShowFPS = true;
        rect = new Rect(0, 0, Screen.width, Screen.height);
        Alignment = TextAnchor.UpperCenter;
        FontSize = Screen.height / 25;
        TextColor = new Color(0.0f, 0.0f, 0.5f, 1.0f);
        Format = "FPS: {0}";
    }

    void OnGUI()
	{
        style.alignment = Alignment;
        style.fontSize = FontSize;
        style.normal.textColor = TextColor;

        string text = string.Format(Format, fps);
        GUI.Label(rect, text, style);
    }
    #endregion

    private void OnShowFPSChanged()
    {
        if (coroutineUpdate != null)
        {
            StopCoroutine(coroutineUpdate);
            coroutineUpdate = null;
        }

        if (ShowFPS)
        {
            coroutineUpdate = StartCoroutine(CoUpdateFPS());
        }
    }

    private IEnumerator CoUpdateFPS()
    {
        while (true)
        {
            yield return new WaitForSeconds(UpdateTimeout);
            fps = 1.0f / Time.deltaTime;
        }
    }
}
