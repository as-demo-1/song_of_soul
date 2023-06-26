/// ---------------------------------------------
/// Opsive Shared
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.Shared.Editor.UIElements.Controls.Types
{
    using Opsive.Shared.UI;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using UnityEditor;
    using UnityEditor.UIElements;
    using UnityEngine;
    using UnityEngine.UIElements;
    
    using EditorUtility = Opsive.Shared.Editor.Utility.EditorUtility;
    using Object = UnityEngine.Object;

    /// <summary>
    /// Implements TypeControlBase for the UnityEngine.Object ControlType.
    /// </summary>
    [ControlType(typeof(Text))]
    public class TextControl : TypeControlBase
    {
        protected bool m_UsingTMP;
        
        /// <summary>
        /// Does the control use a label?
        /// </summary>
        public override bool UseLabel { get { return true; } }

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
        /// <param name="userdata">Optional data which can be used by the controls.</param>
        /// <returns>The created control.</returns>
        public override VisualElement GetControl(UnityEngine.Object unityObject, object target, System.Reflection.FieldInfo field, int arrayIndex, System.Type type, object value, System.Func<object, bool> onChangeEvent, object userData)
        {
            Text sharedText = (Text)value;
#if TEXTMESH_PRO_PRESENT
            m_UsingTMP = SharedTextDropdown.IsDefaultUseTMP(sharedText);
#else
            m_UsingTMP = false;
#endif
            
            var objectField = new ObjectField();
            objectField.style.flexShrink = objectField.style.flexGrow = 1;
            objectField.style.flexDirection = FlexDirection.Row;
            
            if (m_UsingTMP) {
#if TEXTMESH_PRO_PRESENT
                objectField.objectType = typeof(TMPro.TMP_Text);
                objectField.SetValueWithoutNotify(sharedText.TextMeshProText);
#endif
            } else {
                objectField.objectType = typeof(UnityEngine.UI.Text);
                objectField.SetValueWithoutNotify(sharedText.UnityText);
            }
            
            // Ensure the control is kept up to date as the value changes.
            if (field != null) {
                System.Action<object> onBindingUpdateEvent = (object newValue) =>
                {
                    var newSharedText = (Text)newValue;
                    if (m_UsingTMP) {
#if TEXTMESH_PRO_PRESENT
                        objectField.SetValueWithoutNotify(newSharedText.TextMeshProText);
#endif
                    } else {
                        objectField.SetValueWithoutNotify(newSharedText.UnityText);
                    }
                };
                objectField.RegisterCallback<AttachToPanelEvent>(c =>
                {
                    BindingUpdater.AddBinding(field, arrayIndex, target, onBindingUpdateEvent);
                });
                objectField.RegisterCallback<DetachFromPanelEvent>(c =>
                {
                    BindingUpdater.RemoveBinding(onBindingUpdateEvent);
                });
            }
            objectField.RegisterValueChangedCallback(c =>
            {
                if (!onChangeEvent(c.newValue)) {
                    objectField.SetValueWithoutNotify(c.previousValue);
                }
                c.StopPropagation();
            });
            
#if TEXTMESH_PRO_PRESENT
            var dropdown = new SharedTextDropdown(sharedText);
            dropdown.OnUseTMPChange += useTMP =>
            {
                objectField.objectType = useTMP ? typeof(TMPro.TMP_Text) : typeof(UnityEngine.UI.Text);
                objectField.SetValueWithoutNotify(useTMP ? (Object)sharedText.TextMeshProText : (Object)sharedText.UnityText);
            };
            
            objectField.Insert(0,dropdown);
#endif
            
            
            return objectField;
        }
    }

#if TEXTMESH_PRO_PRESENT
    public class SharedTextDropdown : VisualElement
    {
        public event Action<bool> OnUseTMPChange;
        
        private static string c_Opsive_TextControl_UsingTMP_EditorPrefKey = "Opsive_TextControl_UsingTMP_EditorPrefKey";

        public static bool Opsive_TextControl_UsingTMP_EditorPrefKey
        {
            get => EditorPrefs.GetBool(c_Opsive_TextControl_UsingTMP_EditorPrefKey, true);
            set => EditorPrefs.SetBool(c_Opsive_TextControl_UsingTMP_EditorPrefKey, value);
        }
        
        protected bool m_UsingTMP;

        public static bool IsDefaultUseTMP(Text sharedText)
        {
            return sharedText.TextMeshProText != null ||
                (sharedText.UnityText == null && Opsive_TextControl_UsingTMP_EditorPrefKey);
        }
        
        public SharedTextDropdown(Text sharedText)
        {
            m_UsingTMP = IsDefaultUseTMP(sharedText);
            
            var dropDownList = new List<string>() { "TMP", "Text" };
            var defaultIndex = m_UsingTMP ? 0 : 1;
            var dropdown = new PopupField<string>(dropDownList,defaultIndex, (formatValue) =>
            {
                return formatValue;
            },(formatValue) =>
            {
                return formatValue;
            });
            dropdown.RegisterValueChangedCallback(evt =>
            {
                var useTMP = dropDownList.IndexOf(evt.newValue) == 0;
                m_UsingTMP = useTMP;
                Opsive_TextControl_UsingTMP_EditorPrefKey = useTMP;
                OnUseTMPChange?.Invoke(useTMP);
            });
            
            Add(dropdown);
        }
    }
#endif

    [CustomPropertyDrawer(typeof(Text))]
    public class TextDrawer : PropertyDrawer
    {
        public static object GetTargetObjectOfProperty(SerializedProperty prop)
        {
            if (prop == null) return null;

            var path = prop.propertyPath.Replace(".Array.data[", "[");
            object obj = prop.serializedObject.targetObject;
            var elements = path.Split('.');
            foreach (var element in elements)
            {
                if (element.Contains("["))
                {
                    var elementName = element.Substring(0, element.IndexOf("["));
                    var index = System.Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                    obj = GetValue_Imp(obj, elementName, index);
                }
                else
                {
                    obj = GetValue_Imp(obj, element);
                }
            }
            return obj;
        }
        
        private static object GetValue_Imp(object source, string name, int index)
        {
            var enumerable = GetValue_Imp(source, name) as System.Collections.IEnumerable;
            if (enumerable == null) return null;
            var enm = enumerable.GetEnumerator();
            //while (index-- >= 0)
            //    enm.MoveNext();
            //return enm.Current;

            for (int i = 0; i <= index; i++)
            {
                if (!enm.MoveNext()) return null;
            }
            return enm.Current;
        }
        
        private static object GetValue_Imp(object source, string name)
        {
            if (source == null)
                return null;
            var type = source.GetType();

            while (type != null)
            {
                var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (f != null)
                    return f.GetValue(source);

                var p = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (p != null)
                    return p.GetValue(source, null);

                type = type.BaseType;
            }
            return null;
        }

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            // Create property container element
            var objectField = new ObjectField();
            objectField.style.flexShrink = objectField.style.flexGrow = 1;
            objectField.style.flexDirection = FlexDirection.Row;
            
            var labelControl = new LabelControl(property.name, property.tooltip, objectField);
            Text sharedText = (Text)GetTargetObjectOfProperty(property);

            labelControl.Add(objectField);

            var usingTMP = false;
#if TEXTMESH_PRO_PRESENT
            usingTMP = SharedTextDropdown.IsDefaultUseTMP(sharedText);
#endif

            if (usingTMP) {
#if TEXTMESH_PRO_PRESENT
                objectField.objectType = typeof(TMPro.TMP_Text);
                objectField.SetValueWithoutNotify(sharedText.TextMeshProText);
#endif
            } else {
                objectField.objectType = typeof(UnityEngine.UI.Text);
                objectField.SetValueWithoutNotify(sharedText.UnityText);
            }
            
            objectField.RegisterValueChangedCallback(c =>
            {
                if (usingTMP) {
                    //sharedText.TextMeshProText = c.newValue as TMPro.TMP_Text;
                    property.FindPropertyRelative("m_TextMeshProText").objectReferenceValue = c.newValue;
                } else {
                    //sharedText.UnityText = c.newValue as UnityEngine.UI.Text;
                    property.FindPropertyRelative("m_UnityText").objectReferenceValue = c.newValue;
                }
                
                property.serializedObject.ApplyModifiedProperties();
                
            });
            
#if TEXTMESH_PRO_PRESENT
            var dropdown = new SharedTextDropdown(sharedText);
            dropdown.OnUseTMPChange += useTMP =>
            {
                objectField.objectType = useTMP ? typeof(TMPro.TMP_Text) : typeof(UnityEngine.UI.Text);
                objectField.SetValueWithoutNotify(useTMP ? (Object)sharedText.TextMeshProText : (Object)sharedText.UnityText);
            };
            
            objectField.Insert(0,dropdown);
#endif

            // Add fields to the container.
            return labelControl;
        }

        protected int m_PreviousDropdownIndex = -1;
        
        // Draw the property inside the given rect
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property.
            EditorGUI.BeginProperty(position, label, property);

            // Draw label
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            // Don't make child fields be indented
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            
#if !TEXTMESH_PRO_PRESENT
            EditorGUI.PropertyField(position, property.FindPropertyRelative("m_UnityText"), GUIContent.none);
#else
            // Calculate rects
            var dropdownRect = new Rect(position.x, position.y, 60, position.height);
            var objectRect = new Rect(position.x + 60, position.y, position.width - 60, position.height);

            Text sharedText = (Text)GetTargetObjectOfProperty(property);
            
            var usingTMP = SharedTextDropdown.IsDefaultUseTMP(sharedText);

            // Draw fields - passs GUIContent.none to each so they are drawn without labels
            var selectedIndex = m_PreviousDropdownIndex == -1 ? (usingTMP ? 0 : 1) : m_PreviousDropdownIndex;
            var value = EditorGUI.Popup(dropdownRect, selectedIndex, new string[] { "TMP", "Text" });

            if (value == 0 && !usingTMP) {
                usingTMP = true;
                SharedTextDropdown.Opsive_TextControl_UsingTMP_EditorPrefKey = usingTMP;
            }else if (value == 1 && usingTMP) {
                usingTMP = false;
                SharedTextDropdown.Opsive_TextControl_UsingTMP_EditorPrefKey = usingTMP;
            }

            m_PreviousDropdownIndex = value;
            
            if (usingTMP) {
                EditorGUI.PropertyField(objectRect, property.FindPropertyRelative("m_TextMeshProText"), GUIContent.none);
            } else {
                EditorGUI.PropertyField(objectRect, property.FindPropertyRelative("m_UnityText"), GUIContent.none);
            }
#endif

            // Set indent back to what it was
            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
        }
    }
}