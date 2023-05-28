// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using System.Collections;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// Deprecated by DialogueSystemTrigger.
    /// Stops a conversation if the distance between the participants exceeds a specified maximum.
    /// </summary>
    [AddComponentMenu("")] // Deprecated
    public class StopConversationIfTooFar : MonoBehaviour
    {

        /// <summary>
        /// The max distance to allow the continue to continue.
        /// </summary>
        public float maxDistance = 5f;

        /// <summary>
        /// The frequency at which to check distance.
        /// </summary>
        public float monitorFrequency = 1f;

        void OnConversationStart(Transform actor)
        {
            StopAllCoroutines();
            StartCoroutine(MonitorDistance(actor));
        }

        void OnConversationEnd(Transform actor)
        {
            StopAllCoroutines();
        }

        void OnDisable()
        {
            StopAllCoroutines();
        }

        private IEnumerator MonitorDistance(Transform actor)
        {
            if (actor != null)
            {
                Transform myTransform = transform;
                while (true)
                {
                    yield return StartCoroutine(DialogueTime.WaitForSeconds(monitorFrequency)); // new WaitForSeconds(monitorFrequency);
                    if (Vector3.Distance(myTransform.position, actor.position) > maxDistance)
                    {
                        if (DialogueDebug.logInfo) Debug.Log(string.Format("{0}: Stopping conversation. Exceeded max distance {1} between {2} and {3}", new System.Object[] { DialogueDebug.Prefix, maxDistance, name, actor.name }));
                        DialogueManager.StopConversation();
                        yield break;
                    }
                }
            }
        }

    }

}
