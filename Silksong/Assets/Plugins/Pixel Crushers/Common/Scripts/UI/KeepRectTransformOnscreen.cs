// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers
{

    /// <summary>
    /// Keeps a RectTransform's bounds in view of the main camera. 
    /// Works best on world space panels.
    /// </summary>
    [AddComponentMenu("")] // Use wrapper.
    public class KeepRectTransformOnscreen : MonoBehaviour
    {
        private Vector3 originalLocalPosition;

        private void Awake()
        {
            originalLocalPosition = transform.localPosition;
        }

        private void LateUpdate()
        {
            var mainCamera = Camera.main;
            if (mainCamera == null) return;
            transform.localPosition = originalLocalPosition;
            var pos = mainCamera.WorldToViewportPoint(transform.position);
            pos.x = Mathf.Clamp01(pos.x);
            pos.y = Mathf.Clamp01(pos.y);
            transform.position = mainCamera.ViewportToWorldPoint(pos);
        }

    }
}