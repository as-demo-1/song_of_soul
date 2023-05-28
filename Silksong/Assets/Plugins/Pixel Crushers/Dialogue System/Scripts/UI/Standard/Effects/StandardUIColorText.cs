// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// This script provides methods to change a Text element's color.
    /// You can tie it to hover events on buttons if you want the button's
    /// text color to change when hovered.
    /// </summary>
    [AddComponentMenu("")] // Use wrapper.
    public class StandardUIColorText : MonoBehaviour
    {

        public Color color;

        public UITextField text;

        private Color originalColor;

        private void Awake()
        {
            if (text.gameObject == null) text.uiText = GetComponentInChildren<UnityEngine.UI.Text>();
#if TMP_PRESENT
            if (text.gameObject == null) text.textMeshProUGUI = GetComponentInChildren<TMPro.TextMeshProUGUI>();
#endif
            originalColor = text.color;
        }

        public void ApplyColor()
        {
            originalColor = text.color;
            text.color = color;
        }

        public void UndoColor()
        {
            text.color = originalColor;
        }

    }

}
