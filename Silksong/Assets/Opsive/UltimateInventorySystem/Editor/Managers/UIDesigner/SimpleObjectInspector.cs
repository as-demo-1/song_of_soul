namespace Opsive.UltimateInventorySystem.Editor.Managers.UIDesigner
{
    using System.Collections.Generic;
    using Opsive.UltimateInventorySystem.Editor.Inspectors;
    using UnityEngine.UIElements;

    /// <summary>
    /// A basic inspector.
    /// </summary>
    public class SimpleObjectInspector : InspectorBase
    {
        protected override List<string> ExcludedFields => new List<string>() { };

        /// <summary>
        /// Initialize the inspector when it is first selected.
        /// </summary>
        protected override void InitializeInspector()
        {
            base.InitializeInspector();
        }

        protected override void ShowFooterElements(VisualElement container)
        { }
    }
}