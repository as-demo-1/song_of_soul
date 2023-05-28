// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// Custom property drawer for bit mask enums.
    /// </summary>
    [CustomPropertyDrawer(typeof(BitMaskAttribute))]
    public class BitMaskDrawer : PropertyDrawer
    {

        public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
        {
            BitMaskAttribute bitMaskAttribute = attribute as BitMaskAttribute;
            string[] names = System.Enum.GetNames(bitMaskAttribute.propType);
            prop.intValue = EditorGUI.MaskField(position, label, prop.intValue, names);
        }

    }

}
