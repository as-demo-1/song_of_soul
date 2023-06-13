// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem.DialogueEditor
{

    /// <summary>
    /// This part of the Dialogue Editor window handles drawing a list of fields 
    /// in a multinode selection.
    /// Drawing fields is complicated because a field can be one of several types.
    /// Actor fields need to provide a popup menu of the actors in the database,
    /// quest state fields need to provide a popup menu of the quest states, etc.
    /// </summary>
    public partial class DialogueEditorWindow
    {

        private bool DrawMultinodeFieldsSection()
        {
            EditorWindowTools.StartIndentedSection();
            DrawFieldsHeading();
            var changed = DrawMultinodeFieldsContent();
            EditorWindowTools.EndIndentedSection();
            return changed;
        }

        private bool DrawMultinodeFieldsContent()
        {
            if (multinodeSelection.nodes.Count == 0) return false;
            var changed = false;
            var entry = multinodeSelection.nodes[0];
            var fields = entry.fields;
            for (int i = 0; i < fields.Count; i++)
            {
                var field = new Field(fields[i]);

                for (int j = 1; j < multinodeSelection.nodes.Count; j++)
                {
                    if (i >= multinodeSelection.nodes[j].fields.Count) continue;
                    if (!string.Equals(multinodeSelection.nodes[j].fields[i].title, field.title)) continue;
                    if (!string.Equals(multinodeSelection.nodes[j].fields[i].value, field.value))
                    {
                        field.value = "-";
                        break;
                    }
                }

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.BeginHorizontal();
                if (IsTextAreaField(fields[i]))
                {
                    DrawTextAreaFirstPart(field);
                    DrawTextAreaSecondPart(field);
                }
                else
                {
                    DrawField(field);
                }
                EditorGUILayout.EndHorizontal();
                if (EditorGUI.EndChangeCheck())
                {
                    changed = true;
                    for (int j = 0; j < multinodeSelection.nodes.Count; j++)
                    {
                        Field.SetValue(multinodeSelection.nodes[j].fields, field.title, field.value, field.type);
                    }
                }
            }
            return changed;
        }
    }

}