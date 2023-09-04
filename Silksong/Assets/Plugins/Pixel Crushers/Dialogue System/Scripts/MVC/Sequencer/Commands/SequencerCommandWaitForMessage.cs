// Copyright (c) Pixel Crushers. All rights reserved.

using System.Collections.Generic;
using UnityEngine;

namespace PixelCrushers.DialogueSystem.SequencerCommands
{

    /// <summary>
    /// Implements sequencer command: WaitForMessage(message), which waits
    /// until it receives OnSequencerMessage(message) for all specified messages.
    /// </summary>
    [AddComponentMenu("")] // Hide from menu.
    public class SequencerCommandWaitForMessage : SequencerCommand
    {

        private List<string> requiredMessages = new List<string>();

        public void Awake()
        {
            requiredMessages.AddRange(parameters);
            if (DialogueDebug.logInfo) Debug.Log(string.Format("{0}: Sequencer: WaitForMessage({1})", new System.Object[] { DialogueDebug.Prefix, GetParameters() }));
            requiredMessages.RemoveAll(x => string.IsNullOrEmpty(x));
            if (requiredMessages.Count == 0) Stop();
        }

        public void OnSequencerMessage(string message)
        {
            if (requiredMessages.Contains(message))
            {
                if (DialogueDebug.logInfo) Debug.Log(string.Format("{0}: Sequencer: WaitForMessage({1}) received message", new System.Object[] { DialogueDebug.Prefix, message }));
                requiredMessages.RemoveAll(x => x == message);
                if (requiredMessages.Count == 0)
                {
                    Stop();
                }
            }
        }

    }

}
