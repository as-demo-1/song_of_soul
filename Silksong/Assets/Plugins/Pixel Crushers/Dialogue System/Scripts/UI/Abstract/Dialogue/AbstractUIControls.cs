// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// Base class for abstract UI controls.
    /// </summary>
    [System.Serializable]
    public abstract class AbstractUIControls
    {

        /// <summary>
        /// Sets the controls active/inactive.
        /// </summary>
        /// <param name='value'>
        /// <c>true</c> for active, <c>false</c> for inactive.
        /// </param>
        public abstract void SetActive(bool value);

        /// <summary>
        /// Shows the controls by setting them active.
        /// </summary>
        public void Show()
        {
            SetActive(true);
        }

        /// <summary>
        /// Hides the controls by setting them inactive.
        /// </summary>
        public void Hide()
        {
            SetActive(false);
        }

    }

}
