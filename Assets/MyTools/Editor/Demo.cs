using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Demo : MonoBehaviour
{

    // GUI var
    // Main GUI
    private bool _isGuiOnShowing;
    private bool _showMainGUI;
    private bool _showOffsetGUI;
    private bool _showLogGUI;
    private float _holdTime = 0;

    private GUIStyle _labelStyle;
    private GUIStyle _buttonStyle;
    private GUIStyle _toggleStyle;

    // offset
    public static float currentOffsetDelay = 0f;
    private const float OffsetDelayDefault = 0.06f;
    private float _offsetDelayPerIncrease;
    private int _addCount;
    private bool _offset;
    private bool _offsetDefaultToggle = true;
    private bool _offsetRefreshed;

    // Auto play
    public static bool isAutoPlay;

    // Log
    private List<GUILog> logs;

    // Speed
    private bool _isSpeedUp;

    private void Start()
    {
        // Time.timeScale = 0.2f;
        logs = new List<GUILog>();

        _offsetDelayPerIncrease = (600 * 0.25f) / 11000f;
        _offsetDelayPerIncrease = (float)Math.Round((Decimal)_offsetDelayPerIncrease, 3, MidpointRounding.AwayFromZero);
        Debug.Log("_offsetDelayPerIncrease " + _offsetDelayPerIncrease);
        currentOffsetDelay = OffsetDelayDefault;
    }


    public void AddLog(string log)
    {
        if (!_showLogGUI) return;

        if (logs.Count > 10)
        {
            logs.RemoveAt(0);
        }
        logs.Add(new GUILog(log));
    }
    public void AddLog(string log, Color color)
    {
        if (!_showLogGUI) return;

        if (logs.Count > 10)
        {
            logs.RemoveAt(0);
        }

        logs.Add(new GUILog(log, color));
    }

    private void Update()
    {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        // if (!Debug.isDebugBuild) return;

        if (Input.GetMouseButton(0))
        {
            _holdTime += Time.deltaTime;
        }

        if (Input.GetMouseButtonUp(0))
        {
            _holdTime = 0;
        }

        if (_holdTime > 5f)
        {
            if (!_showMainGUI && !_isGuiOnShowing)
            {
                _isGuiOnShowing = true;
                _showMainGUI = true;
            }
        }
#endif
    }

    private void OnGUI()
    {
        // Initial
        // Style 
        InitialGUIStyle();

        MainGui();
        OffsetGUI();
        DebugLogGUI();
    }

    private void InitialGUIStyle()
    {
        // Style 
        _labelStyle = new GUIStyle
        {
            fontSize = 32,
            alignment = TextAnchor.MiddleCenter,
            normal =
            {
                textColor = Color.white
            }
        };
        _buttonStyle = new GUIStyle(UnityEngine.GUI.skin.button)
        {
            fontSize = 32,
            alignment = TextAnchor.MiddleCenter,
            normal =
            {
                textColor = Color.white
            }
        };

        _toggleStyle = new GUIStyle(UnityEngine.GUI.skin.button)
        {
            fontSize = 32,
            normal =
            {
                textColor = Color.white
            },
        };
        _toggleStyle.onNormal.textColor = Color.green;
    }

    private void MainGui()
    {
        if (_showMainGUI)
        {
            // Initial
            UnityEngine.GUI.Box(new Rect(0, 100, 400, 600), "");

            if (UnityEngine.GUI.Button(new Rect(300, 100, 50, 50), "X"))
            {
                _isGuiOnShowing = false;
                _showMainGUI = false;
            }

            if (UnityEngine.GUI.Button(new Rect(50, 150, 200, 50), "Log", _buttonStyle))
            {
                _showMainGUI = false;
                _showLogGUI = true;
            }

            if (UnityEngine.GUI.Button(new Rect(50, 250, 200, 50), "Offset", _buttonStyle))
            {
                _showMainGUI = false;
                _showOffsetGUI = true;
            }

            isAutoPlay = UnityEngine.GUI.Toggle(new Rect(50, 350, 200, 50), isAutoPlay, "Auto play", _toggleStyle);

            if (UnityEngine.GUI.Button(new Rect(50, 450, 200, 50), "Speed up", _buttonStyle))
            {
                _isSpeedUp = !_isSpeedUp;
                if (_isSpeedUp)
                {
                    Time.timeScale = 3;
                }
                else
                {
                    Time.timeScale = 1;
                }
            }

            if (UnityEngine.GUI.Button(new Rect(50, 550, 300, 50), "Add 1000 golds", _buttonStyle))
            {

            }
        }
    }

    private void OffsetGUI()
    {
        if (_showOffsetGUI)
        {
            UnityEngine.GUI.Box(new Rect(0, 100, 400, 515), "");

            if (UnityEngine.GUI.Button(new Rect(50, 130, 50, 50), "<", _buttonStyle))
            {
                _showMainGUI = true;
                _showOffsetGUI = false;
            }

            _labelStyle.normal.textColor = Color.green;
            UnityEngine.GUI.Label(new Rect(60, 200, 250, 50), "Offset:" + currentOffsetDelay, _labelStyle);

            _offsetDefaultToggle =
                UnityEngine.GUI.Toggle(new Rect(50, 280, 130, 70), _offsetDefaultToggle, "Default", _toggleStyle);

            UnityEngine.GUI.enabled = !_offsetDefaultToggle;
            // Controller

            if (UnityEngine.GUI.Button(new Rect(50, 380, 80, 80), "<", _buttonStyle))
            {
                currentOffsetDelay -= _offsetDelayPerIncrease;
                currentOffsetDelay = (float)Math.Round((Decimal)currentOffsetDelay, 3, MidpointRounding.AwayFromZero);
                _addCount--;
            }

            _labelStyle.normal.textColor = Color.white;
            UnityEngine.GUI.Label(new Rect(150, 380, 100, 80), _addCount.ToString(), _labelStyle);

            if (UnityEngine.GUI.Button(new Rect(270, 380, 80, 80), ">", _buttonStyle))
            {
                currentOffsetDelay += _offsetDelayPerIncrease;
                currentOffsetDelay = (float)Math.Round((Decimal)currentOffsetDelay, 4, MidpointRounding.AwayFromZero);
                _addCount++;
            }

            if (UnityEngine.GUI.Button(new Rect(50, 490, 100, 75), "Reset", _buttonStyle))
            {
                currentOffsetDelay = 0;
                _addCount = 0;
            }

            UnityEngine.GUI.enabled = true;

            if (_offsetDefaultToggle)
            {
                currentOffsetDelay = OffsetDelayDefault;
                _addCount = 0;
                _offsetRefreshed = false;
            }
            else
            {
                if (!_offsetRefreshed)
                {
                    currentOffsetDelay = 0;
                    _offsetRefreshed = true;
                }
            }
        }
    }

    private void DebugLogGUI()
    {
        if (_showLogGUI)
        {
            int w = Screen.width, h = Screen.height;

            UnityEngine.GUI.Box(new Rect(0, 100, w / 1.7f + 50, (h * 2 / 100f) * 10 + 230f), "");

            GUIStyle style = new GUIStyle();

            if (UnityEngine.GUI.Button(new Rect(50, 130, 50, 50), "<", _buttonStyle))
            {
                _showMainGUI = true;
                _showLogGUI = false;
            }

            /*string logText = "";
            for (int i = 0; i < logs.Count; i++)
            {
                logText += $"\n {logs[i]}";
            }
            Rect rect = new Rect(50, 150, w / 1.7f, (h * 2 / 90f ) * logs.Count);
            style.alignment = TextAnchor.UpperLeft;
            style.fontSize = h * 2 / 100;
            style.normal.textColor = Color.green;
            
            GUI.Label(rect, logText, style);*/
            for (int i = 0; i < logs.Count; i++)
            {
                var guiLog = logs[i];
                var posY = 200 + ((h * 2 / 90f) * i);
                Rect rect = new Rect(50, posY, w / 1.7f, (h * 2 / 90f));
                style.alignment = TextAnchor.UpperLeft;
                style.fontSize = h * 2 / 100;
                style.normal.textColor = guiLog.color;
                UnityEngine.GUI.Label(rect, guiLog.log, style);
            }
        }
    }
}

[Serializable]
public class GUILog
{
    public GUILog(string log)
    {
        this.log = log;
        this.color = Color.white;
    }

    public GUILog(string log, Color color)
    {
        this.log = log;
        this.color = color;
    }

    public string log;
    public Color color;
}