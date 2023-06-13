// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using System.Collections.Generic;

namespace PixelCrushers
{

    [AddComponentMenu("")] // Use wrapper instead.
    public class LocalizeUI : MonoBehaviour
    {

        [Tooltip("Overrides the global text table.")]
        [SerializeField]
        private TextTable m_textTable;

        [Tooltip("Overrides the UILocalizationManager's Localized Fonts.")]
        [SerializeField]
        private LocalizedFonts m_localizedFonts;

        [Tooltip("(Optional) If assigned, use this instead of the UI element's text's value as the field lookup value.")]
        [SerializeField]
        private string m_fieldName = string.Empty;

        public TextTable textTable
        {
            get { return m_textTable; }
            set { m_textTable = value; }
        }

        public LocalizedFonts localizedFonts
        {
            get { return m_localizedFonts; }
            set { m_localizedFonts = value; }
        }

        public string fieldName
        {
            get { return string.IsNullOrEmpty(m_fieldName) ? null : m_fieldName; }
            set { m_fieldName = value; }
        }

        private bool m_started = false;
        protected bool started
        {
            get { return m_started; }
            private set { m_started = value; }
        }

        private List<string> m_fieldNames = new List<string>();
        protected List<string> fieldNames
        {
            get { return m_fieldNames; }
            set { m_fieldNames = value; }
        }

        private UnityEngine.UI.Text m_text = null;
        public UnityEngine.UI.Text text
        {
            get { return m_text; }
            set { m_text = value; }
        }

        private UnityEngine.UI.Dropdown m_dropdown = null;
        public UnityEngine.UI.Dropdown dropdown
        {
            get { return m_dropdown; }
            set { m_dropdown = value; }
        }

#if TMP_PRESENT
        private TMPro.TextMeshPro m_textMeshPro;
        public TMPro.TextMeshPro textMeshPro
        {
            get { return m_textMeshPro; }
            set { m_textMeshPro = value; }
        }
        private TMPro.TextMeshProUGUI m_textMeshProUGUI;
        public TMPro.TextMeshProUGUI textMeshProUGUI
        {
            get { return m_textMeshProUGUI; }
            set { m_textMeshProUGUI = value; }
        }
        private bool m_lookedForTMP = false;
#endif

        protected virtual void Start()
        {
            started = true;
            UpdateText();
        }

        protected virtual void OnEnable()
        {
            UpdateText();
        }

