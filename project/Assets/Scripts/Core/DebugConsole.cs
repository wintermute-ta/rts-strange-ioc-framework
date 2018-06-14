using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugConsole : MonoBehaviour, IDebugConsole
{
    const int ID_GUI_ERROR_WINDOW = 1;
    public bool Visible { get; set; }

    private class DebugMessageItem
    {
        public string Message;
        public string StackTrace;
        public LogType Type;
    }

    private List<DebugMessageItem> messages = new List<DebugMessageItem>();
    private List<DebugMessageItem> errors = new List<DebugMessageItem>();
    private Vector2 scrollPositionErrorMessageWindow = Vector2.zero;
    #region Unity
    // Use this for initialization after deserialization
    void Awake()
	{
        Application.logMessageReceived += Application_logMessageReceived;
        Visible = false;
    }

    void OnGUI()
    {
        if (Visible)
        {
            OnGUIConsole();
        }
        if (errors.Count > 0)
        {
            OnGUIErrorMessage();
        }
    }
    #endregion

    private void Application_logMessageReceived(string condition, string stackTrace, LogType type)
    {
        DebugMessageItem item = new DebugMessageItem() { Message = condition, StackTrace = stackTrace, Type = type };
        AddToList(item);
    }

    private void AddToList(DebugMessageItem item)
    {
        if ((item.Type == LogType.Assert) || (item.Type == LogType.Error) || (item.Type == LogType.Exception))
        {
            errors.Add(item);
        }
        messages.Add(item);
    }

    private void OnGUIConsole()
    {
        // TODO: Implement console functionality
    }

    private void OnGUIErrorMessage()
    {
        Vector2 windowSize = new Vector2(Screen.width, Screen.height) * 0.5f;
        Rect rect = new Rect(windowSize * 0.5f, windowSize);
        GUI.ModalWindow(ID_GUI_ERROR_WINDOW, rect, GUIErrorMessageWindowHandler, string.Format("Errors ({0})", errors.Count));
    }

    private void GUIErrorMessageWindowHandler(int id)
    {
        GUILayout.BeginVertical();
        scrollPositionErrorMessageWindow = GUILayout.BeginScrollView(scrollPositionErrorMessageWindow);
        GUILayout.Label(errors[0].Message);
        GUILayout.FlexibleSpace();
        GUILayout.Label(errors[0].StackTrace);
        GUILayout.FlexibleSpace();
        GUILayout.EndScrollView();
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Skip All", new GUILayoutOption[] { GUILayout.ExpandWidth(false) }))
        {
            errors.Clear();
        }
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Close", new GUILayoutOption[] { GUILayout.ExpandWidth(false) }))
        {
            errors.RemoveAt(0);
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.EndVertical();
    }
}
