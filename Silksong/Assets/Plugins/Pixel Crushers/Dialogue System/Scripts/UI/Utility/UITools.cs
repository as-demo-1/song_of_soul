// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// Tools for working with Unity UI.
    /// </summary>
    public static class UITools
    {

        /// <summary>
        /// Dialogue databases may use Texture2Ds or Sprites for actor portraits. Unity UI uses sprites.
        /// The CreateSprite method converts textures to sprites. This dictionary contains
        /// converted sprites so we don't need to reconvert them every time we want to show 
        /// an actor's portrait.
        /// </summary>
        public static Dictionary<Texture2D, Sprite> spriteCache = new Dictionary<Texture2D, Sprite>();

#if UNITY_2019_3_OR_NEWER && UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void InitStaticVariables()
        {
            ClearSpriteCache();
        }
#endif

        /// <summary>
        /// Ensures that the scene has an EventSystem.
        /// </summary>
        public static void RequireEventSystem()
        {
            UIUtility.RequireEventSystem(DialogueDebug.logWarnings ? "Dialogue System: The scene is missing an EventSystem. Adding one." : null);
        }

        /// <summary>
        /// Returns the hash for an animator state.
        /// </summary>
        public static int GetAnimatorNameHash(AnimatorStateInfo animatorStateInfo)
        {
			return animatorStateInfo.fullPathHash;
        }

        /// <summary>
        /// Clears the sprite cache, forcing all textures to be converted to sprites
        /// the first time they're used.
        /// </summary>
        public static void ClearSpriteCache()
        {
            spriteCache.Clear();
        }

        /// <summary>
        /// Gets the Sprite version of a Texture2D. Uses a cache so a texture will only be 
        /// converted to a sprite once.
        /// </summary>
        /// <param name="texture">Original Texture2D.</param>
        /// <returns>Sprite version.</returns>
        public static Sprite CreateSprite(Texture2D texture)
        {
            if (texture == null) return null;
            if (spriteCache.ContainsKey(texture))
            {
                var cachedSprite = spriteCache[texture];
                if (cachedSprite != null) return spriteCache[texture];
            }
            var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
            spriteCache[texture] = sprite;
            return sprite;
        }

        /// <summary>
        /// Given a Texture2D and/or Sprite, returns whichever one is not null.
        /// Gives preference to the Texture2D.
        public static Sprite GetSprite(Texture2D texture, Sprite sprite)
        {
            return (sprite != null) 
                ? sprite 
                : (texture != null) 
                    ? UITools.CreateSprite(texture) 
                    : null;
        }

        public static Texture2D GetTexture2D(Sprite sprite)
        {
            return (sprite != null) ? sprite.texture : null;
        }

        /// <summary>
        /// Returns the text inside a FormattedText object.
        /// </summary>
        public static string GetUIFormattedText(FormattedText formattedText)
        {
            if (formattedText == null)
            {
                return string.Empty;
            }
            else if (formattedText.italic)
            {
                return "<i>" + formattedText.text + "</i>";
            }
            else
            {
                return formattedText.text;
            }
        }

        private static AbstractDialogueUI dialogueUI = null;

        /// <summary>
        /// Sends "OnTextChange(text)" to the dialogue UI GameObject.
        /// </summary>
        public static void SendTextChangeMessage(UnityEngine.UI.Text text)
        {
            if (text == null) return;
            if (dialogueUI == null) dialogueUI = text.GetComponentInParent<AbstractDialogueUI>();
            if (dialogueUI == null) return;
            dialogueUI.SendMessage(DialogueSystemMessages.OnTextChange, text, SendMessageOptions.DontRequireReceiver);
        }

        /// <summary>
        /// Sends "OnTextChange(textField)" to the dialogue UI GameObject.
        /// </summary>
        public static void SendTextChangeMessage(UITextField textField)
        {
            if (textField.gameObject == null) return;
            textField.gameObject.SendMessage(DialogueSystemMessages.OnTextChange, textField, SendMessageOptions.DontRequireReceiver);
        }

        /// <summary>
        /// Selects a Selectable UI element and visually shows it as selected.
        /// </summary>
        /// <param name="selectable"></param>
        /// <param name="allowStealFocus"></param>
        public static void Select(UnityEngine.UI.Selectable selectable, bool allowStealFocus = true)
        {
            UIUtility.Select(selectable, allowStealFocus);
        }

        public const string RPGMakerCodeQuarterPause = @"\,";
        public const string RPGMakerCodeFullPause = @"\.";
        public const string RPGMakerCodeSkipToEnd = @"\^";
        public const string RPGMakerCodeInstantOpen = @"\>";
        public const string RPGMakerCodeInstantClose = @"\<";

        /// <summary>
        /// Returns a string without any embedded RPG Maker codes.
        /// </summary>
        public static string StripRPGMakerCodes(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            return s.Contains(@"\") ? s.Replace(RPGMakerCodeQuarterPause, string.Empty).
                Replace(RPGMakerCodeFullPause, string.Empty).
                Replace(RPGMakerCodeSkipToEnd, string.Empty).
                Replace(RPGMakerCodeInstantOpen, string.Empty).
                Replace(RPGMakerCodeInstantClose, string.Empty)
                : s;
        }

        public static string StripEmTags(string s)
        {
            return Regex.Replace(s, @"\[em\d+\]|\[/em\d+\]", string.Empty);
        }

        /// <summary>
        /// Wraps a string in rich text color codes. Properly handles nested 
        /// rich text codes.
        /// </summary>
        /// <param name="text">Original text to be wrapped.</param>
        /// <param name="color">Color to wrap around text.</param>
        public static string WrapTextInColor(string text, Color color)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            var colorCode = "<color=" + Tools.ToWebColor(color) + ">";

            // If text definitely has no other rich text codes, it's easy; just put a color code around the whole thing:
            if (!text.Contains("<")) return colorCode + text + "</color>";

            // Otherwise put color codes only around substrings that aren't already in existing rich text codes:
            var result = string.Empty;
            int index = 0;

            foreach (Match match in Regex.Matches(text, @"<i><color=^(?!.*</color>)</color></i>|<b><color=^(?!.*</color>)</color></b>|<i><b><color=^(?!.*</color>)</color></b></i>|<color=^(?!.*</color>)</color>"))
            {
                result += colorCode + text.Substring(index, match.Index) + "</color>" + match.Value;
                index = match.Index + match.Value.Length;
            }
            if (index < text.Length)
            {
                result += colorCode + text.Substring(index) + "</color>";
            }

            return result;
        }

        public static void EnableInteractivity(GameObject go)
        {
            var canvas = go.GetComponentInChildren<Canvas>() ?? go.GetComponentInParent<Canvas>();
            if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                if (canvas.worldCamera == null) canvas.worldCamera = Camera.main;
            }
            if (InputDeviceManager.instance == null ||
                (InputDeviceManager.instance.controlGraphicRaycasters && InputDeviceManager.currentInputDevice == InputDevice.Mouse))
            {
                var graphicRaycaster = go.GetComponentInChildren<UnityEngine.UI.GraphicRaycaster>() ?? go.GetComponentInParent<UnityEngine.UI.GraphicRaycaster>();
                if (graphicRaycaster != null) graphicRaycaster.enabled = true;
            }
        }

    }

}
