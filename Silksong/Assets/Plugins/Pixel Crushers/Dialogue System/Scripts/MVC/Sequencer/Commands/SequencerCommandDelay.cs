// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.DialogueSystem.SequencerCommands
{

    /// <summary>
    /// Implements sequencer command: Delay(seconds)
    /// NOTE: This command is now implemented directly in Sequencer.
    /// </summary>
    [AddComponentMenu("")] // Hide from menu.
    public class SequencerCommandDelay : SequencerCommand
    {

        private float stopTime;

        public void Start()
        {
            float seconds = GetParameterAsFloat(0);
            stopTime = DialogueTime.time + seconds;
            if (DialogueDebug.logInfo) Debug.Log(string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}: Sequencer: Delay({1})", new System.Object[] { DialogueDebug.Prefix, seconds }));
        }

        public void Update()
        {
            if (DialogueTime.time >= stopTime) Stop();
        }

    }

}
