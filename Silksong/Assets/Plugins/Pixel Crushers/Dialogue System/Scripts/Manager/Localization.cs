// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// This static class contains localization methods and the current language as
    /// defined by a string (e.g., "es" for generic Spanish, "fr-CA" for French - Canadian).
    /// </summary>
    public static class Localization
    {

        /// <summary>
        /// Gets or sets the current language.
        /// </summary>
        /// <value>
        /// The language.
        /// </value>
        public static string language
        {
            get
            {
                return m_language;
            }
            set
            {
                m_language = value;
                m_currentLanguageID = -1;
                if (DialogueManager.instance != null)
                {
                    var uiLocalizationManager = DialogueManager.instance.GetComponent<UILocalizationManager>();
                    if (uiLocalizationManager == null)
                    {
                        uiLocalizationManager = DialogueManager.instance.gameObject.AddComponent<UILocalizationManager>();
                        uiLocalizationManager.textTable = DialogueManager.instance.displaySettings.localizationSettings.textTable;
                    }
                    uiLocalizationManager.currentLanguage = value;
                }
            }
        }

        /// <summary>
        /// Indicates whether localization is set to use default text instead of localized text.
        /// </summary>
        /// <value>
        /// <c>true</c> if default text should be used. If <c>false</c>, the language specified
        /// by the Language property should be used.
        /// </value>
        public static bool isDefaultLanguage
        {
            get { return string.IsNullOrEmpty(language); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to use the default language's field value
        /// if a field is undefined for the current language.
        /// </summary>
        /// <value><c>true</c> if use default if undefined; otherwise, <c>false</c>.</value>
        public static bool useDefaultIfUndefined
        {
            get { return m_useDefaultIfUndefined; }
            set { m_useDefaultIfUndefined = value; }
        }

        /// @cond FOR_V1_COMPATIBILITY
        public static string Language { get { return language; } set { language = value; } }
        public static bool IsDefaultLanguage { get { return isDefaultLanguage; } }
        public static bool UseDefaultIfUndefined { get { return useDefaultIfUndefined; } set { useDefaultIfUndefined = value; } }
        /// @endcond

        private static string m_language = string.Empty;
        private static int m_currentLanguageID = -1;
        private static bool m_useDefaultIfUndefined = true;

#if UNITY_2019_3_OR_NEWER && UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void InitStaticVariables()
        {
            m_language = string.Empty;
            m_currentLanguageID = -1;
            m_useDefaultIfUndefined = true;
        }
#endif

        public static int GetCurrentLanguageID(TextTable textTable)
        {
            if (m_currentLanguageID == -1 && textTable != null)
            {
                m_currentLanguageID = textTable.GetLanguageID(language);
            }
            return (m_currentLanguageID == -1) ? 0 : m_currentLanguageID;
        }

        /// <summary>
        /// Converts a Unity SystemLanguage enum value to a language string.
        /// </summary>
        /// <returns>
        /// The language string representation of the specified systemLanguage.
        /// </returns>
        /// <param name='systemLanguage'>
        /// A Unity SystemLanguage enum value.
        /// </param>
        public static string GetLanguage(SystemLanguage systemLanguage)
        {
            switch (systemLanguage)
            {
                case SystemLanguage.Afrikaans: return "af";
                case SystemLanguage.Arabic: return "ar";
                case SystemLanguage.Basque: return "eu";
                case SystemLanguage.Belarusian: return "be";
                case SystemLanguage.Bulgarian: return "bg";
                case SystemLanguage.Catalan: return "ca";
                case SystemLanguage.Chinese: return "zh";
                case SystemLanguage.Czech: return "cs";
                case SystemLanguage.Danish: return "da";
                case SystemLanguage.Dutch: return "nl";
                case SystemLanguage.English: return "en";
                case SystemLanguage.Estonian: return "et";
                case SystemLanguage.Faroese: return "fo";
                case SystemLanguage.Finnish: return "fi";
                case SystemLanguage.French: return "fr";
                case SystemLanguage.German: return "de";
                case SystemLanguage.Greek: return "el";
                case SystemLanguage.Hebrew: return "he";
                case SystemLanguage.Hungarian: return "hu";
                case SystemLanguage.Icelandic: return "is";
                case SystemLanguage.Indonesian: return "id";
                case SystemLanguage.Italian: return "it";
                case SystemLanguage.Japanese: return "ja";
                case SystemLanguage.Korean: return "ko";
                case SystemLanguage.Latvian: return "lv";
                case SystemLanguage.Lithuanian: return "lt";
                case SystemLanguage.Norwegian: return "no";
                case SystemLanguage.Polish: return "pl";
                case SystemLanguage.Portuguese: return "pt";
                case SystemLanguage.Romanian: return "ro";
                case SystemLanguage.Russian: return "ru";
                case SystemLanguage.SerboCroatian: return "sr";
                case SystemLanguage.Slovak: return "sk";
                case SystemLanguage.Slovenian: return "sl";
                case SystemLanguage.Spanish: return "es";
                case SystemLanguage.Swedish: return "sv";
                case SystemLanguage.Thai: return "th";
                case SystemLanguage.Turkish: return "tr";
                case SystemLanguage.Ukrainian: return "uk";
                case SystemLanguage.Vietnamese: return "vi";
                default: return null;
            }
        }

    }

}
