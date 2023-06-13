// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Linq;

namespace PixelCrushers.DialogueSystem
{

    public class DialogueSystem1xTo2xUpdater
    {

        private const string DialogTitle = "Update to Dialogue System 2.x";

        [MenuItem("Tools/Pixel Crushers/Dialogue System/Tools/Run 1x to 2x Updater...")]
        private static void RunUpdater()
        {

#if UNITY_5
            // In Unity 5, serialization must already be set to Force Text:
            if (EditorSettings.serializationMode != SerializationMode.ForceText)
            {
                if (!EditorUtility.DisplayDialog("Can't Update", "In Unity 5, you must set serialization to Force Text *before* importing " +
                    "version 2.x. Please close this project and restore an earlier version that contains the Dialogue System for Unity 1.x. " +
                    "Then switch serialization to Force Text before deleting the Dialogue System folder and importing version 2.x.", "OK", "More Info"))
                {
                    Application.OpenURL("http://www.pixelcrushers.com/dialogue_system/manual2x/html/getting_started.html#upgradingFrom1x");
                }
                Debug.LogError("Dialogue System: Unity 5.x project's serialization mode is not Force Text. More info: http://www.pixelcrushers.com/dialogue_system/manual2x/html/getting_started.html");
                return;
            }
#endif

            var option = EditorUtility.DisplayDialogComplex(DialogTitle, "BACK UP YOUR PROJECT FIRST! If you haven't backed up your project yet, click Cancel.\n\n" +
            "This utility will update your scenes, prefabs, and dialogue databases from version 1.x to 2.x.\n\n" +
            "Important: If your project contains assets (including third party assets) that were binary serialized in Unity 4, click More Info.\n\n" +
            "Update now? This may take a while.", "Update", "Cancel", "More Info");
            switch (option)
            {
                case 0:
                    if (EditorUtility.DisplayDialog("Confirm Update Process", "Have you backed up your project?\n\n" +
                        "If your project contains assets that were binary serialized in Unity 4, they need to be fixed first. " +
                        "(If you have questions about this, cancel and click More Info.)", "Yes, I Have a Backup", "Cancel"))
                    {
                        UpdateNow();
                    }
                    break;
                case 1: // Cancel
                    break;
                case 2:
                    if (EditorSettings.serializationMode == SerializationMode.ForceText)
                    {
                        EditorUtility.DisplayDialog("Important: Unity 4 32-Bit Binary Assets", "Your project already uses text serialization. If you were using binary or " +
                            "mixed serialization, the updater would have to temporarily convert assets to text serialization to " +
                            "update Dialogue System script references. Assets that were binary serialized in Unity 4 can't be converted to " +
                            "text serialization. (This isn't specific to the Dialogue System; it's a Unity 32-bit vs 64-bit " +
                            "restriction.) You would have to open them in Unity 4, change to text serialization, export the " +
                            "assets, and import them into this project before running the updater. Since you're in Force Text mode already, you shouldn't " +
                            "have to do this.", "OK");
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Important: Unity 4 32-Bit Binary Assets", "This updater must temporarily convert assets to text serialization to " +
                            "update Dialogue System script references. If your project contains assets that were binary serialized in Unity 4, " +
                            "Unity can't convert them to text serialization. (This isn't specific to the Dialogue System; it's a Unity 32-bit vs 64-bit " +
                            "restriction.) You MUST open them in Unity 4, change to text serialization, export the " +
                            "assets, and import them into this project before running the updater.", "OK");
                    }
                    break;
            }
        }

        private const string ExternalVersionControlVisibleMetaFiles = "Visible Meta Files";

        private static string versionControlMode
        {
            get
            {
#if UNITY_2020_1_OR_NEWER
                return VersionControlSettings.mode;
#else
                return EditorSettings.externalVersionControl;
#endif
            }
            set
            {
#if UNITY_2020_1_OR_NEWER
                VersionControlSettings.mode = value;
#else
                EditorSettings.externalVersionControl = value;
#endif
            }
        }



