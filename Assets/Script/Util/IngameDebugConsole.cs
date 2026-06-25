using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class InGameDebugConsole : MonoBehaviour
{
    // Struct to hold individual log data
    private struct LogMessage
    {
        public string message;
        public LogType type;
    }

    [Header("Console Settings")]
    [Tooltip("The key used to open/close the console.")]
    public bool showConsole = false;
    [Tooltip("Maximum number of logs to keep before deleting old ones.")]
    public int maxLogs = 50;

    private List<LogMessage> logs = new List<LogMessage>();
    private Vector2 scrollPosition;

    private void OnEnable()
    {
        // Subscribe to Unity's internal log event
        Application.logMessageReceived += HandleLog;
    }

    private void OnDisable()
    {
        // Unsubscribe to prevent memory leaks
        Application.logMessageReceived -= HandleLog;
    }

    private void Update()
    {
        // Toggle the console on and off
        if (Keyboard.current.backquoteKey.wasPressedThisFrame)
        {
            showConsole = !showConsole;
        }
    }
    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        logs.Add(new LogMessage { message = logString, type = type });
        
        if (logs.Count > maxLogs)
        {
            logs.RemoveAt(0);
        }

        scrollPosition.y = float.MaxValue; 
    }

    private void OnGUI()
    {
        if (!showConsole) return;

        // Adjusted width to 40% so it looks like a proper corner widget
        float width = Screen.width * 0.4f;
        float height = Screen.height * 0.4f;
        
        // Set X to 20 pixels from the left, and Y to 20 pixels from the bottom
        Rect windowRect = new Rect(20, Screen.height - height - 20, width, height);

        GUI.Box(windowRect, "Debug Console (Press ~ to toggle)");

        GUILayout.BeginArea(new Rect(windowRect.x + 10, windowRect.y + 25, windowRect.width - 20, windowRect.height - 35));
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);

        foreach (var log in logs)
        {
            GUI.contentColor = GetLogColor(log.type);
            GUILayout.Label(log.message);
        }

        GUILayout.EndScrollView();

        GUI.contentColor = Color.white;

        if (GUILayout.Button("Clear Console", GUILayout.Height(30)))
        {
            logs.Clear();
        }

        GUILayout.EndArea();
    }

    private Color GetLogColor(LogType type)
    {
        switch (type)
        {
            case LogType.Error:
            case LogType.Exception:
            case LogType.Assert:
                return Color.red;
            case LogType.Warning:
                return Color.yellow;
            default:
                return Color.white;
        }
    }
}