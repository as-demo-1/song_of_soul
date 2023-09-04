// Recompile at 2023/9/4 10:18:33

// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using System;

namespace PixelCrushers
{

    /// <summary>
    /// A UIDropdownField can refer to a UI.Dropdown or TMPro.TMP_Dropdown.
    /// </summary>
    [Serializable]
    public class UIDropdownField
    {

        [SerializeField]
        private UnityEngine.UI.Dropdown m_uiDropdown;

        /// <summary>
        /// The UI.Dropdown assigned to this UI dropdown field.
        /// </summary>
        public UnityEngine.UI.Dropdown uiDropdown
        {
            get { return m_uiDropdown; }
            set { m_uiDropdown = value; }
        }

#if TMP_PRESENT
        [SerializeField]
        private TMPro.TMP_Dropdown m_tmpDropdown;

        /// <summary>
        /// The TMP_Dropdown assigned to this UI dropdown field.
        /// </summary>
        public TMPro.TMP_Dropdown tmpDropdown
        {
            get { return m_tmpDropdown; }
            set { m_tmpDropdown = value; }
        }
#endif

        /// <summary>
        /// The text content of the UI.Text or TextMeshProUGUI.
        /// </summary>
        public int value
        {
            get
            {
#if TMP_PRESENT
                if (m_tmpDropdown != null) return m_tmpDropdown.value;
#endif
                if (m_uiDropdown != null) return m_uiDropdown.value;
                return 0;
            }
            set
            {
#if TMP_PRESENT
                if (m_tmpDropdown != null) m_tmpDropdown.value = value;
#endif
                if (m_uiDropdown != null) m_uiDropdown.value = value;
            }
        }

        public bool enabled
        {
            get
            {
#if TMP_PRESENT
                if (m_tmpDropdown != null) return m_tmpDropdown.enabled;
#endif
                if (m_uiDropdown != null) return m_uiDropdown.enabled;
                return false;
            }
            set
            {
#if TMP_PRESENT
                if (m_tmpDropdown != null) m_tmpDropdown.enabled = value;
#endif
                if (m_uiDropdown != null) m_uiDropdown.enabled = value;
            }
        }

        public UIDropdownField()
        {
            this.uiDropdown = null;
#if TMP_PRESENT
            this.m_tmpDropdown = null;
#endif
        }

        public UIDropdownField(UnityEngine.UI.Dropdown uiDropdown)
        {
            this.uiDropdown = uiDropdown;
#if TMP_PRESENT
            this.m_tmpDropdown = null;
#endif
        }

#if TMP_PRESENT
        public UIDropdownField(TMPro.TMP_Dropdown tmpDropdown)
        {
            this.uiDropdown = null;
            this.m_tmpDropdown = tmpDropdown;
        }
#endif

        public GameObject gameObject
        {
            get
            {
#if TMP_PRESENT
                if (tmpDropdown != null) return tmpDropdown.gameObject;
#endif
                return (uiDropdown != null) ? uiDropdown.gameObject : null;
            }
        }

        public bool isActiveSelf { get { return (gameObject != null) ? gameObject.activeSelf : false; } }

        public bool activeInHierarchy { get { return (gameObject != null) ? gameObject.activeInHierarchy : false; } }

        public void SetActive(bool value)
        {
            if (uiDropdown != null) uiDropdown.gameObject.SetActive(value);
#if TMP_PRESENT
            if (tmpDropdown != null) tmpDropdown.gameObject.SetActive(value);
#endif
        }

        public void ClearOptions()
        {
            if (uiDropdown != null) uiDropdown.ClearOptions();
#if TMP_PRESENT
            if (tmpDropdown != null) tmpDropdown.ClearOptions();
#endif
        }

        public void AddOption(string text)
        {
            if (uiDropdown != null) uiDropdown.options.Add(new UnityEngine.UI.Dropdown.OptionData(text));
#if TMP_PRESENT
            if (tmpDropdown != null) tmpDropdown.options.Add(new TMPro.TMP_Dropdown.OptionData(text));
#endif
        }

        /// <summary>
        /// Checks if a UI element is assigned to a UITextField.
        /// </summary>
        /// <param name="uiTextField">UITextField to check.</param>
        /// <returns>`true` if no UI element is assigned; otherwise `false`.</returns>
        public static bool IsNull(UIDropdownField uiDropdownField)
        {
            if (uiDropdownField == null) return true;
            if (uiDropdownField.uiDropdown != null) return false;
#if TMP_PRESENT
            if (uiDropdownField.tmpDropdown!= null) return false;
#endif
            return true;
        }

    }
}
