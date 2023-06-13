// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers
{

    /// <summary>
    /// Sets a Text or TextMeshProUGUI component's font according to the current
    /// language and the settings in a LocalizedFonts asset.
    /// </summary>
    [AddComponentMenu("")] // Use wrapper.
    public class SetLocalizedFont : MonoBehaviour
    {

        [SerializeField] private bool m_setOnEnable = true;

        [Tooltip("Overrides UILocalizationManager's Localized Fonts if set.")]
        [SerializeField] private LocalizedFonts m_localizedFonts = null;

        private bool m_started = false;
        private float m_initialFontSize = -1;
        private UnityEngine.UI.Text text;
#if TMP_PRESENT
        private TMPro.TextMeshProUGUI textMeshPro;
#endif

        private void Awake()
        {
            text = GetComponent<UnityEngine.UI.Text>();
#if TMP_PRESENT
            textMeshPro = GetComponent<TMPro.TextMeshProUGUI>();
#endif
        }

        private void Start()
        {
            m_started = true;
            if (m_setOnEnable) SetCurrentLocalizedFont();
        }

        private void OnEnable()
        {
            if (m_started) SetCurrentLocalizedFont();
        }

        public void SetCurrentLocalizedFont()
        {
            // Record initial font size if necessary:
            if (m_initialFontSize == -1)
            {
                if (text != null) m_initialFontSize = text.fontSize;
#if TMP_PRESENT
            if (textMeshPro != null) m_initialFontSize = textMeshPro.fontSize;
#endif
            }

            // Get LocalizedFonts asset:
            var localizedFonts = (m_localizedFonts != null) ? m_localizedFonts : UILocalizationManager.instance.localizedFonts;
            if (localizedFonts == null) return;

            if (text != null)
            {
                var localizedFont = localizedFonts.GetFont(UILocalizationManager.instance.currentLanguage);
                if (localizedFont != null)
                {
                    text.font = localizedFont;
                    text.fontSize = Mathf.RoundToInt(localizedFonts.GetFontScale(UILocalizationManager.instance.currentLanguage) * m_initialFontSize);
                }
            }

#if TMP_PRESENT
            if (textMeshPro != null)
            {
                var localizedTextMeshProFont = localizedFonts.GetTextMeshProFont(UILocalizationManager.instance.currentLanguage);
                if (localizedTextMeshProFont != null) 
                {
                    textMeshPro.font = localizedTextMeshProFont;
                    textMeshPro.fontSize = localizedFonts.GetFontScale(UILocalizationManager.instance.currentLanguage) * m_initialFontSize;
                }
            }
#endif
        }
    }
}