        public virtual void UpdateText()
        {
            if (!started) return;
            var language = (UILocalizationManager.instance != null) ? UILocalizationManager.instance.currentLanguage : string.Empty;

            // Skip if no text table or language set:
            if (textTable == null && (UILocalizationManager.instance == null || UILocalizationManager.instance.textTable == null))
            {
                Debug.LogWarning("No localized text table is assigned to " + name + " or a UI Localized Manager component.", this);
                return;
            }

            if (!HasLanguage(language))
            {
                Debug.LogWarning("Text table does not have a language '" + language + "'.", textTable);
                //return; //--- Allow to continue and use default language value.
            }

            // Get LocalizedFonts asset:
            var localizedFonts = (m_localizedFonts != null) ? m_localizedFonts : UILocalizationManager.instance.localizedFonts;
            var localizedFont = (localizedFonts != null) ? localizedFonts.GetFont(language) : null;

            // Make sure we have localizable UI components:
            if (text == null && dropdown == null)
            {
                text = GetComponent<UnityEngine.UI.Text>();
                dropdown = GetComponent<UnityEngine.UI.Dropdown>();
            }
            var hasLocalizableComponent = text != null || dropdown != null;
#if TMP_PRESENT
            var localizedTextMeshProFont = (localizedFonts != null) ? localizedFonts.GetTextMeshProFont(language) : null;
            if (!m_lookedForTMP)
            {
                m_lookedForTMP = true;
                textMeshPro = GetComponent<TMPro.TextMeshPro>();
                textMeshProUGUI = GetComponent<TMPro.TextMeshProUGUI>();
            }
            hasLocalizableComponent = hasLocalizableComponent || textMeshProUGUI != null;
#endif
            if (!hasLocalizableComponent)
            {
                Debug.LogWarning("Localize UI didn't find a localizable UI component on " + name + ".", this);
                return;
            }

            // Get the original values to use as field lookups:
            if (string.IsNullOrEmpty(fieldName))
            {
                fieldName = (text != null) ? text.text : string.Empty;
            }
            if ((fieldNames.Count == 0) && (dropdown != null))
            {
                dropdown.options.ForEach(opt => fieldNames.Add(opt.text));
            }

            // Localize Text:
            if (text != null)
            {
                if (!HasField(fieldName))
                {
                    Debug.LogWarning("Text table does not have a field '" + fieldName + "'.", textTable);
                }
                else
                {
                    text.text = GetLocalizedText(fieldName);
                    if (localizedFont != null) text.font = localizedFont;
                }
            }

            // Localize Dropdown:
            if (dropdown != null)
            {
                for (int i = 0; i < dropdown.options.Count; i++)
                {
                    if (i < fieldNames.Count)
                    {
                        dropdown.options[i].text = GetLocalizedText(fieldNames[i]);
                    }
                }
                dropdown.captionText.text = GetLocalizedText(fieldNames[dropdown.value]);
                if (localizedFont != null)
                {
                    dropdown.captionText.font = localizedFont;
                    dropdown.itemText.font = localizedFont;
                }
            }

#if TMP_PRESENT
            if (!m_lookedForTMP)
            {
                m_lookedForTMP = true;
                textMeshPro = GetComponent<TMPro.TextMeshPro>();
                textMeshProUGUI = GetComponent<TMPro.TextMeshProUGUI>();
            }
            if (textMeshPro != null)
            {
                if (string.IsNullOrEmpty(fieldName))
                {
                    fieldName = (textMeshPro != null) ? textMeshPro.text : string.Empty;
                }
                if (!HasField(fieldName))
                {
                    Debug.LogWarning("Text table does not have a field '" + fieldName + "'.", textTable);
                }
                else
                {
                    textMeshPro.text = GetLocalizedText(fieldName);
                    if (localizedTextMeshProFont != null) textMeshPro.font = localizedTextMeshProFont;
                }
            }
            if (textMeshProUGUI != null)
            {
                if (string.IsNullOrEmpty(fieldName))
                {
                    fieldName = (textMeshProUGUI != null) ? textMeshProUGUI.text : string.Empty;
                }
                if (!HasField(fieldName))
                {
                    Debug.LogWarning("Text table does not have a field '" + fieldName + "'.", textTable);
                }
                else
                {
                    textMeshProUGUI.text = GetLocalizedText(fieldName);
                    if (localizedTextMeshProFont != null) textMeshProUGUI.font = localizedTextMeshProFont;
                }
            }
#endif

        }

        protected virtual bool HasLanguage(string language)
        {
            return (textTable != null && textTable.HasLanguage(language)) ||
                UILocalizationManager.instance.HasLanguage(language);
        }

        protected virtual bool HasField(string fieldName)
        {
            return (textTable != null && textTable.HasField(fieldName)) ||
                UILocalizationManager.instance.HasField(fieldName);
        }

        protected virtual string GetLocalizedText(string fieldName)
        {
            return (textTable != null && textTable.HasField(fieldName))
                ? textTable.GetFieldTextForLanguage(fieldName, GlobalTextTable.currentLanguage)
                : UILocalizationManager.instance.GetLocalizedText(fieldName); //---Was: GlobalTextTable.Lookup(fieldName);
        }

        /// <summary>
        /// Sets the field name, which is the key to use in the text table.
        /// By default, the field name is the initial value of the Text component.
        /// </summary>
        /// <param name="fieldName">Field name.</param>
        public virtual void SetFieldName(string newFieldName = "")
        {
            if (text == null) text = GetComponent<UnityEngine.UI.Text>();
            fieldName = (string.IsNullOrEmpty(newFieldName) && text != null) ? text.text : newFieldName;
        }

        /// <summary>
        /// If changing the Dropdown options, call this afterward to update the localization.
        /// </summary>
        public virtual void UpdateDropdownOptions()
        {
            fieldNames.Clear();
            UpdateText();
        }
    }
}
