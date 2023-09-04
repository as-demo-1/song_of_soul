// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using System;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// Quick Time Event (QTE) indicator controls for StandardDialogueUI.
    /// </summary>
    [Serializable]
    public class StandardUIQTEControls : AbstractUIQTEControls
    {

        [Tooltip("(Optional) Quick Time Event (QTE) indicators. Typically graphics such as images or sprites.")]

        public GameObject[] QTEIndicators;

        private int m_numVisibleQTEIndicators = 0;

        /// <summary>
        /// Are any QTE indicators visible?
        /// </summary>
        public override bool areVisible
        {
            get { return (m_numVisibleQTEIndicators > 0); }
        }

        /// <summary>
        /// Sets the QTE controls active/inactive.
        /// </summary>
        public override void SetActive(bool value)
        {
            if (value == false) HideImmediate();
        }

        public void HideImmediate()
        {
            m_numVisibleQTEIndicators = 0;
            foreach (var qteIndicator in QTEIndicators)
            {
                Tools.SetGameObjectActive(qteIndicator, false);
            }
        }

        /// <summary>
        /// Shows the QTE indicator specified by the index. 
        /// </summary>
        /// <param name='index'>Zero-based index of the indicator.</param>
        public override void ShowIndicator(int index)
        {
            if (!IsQTEIndicatorVisible(index))
            {
                Tools.SetGameObjectActive(QTEIndicators[index], true);
                m_numVisibleQTEIndicators++;
            }
        }

        /// <summary>
        /// Hides the QTE indicator specified by the index.
        /// </summary>
        /// <param name='index'>Zero-based index of the indicator.</param>
        public override void HideIndicator(int index)
        {
            if (IsValidQTEIndex(index) && IsQTEIndicatorVisible(index))
            {
                Tools.SetGameObjectActive(QTEIndicators[index], false);
                m_numVisibleQTEIndicators--;
            }
        }

        private bool IsQTEIndicatorVisible(int index)
        {
            return IsValidQTEIndex(index) ? QTEIndicators[index].gameObject.activeSelf : false;
        }

        private bool IsValidQTEIndex(int index)
        {
            return (0 <= index) && (index < QTEIndicators.Length);
        }

    }

}
