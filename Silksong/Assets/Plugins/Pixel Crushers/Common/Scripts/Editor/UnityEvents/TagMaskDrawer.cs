// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;
using System;

namespace PixelCrushers
{

    [CustomPropertyDrawer(typeof(TagMask), true)]
    public class TagMaskDrawer : PropertyDrawer
    {

        public struct MenuItemTagInfo
        {
            public SerializedProperty property;
            public int allTagsIndex;
            public MenuItemTagInfo(SerializedProperty property, int allTagsIndex)
            {
                this.property = property;
                this.allTagsIndex = allTagsIndex;
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            try
            {
                // Count how many tags are selected in the m_tag property:
                var allTags = UnityEditorInternal.InternalEditorUtility.tags;
                var tagsProperty = property.FindPropertyRelative("m_tags");
                var lastSelectedTag = string.Empty;
                int numSelected = 0;
                for (int i = 0; i < allTags.Length; i++)
                {
                    for (int j = 0; j < tagsProperty.arraySize; j++)
                    {
                        var selectedTag = tagsProperty.GetArrayElementAtIndex(j).stringValue;
                        if (string.Equals(selectedTag, allTags[i]))
                        {
                            lastSelectedTag = selectedTag;
                            numSelected++;
                            break;
                        }
                    }
                }

                // Show the dropdown button:
                var dropdownButtonSummary = (numSelected == 0) ? "Nothing"
                    : (numSelected == allTags.Length) ? "Everything"
                    : (numSelected > 1) ? "Mixed"
                    : lastSelectedTag;
#if UNITY_5_3 || UNITY_5_4 || UNITY_5_5
                if (GUI.Button(position, dropdownButtonSummary))
#else
                if (EditorGUI.DropdownButton(position, new GUIContent(dropdownButtonSummary), FocusType.Keyboard))
#endif
                {
                    // If dropdown button is clicked, show a context menu that looks like EditorGUI.MaskField:
                    var menu = new GenericMenu();
                    menu.AddItem(new GUIContent("Nothing"), false, OnSelectNothing, property);
                    menu.AddItem(new GUIContent("Everything"), false, OnSelectEverything, property);
                    for (int i = 0; i < allTags.Length; i++)
                    {
                        var isSelected = false;
                        for (int j = 0; j < tagsProperty.arraySize; j++)
                        {
                            var selectedTag = tagsProperty.GetArrayElementAtIndex(j).stringValue;
                            if (string.Equals(selectedTag, allTags[i]))
                            {
                                isSelected = true;
                                break;
                            }
                        }
                        menu.AddItem(new GUIContent(allTags[i]), isSelected, OnSelectTag, new MenuItemTagInfo(property, i));
                    }
                    menu.ShowAsContext();
                }
            }
            finally
            {
                EditorGUI.EndProperty();
            }
        }

        private void OnSelectNothing(object data)
        {
            var property = (SerializedProperty)data;
            var tagsProperty = property.FindPropertyRelative("m_tags");
            property.serializedObject.Update();
            tagsProperty.ClearArray();
            property.serializedObject.ApplyModifiedProperties();
        }

        private void OnSelectEverything(object data)
        {
            var property = (SerializedProperty)data;
            var tagsProperty = property.FindPropertyRelative("m_tags");
            property.serializedObject.Update();
            tagsProperty.ClearArray();
            var allTags = UnityEditorInternal.InternalEditorUtility.tags;
            tagsProperty.arraySize = allTags.Length;
            for (int i = 0; i < allTags.Length; i++)
            {
                tagsProperty.GetArrayElementAtIndex(i).stringValue = allTags[i];
            }
            property.serializedObject.ApplyModifiedProperties();
        }

        private void OnSelectTag(object data)
        {
            var menuItemInfo = (MenuItemTagInfo)data;
            var selectedIndex = menuItemInfo.allTagsIndex;
            var allTags = UnityEditorInternal.InternalEditorUtility.tags;
            if (0 <= selectedIndex && selectedIndex < allTags.Length)
            {
                var selectedTag = allTags[selectedIndex];
                var tagsProperty = menuItemInfo.property.FindPropertyRelative("m_tags");
                int indexInTagsProperty = -1;
                for (int i = 0; i < tagsProperty.arraySize; i++)
                {
                    if (string.Equals(tagsProperty.GetArrayElementAtIndex(i).stringValue, selectedTag))
                    {
                        indexInTagsProperty = i;
                        break;
                    }
                }
                tagsProperty.serializedObject.Update();
                if (indexInTagsProperty == -1)
                {
                    // Add:
                    tagsProperty.arraySize++;
                    tagsProperty.GetArrayElementAtIndex(tagsProperty.arraySize - 1).stringValue = selectedTag;
                }
                else
                {
                    // Delete:
                    tagsProperty.DeleteArrayElementAtIndex(indexInTagsProperty);
                }
                tagsProperty.serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
