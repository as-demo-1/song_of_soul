#if USE_TIMELINE
#if UNITY_2017_1_OR_NEWER
// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEngine.Playables;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem
{

    public class BarkMixerBehaviour : PlayableBehaviour
    {

        private HashSet<int> played = new HashSet<int>();

        // NOTE: This function is called at runtime and edit time.  Keep that in mind when setting the values of properties.
        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            GameObject trackBinding = playerData as GameObject;

            if (!trackBinding) return;

            int inputCount = playable.GetInputCount();

            for (int i = 0; i < inputCount; i++)
            {
                float inputWeight = playable.GetInputWeight(i);
                if (inputWeight > 0.001f && !played.Contains(i))
                {
                    played.Add(i);
                    ScriptPlayable<BarkBehaviour> inputPlayable = (ScriptPlayable<BarkBehaviour>)playable.GetInput(i);
                    BarkBehaviour input = inputPlayable.GetBehaviour();
                    if (Application.isPlaying)
                    {
                        if (input.useConversation)
                        {
                            if (input.barkSpecificEntry)
                            {
                                var subtitle = GetBarkSubtitle(input.conversation, input.entryID, trackBinding.transform, input.listener);
                                DialogueManager.instance.StartCoroutine(BarkController.Bark(subtitle));
                            }
                            else
                            {
                                DialogueManager.Bark(input.conversation, trackBinding.transform, input.listener);
                            }
                        }
                        else
                        {
                            DialogueManager.BarkString(input.text, trackBinding.transform, input.listener);
                        }                        
                    }
                    else
                    {
                        var message = DialogueActor.GetActorName(trackBinding.transform) + " bark: " + input.GetEditorBarkText();
                        if (input.useConversation) message += " (may vary)";
                        PreviewUI.ShowMessage(message, 2, 1);
                    }
                }
                else if (inputWeight <= 0.001f && played.Contains(i))
                {
                    played.Remove(i);
                }
            }
        }

        protected Subtitle GetBarkSubtitle(string conversationTitle, int entryID, Transform speaker, Transform listener)
        {
            var conversation = DialogueManager.masterDatabase.GetConversation(conversationTitle);
            if (conversation == null) return null;
            var entry = conversation.GetDialogueEntry(entryID);
            if (entry == null) return null;
            var conversationModel = new ConversationModel(DialogueManager.masterDatabase, conversationTitle, speaker, listener, true, null);
            var speakerInfo = conversationModel.GetCharacterInfo(entry.ActorID, speaker);
            var listenerInfo = conversationModel.GetCharacterInfo(entry.ConversantID, listener);
            var formattedText = FormattedText.Parse(entry.currentDialogueText);
            Lua.Run(entry.userScript);
            return new Subtitle(speakerInfo, listenerInfo, formattedText, entry.Sequence, string.Empty, entry);
        }

        public override void OnGraphStart(Playable playable)
        {
            base.OnGraphStart(playable);
            played.Clear();
        }

        public override void OnGraphStop(Playable playable)
        {
            base.OnGraphStop(playable);
            played.Clear();
        }

    }
}
#endif
#endif
