// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using System.Collections;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// This component strips RPGMaker-style pause codes from a text component when enabled.
    /// </summary>
    [AddComponentMenu("")] // Use wrapper.
    [DisallowMultipleComponent]
    public class UnityUIIgnorePauseCodes : MonoBehaviour
    {

        private UnityEngine.UI.Text control;

        public void Awake()
        {
            control = GetComponent<UnityEngine.UI.Text>();
            Tools.DeprecationWarning(this);
        }

        public void Start()
        {
            CheckText();
        }

        public void OnEnable()
        {
            CheckText();
        }

        public void CheckText()
        {
            if (control != null && control.text.Contains(@"\")) StartCoroutine(Clean());
        }

        private IEnumerator Clean()
        {
            control.text = UITools.StripRPGMakerCodes(control.text);
            yield return null;
            control.text = UITools.StripRPGMakerCodes(control.text);
        }

    }

}
