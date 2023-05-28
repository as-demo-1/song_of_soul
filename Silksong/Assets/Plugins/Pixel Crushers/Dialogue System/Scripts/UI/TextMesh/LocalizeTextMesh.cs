using UnityEngine;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// This script localizes the content of the TextMesh element on this 
    /// GameObject. You can assign the localized text table to this script 
    /// or the Dialogue Manager. The element's starting text value serves 
    /// as the field name to look up in the table.
    /// 
    /// Note: Since TextMesh has been deprecated in later versions of Unity,
    /// this component is only valid for Unity 2018.2 or older.
    /// </summary>
    [AddComponentMenu("")] // Use wrapper.
    public class LocalizeTextMesh : LocalizeUI
    {

#if UNITY_5 || UNITY_2017 || UNITY_2018_1 || UNITY_2018_2

        protected TextMesh m_textMesh;

        public virtual void LocalizeText()
        {
            UpdateText();
        }

        public override void UpdateText()
        {
            if (!started) return;
            base.UpdateText();

            // Skip if no language set:
            var language = UILocalizationManager.instance.currentLanguage;
            if (string.IsNullOrEmpty(language)) return;
            if (textTable == null)
            {
                textTable = DialogueManager.displaySettings.localizationSettings.textTable;
                if (textTable == null)
                {
                    if (DialogueDebug.logWarnings) Debug.LogWarning(DialogueDebug.Prefix + ": No text table is assigned to " + name + " or the Dialogue Manager.", this);
                    return;
                }
            }

            // Make sure we have a TextMesh:
            if (m_textMesh == null)
            {
                m_textMesh = GetComponent<TextMesh>();
                if (m_textMesh == null)
                {
                    if (DialogueDebug.logWarnings) Debug.LogWarning(DialogueDebug.Prefix + ": LocalizeTextMesh didn't find a TextMesh component on " + name + ".", this);
                    return;
                }
            }

            // Get the original values to use as field lookups:
            if (string.IsNullOrEmpty(fieldName)) fieldName = (m_textMesh != null) ? m_textMesh.text : string.Empty;

            // Localize Text:
            if (m_textMesh != null)
            {
                if (!textTable.HasField(fieldName))
                {
                    if (DialogueDebug.logWarnings) Debug.LogWarning(DialogueDebug.Prefix + ": text table '" + textTable.name + "' does not have a field: " + fieldName, this);
                }
                else
                {
                    m_textMesh.text = textTable.GetFieldTextForLanguage(fieldName, Localization.GetCurrentLanguageID(textTable));
                }
            }
        }

        public virtual void UpdateFieldName(string newFieldName = "")
        {
            SetFieldName(newFieldName);
        }

#else

        public virtual void LocalizeText()
        {
        }

        public virtual void UpdateFieldName(string newFieldName = "")
        {
        }

#endif

    }

}
