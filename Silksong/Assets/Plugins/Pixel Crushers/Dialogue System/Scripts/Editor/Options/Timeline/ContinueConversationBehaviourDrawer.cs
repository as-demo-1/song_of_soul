#if USE_TIMELINE
#if UNITY_2017_1_OR_NEWER
using UnityEngine;
using UnityEditor;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEditor.Timeline;

namespace PixelCrushers.DialogueSystem
{

    [CustomPropertyDrawer(typeof(ContinueConversationBehaviour))]
    public class ContinueConversationBehaviourDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            int fieldCount = 4;
            return fieldCount * EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty operationProp = property.FindPropertyRelative(nameof(ContinueConversationBehaviour.operation));
            SerializedProperty clearPanelNumberProp = property.FindPropertyRelative(nameof(ContinueConversationBehaviour.clearPanelNumber));
            SerializedProperty clearAllPanelsProp = property.FindPropertyRelative(nameof(ContinueConversationBehaviour.clearAllPanels));

            Rect singleFieldRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

            if (GUI.Button(singleFieldRect, "Update Duration"))
            {
                UpdateLength((ContinueConversationBehaviour.Operation)operationProp.enumValueIndex);
            }

            singleFieldRect.y += EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(singleFieldRect, operationProp);
            singleFieldRect.y += EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(singleFieldRect, clearPanelNumberProp);
            singleFieldRect.y += EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(singleFieldRect, clearAllPanelsProp);
        }

        private void UpdateLength(ContinueConversationBehaviour.Operation operation)
        {
            if (operation != ContinueConversationBehaviour.Operation.Continue) return;

            var timelineAsset = TimelineEditor.inspectedAsset;
            var currentTime = TimelineEditor.selectedClip.start;

            // Find the latest StartConversationClip up to the current time:
            double startConversationTime = 0;
            string conversationTitle = string.Empty;
            int startingEntryID = -1;
            foreach (var track in timelineAsset.GetOutputTracks())
            {
                foreach (var clip in track.GetClips())
                {
                    if (clip.start > currentTime) break;
                    if (clip.asset.GetType() == typeof(StartConversationClip))
                    {
                        var startConversationClip = clip.asset as StartConversationClip;
                        startConversationTime = clip.start;
                        conversationTitle = startConversationClip.template.conversation;
                        startingEntryID = startConversationClip.template.jumpToSpecificEntry ? startConversationClip.template.entryID : -1;
                    }
                }
            }

            // Count how many continues have passed since last StartConversationClip:
            int numContinues = 0;
            foreach (var track in timelineAsset.GetOutputTracks())
            {
                foreach (var clip in track.GetClips())
                {
                    if (clip.start > currentTime) break;
                    if (clip.start > startConversationTime &&
                        clip.asset.GetType() == typeof(ContinueConversationClip))
                    {
                        var continueClip = clip.asset as ContinueConversationClip;
                        if (continueClip.template != null &&
                            continueClip.template.operation == ContinueConversationBehaviour.Operation.Continue)
                        {
                            numContinues++;
                        }
                    }
                }
            }

            var duration = PreviewUI.GetSequenceDuration(conversationTitle, startingEntryID, numContinues);
            Debug.Log("Best estimate duration: " + duration + "sec");
            var continueConversationClip = TimelineEditor.selectedClip.asset as ContinueConversationClip;
            if (continueConversationClip == null) return;
            continueConversationClip.SetDuration(duration);
        }

    }
}
#endif
#endif
