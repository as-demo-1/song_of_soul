// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using System;

namespace PixelCrushers.DialogueSystem
{

    [Serializable]
    public class StandardUIQuestTemplateAlternateDescriptions
    {

        [Tooltip("(Optional) If set, use if state is success.")]
        public UITextField successDescription;

        [Tooltip("(Optional) If set, use if state is failure.")]
        public UITextField failureDescription;

        public void SetActive(bool value)
        {
            successDescription.SetActive(value);
            failureDescription.SetActive(value);
        }

    }

}
