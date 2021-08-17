using System;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;

namespace Gamekit2D
{
    public class ScrollingTextMixerBehaviour : PlayableBehaviour
    {
        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            TextMeshProUGUI trackBinding = playerData as TextMeshProUGUI;

            if (!trackBinding)
                return;

            int inputCount = playable.GetInputCount ();

            for (int i = 0; i < inputCount; i++)
            {
                float inputWeight = playable.GetInputWeight(i);
                ScriptPlayable<ScrollingTextBehaviour> inputPlayable = (ScriptPlayable<ScrollingTextBehaviour>)playable.GetInput(i);
                ScrollingTextBehaviour input = inputPlayable.GetBehaviour ();

                if (Mathf.Approximately (inputWeight, 1f))
                {
                    string message = input.GetMessage ((float)inputPlayable.GetTime ());
                    trackBinding.text = message;
                }
            }
        }
    }
}