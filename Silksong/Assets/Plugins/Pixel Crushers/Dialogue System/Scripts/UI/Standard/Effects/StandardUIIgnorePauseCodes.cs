// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using System.Collections;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// This component strips RPGMaker-style pause codes from a text element when enabled.
    /// </summary>
    [AddComponentMenu("")] // Use wrapper.
    [DisallowMultipleComponent]
    public class StandardUIIgnorePauseCodes : MonoBehaviour
    {

        private UITextField text = new UITextField();

        public void Start()
        {
            text.uiText = GetComponentInChildren<UnityEngine.UI.Text>();
#if TMP_PRESENT
            text.textMeshProUGUI = GetComponentInChildren<TMPro.TextMeshProUGUI>();
#endif
            CheckText();
        }

        public void OnEnable()
        {
            CheckText();
        }

        public void CheckText()
        {
            if (string.IsNullOrEmpty(text.text)) return;
            if (text.text.Contains(@"\")) StartCoroutine(Clean());
        }

        private IEnumerator Clean()
        {
            text.text = UITools.StripRPGMakerCodes(text.text);
            yield return null;
            text.text = UITools.StripRPGMakerCodes(text.text);
        }

    }

}
