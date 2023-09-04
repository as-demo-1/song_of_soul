// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.DialogueSystem
{

    [AddComponentMenu("")] // Use wrapper.
    public class BringSubtitlePanelToFrontOnFocus : MonoBehaviour
    {

        private StandardUISubtitlePanel subtitlePanel;

        private void Awake()
        {
            subtitlePanel = GetComponent<StandardUISubtitlePanel>();
        }

        private void Start()
        {
            subtitlePanel.onFocus.AddListener(BringPortraitToFront);
        }

        private void BringPortraitToFront()
        {
            subtitlePanel.transform.SetAsLastSibling();
        }
    }
}
