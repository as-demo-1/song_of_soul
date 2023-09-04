// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem.DialogueEditor
{

    /// <summary>
    /// This part of the Dialogue Editor window implements UpdateTemplateFromAssets().
    /// </summary>
    public partial class DialogueEditorWindow
    {

        private void ConfirmUpdateTemplateFromAssets()
        {
            if (EditorUtility.DisplayDialog("Update template from assets?", "This will update the template with any custom fields you've added to any assets in your dialogue database. You cannot undo this action.", "Update", "Cancel"))
            {
                UpdateTemplateFromAssets();
                Debug.Log(string.Format("{0}: Dialogue Editor template now contains all fields listed in any dialogue database asset.", DialogueDebug.Prefix));
            }
        }

        private void ConfirmApplyTemplateToAssets()
        {
            if (EditorUtility.DisplayDialog("Apply template to assets?", "This will apply the template to all assets. You cannot undo this action.", "Update", "Cancel"))
            {
                ApplyTemplateToAssets();
                Debug.Log(string.Format("{0}: All assets now have all fields listed in the template.", DialogueDebug.Prefix));
            }
        }

        private void ConfirmSyncAssetsAndTemplate()
        {
            if (EditorUtility.DisplayDialog("Sync template and assets?", "This will update the template with any custom fields you've added to assets in your dialogue database, and then apply the updated template to all assets. You cannot undo this action.", "Update", "Cancel"))
            {
                SyncAssetsAndTemplate();
                Debug.Log(string.Format("{0}: Dialogue Editor template now contains all fields listed in any dialogue database asset, and those assets now have all fields listed in the template.", DialogueDebug.Prefix));
            }
        }

        private void ConfirmRemoveEmptyFields()
        {
            if (EditorUtility.DisplayDialog("Remove empty fields?", "This will remove fields whose titles are blank from the template and all assets. You cannot undo this action.", "Remove", "Cancel"))
            {
                RemoveEmptyFields();
                Debug.Log(string.Format("{0}: Removed empty fields from the template and assets in the dialogue database.", DialogueDebug.Prefix));
            }
        }

        private void SyncAssetsAndTemplate()
        {
            NormalizeActors();
            NormalizeItems();
            NormalizeLocations();
            NormalizeVariables();
            NormalizeConversations();
            NormalizeDialogueEntries();
        }

        private void NormalizeActors()
        {
            NormalizeAssets<Actor>(database.actors, template.actorFields);
        }

        private void NormalizeItems()
        {
            //--- No. Keep quests and items separate:  AddMissingFieldsToTemplate(template.questFields, template.itemFields);
            NormalizeAssets<Item>(database.items, template.itemFields, true, true);
            NormalizeAssets<Item>(database.items, template.questFields, true, false);
        }

        private void NormalizeLocations()
        {
            NormalizeAssets<Location>(database.locations, template.locationFields);
        }

        private void NormalizeVariables()
        {
            NormalizeAssets<Variable>(database.variables, template.variableFields);
        }

        private void NormalizeConversations()
        {
            NormalizeAssets<Conversation>(database.conversations, template.conversationFields);
        }

        private void NormalizeDialogueEntries()
        {
            foreach (var conversation in database.conversations)
            {
                foreach (var entry in conversation.dialogueEntries)
                {
                    AddMissingFieldsToTemplate(entry.fields, template.dialogueEntryFields);
                }
            }
            foreach (var conversation in database.conversations)
            {
                foreach (var entry in conversation.dialogueEntries)
                {
                    EnforceTemplateOnFields(entry.fields, template.dialogueEntryFields);
                }
            }
        }

        private void NormalizeAssets<T>(List<T> assets, List<Field> templateFields, bool checkIsItem = false, bool requireIsItem = false) where T : Asset
        {
            foreach (var asset in assets)
            {
                AddMissingFieldsToTemplate(asset.fields, templateFields, checkIsItem, requireIsItem);
            }
            foreach (var asset in assets)
            {
                EnforceTemplateOnFields(asset.fields, templateFields, checkIsItem, requireIsItem);
            }
            SetDatabaseDirty("Normalize Assets to Template");
        }

        private void AddMissingFieldsToTemplate(List<Field> assetFields, List<Field> templateFields, bool checkIsItem = false, bool requireIsItem = false)
        {
            if (checkIsItem)
            {
                var isItemField = Field.Lookup(assetFields, DialogueSystemFields.IsItem);
                var isItem = isItemField != null && Tools.StringToBool(isItemField.value);
                if ((requireIsItem == true && !isItem) || (requireIsItem == false && isItem))
                {
                    return;
                }
            }
            foreach (var field in assetFields)
            {
                if (!Field.FieldExists(templateFields, field.title))
                {
                    templateFields.Add(new Field(field.title, string.Empty, field.type));
                }
            }
        }

        private void EnforceTemplateOnFields(List<Field> fields, List<Field> templateFields, bool checkIsItem = false, bool requireIsItem = false)
        {
            if (checkIsItem)
            {
                var isItemField = Field.Lookup(fields, DialogueSystemFields.IsItem);
                var isItem = isItemField != null && Tools.StringToBool(isItemField.value);
                if ((requireIsItem == true && !isItem) || (requireIsItem == false && isItem))
                {
                    return;
                }
            }
            List<Field> newFields = new List<Field>();
            for (int i = 0; i < templateFields.Count; i++)
            {
                Field templateField = templateFields[i];
                if (!string.IsNullOrEmpty(templateField.title))
                {
                    newFields.Add(Field.Lookup(fields, templateField.title) ?? new Field(templateField));
                }
            }
            fields.Clear();
            for (int i = 0; i < newFields.Count; i++)
            {
                fields.Add(newFields[i]);
            }
        }

        private void UpdateTemplateFromAssets()
        {
            UpdateTemplateFromAssets<Actor>(database.actors, template.actorFields);
            //--- No. Keep quests and items separate: AddMissingFieldsToTemplate(template.questFields, template.itemFields);
            UpdateTemplateFromAssets<Item>(database.items, template.itemFields, true, true);
            UpdateTemplateFromAssets<Item>(database.items, template.questFields, true, false);
            UpdateTemplateFromAssets<Location>(database.locations, template.locationFields);
            UpdateTemplateFromAssets<Variable>(database.variables, template.variableFields);
            UpdateTemplateFromAssets<Conversation>(database.conversations, template.conversationFields);
            foreach (var conversation in database.conversations)
            {
                foreach (var entry in conversation.dialogueEntries)
                {
                    AddMissingFieldsToTemplate(entry.fields, template.dialogueEntryFields);
                }
            }
            SaveTemplate();
        }

        private void UpdateTemplateFromAssets<T>(List<T> assets, List<Field> templateFields, bool checkIsItem = false, bool requireIsItem = false) where T : Asset
        {
            foreach (var asset in assets)
            {
                AddMissingFieldsToTemplate(asset.fields, templateFields, checkIsItem, requireIsItem);
            }
        }

        private void ApplyTemplateToAssets()
        {
            ApplyTemplateToAssets<Actor>(database.actors, template.actorFields);
            foreach (var item in database.items)
            {
                if (item.IsItem)
                {
                    ApplyTemplate(item.fields, template.itemFields);
                }
                else
                {
                    ApplyTemplate(item.fields, template.questFields);
                }
            }
            ApplyTemplateToAssets<Location>(database.locations, template.locationFields);
            ApplyTemplateToAssets<Variable>(database.variables, template.variableFields);
            ApplyTemplateToAssets<Conversation>(database.conversations, template.conversationFields);
            foreach (var conversation in database.conversations)
            {
                foreach (var entry in conversation.dialogueEntries)
                {
                    ApplyDialogueEntryTemplate(entry.fields);
                }
            }
        }

        private void ApplyTemplateToAssets<T>(List<T> assets, List<Field> templateFields) where T : Asset
        {
            foreach (var asset in assets)
            {
                ApplyTemplate(asset.fields, templateFields);
            }
        }

        private void RemoveEmptyFields()
        {
            RemoveEmptyFields(template.actorFields);
            RemoveEmptyFields(template.itemFields);
            RemoveEmptyFields(template.questFields);
            RemoveEmptyFields(template.locationFields);
            RemoveEmptyFields(template.variableFields);
            RemoveEmptyFields(template.conversationFields);
            RemoveEmptyFields(template.dialogueEntryFields);
            RemoveEmptyFieldsFromAssets<Actor>(database.actors);
            RemoveEmptyFieldsFromAssets<Item>(database.items);
            RemoveEmptyFieldsFromAssets<Location>(database.locations);
            RemoveEmptyFieldsFromAssets<Variable>(database.variables);
            RemoveEmptyFieldsFromAssets<Conversation>(database.conversations);
            foreach (var conversation in database.conversations)
            {
                foreach (var entry in conversation.dialogueEntries)
                {
                    RemoveEmptyFields(entry.fields);
                }
            }
        }

        private void RemoveEmptyFieldsFromAssets<T>(List<T> assets) where T : Asset
        {
            if (assets == null) return;
            foreach (var asset in assets)
            {
                RemoveEmptyFields(asset.fields);
            }
        }

        private void RemoveEmptyFields(List<Field> fields)
        {
            if (fields == null) return;
            fields.RemoveAll(x => string.IsNullOrEmpty(x.title));
        }

    }

}