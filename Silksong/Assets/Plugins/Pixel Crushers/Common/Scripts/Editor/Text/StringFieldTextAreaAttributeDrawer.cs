// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;

namespace PixelCrushers
{

    [CustomPropertyDrawer(typeof(StringFieldTextAreaAttribute))]
    public class StringFieldTextAreaAttributeDrawer : PropertyDrawer
    {

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var stringFieldSizeAttribute = attribute as StringFieldTextAreaAttribute;
            if (stringFieldSizeAttribute == null) return base.GetPropertyHeight(property, label);
            var expandHeight = stringFieldSizeAttribute.expandHeight;

            var textProperty = property.FindPropertyRelative("m_text");
            var stringAssetProperty = property.FindPropertyRelative("m_stringAsset");
            var textTableProperty = property.FindPropertyRelative("m_textTable");
            if (textProperty == null || stringAssetProperty == null || textTableProperty == null) return base.GetPropertyHeight(property, label);
            var isTextAssigned = (textProperty != null && !string.IsNullOrEmpty(textProperty.stringValue));
            var isStringAssetAssigned = (stringAssetProperty != null &&  stringAssetProperty.objectReferenceValue != null);
            var isTextTableAssigned = (textTableProperty != null && textTableProperty.objectReferenceValue != null);
            var isContentAssigned = isTextAssigned || isStringAssetAssigned || isTextTableAssigned;

            var textLines = expandHeight ? StringFieldDrawer.NumExpandedLines : 1;
            if (isContentAssigned && !isTextAssigned) textLines = 0;
            var nonTextLines = isTextAssigned ? 0 : (isContentAssigned ? 1 : 2);
            return (textLines + nonTextLines) * EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var stringFieldSizeAttribute = attribute as StringFieldTextAreaAttribute;
            var expandHeight = (stringFieldSizeAttribute != null) ? stringFieldSizeAttribute.expandHeight : false;
            StringFieldDrawer.Draw(position, property, label, expandHeight);
        }

    }
}
