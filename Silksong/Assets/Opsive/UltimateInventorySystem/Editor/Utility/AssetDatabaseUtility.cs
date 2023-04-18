/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.Utility
{
    using Opsive.Shared.Editor;
    using System.Text.RegularExpressions;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Utility functions for interacting with the Unity Editor Asset Database.
    /// </summary>
    public static class AssetDatabaseUtility
    {

        /// <summary>
        /// Creates the required folders to the path.
        /// </summary>
        /// <param name="path">The path that the folders should be created from.</param>
        /// <returns>True if the folders were created successfully.</returns>
        public static string GetPathForNewDatabase(string path, out string folderPath)
        {
            var lastSlashIndex = path.LastIndexOf("/");
            folderPath = path.Substring(0, lastSlashIndex);

            var databaseNameAndExtension = path.Substring(lastSlashIndex + 1);
            var databaseName = databaseNameAndExtension.Substring(0, databaseNameAndExtension.IndexOf("."));

            path = folderPath + "/" + databaseName + "/" + databaseNameAndExtension;

            path = ValidatePath(path);

            lastSlashIndex = path.LastIndexOf("\\");
            folderPath = path.Substring(0, lastSlashIndex);

            return path;
        }

        /// <summary>
        /// Creates a new asset within the project.
        /// </summary>
        /// <param name="newAsset">The asset that should be created.</param>
        /// <param name="pathName">The path that the file should be created at.</param>
        /// <param name="labels">The labels to set on the asset once created.</param>
        /// <returns>True if the asset was created successfully.</returns>
        public static bool CreateAsset(ScriptableObject newAsset, string pathName, string[] labels)
        {
            if (newAsset == null) {
                return false;
            }

            pathName = pathName.Replace("/", "\\");

            var folderPath = pathName.Substring(0, pathName.LastIndexOf("\\"));
            if (CreateFoldersToPath(folderPath) == false) {
                Debug.LogError($"Could not create path to folder {pathName}.");
                return false;
            }

            var assetPath = $"{pathName}.asset";
            var existingAsset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath);
            if (existingAsset != null) {
                DeleteAsset(existingAsset);
            }

            assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);
            AssetDatabase.CreateAsset(newAsset, assetPath);

            if (labels != null) { AssetDatabase.SetLabels(newAsset, labels); }

            Shared.Editor.Utility.EditorUtility.SetDirty(newAsset);
            return true;
        }

        /// <summary>
        /// Creates the required folders to the path.
        /// </summary>
        /// <param name="path">The path that the folders should be created from.</param>
        /// <returns>True if the folders were created successfully.</returns>
        public static bool CreateFoldersToPath(string path)
        {

            var index = path.LastIndexOf('\\');
            if (index <= 0) {
                if (path == "Assets") {
                    return true;
                }
                return false;
            }

            var startPath = path.Substring(0, index);
            var folderName = path.Substring(index + 1);

            if (!AssetDatabase.IsValidFolder(path)) {
                if (!CreateFoldersToPath(startPath)) {
                    return false;
                }
                AssetDatabase.CreateFolder(startPath, folderName);
            }

            return true;
        }

        /// <summary>
        /// Validate the path of an asset. Also creates folders if they are missing.
        /// </summary>
        /// <param name="path">The path to validate.</param>
        /// <returns>Returns a valid path.</returns>
        public static string ValidatePath(string path)
        {
            path = path.Replace("/", "\\");

            var folderPath = path.Substring(0, path.LastIndexOf("\\"));
            if (CreateFoldersToPath(folderPath) == false) {
                Debug.LogError($"Could not create path to folder {path}.");
                return null;
            }

            return path;
        }

        /// <summary>
        /// Deletes the asset from the project.
        /// </summary>
        /// <param name="asset">The asset to delete.</param>
        public static void DeleteAsset(ScriptableObject asset)
        {
            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(asset));

            Shared.Editor.Utility.EditorUtility.SetDirty(asset);
        }

        /// <summary>
        /// Returns a new name that does not match any of the existing object names.
        /// </summary>
        /// <param name="originalName">The original name.</param>
        /// <param name="existingObjs">The existing objects.</param>
        /// <returns>A valid name that does not already exist.</returns>
        public static string FindValidName(string originalName, ScriptableObject[] existingObjs)
        {
            var baseName = GetBaseName(originalName);

            var validName = baseName;
            var count = 1;
            while (true) {
                var match = false;
                for (int i = 0; i < existingObjs.Length; ++i) {
                    if (existingObjs[i].name != validName) { continue; }

                    match = true;
                    break;
                }

                if (match == false) {
                    break;
                }

                validName = baseName + "_" + count;
                count++;
            }

            return validName;
        }

        /// <summary>
        /// Returns a string name without _X extensions where X is a number.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The base name.</returns>
        public static string GetBaseName(string name)
        {
            var regex = new Regex(@"(.*)_\d*$");

            var group1 = regex.Match(name).Groups[1]?.Value;
            var baseName = string.IsNullOrWhiteSpace(group1) ? name : group1;

            return baseName;
        }
    }
}

