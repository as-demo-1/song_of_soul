// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers
{

    /// <summary>
    /// Marks the GameObject as DontDestroyOnLoad.
    /// </summary>
    [AddComponentMenu("")] // Use wrapper instead.
    public class DontDestroyGameObject : MonoBehaviour
    {

        private void Awake()
        {
#if UNITY_EDITOR && UNITY_2019_3_OR_NEWER
            if (Application.isPlaying)
            {
                UnityEditor.SceneVisibilityManager.instance.Show(gameObject, false);
            }
#endif
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }

    }

}
