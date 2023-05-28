// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using System.Collections;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// Deprecated by DialogueSystemTrigger.
    /// Shows the mouse cursor during conversations. If your regular gameplay hides the mouse cursor,
    /// attach this script to your player object. When the player starts a conversation, it will
    /// show the cursor so the player can use the response menu.
    /// </summary>
    [AddComponentMenu("")] // Deprecated
    public class ShowCursorOnConversation : MonoBehaviour
    {

#if UNITY_4_3 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7

		private bool wasCursorVisible;
		private bool wasCursorLocked;

		public void OnConversationStart(Transform actor) {
			wasCursorVisible = Screen.showCursor;
			wasCursorLocked = Screen.lockCursor;
			StartCoroutine(ShowCursorAfterOneFrame());
		}
		
		private IEnumerator ShowCursorAfterOneFrame() {
			yield return null;
			Screen.showCursor = true;	
			Screen.lockCursor = false;
		}
		
		public void OnConversationEnd(Transform actor) {
			Screen.showCursor = wasCursorVisible;
			Screen.lockCursor = wasCursorLocked;
		}

#else

        private bool wasCursorVisible;
        private CursorLockMode savedLockState;

        public void OnConversationStart(Transform actor)
        {
            wasCursorVisible = Cursor.visible;
            savedLockState = Cursor.lockState;
            StartCoroutine(ShowCursorAfterOneFrame());
        }

        private IEnumerator ShowCursorAfterOneFrame()
        {
            yield return null;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        public void OnConversationEnd(Transform actor)
        {
            Cursor.visible = wasCursorVisible;
            Cursor.lockState = savedLockState;
        }

#endif

    }

}
