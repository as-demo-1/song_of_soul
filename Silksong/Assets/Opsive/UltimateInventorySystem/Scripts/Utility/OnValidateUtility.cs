/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Utility
{
    using UnityEngine;
#if UNITY_EDITOR
    using UnityEditor;
#endif

    public static class OnValidateUtility
    {
        /// <summary>
        /// Checks if an object is a prefab.
        /// </summary>
        /// <param name="obj">The object to check.</param>
        /// <returns>True if it is a prefab.</returns>
        public static bool IsPrefab(MonoBehaviour obj)
        {
            return IsPrefab(obj.gameObject);
        }

        /// <summary>
        /// Checks if an object is a prefab.
        /// </summary>
        /// <param name="obj">The object to check.</param>
        /// <returns>True if it is a prefab.</returns>
        public static bool IsPrefab(GameObject obj)
        {
            if (obj.gameObject.scene.name == null) { return true; }
#if UNITY_EDITOR
            if (Application.isPlaying) { return false; }
            if (obj.activeInHierarchy == false) { return true; }
            if (obj.transform.root.gameObject.activeInHierarchy == false) { return true; }

            var path = UnityEditor.AssetDatabase.GetAssetPath(obj);
            if (!string.IsNullOrEmpty(path)) { return true; }

            if (Selection.assetGUIDs.Length > 0) { return true; }
#endif
            return false;
        }
    }
}
