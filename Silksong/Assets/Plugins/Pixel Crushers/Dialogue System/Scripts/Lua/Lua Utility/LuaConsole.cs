// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using System;
using System.Collections.Generic;
using System.Text;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// An in-game Lua console presented using Unity GUI. This console is activated by the
    /// key combination ~(tilde) + L (customizable) and allows you to enter Lua commands and 
    /// view the results. The up and down keys scroll through previous commands, and Escape 
    /// closes the console.
    /// </summary>
    [AddComponentMenu("")] // Use wrapper.
    public class LuaConsole : MonoBehaviour
    {

        /// <summary>
        /// Hold down this key and press Second Key to open console.
        /// </summary>
        [Tooltip("Hold down this key and press Second Key to open console.")]
        public KeyCode firstKey = KeyCode.BackQuote;

        /// <summary>
        /// Hold down First Key and press this key to open console.
        /// </summary>
        [Tooltip("Hold down First Key and press this key to open console.")]
        public KeyCode secondKey = KeyCode.L;

        /// <summary>
        /// Is the console visible or hidden?
        /// </summary>
        [Tooltip("Console is visible.")]
        public bool visible = false;

        /// <summary>
        /// Optional GUI Skin to style console window.
        /// </summary>
        [Tooltip("Optional GUI Skin to style console window.")]
        public GUISkin guiSkin;

        /// <summary>
        /// The minimum size of the console window.
        /// </summary>
        [Tooltip("Minimum size of console window.")]
        public Vector2 minSize = new Vector2(384f, 384f);

        /// <summary>
        /// The max number of previous commands to remember.
        /// </summary>
        [Tooltip("Max number of previous commands to remember.")]
        public int maxHistory = 20;

        /// <summary>
        /// If true, then while open set Time.timeScale to 0.
        /// </summary>
        [Tooltip("While open, set Time.timeScale to 0.")]
        public bool pauseGameWhileOpen = false;

        protected List<string> m_history = new List<string>();

        protected int m_historyPosition = 0;

        protected string m_input = string.Empty;

        protected string m_output = string.Empty;

        protected Rect m_windowRect = new Rect(0, 0, 0, 0);

        protected Rect m_closeButtonRect = new Rect(0, 0, 0, 0);

        protected Vector2 m_scrollPosition = new Vector2(0, 0);

        protected bool m_isFirstKeyDown = false;

        protected virtual void Start()
        {
            SetVisible(visible);
        }

        protected virtual void SetVisible(bool newValue)
        {
            visible = newValue;
            if (pauseGameWhileOpen) Time.timeScale = visible ? 0 : 1;
        }

        /// <summary>
        /// OnGUI draws the console if it's visible, and toggles visibility based on the key 
        /// trigger.
        /// </summary>
        protected virtual void OnGUI()
        {
            // Check for key combo to open console:
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == firstKey) m_isFirstKeyDown = true;
            else if (Event.current.type == EventType.KeyUp && Event.current.keyCode == firstKey) m_isFirstKeyDown = false;
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == secondKey && m_isFirstKeyDown)
            {
                Event.current.Use();
                SetVisible(!visible);
            }

            // If visible, draw:
            if (visible)
            {
                if (Event.current.type == EventType.Repaint) GUI.skin = UnityGUI.UnityGUITools.GetValidGUISkin(guiSkin);
                if (pauseGameWhileOpen) Time.timeScale = 0;
                DrawConsole();
            }
        }

        protected virtual void DrawConsole()
        {
            if (m_windowRect.width <= 0)
            {
                m_windowRect = DefineWindowRect();
                m_closeButtonRect = new Rect(m_windowRect.width - 30, 2, 26, 16);
            }
            m_windowRect = GUI.Window(0, m_windowRect, DrawConsoleWindow, "Lua Console");
        }

        protected Rect DefineWindowRect()
        {
            float width = Mathf.Max(minSize.x, Screen.width / 4f);
            float height = Mathf.Max(minSize.y, Screen.height / 4f);
            return new Rect(Screen.width - width, 0, width, height);
        }

        protected virtual void DrawConsoleWindow(int id)
        {
            if (IsKeyEvent(KeyCode.Return))
            {
                RunLuaCommand();
            }
            else if (IsKeyEvent(KeyCode.UpArrow))
            {
                UseHistory(-1);
            }
            else if (IsKeyEvent(KeyCode.DownArrow))
            {
                UseHistory(1);
            }
            else if (IsKeyEvent(KeyCode.Escape) || GUI.Button(m_closeButtonRect, "X"))
            {
                SetVisible(false);
                return;
            }
            GUI.SetNextControlName("Input");
            GUI.FocusControl("Input");
            if (string.Equals(m_input, "\n")) m_input = string.Empty;
            m_input = GUILayout.TextArea(m_input);
            m_scrollPosition = GUILayout.BeginScrollView(m_scrollPosition);
            GUILayout.Label(m_output);
            GUILayout.EndScrollView();
        }

        protected virtual bool IsKeyEvent(KeyCode keyCode)
        {
            if ((Event.current.type == EventType.KeyDown) && (Event.current.keyCode == keyCode))
            {
                Event.current.Use();
                return true;
            }
            else
            {
                return false;
            }
        }

        protected virtual void RunLuaCommand()
        {
            if (string.IsNullOrEmpty(m_input)) return;
            try
            {
                Lua.Result result = Lua.Run(m_input, DialogueDebug.logInfo);
                m_output = "Output: " + GetLuaResultString(result);
            }
            catch (Exception e)
            {
                m_output = "Output: [Exception] " + e.Message;
            }
            m_history.Add(m_input);
            if (m_history.Count >= maxHistory) m_history.RemoveAt(0);
            m_historyPosition = m_history.Count;
            m_input = string.Empty;
        }

        protected virtual string GetLuaResultString(Lua.Result result)
        {
            if (!result.hasReturnValue) return "(no return value)";
            return result.isTable ? FormatTableResult(result) : result.asString;
        }

        protected virtual string FormatTableResult(Lua.Result result)
        {
            if (!result.isTable) return result.asString;
            LuaTableWrapper table = result.asTable;
            StringBuilder sb = new StringBuilder();
            sb.Append("Table:\n");
            foreach (object key in table.keys)
            {
                sb.Append(string.Format("[{0}]: {1}\n", new System.Object[] { key.ToString(), table[key.ToString()].ToString() }));
            }
            return sb.ToString();
        }

        protected virtual void UseHistory(int direction)
        {
            m_historyPosition = Mathf.Clamp(m_historyPosition + direction, 0, m_history.Count);
            m_input = ((m_history.Count > 0) && (m_historyPosition < m_history.Count)) ? m_history[m_historyPosition] : string.Empty;
        }

    }

}
