//--- Now incorporated into Sequencer.
// Copyright (c) Pixel Crushers. All rights reserved.

//using UnityEngine;

//namespace PixelCrushers.DialogueSystem.SequencerCommands
//{

//    /// <summary>
//    /// This script implements the sequencer command SetTimeout(duration),
//    /// which sets the response menu timeout duration. Set duration 
//    /// to 0 (zero) to disable the timer.
//    /// </summary>
//    [AddComponentMenu("")] // Hide from menu.
//    public class SequencerCommandSetTimeout : SequencerCommand
//    {

//        public void Start()
//        {
//            float duration = GetParameterAsFloat(0);
//            if (DialogueDebug.logInfo) Debug.Log(string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}: Sequencer: SetTimeout({1})", DialogueDebug.Prefix, duration));
//            if (DialogueManager.displaySettings != null && DialogueManager.displaySettings.inputSettings != null)
//            {
//                DialogueManager.displaySettings.inputSettings.responseTimeout = duration;
//            }
//            Stop();
//        }
//    }
//}