        private static void UpdateNow()
        {
            if (!SaveCurrentScenes())
            {
                Debug.Log("Dialogue System: Update process cancelled.");
                return;
            }
            AssetDatabase.SaveAssets();

            var cancel = false;

            // Deselect database or localized text table:
            if (Selection.activeObject is DialogueDatabase || Selection.activeObject is LocalizedTextTable)
            {
                Selection.activeObject = null;
            }

            // Close Dialogue Editor:
            if (DialogueEditor.DialogueEditorWindow.instance != null)
            {
                DialogueEditor.DialogueEditorWindow.instance.Close();
            }

            // Record serialization mode and force text:
            var originalSerializationMode = EditorSettings.serializationMode;
            if (EditorSettings.serializationMode != SerializationMode.ForceText)
            {
                cancel = EditorUtility.DisplayCancelableProgressBar(DialogTitle, "Switching to Force Text serialization mode. This may take a while.", 0);
                Debug.Log("Dialogue System: Set serialization mode to Force Text.");
                EditorSettings.serializationMode = SerializationMode.ForceText;
                if (cancel)
                {
                    Debug.Log("Dialogue System: Update process cancelled. Serialization mode is set to Force Text. To set it back to " + originalSerializationMode + " use menu item Edit > Project Settings > Editor.");
                    return;
                }
            }

            // Visible meta files:
            var originalExternalVersionControl = versionControlMode;
            if (!versionControlMode.Equals(ExternalVersionControlVisibleMetaFiles))
            {
                cancel = EditorUtility.DisplayCancelableProgressBar(DialogTitle, "Switching Version Control Mode to Visible Meta Files. This may take a while.", 0);
                Debug.Log("Dialogue System: Set version control mode to Visible Meta Files.");
                versionControlMode = ExternalVersionControlVisibleMetaFiles;
                if (cancel)
                {
                    Debug.Log("Dialogue System: Update process cancelled. Serialization mode is " + EditorSettings.serializationMode + " and Version Control Mode is " +
                        versionControlMode + ". To change these settings, use menu item Edit > Project Settings > Editor.");
                    return;
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            try
            {
                // Update all scene, scriptable object, and prefab files:
                var files = Directory.GetFiles(Application.dataPath, "*.*", SearchOption.AllDirectories)
                    .Where(s => s.EndsWith(".unity") || s.EndsWith(".asset") || s.EndsWith(".prefab")).ToArray();
                for (int i = 0; i < files.Length; i++)
                {
                    var filename = files[i];
                    var progress = i / (float)files.Length;
                    cancel = EditorUtility.DisplayCancelableProgressBar(DialogTitle, filename, progress);
                    UpdateFile(filename);
                    if (cancel)
                    {
                        if (EditorSettings.serializationMode != originalSerializationMode)
                        {
                            Debug.Log("Dialogue System: Update process cancelled. Serialization mode is set to Force Text. To set it back to " + originalSerializationMode + " use menu item Edit > Project Settings > Editor.");
                        }
                        return;
                    }
                }
            }
            catch (UnauthorizedAccessException e)
            {
                Debug.LogException(e);
            }
            catch (PathTooLongException e)
            {
                Debug.LogException(e);
            }
            finally
            {
                AssetDatabase.Refresh();

                if (!cancel)
                {
                    // Restore original serialization mode:
                    if (originalSerializationMode != SerializationMode.ForceText)
                    {
                        EditorUtility.DisplayProgressBar(DialogTitle, "Restoring original serialization mode. This may take a while.", 0.99f);
                        Debug.Log("Dialogue System: Set serialization mode to " + originalSerializationMode + ".");
                        EditorSettings.serializationMode = originalSerializationMode;
                    }
                    // Restore original version control mode:
                    if (originalExternalVersionControl != ExternalVersionControlVisibleMetaFiles)
                    {
                        EditorUtility.DisplayProgressBar(DialogTitle, "Restoring original version control mode. This may take a while.", 0.995f);
                        Debug.Log("Dialogue System: Set version control mode to " + originalExternalVersionControl + ".");
                        versionControlMode = originalExternalVersionControl;
                    }
                    EditorUtility.ClearProgressBar();
                    EditorUtility.DisplayDialog(DialogTitle, "Update complete. Please check your project thoroughly. If you notice any issues or have any questions, please contact us at support@pixelcrushers.com.", "OK");
                }
            }
        }

        private static bool SaveCurrentScenes()
        {
#if UNITY_4_6 || UNITY_4_7 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2
        return EditorApplication.SaveCurrentSceneIfUserWantsTo();
#else
            return UnityEditor.SceneManagement.EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
#endif
        }

        private static void UpdateFile(string filename)
        {
            if (string.IsNullOrEmpty(filename)) return;
            var updated = false;
            try
            {
                var text = File.ReadAllText(filename);
                for (int i = 0; i < pairs.Length; i++)
                {
                    var pair = pairs[i];
                    if (text.Contains(pair.oldID))
                    {
                        text = text.Replace(pair.oldID, pair.newID);
                        updated = true;
                    }
                }
                if (updated)
                {
                    File.WriteAllText(filename, text);
                    Debug.Log("Updated " + filename);
                }
            }
            catch (System.Exception e)
            {
                if (filename.Contains("NavMesh"))
                {
                    Debug.Log("Dialogue System: Updater did not process " + filename + " but it appears to be a NavMesh so this should be fine. Result: " + e.Message);
                }
                else if (filename.Contains("Terrain"))
                {
                    Debug.Log("Dialogue System: Updater did not process " + filename + " but it appears to be a Terrain so this should be fine. Result: " + e.Message);
                }
                else
                {
                    Debug.LogError("Dialogue System: Updater was unable to process " + filename + ": " + e.Message);
                }
            }
        }

        private class Pair
        {
            public string oldID; // DLL
            public string newID; // wrapper
            public Pair(string oldID, string newID)
            {
                this.oldID = oldID;
                this.newID = newID;
            }
        }

        private static Pair[] pairs = new Pair[] {
new Pair("m_Script: {fileID: -1936938464, guid: b5a7519e94754fb4a8ea0c272031db78, type: 3}","m_Script: {fileID: 11500000, guid: a859f480e4e0a8e4694ab73b436d975e, type: 3}"),
new Pair("m_Script: {fileID: 1204389749, guid: b5a7519e94754fb4a8ea0c272031db78, type: 3}","m_Script: {fileID: 11500000, guid: aacc795d66f43c840aa8537e6be04463, type: 3}"),
new Pair("m_Script: {fileID: -1604753293, guid: b5a7519e94754fb4a8ea0c272031db78, type: 3}","m_Script: {fileID: 11500000, guid: e6826db3b75174641a9b58531b63e97c, type: 3}"),
new Pair("m_Script: {fileID: -1509285629, guid: b5a7519e94754fb4a8ea0c272031db78, type: 3}","m_Script: {fileID: 11500000, guid: 1402baf97767d2c48b5d84c6d3fce2b7, type: 3}"),
new Pair("m_Script: {fileID: -2037260565, guid: b5a7519e94754fb4a8ea0c272031db78, type: 3}","m_Script: {fileID: 11500000, guid: ae33fffd0c0859d459ed90d1b2e8f607, type: 3}"),
new Pair("m_Script: {fileID: 653806763, guid: b5a7519e94754fb4a8ea0c272031db78, type: 3}","m_Script: {fileID: 11500000, guid: 6261a88664dbeef4b80de76a1271dba8, type: 3}"),
new Pair("m_Script: {fileID: -870492868, guid: b5a7519e94754fb4a8ea0c272031db78, type: 3}","m_Script: {fileID: 11500000, guid: 505dcd4c4aa3a1741ab98b9ad39eb7a5, type: 3}"),
new Pair("m_Script: {fileID: 1643293417, guid: b5a7519e94754fb4a8ea0c272031db78, type: 3}","m_Script: {fileID: 11500000, guid: 852c4a3d23d220e49a537c981b83c44c, type: 3}"),
new Pair("m_Script: {fileID: 158080175, guid: b5a7519e94754fb4a8ea0c272031db78, type: 3}","m_Script: {fileID: 11500000, guid: 9cedf348ece7ef64aa74cb295e8ed71c, type: 3}"),
new Pair("m_Script: {fileID: -797819540, guid: b5a7519e94754fb4a8ea0c272031db78, type: 3}","m_Script: {fileID: 11500000, guid: 9789ac40970ea9346ae2ce6497ffee0f, type: 3}"),
new Pair("m_Script: {fileID: 567319595, guid: b5a7519e94754fb4a8ea0c272031db78, type: 3}","m_Script: {fileID: 11500000, guid: 6e2c2a6631002944d9b39d43412301e1, type: 3}"),
new Pair("m_Script: {fileID: -434793028, guid: b5a7519e94754fb4a8ea0c272031db78, type: 3}","m_Script: {fileID: 11500000, guid: 8875f090515668948a570c7325694ba3, type: 3}"),
new Pair("m_Script: {fileID: -1097633651, guid: b5a7519e94754fb4a8ea0c272031db78, type: 3}","m_Script: {fileID: 11500000, guid: 935899b62f48ae5498594680ed17d133, type: 3}"),
new Pair("m_Script: {fileID: -1229025713, guid: b5a7519e94754fb4a8ea0c272031db78, type: 3}","m_Script: {fileID: 11500000, guid: ab8b685e62a9aeb4a9472b30ec2d86d9, type: 3}"),
new Pair("m_Script: {fileID: 1185165055, guid: b5a7519e94754fb4a8ea0c272031db78, type: 3}","m_Script: {fileID: 11500000, guid: c593457cd8105e148906690e1707c592, type: 3}"),
new Pair("m_Script: {fileID: -1430733816, guid: b5a7519e94754fb4a8ea0c272031db78, type: 3}","m_Script: {fileID: 11500000, guid: bf7b91960d98f1b44ba6805653d184bd, type: 3}"),
new Pair("m_Script: {fileID: -642255930, guid: b5a7519e94754fb4a8ea0c272031db78, type: 3}","m_Script: {fileID: 11500000, guid: 7cdb60899b9a6114e82657d5b679c74a, type: 3}"),
new Pair("m_Script: {fileID: 2143113903, guid: b5a7519e94754fb4a8ea0c272031db78, type: 3}","m_Script: {fileID: 11500000, guid: dc147552835385f4eab7d97b4c8aaca9, type: 3}"),
new Pair("m_Script: {fileID: -125749421, guid: b5a7519e94754fb4a8ea0c272031db78, type: 3}","m_Script: {fileID: 11500000, guid: dbddc4cefd1de8e43ad762b2798cda14, type: 3}"),
new Pair("m_Script: {fileID: -697685688, guid: b5a7519e94754fb4a8ea0c272031db78, type: 3}","m_Script: {fileID: 11500000, guid: 86a40e129f2cbc840aa07eeabd0f6056, type: 3}"),
new Pair("m_Script: {fileID: 1350695639, guid: b5a7519e94754fb4a8ea0c272031db78, type: 3}","m_Script: {fileID: 11500000, guid: ea12e7461b242604388f78eb4b2782ef, type: 3}"),
new Pair("m_Script: {fileID: 1287875049, guid: b5a7519e94754fb4a8ea0c272031db78, type: 3}","m_Script: {fileID: 11500000, guid: 06fc588754dcd504e899d843da4bb661, type: 3}"),
new Pair("m_Script: {fileID: 823590142, guid: b5a7519e94754fb4a8ea0c272031db78, type: 3}","m_Script: {fileID: 11500000, guid: 58b59ad33059c2d4798d54acbc32130e, type: 3}"),
new Pair("m_Script: {fileID: -808425172, guid: b5a7519e94754fb4a8ea0c272031db78, type: 3}","m_Script: {fileID: 11500000, guid: 79b7f4b23d5fee541951feb5a0ce6030, type: 3}"),
new Pair("m_Script: {fileID: 1679897950, guid: b5a7519e94754fb4a8ea0c272031db78, type: 3}","m_Script: {fileID: 11500000, guid: c85cea89a1f0313409bdd92c7f74040d, type: 3}"),
new Pair("m_Script: {fileID: 1083355078, guid: b5a7519e94754fb4a8ea0c272031db78, type: 3}","m_Script: {fileID: 11500000, guid: 61741e9f916d0e340ac16e38117976d6, type: 3}"),
new Pair("m_Script: {fileID: 1287228182, guid: b5a7519e94754fb4a8ea0c272031db78, type: 3}","m_Script: {fileID: 11500000, guid: f063320ae3340f746b0a7d1f221b6702, type: 3}"),
new Pair("m_Script: {fileID: -889199951, guid: b5a7519e94754fb4a8ea0c272031db78, type: 3}","m_Script: {fileID: 11500000, guid: 75b03f00b69c5e144976cb1b7f9800c0, type: 3}"),
new Pair("m_Script: {fileID: -508741968, guid: b5a7519e94754fb4a8ea0c272031db78, type: 3}","m_Script: {fileID: 11500000, guid: 64318a909173d4647b185b48389651a0, type: 3}"),
new Pair("m_Script: {fileID: 1626302282, guid: b5a7519e94754fb4a8ea0c272031db78, type: 3}","m_Script: {fileID: 11500000, guid: 4d459c24cb34074479901880fe97a2c4, type: 3}"),
new Pair("m_Script: {fileID: 264566500, guid: b5a7519e94754fb4a8ea0c272031db78, type: 3}","m_Script: {fileID: 11500000, guid: 372087ff1dba1c94ab4217d10cbb462f, type: 3}"),
new Pair("m_Script: {fileID: -360346005, guid: b5a7519e94754fb4a8ea0c272031db78, type: 3}","m_Script: {fileID: 11500000, guid: 66a4432e5474e1d45b552b6be0376c93, type: 3}"),
new Pair("m_Script: {fileID: 315642041, guid: b5a7519e94754fb4a8ea0c272031db78, type: 3}","m_Script: {fileID: 11500000, guid: 81fbe85055c4afd4294183a6fa137aff, type: 3}"),
new Pair("m_Script: {fileID: 1810267305, guid: b5a7519e94754fb4a8ea0c272031db78, type: 3}","m_Script: {fileID: 11500000, guid: a64578247383140469c45370654a984a, type: 3}"),
new Pair("m_Script: {fileID: 745944170, guid: b5a7519e94754fb4a8ea0c272031db78, type: 3}","m_Script: {fileID: 11500000, guid: 205c6d4e7606f8d40b5d2763ea70a42d, type: 3}"),
new Pair("m_Script: {fileID: -1725351101, guid: b5a7519e94754fb4a8ea0c272031db78, type: 3}","m_Script: {fileID: 11500000, guid: f672f349a96246545a95dbb941ad0f44, type: 3}"),
new Pair("m_Script: {fileID: 101716540, guid: b5a7519e94754fb4a8ea0c272031db78, type: 3}","m_Script: {fileID: 11500000, guid: e3c5c9d31be71b548ac8cae1c6fd515c, type: 3}"),
new Pair("m_Script: {fileID: 1005983785, guid: b5a7519e94754fb4a8ea0c272031db78, type: 3}","m_Script: {fileID: 11500000, guid: 42c84023fc44c364faaf12f00d75e52f, type: 3}"),
new Pair("m_Script: {fileID: -1341398357, guid: b5a7519e94754fb4a8ea0c272031db78, type: 3}","m_Script: {fileID: 11500000, guid: a4f5dab17b4162641940c1ad926ff72d, type: 3}"),
new Pair("m_Script: {fileID: 399994209, guid: b5a7519e94754fb4a8ea0c272031db78, type: 3}","m_Script: {fileID: 11500000, guid: e1213303bc5604940adfcf2245a4d459, type: 3}"),
new Pair("m_Script: {fileID: 1298482683, guid: b5a7519e94754fb4a8ea0c272031db78, type: 3}","m_Script: {fileID: 11500000, guid: 3bd27235dca69114baf4bc6283f3adaf, type: 3}"),
new Pair("m_Script: {fileID: -927678970, guid: b5a7519e94754fb4a8ea0c272031db78, type: 3}","m_Script: {fileID: 11500000, guid: 0a9990037fcb2384ba5469515c75575d, type: 3}"),
new Pair("m_Script: {fileID: -1948536447, guid: b5a7519e94754fb4a8ea0c272031db78, type: 3}","m_Script: {fileID: 11500000, guid: ad8206dbaf03f8c4786b7b786a72241b, type: 3}"),
new Pair("m_Script: {fileID: -1901790233, guid: b5a7519e94754fb4a8ea0c272031db78, type: 3}","m_Script: {fileID: 11500000, guid: d5d04d2c12ab63342ad50ca09235a6bf, type: 3}"),
new Pair("m_Script: {fileID: 100165095, guid: b5a7519e94754fb4a8ea0c272031db78, type: 3}","m_Script: {fileID: 11500000, guid: 18f0e7209ff09cc468f7426401cb773c, type: 3}"),
new Pair("m_Script: {fileID: 1195045865, guid: b5a7519e94754fb4a8ea0c272031db78, type: 3}","m_Script: {fileID: 11500000, guid: d98081209270c3f408f75925325a7d48, type: 3}"),
new Pair("m_Script: {fileID: -479791062, guid: b5a7519e94754fb4a8ea0c272031db78, type: 3}","m_Script: {fileID: 11500000, guid: 8d351540ff325e24aaeaa3fe828c860b, type: 3}"),
new Pair("m_Script: {fileID: 1866253743, guid: b5a7519e94754fb4a8ea0c272031db78, type: 3}","m_Script: {fileID: 11500000, guid: 55b87f2cb08c9364eb07bfd5deb7414a, type: 3}"),
new Pair("m_Script: {fileID: -855550762, guid: b5a7519e94754fb4a8ea0c272031db78, type: 3}","m_Script: {fileID: 11500000, guid: eaf6a3843af869e46a0010cf44652f35, type: 3}"),
new Pair("m_Script: {fileID: -713665266, guid: b5a7519e94754fb4a8ea0c272031db78, type: 3}","m_Script: {fileID: 11500000, guid: 981993f01c5b0db4499969008e1531aa, type: 3}"),
new Pair("m_Script: {fileID: 456116103, guid: b5a7519e94754fb4a8ea0c272031db78, type: 3}","m_Script: {fileID: 11500000, guid: 26a8546376653094b81a0700a82c3f23, type: 3}"),
new Pair("m_Script: {fileID: 1008128844, guid: b5a7519e94754fb4a8ea0c272031db78, type: 3}","m_Script: {fileID: 11500000, guid: 8896a90a27f1c434f92b67306392521c, type: 3}"),
new Pair("m_Script: {fileID: -1299841724, guid: b5a7519e94754fb4a8ea0c272031db78, type: 3}","m_Script: {fileID: 11500000, guid: bb23565486e11104eac22e26078a9018, type: 3}"),
new Pair("m_Script: {fileID: 474770273, guid: b5a7519e94754fb4a8ea0c272031db78, type: 3}","m_Script: {fileID: 11500000, guid: b71bc7613034af34cab00b59795431c1, type: 3}"),
new Pair("m_Script: {fileID: 1413299683, guid: b5a7519e94754fb4a8ea0c272031db78, type: 3}","m_Script: {fileID: 11500000, guid: 730b534d43b6a5b4d8e0843e523c845e, type: 3}"),
new Pair("m_Script: {fileID: 2108538427, guid: b5a7519e94754fb4a8ea0c272031db78, type: 3}","m_Script: {fileID: 11500000, guid: f7edf2ddcb4ebb7418f5ceac3f460246, type: 3}"),
new Pair("m_Script: {fileID: 2103386216, guid: b5a7519e94754fb4a8ea0c272031db78, type: 3}","m_Script: {fileID: 11500000, guid: 08db02e09c70dae41a7f14f942444140, type: 3}"),
new Pair("m_Script: {fileID: 153521277, guid: b5a7519e94754fb4a8ea0c272031db78, type: 3}","m_Script: {fileID: 11500000, guid: 85d7493b5ec1a4a489ede41c8064d254, type: 3}"),
new Pair("m_Script: {fileID: -1553868166, guid: b5a7519e94754fb4a8ea0c272031db78, type: 3}","m_Script: {fileID: 11500000, guid: 82108a54a0154e54391e877dad198cf3, type: 3}"),
new Pair("m_Script: {fileID: 739958932, guid: b5a7519e94754fb4a8ea0c272031db78, type: 3}", "m_Script: {fileID: 11500000, guid: 55b94953a9871c543bb857d69986bb28, type: 3}"),
new Pair("m_Script: {fileID: 1906560132, guid: b5a7519e94754fb4a8ea0c272031db78, type: 3}", "m_Script: {fileID: 11500000, guid: 57491ef32a12c6c4a91e9e9d1df87eb9, type: 3}"),
new Pair("m_Script: {fileID: -1280735392, guid: b5a7519e94754fb4a8ea0c272031db78, type: 3}","m_Script: {fileID: 11500000, guid: 770763badfe5e144fb5374a7dcab2789, type: 3}")
};

    }
}