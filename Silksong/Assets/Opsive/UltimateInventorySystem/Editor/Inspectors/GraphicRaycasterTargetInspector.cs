/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.Inspectors
{
    using Opsive.UltimateInventorySystem.UI;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine.UIElements;

    /// <summary>
    /// Custom editor to hide the Material and Color fields of the Graphic component.
    /// </summary>
    [CustomEditor(typeof(GraphicRaycasterTarget), true)]
    public class GraphicRaycasterTargetInspector : InspectorBase
    {
        protected override List<string> ExcludedFields => new List<string>() { "m_Material", "m_Color" };

        protected override void ShowFooterElements(VisualElement container)
        { }
    }
}