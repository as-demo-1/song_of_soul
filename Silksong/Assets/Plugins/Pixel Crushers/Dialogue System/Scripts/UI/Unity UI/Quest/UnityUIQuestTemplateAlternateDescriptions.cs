// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.DialogueSystem
{

    [System.Serializable]
    public class UnityUIQuestTemplateAlternateDescriptions
    {

        [Tooltip("(Optional) If set, use if state is success")]
        public UnityEngine.UI.Text successDescription;

        [Tooltip("(Optional) If set, use if state is failure")]
        public UnityEngine.UI.Text failureDescription;

        public void SetActive(bool value)
        {
            if (successDescription != null) successDescription.gameObject.SetActive(value);
            if (failureDescription != null) failureDescription.gameObject.SetActive(value);
        }

    }

}
