// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem.DialogueEditor
{

    /// <summary>
    /// This part of the Dialogue Editor window handles the Template tab, which allows the user
    /// to modify the template used by dialogue databases.
    /// </summary>
    public partial class DialogueEditorWindow
    {

        [Serializable]
        private class TemplateFoldouts
        {
            public bool templates = false;
            public bool actors = false;
            public bool items = false;
            public bool quests = false;
            public bool locations = false;
            public bool conversations = false;
            public bool dialogueEntries = false;
            public bool variables = false;
        }

        [SerializeField]
        private TemplateFoldouts templateFoldouts = new TemplateFoldouts();

        private string templateExportPath = string.Empty;

        private string currentTemplateFoldout;

        private void DrawTemplateSection()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Templates", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            DrawTemplateMenu();
            EditorGUILayout.EndHorizontal();
            DrawTemplates();
        }

        private void DrawTemplateMenu()
        {
            if (GUILayout.Button("Menu", "MiniPullDown", GUILayout.Width(56)))
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("Export XML..."), false, ExportTemplate);
                menu.AddItem(new GUIContent("Import XML..."), false, ImportTemplate);
                menu.AddItem(new GUIContent("Update Template From Assets"), false, ConfirmUpdateTemplateFromAssets);
                menu.AddItem(new GUIContent("Apply Template To Assets"), false, ConfirmApplyTemplateToAssets);
                menu.AddItem(new GUIContent("Sync Assets and Template"), false, ConfirmSyncAssetsAndTemplate);
                menu.AddItem(new GUIContent("Remove Empty Fields"), false, ConfirmRemoveEmptyFields);
                menu.AddItem(new GUIContent("Reset"), false, ResetTemplate);
                menu.ShowAsContext();
            }
        }

        private void DrawTemplates()
        {
            EditorGUI.BeginChangeCheck();
            EditorWindowTools.StartIndentedSection();
            DrawTemplate("Actors", template.actorFields, template.actorPrimaryFieldTitles, ref templateFoldouts.actors);
            DrawTemplate("Items", template.itemFields, null, ref templateFoldouts.items);
            DrawTemplate("Quests", template.questFields, template.questPrimaryFieldTitles, ref templateFoldouts.quests);
            DrawTemplate("Locations", template.locationFields, null, ref templateFoldouts.locations);
            DrawTemplate("Variables", template.variableFields, null, ref templateFoldouts.variables);
            DrawTemplate("Conversations", template.conversationFields, template.conversationPrimaryFieldTitles, ref templateFoldouts.conversations);
            DrawTemplate("Dialogue Entries", template.dialogueEntryFields, template.dialogueEntryPrimaryFieldTitles, ref templateFoldouts.dialogueEntries);
            DrawDialogueLineColors();
            EditorWindowTools.EndIndentedSection();
            if (EditorGUI.EndChangeCheck()) SaveTemplate();
        }

        private void DrawTemplate(string foldoutName, List<Field> fields, List<string> primaryFieldTitles, ref bool foldout)
        {
            currentTemplateFoldout = foldoutName;
            EditorGUILayout.BeginHorizontal();
            foldout = EditorGUILayout.Foldout(foldout, foldoutName);
            if (GUILayout.Button(new GUIContent(" ", "Add new field to template."), "OL Plus", GUILayout.Width(16))) fields.Add(new Field());
            EditorGUILayout.EndHorizontal();
            if (foldout)
            {
                EditorWindowTools.StartIndentedSection();
                EditorGUILayout.BeginVertical("button");
                DrawFieldsSection(fields, primaryFieldTitles);
                EditorGUILayout.EndVertical();
                EditorWindowTools.EndIndentedSection();
            }
        }

        private void DrawDialogueLineColors()
        {
            template.npcLineColor = EditorGUILayout.ColorField(new GUIContent("NPC Lines", "Color for NPC lines in Outline mode."), template.npcLineColor);
            template.pcLineColor = EditorGUILayout.ColorField(new GUIContent("PC Lines", "Color for PC lines in Outline mode."), template.pcLineColor);
            template.repeatLineColor = EditorGUILayout.ColorField(new GUIContent("Repeat Lines", "Color for repeated lines in Outline mode."), template.repeatLineColor);
        }

        private void ExportTemplate()
        {
            string newExportPath = EditorUtility.SaveFilePanel("Export Template to XML", EditorWindowTools.GetDirectoryName(templateExportPath), templateExportPath, "xml");
            if (!string.IsNullOrEmpty(newExportPath))
            {
                templateExportPath = newExportPath;
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(Template));
                StreamWriter streamWriter = new StreamWriter(templateExportPath, false, System.Text.Encoding.Unicode);
                xmlSerializer.Serialize(streamWriter, template);
                streamWriter.Close();
            }
        }

        private void ImportTemplate()
        {
            string importPath = EditorUtility.OpenFilePanel("Import Template from XML", EditorWindowTools.GetDirectoryName(templateExportPath), "xml");
            if (!string.IsNullOrEmpty(importPath))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(Template));
                Template newTemplate = xmlSerializer.Deserialize(new StreamReader(importPath)) as Template;
                if (newTemplate != null)
                {
                    template = newTemplate;
                    SaveTemplate();
                }
                else {
                    EditorUtility.DisplayDialog("Import Error", "Unable to import template data from the XML file.", "OK");
                }
            }
        }

        private void ResetTemplate()
        {
            if (EditorUtility.DisplayDialog("Reset template?", "You cannot undo this action.", "Reset", "Cancel"))
            {
                template = Template.FromDefault();
                SaveTemplate();
                Repaint();
            }
        }

        private void ScrubFieldFromDatabase(string fieldTitleToRemove)
        {
            database.actors.ForEach(x => ScrubField(x.fields, fieldTitleToRemove));
            database.items.ForEach(x => ScrubField(x.fields, fieldTitleToRemove));
            database.locations.ForEach(x => ScrubField(x.fields, fieldTitleToRemove));
            foreach (var conversation in database.conversations)
            {
                conversation.dialogueEntries.ForEach(x => ScrubField(x.fields, fieldTitleToRemove));
            }
        }

        private void ScrubFieldFromCurrentDatabaseCategory(string fieldTitleToRemove)
        {
            if (string.Equals(currentTemplateFoldout, "Actors"))
            {
                database.actors.ForEach(x => ScrubField(x.fields, fieldTitleToRemove));
            }
            else if (string.Equals(currentTemplateFoldout, "Items"))
            {
                database.items.ForEach(x => { if (x.IsItem) ScrubField(x.fields, fieldTitleToRemove); });
            }
            else if (string.Equals(currentTemplateFoldout, "Quests"))
            {
                database.items.ForEach(x => { if (!x.IsItem) ScrubField(x.fields, fieldTitleToRemove); });
            }
            else if (string.Equals(currentTemplateFoldout, "Locations"))
            {
                database.locations.ForEach(x => ScrubField(x.fields, fieldTitleToRemove));
            }
            else if (string.Equals(currentTemplateFoldout, "Variables"))
            {
                database.variables.ForEach(x => ScrubField(x.fields, fieldTitleToRemove));
            }
            else
            {
                foreach (var conversation in database.conversations)
                {
                    if (string.Equals(currentTemplateFoldout, "Conversations"))
                    {
                        ScrubField(conversation.fields, fieldTitleToRemove);
                    }
                    else if (string.Equals(currentTemplateFoldout, "Dialogue Entries"))
                    {
                        conversation.dialogueEntries.ForEach(x => ScrubField(x.fields, fieldTitleToRemove));
                    }
                }
            }
        }

        private void ScrubField(List<Field> fields, string fieldTitleToRemove)
        {
            if (fields == null) return;
            var field = fields.Find(f => string.Equals(f.title, fieldTitleToRemove));
            if (field != null) fields.Remove(field);
        }

    }

}