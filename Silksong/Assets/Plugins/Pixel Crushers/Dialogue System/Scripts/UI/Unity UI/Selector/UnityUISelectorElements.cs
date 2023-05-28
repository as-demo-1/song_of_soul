// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// Optional component to provide references to selector UI elements.
    /// </summary>
    [AddComponentMenu("")] // Use wrapper.
    public class UnityUISelectorElements : MonoBehaviour
    {

        /// <summary>
        /// The main graphic (optional). Assign this if you have created an entire 
        /// panel for the selector.
        /// </summary>
        public UnityEngine.UI.Graphic mainGraphic = null;

        /// <summary>
        /// The text for the name of the current selection.
        /// </summary>
        public UnityEngine.UI.Text nameText = null;

        /// <summary>
        /// The text for the use message (e.g., "Press spacebar to use").
        /// </summary>
        public UnityEngine.UI.Text useMessageText = null;

        public Color inRangeColor = Color.yellow;

        public Color outOfRangeColor = Color.gray;

        /// <summary>
        /// The graphic to show if the selection is in range.
        /// </summary>
        public UnityEngine.UI.Graphic reticleInRange = null;

        /// <summary>
        /// The graphic to show if the selection is out of range.
        /// </summary>
        public UnityEngine.UI.Graphic reticleOutOfRange = null;

        public UnityUISelectorDisplay.AnimationTransitions animationTransitions = new UnityUISelectorDisplay.AnimationTransitions();

        public static UnityUISelectorElements instance = null;

        private void Awake()
        {
            instance = this;
            Tools.DeprecationWarning(this);
        }
    }

}
