/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.Styles
{
    using Opsive.Shared.Editor.Utility;
    using UnityEngine.UIElements;

    /// <summary>
    /// A static class that compiles the control type styles.
    /// </summary>
    public static class ControlTypeStyles
    {
        public static StyleSheet StyleSheet =>
            EditorUtility.LoadAsset<StyleSheet>("c9bc54c2b06fafa4ca0b0b8f3f4d6f66");

        public static readonly string s_ObjectControlView = "object-control-view";
        public static readonly string s_ObjectAmountView = "object-amount-view";
        public static readonly string s_AddRemoveField = "add-remove-field";
        public static readonly string s_AddRemoveFieldButton = "add-remove-field-button";
        public static readonly string s_BooleanGridAnchor = "boolean-grid-anchor";



    }
}
