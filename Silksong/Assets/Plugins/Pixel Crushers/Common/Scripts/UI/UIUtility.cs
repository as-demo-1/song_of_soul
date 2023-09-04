// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers
{

    public static class UIUtility
    {

        /// <summary>
        /// Ensures that the scene has an EventSystem.
        /// </summary>
        /// <param name="message">If needing to add an EventSystem, show this message.</param>
        public static void RequireEventSystem(string message = null)
        {
            var eventSystem = GameObjectUtility.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>();
            if (eventSystem == null)
            {
                if (message != null) Debug.LogWarning(message);
                eventSystem = new GameObject("EventSystem").AddComponent<UnityEngine.EventSystems.EventSystem>();
#if USE_NEW_INPUT
                var inputModule = eventSystem.gameObject.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
                var inputModule = eventSystem.gameObject.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
#if !UNITY_2020_1_OR_NEWER
                inputModule.forceModuleActive = true;
#endif
#endif
            }
        }

        public static int GetAnimatorNameHash(AnimatorStateInfo animatorStateInfo)
        {
#if UNITY_4_3 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7
			return animatorStateInfo.nameHash;
#else
            return animatorStateInfo.fullPathHash;
#endif
        }

        /// <summary>
        /// Selects a Selectable UI element and visually shows it as selected.
        /// </summary>
        /// <param name="selectable"></param>
        /// <param name="allowStealFocus"></param>
        public static void Select(UnityEngine.UI.Selectable selectable, bool allowStealFocus = true)
        {
            var currentEventSystem = UnityEngine.EventSystems.EventSystem.current;
            if (currentEventSystem == null || selectable == null) return;
            if (currentEventSystem.alreadySelecting) return;
            if (currentEventSystem.currentSelectedGameObject == null || allowStealFocus)
            {
                currentEventSystem.SetSelectedGameObject(selectable.gameObject);
                selectable.Select();
                selectable.OnSelect(null);
            }
        }

        public static Font GetDefaultFont()
        {
            var majorVersion = SafeConvert.ToInt(Application.unityVersion.Split('.')[0]);
            var fontName = (majorVersion >= 2022) ? "LegacyRuntime.ttf" : "Arial.ttf";
            return Resources.GetBuiltinResource<Font>(fontName);
        }

    }

}
