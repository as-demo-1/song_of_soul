/* [REMOVE THIS LINE]
 * [REMOVE THIS LINE] To use this template, make a copy and remove the lines that start
 * [REMOVE THIS LINE] with "[REMOVE THIS LINE]". Then add your code where the comments indicate.
 * [REMOVE THIS LINE]
 * [REMOVE THIS LINE] See CustomFieldType_SceneType.cs for an example.
 * [REMOVE THIS LINE]



using UnityEngine;
using UnityEditor;
using System;
using System.Collections;

namespace PixelCrushers.DialogueSystem
{

    // STEP 1: Name your type below, replacing "My Type":
    [CustomFieldTypeService.Name("My Type")]

    // STEP 2: Rename the class by changing TemplateType to your type name:
    public class CustomFieldType_TemplateType : CustomFieldType
    {

        // STEP 3: Replace the code in this Draw method with your own 
        // editor GUI code. If you leave it as-is, it will just draw
        // it as a plain text field. This is the GUILayout version.
        public override string Draw(string currentValue, DialogueDatabase database)
        {
            return EditorGUILayout.TextField(currentValue);
        }

        // STEP 4: Replace the code in this Draw method with your own 
        // editor GUI code. If you leave it as-is, it will just draw
        // it as a plain text field. This is the GUI version, which
        // uses an absolute Rect position instead of auto-layout.
        public override string Draw(Rect rect, string currentValue, DialogueDatabase database)
        {
            return EditorGUI.TextField(rect, currentValue);
        }
    }
}



/**/
