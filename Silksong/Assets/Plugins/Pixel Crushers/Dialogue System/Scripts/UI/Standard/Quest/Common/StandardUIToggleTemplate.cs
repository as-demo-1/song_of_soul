// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using System;

namespace PixelCrushers.DialogueSystem
{

    public delegate void ToggleChangedDelegate(bool value, object data);

    /// <summary>
    /// UI template for a toggle.
    /// </summary>
    [AddComponentMenu("")] // Use wrapper.
    public class StandardUIToggleTemplate : StandardUIContentTemplate
    {

        [Tooltip("Toggle UI element.")]
        public UnityEngine.UI.Toggle toggle;

        protected object m_data;

        public event ToggleChangedDelegate onToggleChanged = delegate { };

        public virtual void Awake()
        {
            if (toggle == null && DialogueDebug.logWarnings) Debug.LogWarning("Dialogue System: UI Toggle is unassigned.", this);
        }

        public virtual void Assign(bool isVisible, bool isOn, object data, ToggleChangedDelegate toggleDelegate)
        {
            m_data = data;
            if (toggle != null)
            {
                if (isVisible)
                {
                    toggle.isOn = isOn;
                    toggle.onValueChanged.AddListener(OnToggleChanged);
                    onToggleChanged += toggleDelegate;
                }
                else
                {
                    gameObject.SetActive(false);
                }
            }
        }

        protected virtual void OnToggleChanged(bool value)
        {
            try
            {
                onToggleChanged.Invoke(value, m_data);
            }
            catch (Exception e) // Don't let exceptions in user-added events break our code.
            {
                if (Debug.isDebugBuild) Debug.LogException(e);
            }
        }

    }
}
