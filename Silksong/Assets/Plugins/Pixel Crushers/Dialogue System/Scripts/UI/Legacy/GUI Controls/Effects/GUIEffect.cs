using UnityEngine;
using System.Collections;

namespace PixelCrushers.DialogueSystem.UnityGUI
{

    /// <summary>
    /// The base abstract class for GUI effect "decorators" that can be added to GUI controls.
    /// These effects change the way GUI controls are displayed.
    /// </summary>
    public abstract class GUIEffect : MonoBehaviour
    {

        /// <summary>
        /// The trigger that makes the effect play.
        /// </summary>
        public GUIEffectTrigger trigger = GUIEffectTrigger.OnEnable;

        /// <summary>
        /// Plays the effect. Each effect overrides this abstract method.
        /// </summary>
        public abstract IEnumerator Play();

        public virtual void Stop()
        {
            StopAllCoroutines();
        }

        public void OnEnable()
        {
            if (enabled && (trigger == GUIEffectTrigger.OnEnable))
            {
                StartCoroutine(Play());
            }
        }

        public void OnDisable()
        {
            Stop();
        }

    }

}
