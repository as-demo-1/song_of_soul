#if USE_TIMELINE
#if UNITY_2017_1_OR_NEWER
// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEngine.Playables;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem
{

    public class QuestStateMixerBehaviour : PlayableBehaviour
    {

        private HashSet<int> played = new HashSet<int>();

        // NOTE: This function is called at runtime and edit time.  Keep that in mind when setting the values of properties.
        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            int inputCount = playable.GetInputCount();

            for (int i = 0; i < inputCount; i++)
            {
                float inputWeight = playable.GetInputWeight(i);
                if (inputWeight > 0.001f && !played.Contains(i))
                {
                    played.Add(i);
                    ScriptPlayable<SetQuestStateBehaviour> inputPlayable = (ScriptPlayable<SetQuestStateBehaviour>)playable.GetInput(i);
                    SetQuestStateBehaviour input = inputPlayable.GetBehaviour();
                    if (Application.isPlaying)
                    {
                        if (input.setQuestState)
                        {
                            QuestLog.SetQuestState(input.quest, input.questState);
                        }
                        if (input.setQuestEntryState)
                        {
                            QuestLog.SetQuestEntryState(input.quest, input.questEntryNumber, input.questEntryState);
                        }
                    }
                    else
                    {
                        var message = string.Empty;
                        if (input.setQuestState)
                        {
                            message = "Set quest " + input.quest + " to " + input.questState;
                            if (input.setQuestEntryState)
                            {
                                message += " and entry #" + input.questEntryNumber + " to " + input.questEntryState;
                            }
                        }
                        else if (input.setQuestEntryState)
                        {
                            message = "Set quest " + input.quest + " entry #" + input.questEntryNumber + " to " + input.questEntryState;
                        }
                        if (!string.IsNullOrEmpty(message)) PreviewUI.ShowMessage(message, 2, -2);
                    }
                }
                else if (inputWeight <= 0.001f && played.Contains(i))
                {
                    played.Remove(i);
                }
            }
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
