// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem.DialogueEditor
{

    /// <summary>
    /// This part of the Dialogue Editor window provides generic functionality for assets.
    /// Most of the tabs (Actor, Item, etc.) use the generic methods in this part as a base.
    /// </summary>
    public partial class DialogueEditorWindow
    {

        private List<Field> clipboardFields = null;

        private void DrawAssetSection<T>(string label, List<T> assets, AssetFoldouts foldouts, ref string filter) where T : Asset, new()
        {
            DrawAssetSection<T>(label, assets, foldouts, null, null, ref filter);
        }

        private void DrawAssetSection<T>(string label, List<T> assets, AssetFoldouts foldouts, Action menuDelegate, ref string filter) where T : Asset, new()
        {
            DrawAssetSection<T>(label, assets, foldouts, menuDelegate, null, ref filter);
        }

        private void DrawAssetSection<T>(string label, List<T> assets, AssetFoldouts foldouts, Action menuDelegate, Action syncDatabaseDelegate, ref string filter) where T : Asset, new()
        {
            DrawFilterMenuBar(label, menuDelegate, ref filter);
            if (syncDatabaseDelegate != null) syncDatabaseDelegate();
            DrawAssets<T>(label, assets, foldouts, filter);
        }

        private void DrawFilterMenuBar(string label, Action menuDelegate, ref string filter)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label + "s", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();

            filter = EditorGUILayout.TextField(GUIContent.none, filter, "ToolbarSeachTextField");
            GUILayout.Label(string.Empty, "ToolbarSeachCancelButtonEmpty");

            if (menuDelegate != null) menuDelegate();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawAssets<T>(string label, List<T> assets, AssetFoldouts foldouts, string filter) where T : Asset
        {
            EditorWindowTools.StartIndentedSection();
            showStateFieldAsQuest = false;
            DrawAssetSpecificFoldoutProperties<T>(assets);
            T assetToRemove = null;
            int indexToMoveUp = -1;
            int indexToMoveDown = -1;
            for (int index = 0; index < assets.Count; index++)
            {
                T asset = assets[index];
                if (!EditorTools.IsAssetInFilter(asset, filter)) continue;
                EditorGUILayout.BeginHorizontal();
                if (!foldouts.properties.ContainsKey(index)) foldouts.properties.Add(index, false);
                foldouts.properties[index] = EditorGUILayout.Foldout(foldouts.properties[index], EditorTools.GetAssetName(asset));
                EditorGUI.BeginDisabledGroup(index >= (assets.Count - 1));
                if (GUILayout.Button(new GUIContent("↓", "Move down"), GUILayout.Width(16))) indexToMoveDown = index;
                EditorGUI.EndDisabledGroup();
                EditorGUI.BeginDisabledGroup(index == 0);
                if (GUILayout.Button(new GUIContent("↑", "Move up"), GUILayout.Width(16))) indexToMoveUp = index;
                EditorGUI.EndDisabledGroup();
                if (GUILayout.Button(new GUIContent(" ", string.Format("Delete {0}.", EditorTools.GetAssetName(asset))), "OL Minus", GUILayout.Width(16))) assetToRemove = asset;
                EditorGUILayout.EndHorizontal();
                if (foldouts.properties[index]) DrawAsset<T>(asset, index, foldouts);
            }
            if (indexToMoveDown >= 0)
            {
                T asset = assets[indexToMoveDown];
                assets.RemoveAt(indexToMoveDown);
                assets.Insert(indexToMoveDown + 1, asset);
                SetDatabaseDirty("Move Down");
            }
            else if (indexToMoveUp >= 0)
            {
                T asset = assets[indexToMoveUp];
                assets.RemoveAt(indexToMoveUp);
                assets.Insert(indexToMoveUp - 1, asset);
                SetDatabaseDirty("Move Up");
            }
            else if (assetToRemove != null)
            {
                if (EditorUtility.DisplayDialog(string.Format("Delete '{0}'?", EditorTools.GetAssetName(assetToRemove)), "Are you sure you want to delete this?", "Delete", "Cancel"))
                {
                    assets.Remove(assetToRemove);
                    SetDatabaseDirty("Delete");
                }
            }
            EditorWindowTools.EndIndentedSection();
        }

        private string GetAssetName(Asset asset)
        {
            return (asset is Conversation) ? (asset as Conversation).Title : asset.Name;
        }

        private void DrawAssetSpecificFoldoutProperties<T>(List<T> assets)
        {
            //--- No longer used: if (typeof(T) == typeof(Item)) DrawItemSpecificFoldoutProperties();
        }

        private void DrawAsset<T>(T asset, int index, AssetFoldouts foldouts) where T : Asset
        {
            EditorWindowTools.StartIndentedSection();
            EditorGUILayout.BeginVertical("button");
            DrawAssetSpecificPropertiesFirstPart(asset);
            DrawFieldsFoldout<T>(asset, index, foldouts);
            DrawAssetSpecificPropertiesSecondPart(asset, index, foldouts);
            EditorGUILayout.EndVertical();
            EditorWindowTools.EndIndentedSection();
        }

        public void DrawAssetSpecificPropertiesFirstPart(Asset asset)
        {
            if (!(asset is Variable))
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.IntField(new GUIContent("ID", "Internal ID. Change at your own risk."), asset.id);
                EditorGUI.EndDisabledGroup();
            }
            EditorGUI.BeginChangeCheck();
            asset.Name = EditorGUILayout.TextField(new GUIContent("Name", "Name of this asset."), asset.Name);
            if (EditorGUI.EndChangeCheck()) SetDatabaseDirty("Name");
            if (asset is Actor) DrawActorPortrait(asset as Actor);
            if (asset is Item) DrawItemPropertiesFirstPart(asset as Item);
            if (customDrawAssetInspector != null)
            {
                customDrawAssetInspector(database, asset);
            }
        }

        public void DrawAssetSpecificPropertiesSecondPart(Asset asset, int index, AssetFoldouts foldouts)
        {
            if (asset is Item) DrawItemSpecificPropertiesSecondPart(asset as Item, index, foldouts);
        }

        public void DrawFieldsFoldout<T>(T asset, int index, AssetFoldouts foldouts) where T : Asset
        {
            EditorGUI.BeginChangeCheck();
            if (!foldouts.fields.ContainsKey(index)) foldouts.fields.Add(index, false);
            EditorGUILayout.BeginHorizontal();
            foldouts.fields[index] = EditorGUILayout.Foldout(foldouts.fields[index], "All Fields");
            if (foldouts.fields[index])
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(new GUIContent("Template", "Add any missing fields from the template."), EditorStyles.miniButton, GUILayout.Width(68)))
                {
                    ApplyTemplate(asset.fields, GetTemplateFields(asset));
                }
                if (GUILayout.Button(new GUIContent("Copy", "Copy these fields to the clipboard."), EditorStyles.miniButton, GUILayout.Width(60)))
                {
                    CopyFields(asset.fields);
                }
                EditorGUI.BeginDisabledGroup(clipboardFields == null);
                if (GUILayout.Button(new GUIContent("Paste", "Paste the clipboard into these fields."), EditorStyles.miniButton, GUILayout.Width(60)))
                {
                    PasteFields(asset.fields);
                }
                EditorGUI.EndDisabledGroup();
            }
            if (GUILayout.Button(new GUIContent(" ", "Add new field."), "OL Plus", GUILayout.Width(16)))
            {
                asset.fields.Add(new Field());
                foldouts.fields[index] = true;
                SetDatabaseDirty("Add Field");
            }
            EditorGUILayout.EndHorizontal();
            if (foldouts.fields[index]) DrawFieldsSection(asset.fields);
            if (EditorGUI.EndChangeCheck())
            {
                if (asset is Item) BuildLanguageListFromFields(asset.fields);
            }
        }

        private void CopyFields(List<Field> fields)
        {
            clipboardFields = fields;
        }

        private void PasteFields(List<Field> fields)
        {
            foreach (Field clipboardField in clipboardFields)
            {
                Field field = Field.Lookup(fields, clipboardField.title);
                if (field != null)
                {
                    field.value = clipboardField.value;
                    field.type = clipboardField.type;
                }
                else
                {
                    fields.Add(new Field(clipboardField));
                }
            }
            SetDatabaseDirty("Paste Fields");
        }

        private void ApplyTemplate(List<Field> fields, List<Field> templateFields)
        {
            if (fields == null || templateFields == null) return;
            foreach (Field templateField in templateFields)
            {
                if (!string.IsNullOrEmpty(templateField.title))
                {
                    var field = Field.Lookup(fields, templateField.title);
                    if (field != null)
                    {
                        // Variables' Initial Value should never be applied from template.
                        var shouldApplyTemplateFieldType = (field.type != templateField.type) && !string.Equals(field.title, "Initial Value");
                        if (shouldApplyTemplateFieldType)
                        {
                            field.type = templateField.type;
                            field.typeString = string.Empty;
                        }
                    }
                    else
                    {
                        fields.Add(new Field(templateField));
                    }
                }
            }
            SetDatabaseDirty("Apply Template");
        }

        private T AddNewAsset<T>(List<T> assets) where T : Asset, new()
        {
            T asset = new T();
            int highestID = database.baseID - 1;
            assets.ForEach(a => highestID = Mathf.Max(highestID, a.id));
            asset.id = Mathf.Max(1, highestID + 1);
            asset.fields = template.CreateFields(GetTemplateFields(asset));
            if (asset is Conversation)
            {
                InitializeConversation(asset as Conversation);
            }
            else if (asset is Item)
            {
                string itemTypeLabel = template.treatItemsAsQuests ? "Quest" : "Item";
                asset.Name = string.Format("New {0} {1}", itemTypeLabel, asset.id);
                Field.SetValue(asset.fields, "Is Item", !template.treatItemsAsQuests);
            }
            else
            {
                asset.Name = string.Format("New {0} {1}", typeof(T).Name, asset.id);
            }
            assets.Add(asset);
            SetDatabaseDirty("Add New Asset");
            return asset;
        }

        private T AddNewAssetFromTemplate<T>(List<T> assets, List<Field> templateFields, string typeLabel) where T : Asset, new()
        {
            T asset = new T();
            int highestID = database.baseID - 1;
            assets.ForEach(a => highestID = Mathf.Max(highestID, a.id));
            asset.id = Mathf.Max(1, highestID + 1);
            if (templateFields == null) templateFields = GetTemplateFields(asset);
            asset.fields = template.CreateFields(templateFields);
            if (asset is Conversation)
            {
                InitializeConversation(asset as Conversation);
            }
            else
            {
                asset.Name = string.Format("New {0} {1}", typeLabel, asset.id);
            }
            assets.Add(asset);
            SetDatabaseDirty("Add New Asset from Template");
            return asset;
        }

        private void InitializeConversation(Conversation conversation)
        {
            conversation.Title = string.Format("New Conversation {0}", conversation.id);
            conversation.ActorID = database.playerID;
            DialogueEntry startEntry = new DialogueEntry();
            startEntry.fields = new List<Field>();
            InitializeFieldsFromTemplate(startEntry.fields, template.dialogueEntryFields);
            startEntry.Title = "START";
            startEntry.currentSequence = "None()";
            startEntry.ActorID = database.playerID;
            conversation.dialogueEntries.Add(startEntry);
            SetDatabaseDirty("Initialize Conversation");
        }

        private void InitializeFieldsFromTemplate(List<Field> fields, List<Field> templateFields)
        {
            templateFields.ForEach(templateField => fields.Add(new Field(templateField.title, templateField.value, templateField.type)));
            SetDatabaseDirty("Initialize Fields from Template");
        }

        private List<Field> GetTemplateFields(Asset asset)
        {
            if (asset is Actor) return template.actorFields;
            if (asset is Item) return (asset as Item).IsItem ? template.itemFields : template.questFields;
            if (asset is Location) return template.locationFields;
            if (asset is Variable) return template.variableFields;
            if (asset is Conversation) return template.conversationFields;
            return template.locationFields;
        }

    }

}