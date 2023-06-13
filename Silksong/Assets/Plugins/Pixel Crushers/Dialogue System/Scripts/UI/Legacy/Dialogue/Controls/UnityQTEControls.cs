using UnityEngine;
using System.Collections;

namespace PixelCrushers.DialogueSystem.UnityGUI
{

    /// <summary>
    /// Unity QTE indicator controls.
    /// </summary>
    [System.Serializable]
    public class UnityQTEControls : AbstractUIQTEControls
    {

        /// <summary>
        /// The QTE (Quick Time Event) indicators.
        /// </summary>
        public GUIControl[] qteIndicators;

        private int numVisibleQTEIndicators = 0;

        public UnityQTEControls(GUIControl[] qteIndicators)
        {
            this.qteIndicators = qteIndicators;
        }

        /// <summary>
        /// Gets a value indicating whether any QTE indicators are visible.
        /// </summary>
        /// <value>
        /// <c>true</c> if visible; otherwise, <c>false</c>.
        /// </value>
        public override bool areVisible
        {
            get { return (numVisibleQTEIndicators > 0); }
        }

        /// <summary>
        /// Sets the QTE controls active/inactive.
        /// </summary>
        /// <param name='value'>
        /// <c>true</c> for active; <c>false</c> for inactive.
        /// </param>
        public override void SetActive(bool value)
        {
            if (value == false)
            {
                numVisibleQTEIndicators = 0;
                foreach (var qteIndicator in qteIndicators)
                {
                    qteIndicator.gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Shows the QTE indicator specified by the index. 
        /// </summary>
        /// <param name='index'>
        /// Zero-based index of the indicator.
        /// </param>
        public override void ShowIndicator(int index)
        {
            if (IsValidQTEIndex(index) && !IsQTEIndicatorVisible(index))
            {
                qteIndicators[index].gameObject.SetActive(true);
                numVisibleQTEIndicators++;
            }
        }

        /// <summary>
        /// Hides the QTE indicator specified by the index.
        /// </summary>
        /// <param name='index'>
        /// Zero-based index of the indicator.
        /// </param>
        public override void HideIndicator(int index)
        {
            if (IsValidQTEIndex(index) && IsQTEIndicatorVisible(index))
            {
                qteIndicators[index].gameObject.SetActive(false);
                numVisibleQTEIndicators--;
            }
        }

        private bool IsQTEIndicatorVisible(int index)
        {
            return IsValidQTEIndex(index) ? qteIndicators[index].gameObject.activeSelf : false;
        }

        private bool IsValidQTEIndex(int index)
        {
            return (0 <= index) && (index < qteIndicators.Length);
        }

    }

}
