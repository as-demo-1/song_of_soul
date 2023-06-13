using UnityEngine;
using System.Collections;
using UnityEditor;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// The abstract base custom field type.
    /// </summary>
    public abstract class CustomFieldType
    {

        public virtual FieldType storeFieldAsType
        {
            get
            {
                return FieldType.Text;
            }
        }

        public abstract string Draw(string currentValue, DialogueDatabase database);

        public virtual string Draw(GUIContent label, string currentValue, DialogueDatabase database)
        {
            return Draw(currentValue, database);
        }

        public virtual string Draw(Rect rect, string currentValue, DialogueDatabase database)
        {
            Debug.LogWarning("Dialogue System: API change notice: Since version 1.6.8, CustomFieldType has added an overload to Draw() that uses a Rect. Please add an implementation of this overload to " + GetType().Name + ".");
            return EditorGUI.TextField(rect, currentValue);
        }

        protected void RequireDialogueEditorWindow(DialogueDatabase database)
        {
            if (PixelCrushers.DialogueSystem.DialogueEditor.DialogueEditorWindow.instance == null)
            {
                var win = PixelCrushers.DialogueSystem.DialogueEditor.DialogueEditorWindow.OpenDialogueEditorWindow();
                win.SelectObject(database);
            }

        }
    }

}
