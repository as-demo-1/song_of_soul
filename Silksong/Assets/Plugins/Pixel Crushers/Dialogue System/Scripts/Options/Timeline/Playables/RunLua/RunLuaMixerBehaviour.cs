#if USE_TIMELINE
#if UNITY_2017_1_OR_NEWER
// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEngine.Playables;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem
{

    public class RunLuaMixerBehaviour : PlayableBehaviour
    {

        private HashSet<int> played = new HashSet<int>();

        // NOTE: This function is called at runtime and edit time.  Keep that in mind when setting the values of properties.
        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            GameObject trackBinding = playerData as GameObject;

            Transform speaker = (trackBinding != null) ? trackBinding.transform : null;

            int inputCount = playable.GetInputCount();

            for (int i = 0; i < inputCount; i++)
            {
                float inputWeight = playable.GetInputWeight(i);
                if (inputWeight > 0.001f && !played.Contains(i))
                {
                    played.Add(i);
                    ScriptPlayable<RunLuaBehaviour> inputPlayable = (ScriptPlayable<RunLuaBehaviour>)playable.GetInput(i);
                    RunLuaBehaviour input = inputPlayable.GetBehaviour();
                    var luaCode = input.luaCode;
                    if (Application.isPlaying)
                    {
                        Lua.Run(luaCode, DialogueDebug.logInfo, false);
                    }
                    else
                    {
                        PreviewUI.ShowMessage("Lua: " + luaCode, 3, -1);
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
