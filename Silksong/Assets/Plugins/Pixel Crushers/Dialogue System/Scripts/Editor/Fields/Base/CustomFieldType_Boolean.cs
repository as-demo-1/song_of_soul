using UnityEngine;
using System.Collections;
using UnityEditor;
using System;

namespace PixelCrushers.DialogueSystem
{

    [CustomFieldTypeService.Name("Boolean")]
    public class CustomFieldType_Boolean : CustomFieldType
    {
        public override FieldType storeFieldAsType
        {
            get
            {
                return FieldType.Boolean;
            }
        }

        private enum BooleanType { True, False }

        public override string Draw(string currentValue, DialogueDatabase dataBase)
        {
            return EditorGUILayout.EnumPopup(StringToBooleanType(currentValue)).ToString();
        }

        public override string Draw(Rect rect, string currentValue, DialogueDatabase dataBase)
        {
            return EditorGUI.EnumPopup(rect, StringToBooleanType(currentValue)).ToString();
        }

        private static BooleanType StringToBooleanType(string s)
        {
            return (string.Compare(s, "true", System.StringComparison.OrdinalIgnoreCase) == 0) ? BooleanType.True : BooleanType.False;
        }
    }
}