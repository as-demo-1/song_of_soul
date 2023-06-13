// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelCrushers
{

    /// <summary>
    /// Manages localization settings.
    /// </summary>
    [AddComponentMenu("")] // Use wrapper.
    public class UILocalizationManager : MonoBehaviour
    {

        [Tooltip("The PlayerPrefs key to store the player's selected language code.")]
        [SerializeField]
        private string m_currentLanguagePlayerPrefsKey = "Language";

        [Tooltip("Overrides the global text table.")]
        [SerializeField]
        private TextTable m_textTable = null;

        [Tooltip("Any additional text tables.")]
        [SerializeField]
        private List<TextTable> m_additionalTextTables = null;

        [Tooltip("Table of fonts to use for specific languages.")]
        [SerializeField]
        private LocalizedFonts m_localizedFonts;

        [Tooltip("When starting, set current language to value saved in PlayerPrefs.")]
        [SerializeField]
        private bool m_saveLanguageInPlayerPrefs = true;

        [Tooltip("When updating UIs, perform longer search that also finds LocalizeUI components on inactive GameObjects.")]
        [SerializeField]
        private bool m_alsoUpdateInactiveLocalizeUI = true;

        [Tooltip("If a language's field value is blank, use default language's field value.")]
        [SerializeField]
        private bool m_useDefaultLanguageForBlankTranslations = true;

        private string m_currentLanguage = string.Empty;

        public LocalizedFonts localizedFonts { get { return m_localizedFonts; } set { m_localizedFonts = value; } }

        private static UILocalizationManager s_instance = null;

        /// <summary>
        /// Current global instance of UILocalizationManager. If one doesn't exist,
        /// a default one will be created.
        /// </summary>
        public static UILocalizationManager instance
        {
            get
            {
                if (s_instance == null)
                {
                    s_instance = FindObjectOfType<UILocalizationManager>();
                    if (s_instance == null && Application.isPlaying)
                    {
                        var globalTextTable = FindObjectOfType<GlobalTextTable>();
                        s_instance = (globalTextTable != null) ? globalTextTable.gameObject.AddComponent<UILocalizationManager>()
                            : new GameObject("UILocalizationManager").AddComponent<UILocalizationManager>();
                    }
                }
                return s_instance;
            }
            set
            {
                s_instance = value;
            }
        }

#if UNITY_2019_3_OR_NEWER && UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void InitStaticVariables()
        {
            s_instance = null;
        }
#endif

        /// <summary>
        /// Overrides the global text table.
        /// </summary>
        public TextTable textTable
        {
            get { return (m_textTable != null) ? m_textTable : GlobalTextTable.textTable; }
            set { m_textTable = value; }
        }

        /// <summary>
        /// (Optional) Any additional text tables.
        /// </summary>
        public List<TextTable> additionalTextTables
        {
            get { return m_additionalTextTables; }
            set { m_additionalTextTables = value; }
        }

        /// <summary>
        /// Gets or sets the current language. Setting the current language also updates localized UIs.
        /// </summary>
        public string currentLanguage
        {
            get
            {
                return instance.m_currentLanguage;
            }
            set
            {
                instance.m_currentLanguage = value;
                instance.UpdateUIs(value);
            }
        }

        /// <summary>
        /// The PlayerPrefs key to store the player's selected language code.
        /// </summary>
        public string currentLanguagePlayerPrefsKey
        {
            get { return m_currentLanguagePlayerPrefsKey; }
            set { m_currentLanguagePlayerPrefsKey = value; }
        }

        public bool saveLanguageInPlayerPrefs
        {
            get { return m_saveLanguageInPlayerPrefs; }
            set { m_saveLanguageInPlayerPrefs = value; }
        }

        public bool useDefaultLanguageForBlankTranslations
        {
            get { return m_useDefaultLanguageForBlankTranslations; }
            set { m_useDefaultLanguageForBlankTranslations = value; TextTable.useDefaultLanguageForBlankTranslations = value; }
        }

        private void Awake()
        {
            if (s_instance == null) s_instance = this;
            Initialize();
        }

        public void Initialize()
        { 
            if (saveLanguageInPlayerPrefs)
            {
                if (!string.IsNullOrEmpty(currentLanguagePlayerPrefsKey) && PlayerPrefs.HasKey(currentLanguagePlayerPrefsKey))
                {
                    m_currentLanguage = PlayerPrefs.GetString(currentLanguagePlayerPrefsKey);
                }
            }
            TextTable.useDefaultLanguageForBlankTranslations = m_useDefaultLanguageForBlankTranslations;
        }

        private IEnumerator Start()
        {
            yield return new WaitForEndOfFrame(); // Wait for Text components to start.
            UpdateUIs(currentLanguage);
        }

        /// <summary>
        /// Checks if text table(s) support the specified language.
        /// </summary>
        public bool HasLanguage(string language)
        {
            if (textTable != null && textTable.HasLanguage(language)) return true;
            if (additionalTextTables != null)
            {
                foreach (TextTable additionalTable in additionalTextTables)
                {
                    if (additionalTable != null && additionalTable.HasLanguage(language)) return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Checks if text table(s) have a specified field.
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public bool HasField(string fieldName)
        {
            if (textTable != null && textTable.HasField(fieldName)) return true;
            if (additionalTextTables != null)
            {
                foreach (TextTable additionalTable in additionalTextTables)
                {
                    if (additionalTable != null && additionalTable.HasField(fieldName)) return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns the localized text for a field in a specified language.
        /// </summary>
        public string GetFieldTextForLanguage(string fieldName, string language)
        {
            if (textTable != null && textTable.HasField(fieldName)) return textTable.GetFieldTextForLanguage(fieldName, language);
            if (additionalTextTables != null)
            {
                foreach (TextTable additionalTable in additionalTextTables)
                {
                    if (additionalTable != null && additionalTable.HasField(fieldName)) return additionalTable.GetFieldTextForLanguage(fieldName, language);
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// Returns the localized text for a field in the current language.
        /// </summary>
        public string GetLocalizedText(string fieldName)
        {
            return GetFieldTextForLanguage(fieldName, GlobalTextTable.currentLanguage);
        }

        /// <summary>
        /// Updates the current language and all localized UIs.
        /// </summary>
        /// <param name="language">Language code defined in your Text Table.</param>
        public void UpdateUIs(string language)
        {
            m_currentLanguage = language;
            if (saveLanguageInPlayerPrefs)
            {
                if (!string.IsNullOrEmpty(currentLanguagePlayerPrefsKey))
                {
                    PlayerPrefs.SetString(currentLanguagePlayerPrefsKey, language);
                }
            }

            var localizeUIs = m_alsoUpdateInactiveLocalizeUI
                ? GameObjectUtility.FindObjectsOfTypeAlsoInactive<LocalizeUI>()
                : FindObjectsOfType<LocalizeUI>();
            for (int i = 0; i < localizeUIs.Length; i++)
            {
                localizeUIs[i].UpdateText();
            }
        }

    }
}
