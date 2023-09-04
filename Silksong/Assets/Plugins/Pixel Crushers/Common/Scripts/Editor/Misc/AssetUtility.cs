// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;
using System.IO;

namespace PixelCrushers
{

    public static class AssetUtility
    {

        /// <summary>
        /// Creates a new ScriptableObject asset in the project's asset database.
        /// </summary>
        /// <typeparam name="T">The ScriptableObject type.</typeparam>
        /// <param name="typeName">The type name to use when naming the asset file in the project's asset database.</param>
        /// <param name="select">If true, select the asset.</param>
        /// <param name="prependNew">If true, prepend 'New' to the beginning of the asset name.</param>
        /// <returns>The asset.</returns>
        public static T CreateAsset<T>(string typeName, bool select = true, bool prependNew = true) where T : ScriptableObject
        {
            var asset = ScriptableObjectUtility.CreateScriptableObject<T>() as T;
            SaveAsset(asset, typeName, select, prependNew);
            return asset;
        }

        /// <summary>
        /// Creates a new ScriptableObject asset in the project's asset database.
        /// </summary>
        /// <param name="type">The ScriptableObject type.</param>
        /// <param name="typeName">The type name to use when naming the asset file in the project's asset database.</param>
        /// <param name="select">If true, select the asset.</param>
        /// <param name="prependNew">If true, prepend 'New' to the beginning of the asset name.</param>
        /// <returns>The asset.</returns>
        public static ScriptableObject CreateAsset(System.Type type, string typeName, bool select = true, bool prependNew = true)
        {
            var asset = ScriptableObjectUtility.CreateScriptableObject(type);
            SaveAsset(asset, typeName, select, prependNew);
            return asset;
        }

        /// <summary>
        /// Creates a new ScriptableObject asset in the project's asset database.
        /// </summary>
        /// <typeparam name="T">The ScriptableObject type.</typeparam>
        /// <param name="filename">The filename to save the asset as.</param>
        /// <param name="select">If true, select the asset.</param>
        /// <returns>The asset.</returns>
        public static T CreateAssetWithFilename<T>(string filename, bool select = true) where T : ScriptableObject
        {
            var asset = ScriptableObjectUtility.CreateScriptableObject<T>() as T;
            SaveAssetWithFilename(asset, filename, select);
            return asset;
        }

        /// <summary>
        /// Creates a new ScriptableObject asset in the project's asset database.
        /// </summary>
        /// <param name="type">The ScriptableObject type.</param>
        /// <param name="filename">The filename to save the asset as.</param>
        /// <param name="select">If true, select the asset.</param>
        /// <returns>The asset.</returns>
        public static ScriptableObject CreateAssetWithFilename(System.Type type, string filename, bool select = true)
        {
            var asset = ScriptableObjectUtility.CreateScriptableObject(type);
            SaveAssetWithFilename(asset, filename, select);
            return asset;
        }


        private static void SaveAsset(ScriptableObject asset, string typeName, bool select = true, bool prependNew = true)
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (string.IsNullOrEmpty(path))
            {
                path = "Assets";
            }
            else if (!string.IsNullOrEmpty(Path.GetExtension(path)))
            {
                path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
            }
            string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + (prependNew ? "/New " : "/") + typeName + ".asset");
            SaveAssetWithFilename(asset, assetPathAndName, select);
        }

        private static void SaveAssetWithFilename(ScriptableObject asset, string filename, bool select = true)
        {
            AssetDatabase.CreateAsset(asset, filename);
            //AssetDatabase.SaveAssets();
            if (select)
            {
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = asset;
            }
        }

        /// <summary>
        /// (Editor only) Registers a ScriptableObject as part of an asset in the project's asset database.
        /// </summary>
        /// <param name="scriptableObject">The ScriptableObject to add.</param>
        /// <param name="asset">The asset to add the ScriptableObject to.</param>
        public static void AddToAsset(ScriptableObject scriptableObject, UnityEngine.Object asset)
        {
            scriptableObject.hideFlags = HideFlags.HideInHierarchy;
            AssetDatabase.AddObjectToAsset(scriptableObject, asset);
            //AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(scriptableObject));
            //AssetDatabase.SaveAssets();
            //AssetDatabase.Refresh();
        }

        /// <summary>
        /// (Editor only) Deletes a ScriptableObject from an asset.
        /// </summary>
        /// <param name="scriptableObject">The ScriptableObject to add.</param>
        /// <param name="asset">The asset to delete the ScriptableObject from.</param>
        public static void DeleteFromAsset(ScriptableObject scriptableObject, UnityEngine.Object asset)
        {
            if (scriptableObject == null) return;
            UnityEngine.Object.DestroyImmediate(scriptableObject, true);
            //AssetDatabase.SaveAssets();
            //AssetDatabase.Refresh();
        }

    }

}