/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.VisualElements.ControlTypes
{
    using Opsive.Shared.Editor.UIElements.Controls.Types;
    using Opsive.UltimateInventorySystem.Editor.Managers;
    using Opsive.UltimateInventorySystem.Editor.Styles;
    using System;
    using System.Reflection;
    using UnityEngine.UIElements;
    using Object = UnityEngine.Object;

    /// <summary>
    /// The base class for unity object controls a preview.
    /// </summary>
    public class UnityObjectControlWithPreview : UnityObjectControl
    {
        /// <summary>
        /// Returns the control that should be used for the specified ControlType.
        /// </summary>
        /// <param name="unityObject">A reference to the owning Unity Object.</param>
        /// <param name="target">The object that should have its fields displayed.</param>
        /// <param name="field">The field responsible for the control (can be null).</param>
        /// <param name="arrayIndex">The index of the object within the array (-1 indicates no array).</param>
        /// <param name="type">The type of control being retrieved.</param>
        /// <param name="value">The value of the control.</param>
        /// <param name="onChangeEvent">An event that is sent when the value changes. Returns false if the control cannot be changed.</param>
        /// <param name="userData">Optional data which can be used by the controls.</param>
        /// <returns>The created control.</returns>
        public override VisualElement GetControl(Object unityObject, object target, FieldInfo field, int arrayIndex,
            Type type, object value, System.Func<object, bool> onChangeEvent, object userData)
        {
            if (field != null) {
                return base.GetControl(unityObject, target, field, arrayIndex, type, value, onChangeEvent, userData);
            }

            var obj = (Object)value;
            var objectView = new VisualElement();
            var objectPreview = new VisualElement();

            if (obj == null) { objectView.Add(new Label("None")); } else {
                objectView.Add(new Label(obj.name));
                ManagerUtility.ObjectPreview(objectPreview, obj);
                objectView.Add(objectPreview);
                objectView.AddToClassList(ControlTypeStyles.s_ObjectControlView);
            }

            return objectView;
        }
    }
}