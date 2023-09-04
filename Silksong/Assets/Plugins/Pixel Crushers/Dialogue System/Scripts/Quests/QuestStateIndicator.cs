// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// Associates GameObjects (e.g., world space canvas elements) with indicator levels. A typical use is to
    /// associate indicator level 0 = nothing (unassigned), level 1 = question mark, and level 2 = exclamation mark.
    /// Other scripts such as QuestStateListener can specify their indicator level. This script then shows the
    /// GameObject of the highest indicator level that's in use.
    /// </summary>
    [AddComponentMenu("")] // Use wrapper.
    public class QuestStateIndicator : MonoBehaviour
    {

        [Tooltip("GameObject such as a world space canvas element associated with each indicator level. A typical use is to associate indicator level 0 = nothing (unassigned), level 1 = question mark, and level 2 = exclamation mark.")]
        public GameObject[] indicators = new GameObject[0];

        private List<List<QuestStateListener>> m_currentIndicatorCount = new List<List<QuestStateListener>>();

        void Awake()
        {
            InitializeCurrentIndicatorCount();
        }

        void Start()
        {
            UpdateIndicator();
        }

        private void InitializeCurrentIndicatorCount()
        {
            m_currentIndicatorCount.Clear();
            for (int i = 0; i < indicators.Length; i++)
            {
                m_currentIndicatorCount.Add(new List<QuestStateListener>());
            }
        }

        public void SetIndicatorLevel(QuestStateListener listener, int indicatorLevel)
        {
            if (DialogueDebug.logInfo) Debug.Log("Dialogue System: " + name + ": SetIndicatorLevel(" + listener + ", " + indicatorLevel + ")", listener);
            // Remove listener from whatever indicator level it's currently in:
            for (int i = 0; i < indicators.Length; i++)
            {
                if (m_currentIndicatorCount[i].Contains(listener))
                {
                    m_currentIndicatorCount[i].Remove(listener);
                    break;
                }
            }
            // Add to new indicator level:
            if (0 <= indicatorLevel && indicatorLevel < indicators.Length)
            {
                m_currentIndicatorCount[indicatorLevel].Add(listener);
            }
            UpdateIndicator();  
        }

        public void UpdateIndicator()
        {
            // Hide all indicators:
            for (int i = 0; i < indicators.Length; i++)
            {
                if (indicators[i] != null)
                {
                    indicators[i].SetActive(false);
                }
            }
            // Then activate the highest priority indicator:
            for (int i = indicators.Length - 1; i >= 0; i--)
            {
                if (m_currentIndicatorCount[i].Count > 0)
                {
                    if (indicators[i] != null)
                    {
                        indicators[i].SetActive(true);
                        if (DialogueDebug.logInfo) Debug.Log("Dialogue System: " + name + ": Activating GameObject associated with indicator level " + i, this);
                    }
                    break;
                }
            }
        }
    }
}