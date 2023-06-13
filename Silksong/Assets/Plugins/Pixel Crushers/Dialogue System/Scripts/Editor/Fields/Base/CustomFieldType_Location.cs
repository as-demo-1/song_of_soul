using UnityEngine;
using System.Collections;
using UnityEditor;

namespace PixelCrushers.DialogueSystem
{

    [CustomFieldTypeService.Name("Location")]
    public class CustomFieldType_Location : CustomFieldType
    {
        public override FieldType storeFieldAsType
        {
            get
            {
                return FieldType.Location;
            }
        }
        public override string Draw(string currentValue, DialogueDatabase database)
        {
            RequireDialogueEditorWindow(database);
            return PixelCrushers.DialogueSystem.DialogueEditor.DialogueEditorWindow.instance.DrawAssetPopup<Location>(currentValue, (database != null) ? database.locations : null, null);
        }

        public override string Draw(Rect rect, string currentValue, DialogueDatabase database)
        {
            RequireDialogueEditorWindow(database);
            return PixelCrushers.DialogueSystem.DialogueEditor.DialogueEditorWindow.instance.DrawAssetPopup<Location>(rect, currentValue, (database != null) ? database.locations : null, null);
        }

    }
}