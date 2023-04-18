/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.Inspectors
{
    using Opsive.Shared.Editor.UIElements;
    using Opsive.Shared.Editor.UIElements.Managers;
    using Opsive.UltimateInventorySystem.Editor.Styles;
    using UnityEditor;
    using UnityEngine.UIElements;

    /// <summary>
    /// The base inspector for components and scriptable object within Ultimate Inventory System.
    /// </summary>
    public abstract class InspectorBase : UIElementsInspector
    {
        protected override void AddStyleSheets(VisualElement container)
        {
            base.AddStyleSheets(container);
            container.styleSheets.Add(Shared.Editor.Utility.EditorUtility.LoadAsset<StyleSheet>("e70f56fae2d84394b861a2013cb384d0")); // Shared stylesheet.
            container.styleSheets.Add(CommonStyles.StyleSheet);
            container.styleSheets.Add(ManagerStyles.StyleSheet);
            container.styleSheets.Add(ControlTypeStyles.StyleSheet);
        }
    }
}