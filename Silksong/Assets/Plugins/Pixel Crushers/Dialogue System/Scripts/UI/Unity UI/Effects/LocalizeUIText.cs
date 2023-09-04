// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// This older LocalizeUIText script is now a wrapper for the newer LocalizeUI script.
    /// </summary>
    [AddComponentMenu("")] // Use wrapper.
    public class LocalizeUIText : LocalizeUI
    {
        private void Awake()
        {
            Tools.DeprecationWarning(this, "Use LocalizeUI instead.");
        }

        public virtual void LocalizeText()
        {
            UpdateText();
        }
    }

    ///// <summary>
    ///// This script localizes the content of the Text element or Dropdown element
    ///// on this GameObject. You can assign the localized text table to this script 
    ///// or the Dialogue Manager. The element's starting text value(s) serves as the 
    ///// field name(s) to look up in the table.
    ///// </summary>
    //[AddComponentMenu("")] // Use wrapper.
    //public class LocalizeUIText : MonoBehaviour
    //{

    //    [Tooltip("Optional; overrides the Dialogue Manager's table.")]
    //    public TextTable textTable;
    //    //---Was: public TextTable localizedTextTable;

    //    [Tooltip("Optional; if assigned, use this instead of the Text field's value as the field lookup value.")]
    //    public string fieldName = string.Empty;

    //    protected UnityEngine.UI.Text text = null;
    //    protected List<string> fieldNames = new List<string>();
    //    protected bool started = false;
    //    protected UnityEngine.UI.Dropdown dropdown = null;

    //    protected virtual void Start()
    //    {
    //        started = true;
    //        LocalizeText();
    //    }

    //    protected virtual void OnEnable()
    //    {
    //        LocalizeText();
    //    }

    //    public virtual void LocalizeText()
    //    {
    //        if (!started) return;

    //        // Skip if no language set:
    //        if (string.IsNullOrEmpty(PixelCrushers.DialogueSystem.Localization.language)) return;
    //        if (textTable == null)
    //        {
    //            textTable = DialogueManager.displaySettings.localizationSettings.textTable;
    //            if (textTable == null)
    //            {
    //                if (DialogueDebug.logWarnings) Debug.LogWarning(DialogueDebug.Prefix + ": No localized text table is assigned to " + name + " or the Dialogue Manager.", this);
    //                return;
    //            }
    //        }

    //        if (!HasCurrentLanguage())
    //        {
    //            if (DialogueDebug.logWarnings) Debug.LogWarning(DialogueDebug.Prefix + "Localized text table '" + textTable + "' does not have a language '" + PixelCrushers.DialogueSystem.Localization.language + "'", this);
    //            return;
    //        }

    //        // Make sure we have a Text or Dropdown:
    //        if (text == null && dropdown == null)
    //        {
    //            text = GetComponent<UnityEngine.UI.Text>();
    //            dropdown = GetComponent<UnityEngine.UI.Dropdown>();
    //            if (text == null && dropdown == null)
    //            {
    //                if (DialogueDebug.logWarnings) Debug.LogWarning(DialogueDebug.Prefix + ": LocalizeUIText didn't find a Text or Dropdown component on " + name + ".", this);
    //                return;
    //            }
    //        }

    //        // Get the original values to use as field lookups:
    //        if (string.IsNullOrEmpty(fieldName)) fieldName = (text != null) ? text.text : string.Empty;
    //        if ((fieldNames.Count == 0) && (dropdown != null)) dropdown.options.ForEach(opt => fieldNames.Add(opt.text));
    //        // Localize Text:
    //        if (text != null)
    //        {
    //            if (!textTable.HasField(fieldName))
    //            {
    //                if (DialogueDebug.logWarnings) Debug.LogWarning(DialogueDebug.Prefix + ": Localized text table '" + textTable.name + "' does not have a field: " + fieldName, this);
    //            }
    //            else
    //            {
    //                text.text = textTable.GetFieldTextForLanguage(fieldName, Localization.GetCurrentLanguageID(textTable));
    //            }
    //        }

    //        // Localize Dropdown:
    //        if (dropdown != null)
    //        {
    //            for (int i = 0; i < dropdown.options.Count; i++)
    //            {
    //                if (i < fieldNames.Count)
    //                {
    //                    dropdown.options[i].text = textTable.GetFieldTextForLanguage(fieldNames[i], Localization.GetCurrentLanguageID(textTable));
    //                }
    //            }
    //            dropdown.captionText.text = textTable.GetFieldTextForLanguage(fieldNames[dropdown.value], Localization.GetCurrentLanguageID(textTable));
    //        }
    //    }

    //    protected virtual bool HasCurrentLanguage()
    //    {
    //        if (textTable == null) return false;
    //        return textTable.HasLanguage(PixelCrushers.DialogueSystem.Localization.language);
    //        //---Was: (for LocalizedTextTable)
    //        //foreach (var language in textTable.languages)
    //        //{
    //        //    if (string.Equals(language, PixelCrushers.DialogueSystem.Localization.language))
    //        //    {
    //        //        return true;
    //        //    }
    //        //}
    //        //return false;
    //    }

    //    /// <summary>
    //    /// Sets the field name, which is the key to use in the localized text table.
    //    /// By default, the field name is the initial value of the Text component.
    //    /// </summary>
    //    /// <param name="fieldName"></param>
    //    public virtual void UpdateFieldName(string newFieldName = "")
    //    {
    //        if (text == null) text = GetComponent<UnityEngine.UI.Text>();
    //        fieldName = string.IsNullOrEmpty(newFieldName) ? text.text : newFieldName;
    //    }

    //    /// <summary>
    //    /// If changing the Dropdown options, call this afterward to update the localization.
    //    /// </summary>
    //    public virtual void UpdateOptions()
    //    {
    //        fieldNames.Clear();
    //        LocalizeText();
    //    }
    //}

}
