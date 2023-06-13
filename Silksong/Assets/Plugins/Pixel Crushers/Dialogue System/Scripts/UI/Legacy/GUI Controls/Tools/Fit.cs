using UnityEngine;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem.UnityGUI
{

    /// <summary>
    /// Specifies how to fit a control in with its siblings. To use the control's scaled rect 
    /// as-is, leave the properties below unassigned. If you want to fit the control exactly
    /// below another control, assign that control to the property 'below'. You can assign
    /// any combination of the four fit properties (e.g., below one control and to the right
    /// of another).
    /// </summary>
    [System.Serializable]
    public class Fit
    {

        /// <summary>
        /// Fit above the control assigned to this property.
        /// </summary>
        public GUIControl above;

        /// <summary>
        /// Fit below the control assigned to this property.
        /// </summary>
        public GUIControl below;

        /// <summary>
        /// Fit to the left of the control assigned to this property.
        /// </summary>
        public GUIControl leftOf;

        /// <summary>
        /// Fit to the right of the control assigned to this property.
        /// </summary>
        public GUIControl rightOf;

        /// <summary>
        /// Set <c>true</c> to expand the size of the control to fit; <c>false</c> to move the control.
        /// </summary>
        public bool expandToFit = true;

        /// <summary>
        /// Indicates whether any fit properties are specified.
        /// </summary>
        /// <value>
        /// <c>true</c> if any fit properties specified; otherwise, <c>false</c>.
        /// </value>
        public bool IsSpecified
        {
            get { return (above != null) || (below != null) || (leftOf != null) || (rightOf != null); }
        }

    }

}
