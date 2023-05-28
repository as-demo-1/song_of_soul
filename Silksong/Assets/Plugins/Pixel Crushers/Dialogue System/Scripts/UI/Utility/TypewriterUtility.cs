// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.DialogueSystem
{

    public static class TypewriterUtility
    {

        public static AbstractTypewriterEffect GetTypewriter(UITextField textField)
        {
            AbstractTypewriterEffect typewriter = null;
#if TMP_PRESENT
            if (textField.textMeshProUGUI != null) typewriter = textField.textMeshProUGUI.GetComponent<AbstractTypewriterEffect>();
#endif
            if (typewriter == null && textField.uiText != null) typewriter = textField.uiText.GetComponent<AbstractTypewriterEffect>();
            return typewriter;
        }

        public static bool HasTypewriter(UITextField textField)
        {
            return GetTypewriter(textField) != null;
        }

        public static float GetTypewriterSpeed(AbstractTypewriterEffect typewriter)
        {
            return (typewriter != null) ? typewriter.GetSpeed() : -1;
        }

        public static float GetTypewriterSpeed(UITextField textField)
        {
            return GetTypewriterSpeed(GetTypewriter(textField));
            
        }

        public static void SetTypewriterSpeed(UITextField textField, float charactersPerSecond)
        {
            var typewriter = GetTypewriter(textField);
            if (typewriter != null) typewriter.SetSpeed(charactersPerSecond);
        }

        public static void StartTyping(UITextField textField, string text, int fromIndex = 0)
        {
            var typewriter = GetTypewriter(textField);
            if (typewriter != null && typewriter.enabled) typewriter.StartTyping(text, fromIndex);
        }

        public static void StopTyping(UITextField textField)
        {
            var typewriter = GetTypewriter(textField);
            if (typewriter != null && typewriter.enabled) typewriter.StopTyping();
        }

    }
}
