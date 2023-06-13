/* [REMOVE THIS LINE]
 * [REMOVE THIS LINE] To use this template, make a copy named SequencerCommand<YourCommand>, where
 * [REMOVE THIS LINE] "<YourCommand>" is the name of your sequencer command. Example: For a command
 * [REMOVE THIS LINE] named Foo, name the script SequencerCommandFoo.
 * [REMOVE THIS LINE]
 * [REMOVE THIS LINE] Then remove the lines that start with "[REMOVE THIS LINE]" and add your code
 * [REMOVE THIS LINE] where the comments indicate.
 * [REMOVE THIS LINE]
 * [REMOVE THIS LINE] If your code references scripts or assets that are outside of the Plugins
 * [REMOVE THIS LINE] folder, move this script outside of the Plugins folder, too.
 * [REMOVE THIS LINE]



using UnityEngine;
using System.Collections;
using PixelCrushers.DialogueSystem;

namespace PixelCrushers.DialogueSystem.SequencerCommands
{

    public class SequencerCommandTemplate : SequencerCommand
    { // Rename to SequencerCommand<YourCommand>

        public void Awake()
        {
            // Add your initialization code here. You can use the GetParameter***() and GetSubject()
            // functions to get information from the command's parameters. You can also use the
            // Sequencer property to access the SequencerCamera, CameraAngle, Speaker, Listener,
            // SubtitleEndTime, and other properties on the sequencer. If IsAudioMuted() is true, 
            // the player has muted audio.
            //
            // If your sequencer command only does something immediately and then finishes,
            // you can call Stop() here and remove the Update() method:
            //
            // Stop();
            //
            // If you want to use a coroutine, use a Start() method in place of or in addition to
            // this method.
        }

        public void Update()
        {
            // Add any update code here. When the command is done, call Stop().
            // If you've called stop above in Awake(), you can delete this method.
        }

        public void OnDestroy()
        {
            // Add your finalization code here. This is critical. If the sequence is cancelled and this
            // command is marked as "required", then only Awake() and OnDestroy() will be called.
            // Use it to clean up whatever needs cleaning at the end of the sequencer command.
            // If you don't need to do anything at the end, you can delete this method.
        }

    }

}


/**/
