/* [REMOVE THIS LINE]
 * [REMOVE THIS LINE] This is an example custom field type. To use it, remove all lines that
 * [REMOVE THIS LINE] contain the text "[REMOVE THIS LINE]".
 * [REMOVE THIS LINE]



using UnityEngine;
using UnityEditor;
using System;
using System.Collections;

namespace PixelCrushers.DialogueSystem
{

    public enum SceneType
    {
        Menu,
        WorldMap,
        Region,
        Battle
    }

    /// <summary>
    /// This example class adds a custom field type named "Scene" that lets you choose
    /// from a list of scene types (Menu, WorldMap, etc.).
    /// </summary>
    [CustomFieldTypeService.Name("Scene Type")]
    public class CustomFieldType_SceneType : CustomFieldType
    {

        public override string Draw(string currentValue, DialogueDatabase dataBase)
        {
            var enumValue = GetCurrentSceneType(currentValue);
            return EditorGUILayout.EnumPopup(enumValue).ToString();
        }

        public override string Draw(Rect rect, string currentValue, DialogueDatabase dataBase)
        {
            var enumValue = GetCurrentSceneType(currentValue);
            return EditorGUI.EnumPopup(rect, enumValue).ToString();
        }

        private SceneType GetCurrentSceneType(string currentValue)
        {
            if (string.IsNullOrEmpty(currentValue)) currentValue = SceneType.Menu.ToString();
            try
            {
                return (SceneType)Enum.Parse(typeof(SceneType), currentValue, true);
            }
            catch (Exception)
            {
                return SceneType.Menu;
            }
        }

    }
}



/**/
