using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem.UnityGUI
{

    /// <summary>
    /// Static utility class for Unity GUI Dialogue UI.
    /// </summary>
    [System.Serializable]
    public static class UnityDialogueUIControls
    {

        /// <summary>
        /// Sets a Unity GUI control active.
        /// </summary>
        /// <param name='control'>
        /// Control to set.
        /// </param>
        /// <param name='value'>
        /// <c>true</c> for active; <c>false</c> for inactive.
        /// </param>
        public static void SetControlActive(GUIControl control, bool value)
        {
            if (control != null)
            {
                if ((value == true) && !control.gameObject.activeSelf)
                {
                    control.gameObject.SetActive(true);
                    CheckSlideEffect(control);
                }
                else if ((value == false) && control.gameObject.activeSelf)
                {
                    control.gameObject.SetActive(false);
                }
            }
        }

        private static void CheckSlideEffect(GUIControl control)
        {
            SlideEffect slideEffect = control.GetComponent<SlideEffect>();
            if (slideEffect != null)
            {
                if (slideEffect.trigger == GUIEffectTrigger.OnEnable)
                {
                    control.visible = false;
                }
            }
        }

    }

}
