// Copyright (c) Pixel Crushers. All rights reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PixelCrushers
{

    /// <summary>
    /// Specifies fonts to use for different languages. Used by SetLocalizedFont
    /// and LocalizeUI.
    /// </summary>
    public class LocalizedFonts : ScriptableObject
    {

        [System.Serializable]
        public class FontForLanguage
        {
            public string language;
            public Font font;
#if TMP_PRESENT
            public TMPro.TMP_FontAsset textMeshProFont;
#endif
            [Tooltip("Scale this language's font size relative to initial font's size. (0.5 scales to half size, 2.0 scales to double size.)")]
            public float scaleRelativeToInitialFont = 1f;
        }

        public Font defaultFont;

#if TMP_PRESENT
            public TMPro.TMP_FontAsset defaultTextMeshProFont;
#endif

        public List<FontForLanguage> fontsForLanguages = new List<FontForLanguage>();

        /// <summary>
        /// Returns the font for a specified language, or default font if none.
        /// </summary>
        public Font GetFont(string language)
        {
            var record = fontsForLanguages.Find(x => string.Equals(x.language, language));
            return (record != null && record.font != null) ? record.font : defaultFont;
        }

#if TMP_PRESENT
        /// <summary>
        /// Returns the TextMesh Pro font for a specified language, or default TextMesh Pro font if none.
        /// </summary>
        public TMPro.TMP_FontAsset GetTextMeshProFont(string language)
        {
            var record = fontsForLanguages.Find(x => string.Equals(x.language, language));
            return (record != null && record.textMeshProFont != null) ? record.textMeshProFont : defaultTextMeshProFont;
        }
#endif

        public float GetFontScale(string language)
        {
            var record = fontsForLanguages.Find(x => string.Equals(x.language, language));
            return (record != null && record.font != null) ? record.scaleRelativeToInitialFont : 1;
        }

    }
}
