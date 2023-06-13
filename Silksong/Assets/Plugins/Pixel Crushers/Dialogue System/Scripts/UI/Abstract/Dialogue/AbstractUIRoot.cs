// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// Abstract UI root. Each GUI system implementation derives its own subclass from this.
    /// For example, the Dialogue System's Unity GUI system implements this using GUIRoot,
    /// whereas NGUI implements it using an NGUI UIRoot.
    /// </summary>
    [System.Serializable]
    public abstract class AbstractUIRoot
    {

        /// <summary>
        /// Shows the UI root.
        /// </summary>
        public abstract void Show();

        /// <summary>
        /// Hides the UI root.
        /// </summary>
        public abstract void Hide();

    }

}
