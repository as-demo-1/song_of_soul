#if USE_TIMELINE
#if UNITY_2017_1_OR_NEWER
// Copyright (c) Pixel Crushers. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#if USE_ADDRESSABLES
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
#endif
#endif

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// This MonoBehaviour is used internally by the Dialogue System's
    /// playables to show an editor representation of activity that can
    /// only be accurately viewed at runtime.
    /// </summary>
    [AddComponentMenu("")] // No menu item. Only used internally.
    [ExecuteInEditMode]
    public class PreviewUI : MonoBehaviour
    {

        private string message;
        private float endTime;
        private int lineOffset;
        private bool computedRect;
        private Rect rect;

        /// <summary>
        /// Returns a best guess of what the dialogue text will be.
        /// </summary>
        /// <param name="conversationTitle">Conversation (or bark conversation) started.</param>
        /// <param name="startingEntryID">Entry started, or -1 for beginning of conversation.</param>
        /// <param name="numContinues">Number of nodes to continue past.</param>
        /// <returns></returns>
        private static void GetDialogueEntry(string conversationTitle, int startingEntryID, int numContinues,
            out DialogueEntry entry, out bool isPlayer)
        {
            entry = null;
            isPlayer = false;
            var dialogueManager = FindObjectOfType<DialogueSystemController>();
            if (dialogueManager != null && dialogueManager.initialDatabase != null)
            {
                var database = dialogueManager.initialDatabase;
                var conversation = database.GetConversation(conversationTitle);
                if (conversation != null)
                {
                    if (startingEntryID == -1)
                    {
                        var startNode = conversation.GetFirstDialogueEntry();
                        if (startNode != null && startNode.outgoingLinks.Count > 0)
                        {
                            entry = database.GetDialogueEntry(startNode.outgoingLinks[0]);
                        }
                    }
                    else
                    {
                        entry = database.GetDialogueEntry(conversation.id, startingEntryID);
                    }
                    for (int i = 0; i < numContinues; i++)
                    {
                        if (entry == null) break;
                        if (entry.outgoingLinks.Count == 0) { entry = null; break; }
                        entry = database.GetDialogueEntry(entry.outgoingLinks[0]);
                        int safeguard = 0;
                        while (entry != null && entry.isGroup && entry.outgoingLinks.Count > 0 && safeguard++ < 9999)
                        { // Bypass group links:
                            entry = database.GetDialogueEntry(entry.outgoingLinks[0]);
                        }
                    }
                }                
            }
            isPlayer = false;
            if (entry != null && dialogueManager != null && dialogueManager.initialDatabase != null)
            {
                var actorID = entry.ActorID;
                var actor = dialogueManager.initialDatabase.actors.Find(x => x.id == actorID);
                if (actor != null && actor.IsPlayer) isPlayer = true;
            }
        }

        /// <summary>
        /// Returns a best guess of what the dialogue text will be.
        /// </summary>
        /// <param name="conversationTitle">Conversation (or bark conversation) started.</param>
        /// <param name="startingEntryID">Entry started, or -1 for beginning of conversation.</param>
        /// <param name="numContinues">Number of nodes to continue past.</param>
        /// <returns></returns>
        public static string GetDialogueText(string conversationTitle, int startingEntryID, int numContinues = 0)
        {
            DialogueEntry entry;
            bool isPlayer;
            GetDialogueEntry(conversationTitle, startingEntryID, numContinues, out entry, out isPlayer);
            return (entry != null) ? (!string.IsNullOrEmpty(entry.MenuText) ? entry.MenuText : entry.DialogueText)
                : "(determined at runtime)";
        }

        /// <summary>
        /// Returns a best guess of what the sequence will be.
        /// </summary>
        /// <param name="conversationTitle">Conversation (or bark conversation) started.</param>
        /// <param name="startingEntryID">Entry started, or -1 for beginning of conversation.</param>
        /// <param name="numContinues">Number of nodes to continue past.</param>
        /// <returns></returns>
        public static string GetSequence(string conversationTitle, int startingEntryID, out DialogueEntry entry, int numContinues = 0)
        {
            bool isPlayer;
            string sequence = string.Empty;                 
            GetDialogueEntry(conversationTitle, startingEntryID, numContinues, out entry, out isPlayer);
            if (entry == null) return string.Empty;
            if (!string.IsNullOrEmpty(entry.Sequence))
            {
                sequence = entry.Sequence;
                if (sequence.Contains("{{default}}"))
                {
                    sequence = sequence.Replace("{{default}}", GetDefaultSequence(isPlayer));
                }
            }
            else
            {
                sequence = GetDefaultSequence(isPlayer);
            }
            if (sequence.Contains("entrytaglocal"))
            {
                sequence = sequence.Replace("entrytaglocal", GetEntrytag(entry));
            }
            else if (sequence.Contains("entrytag"))
            {
                sequence = sequence.Replace("entrytag", GetEntrytag(entry));
            }
            return sequence;
        }

        private static string GetEntrytag(DialogueEntry entry)
        {
            if (entry == null) return string.Empty;
            var dialogueManager = FindObjectOfType<DialogueSystemController>();
            if (dialogueManager == null || dialogueManager.initialDatabase == null) return "entrytag";
            return dialogueManager.initialDatabase.GetEntrytag(entry.conversationID, entry.id, dialogueManager.displaySettings.cameraSettings.entrytagFormat);
        }

        private static string GetDefaultSequence(bool isPlayer)
        {
            var dialogueManager = FindObjectOfType<DialogueSystemController>();
            if (dialogueManager == null) return string.Empty;
            if (isPlayer) return dialogueManager.displaySettings.cameraSettings.defaultPlayerSequence;
            return dialogueManager.displaySettings.cameraSettings.defaultSequence;
        }

        public static float GetSequenceDuration(string conversationTitle, int startingEntryID, int numContinues = 0)
        {
            DialogueEntry entry;
            var sequence = GetSequence(conversationTitle, startingEntryID, out entry, numContinues);
            if (sequence == null) return DefaultSequenceDuration;
            if (sequence.Contains("AudioWait("))
            {
                return GetAudioLength("AudioWait(", sequence, false);
            }
            else if (sequence.Contains("SALSA("))
            {
                return GetAudioLength("SALSA(", sequence, false);
            }
            //#if USE_LIPSYNC
            else if (sequence.Contains("LipSync("))
            {
                return GetAudioLength("LipSync(", sequence, false); //[TODO] Set true, but then need to access LipSync outside Plugins.
            }
            //#endif
            else
            {
                return GetTypewriterLength(entry.DialogueText);
            }
        }

        private const float DefaultSequenceDuration = 1;
        private const float MinSubtitleSeconds = 1;
        private static bool hasLookedForTypewriter = false;
        private static float typewriterCharsPerSecond = 50;

        private static float GetTypewriterLength(string text)
        {
            if (!hasLookedForTypewriter)
            {
                hasLookedForTypewriter = true;
                AbstractTypewriterEffect typewriterEffect = null;
                var dialogueManager = FindObjectOfType<DialogueSystemController>();
                if (dialogueManager != null)
                {
                    var ui = DialogueManager.dialogueUI as StandardDialogueUI;
                    if (ui != null && ui.conversationUIElements.defaultNPCSubtitlePanel != null &&
                        ui.conversationUIElements.defaultNPCSubtitlePanel.subtitleText != null)
                    {
                        typewriterEffect = ui.conversationUIElements.defaultNPCSubtitlePanel.subtitleText.gameObject.GetComponent<AbstractTypewriterEffect>();
                    }
                }
                if (typewriterEffect == null) typewriterEffect = FindObjectOfType<AbstractTypewriterEffect>();
                if (typewriterEffect != null) typewriterCharsPerSecond = typewriterEffect.charactersPerSecond;
            }

            int numCharacters = string.IsNullOrEmpty(text) ? 0 : Tools.StripRichTextCodes(text).Length;
            float numRPGMakerPauses = 0;
            if (text.Contains("\\"))
            {
                var numFullPauses = (text.Length - text.Replace("\\.", string.Empty).Length) / 2;
                var numQuarterPauses = (text.Length - text.Replace("\\,", string.Empty).Length) / 2;
                numRPGMakerPauses = (1.0f * numFullPauses) + (0.25f * numQuarterPauses);
            }
            return Mathf.Max(MinSubtitleSeconds, numRPGMakerPauses + (numCharacters / Mathf.Max(1, typewriterCharsPerSecond)));
        }

        private static float GetAudioLength(string command, string sequence, bool lipSync)
        {
            if (string.IsNullOrEmpty(sequence)) return DefaultSequenceDuration;
            var pos1 = sequence.IndexOf(command) + command.Length;
            var posParen = sequence.IndexOf(")", pos1 + 1);
            var posComma = sequence.IndexOf(",", pos1 + 1);
            var pos2 = sequence.Length;
            if (posParen != -1) pos2 = posParen;
            if (posComma != -1 && posComma < posParen) pos2 = posComma;
            var audioFileName = sequence.Substring(pos1, pos2 - pos1).Trim();
            //if (lipSync)
            //{
            //    return GetLipSyncLength(audioFileName);
            //}
            //else
            {
                var audioClip = LoadAudioClip(audioFileName);
                if (audioClip != null) return audioClip.length;
                return DefaultSequenceDuration;
            }
        }

//#if USE_LIPSYNC
//        private static float GetLipSyncLength(string lipSyncDataFileName)
//        {

//            return DefaultSequenceDuration;
//        }

//        private static RogoDigital.Lipsync.LipSyncData LoadAudioClip(string audioFileName)
//        {
//            AudioClip audioClip;
//#if UNITY_EDITOR
//            audioClip = Resources.Load<AudioClip>(audioFileName);
//            if (audioClip != null) return audioClip;

//#if USE_ADDRESSABLES
//            var settings = AddressableAssetSettingsDefaultObject.Settings;
//            var allEntries = new List<AddressableAssetEntry>(settings.groups.SelectMany(g => g.entries));
//            var foundEntry = allEntries.FirstOrDefault(e => e.address == audioFileName);
//            if (foundEntry != null) audioClip = AssetDatabase.LoadAssetAtPath<AudioClip>(foundEntry.AssetPath);
//            if (audioClip != null) return audioClip;
//#endif

//#endif
//            return null;
//        }
//#endif

        private static AudioClip LoadAudioClip(string audioFileName)
        {
#if UNITY_EDITOR || USE_ADDRESSABLES
            AudioClip audioClip;
#if UNITY_EDITOR
            audioClip = Resources.Load<AudioClip>(audioFileName);
            if (audioClip != null) return audioClip;

#if USE_ADDRESSABLES
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            var allEntries = new List<AddressableAssetEntry>(settings.groups.SelectMany(g => g.entries));
            var foundEntry = allEntries.FirstOrDefault(e => e.address == audioFileName);
            if (foundEntry != null) audioClip = AssetDatabase.LoadAssetAtPath<AudioClip>(foundEntry.AssetPath);
            if (audioClip != null) return audioClip;
#endif
#endif
#endif
            return null;
        }

        public static void ShowMessage(string message, float duration, int lineOffset)
        {
            var go = new GameObject("Editor Preview UI: " + message);
            go.tag = "EditorOnly";
            go.hideFlags = HideFlags.DontSave;
            var previewUI = go.AddComponent<PreviewUI>();
            previewUI.Show(message, duration, lineOffset);
        }

        protected void Show(string message, float duration, int lineOffset)
        {
            this.message = message;
            this.lineOffset = lineOffset;
            endTime = Time.realtimeSinceStartup + (Mathf.Approximately(0, duration) ? 2 : duration);
            computedRect = false;
        }

        private void OnGUI()
        {
            if (!computedRect)
            {
                computedRect = true;
                var size = GUI.skin.label.CalcSize(new GUIContent(message));
                rect = new Rect((Screen.width - size.x) / 2, (Screen.height - size.y) / 2 + lineOffset * size.y, size.x, size.y);
            }
            GUI.Label(rect, message);
        }

        private void Update()
        {
            if (Application.isPlaying)
            {
                Destroy(gameObject);
            }
            else if (Time.realtimeSinceStartup >= endTime)
            {
                DestroyImmediate(gameObject);
            }
        }
    }
}
#endif
#endif
