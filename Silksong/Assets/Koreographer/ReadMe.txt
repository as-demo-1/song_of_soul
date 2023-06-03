----------------------------------------------
                Koreographer™                 
    Copyright © 2014-2020 Sonic Bloom, LLC    
               Version 1.6.1                  
----------------------------------------------

Thank you for purchasing Koreographer™!

Use of the Koreographer™ software is bound to terms set forth in Unity's Asset Store Terms of Service and EULA:
http://unity3d.com/legal/as_terms

Credits: we would like to request that, where possible, you mention your use of Koreographer™ in either the introduction credits sequence or the end credits sequence of your game or application. When using the Koreographer™ logo, please use the logo provided, at a size no smaller than 405px by 106px for the Logo and Title, and 106px by 106px for Logo only, in the logo’s native aspect ratio.

---------------------------------------
 Support, Documentation, and Tutorials
---------------------------------------

All can be found on the Koreographer™ website:
http://www.koreographer.com/

If you have any questions, suggestions, comments or feature requests, please see our support pages:
http://www.koreographer.com/support/

-----------------
 Version History
-----------------
* Items marked with the "[Pro]" tag are limited to the "Koreographer Professional Edition" package.

1.6.1 - Tune up that new core timing system! Play Mode Monitoring! A few quality of life enhancements and a dash of bug fixes!
・[NEW] Play Mode Monitoring! When this mode is enabled, the Koreography Editor's playhead will track the playback of the Koreography in the Unity Editor's Play Mode!
・[NEW] Holding the Shift key while creating Koreography Events in the Koreography Editor will ensure that only a single Koreography Event is created for the interaction.
・[NEW] [ADVANCED] Support customizing some aspects of the core timing estimation system. This should only be necessary in extreme circumstances (e.g. extreme frame rates).
・TextPayload Peek UI (shown when a OneOff is selected/hovered) is now somewhat flexible and can show more inline than it did previously. [PRO] Custom Payloads can use this feature and will need to implement the IPayload.GetDisplayWidth() API.
・[PRO] Improve converting spec-non-conformant NoteOn events in MIDI files.
・[PRO] Fix Koreography not properly saving newly added KoreographyTrack assets in the MIDI Converter.
・[PRO] Fix integration compatibility with Wwise 2019.2.0+.
・[PRO] Fix the Master Audio integration's MasterAudioSuperVisor to locate AudioSource components on inactive GameObjects.
・Fix core timing estimation in situations with high frame rates.
・Fix time estimation in scenarios with a Time.timeScale of 0.
・Fix several C# compiler warnings in integration/demo scripts in recent versions of Unity.
・Support email addresses updated in Koreographer's Help window.
・Adjust some spacing in UI elements to support recent Unity Editor font changes.

1.6.0 - Overhaul of the core timing system! Also a handful of bug fixes!
・[NEW] Completely rewritten core timing estimation system for Unity's AudioSource audio system! With this new system, timing updates are more precise, smoother, and more stable.
・Fix final End Sample number display in the Koreography Editor's end position LCD.
・Fix final beat sample location math.
・Fix analysis window end range processing.
・Fix double-click-created event snapped beyond end of track.
・Fix Undo when drawing multiple snapped events during a drag operation.
・Fix draw mode OneOff event snapped beyond end of track.
・Fix draw mode Span event ending beyond end of track.
・Adjust ReadMe language around credits logo usage for clarity.

1.5.1 - One new feature coated with a healthy amount of bug fixes and adjustments!

・[NEW] [PRO] Custom MIDI Conversion support! Building on the Custom Payload system added in the previous release, the Custom MIDI Conversion system allows you to hook into the MIDI Converter's conversion process. Details are included in the Custom Payload Demo documentation. The demo content includes a new payload type example: the MIDIPayload.
・Remove legacy GUI components from Demo Scenes to remove warnings in the console in recent versions of Unity!
・Moved the Koreography and KoroegraphyTrack icons to locations expected by Unity 2018.1.0+.
・Adjust the Rhythm Game Demo's LaneController.Restart() API to take an optional sample time to which to restart to.
・[PRO] Fix external file loading logic [used by certain integrations] for Unity 5.4.0+.
・[PRO] Fix attempting to load the empty string ("") as an audio file path when using external audio file loading.
・[PRO] Fix handling paths with spaces (" ") in recent versions of Unity when using external audio file loading.
・[PRO] Fix GradientPayload rendering in Unity 2018.3+.
・[PRO] Fix warnings in recent versions of Unity with the source code package.
・[PRO] Fix Analysis Panel resizing once when changing tabs.
・[PRO] Fix tooltips not appearing when hovering over the range selector in the Analysis Panel on recent versions of Unity.
・[PRO] Fix integration compatibility with Master Audio 4.2.0+.
・[PRO] Fix integration compatibility with Wwise 2017.1.0+.
・Fixs for several minor API documentation bugs.
・Fix manual Tempo Section editing to correctly track the section being modified.
・Fix Koreographer Music Time APIs to all respect configured delay offsets.
・Fix spelling mistakes in the Help Panel.

1.5.0 - Audio Scrubbing! Playback Anchor! Faster Zoom! Custom Payloads! Oh, and a sprinkling of bug fixes!

・[NEW] [PRO] Custom Payload support! With a flash of inspiration, we figured out how to enable you to supply your own Payload types to the Koreography system. Currently this requires that you also provide a custom KoreographyTrack class to contain them. See the Custom Payload Demo for documentation and a working example.
・[NEW] [PRO] New Demo: Custom Payload! Alongside the Custom Payload support, we've included a working example in the form of a demo package. This package includes detailed documentation of the feature and a working custom payload type: the MaterialPayload.
・[NEW] [PRO] New AssetPayload type! The AssetPayload can contain references to Prefabs, AudioClips, or any asset in the Project View.
・[NEW] Playback Anchor in the Koreography Editor! It is now possible to set a Playback Anchor in the Koreographer Editor. This allows you to specify the location to which the audio resets when playback is Stopped (via the stop button or with keyboard shortcuts). Access this feature by right-clicking on the waveform view.
・[NEW] Audio Scrubbing in the Koreography Editor! You can now hear a preview of the audio at any point in the waveform view by holding the ALT key and moving the mouse across the timeline. Please be aware that this feature is dependent upon the current zoom level.
・More responsive zooming! When zoomed out far enough, the Koreography Editor will automatically switch to a logarithmic zoom, enabling you to more quickly move through useful zoom levels.
・[PRO] Adjust the MIDI Converter to have a fixed width and minimum height to always show available content. The window metrics now match the design intent. Please let us know if this causes any usability issues.
・Fixed a number of locations in rarely used runtime code that could cause some unexpected allocations.
・Fix overwriting Koreography references when selecting duplicate objects with separate Koreography specified for a common Koreography field. The Koreography Property Drawer now properly supports reporting "multiple values" in these situations.
・Fix small bug in zoom level calculation.
・Fix the path used in New/Load operations on Koreography and KoreogrpahyTrack assets to properly point at the most-recently-used path, if one exists.
・Fix triggering the API Updater on Unity 5.6 and up when first importing Koreographer.
・[PRO] Fix an edge in the MIDI Converter wherein notes that would end-and-begin in the same instant (MIDI delta of 0) would be imported incorrectly.
・[PRO] Fix the contents of the tooltip for the Event ID in the Koreography Track Export tab of the MIDI Converter.

1.4.2 - Fix that stuttery Koreography Editor timeline in Unity 5.5.0+ and improve smoothness across all versions!

・[NEW] Added two basic example scenes for the PlayMaker integration to show Koreography Event handling with PlayMaker FSMs.
・Fix audio playback time estimation in the Koreography Editor. The code never really worked as expected. This was revealed by a bug introduced by Unity in Unity 5.5.0. Unity has been notified of the associated bug on their end.
・Minor documentation fixes.

1.4.1 - A strong serving of bug fixes! A pinch of optimizations! A dash of new features!

・[NEW] Overloaded the AudioVisor.ResyncTimings() API to take a specific sample time value. This helps guard against race conditions between the Main and Audio threads.
・[NEW] The AudioSourceVisor now has a ResyncTimings() API to enable notification about intentional changes to its AudioSource.
・[NEW] The AudioSourceVisor now has an [optional] "Target Audio Source" field in the Inspector. This removes the implicit requirement of having a single AudioSourceVisor per GameObject. To support this, the AudioSourceVisor no longer auto-adds an AudioSource to the GameObject when added in the Editor. If no AudioSource is specified in the "Target AudioSource" field and no AudioSource is attached to the same GameObject, the AudioSource will log a warning to the console and disable itself.
・[NEW] The AudioSourceVisor now has a ScheduledPlayTime property that enables proper event handling when using the AudioSource.PlayScheduled() APIs.
・[NEW] Brand new PlayMaker Action: Get Koreography Event Timing Offsets! This provides sub-frame timing accuracy for reported Koreography Events. Reported Timing Offsets are intended for OneOff events or only frames wherein the Start or End of a Span are processed.
・Improved the accuracy and reliability of internal timing calculations. This helps improve the reliability of event tracking when looping, as well as better accuracy when the engine is under heavy load.
・Fixes for the Event Delay system to ensure proper event ordering, as well as consistent and correct reporting of time slices.
・Fixes for event triggering when looping with Event Delay.
・Fix events triggering early when the MultiMusicPlayer is configured with a large Sync Play Delay.
・Fix to stop CurvePayload.GetValueAtDelta() from allocating memory.
・Fix AudioSourceVisor initialization incorrectly occurring in Start() instead of Awake().
・Fix the Event ID Tooltip for string fields marked with the [EventID] Attribute appearing over other fields and controls in the Inspector.
・Fix spamming the Console with errors in Unity 5.0+ when scrolling the waveform near the end of the loaded AudioClip in the Koreography Editor.
・Fix for certain Sample Delta functions being off by one sample.
・Very slight math optimizations in internal functionality (reduction in floating point division).
・Fixes to script API documentation (information shown by IntelliSense or equivalent).
・[Pro] Fix for the MIDI Converter not showing MIDI Tracks Converter that had only Lyrics in them. The "Channels" dropdown is not shown for MIDI Tracks that only contain Lyrics.
・[Pro] Fix to stop hiding the actual type of unhandled MIDI Status messages, providing better debug information with certain MIDI files.
・[Pro] Fix for the MIDI Converter incorrectly handling MIDI files with certain SysEx messages. This would cause the next MIDI Event's timing (as well as subsequent events) to be off by a large margin.
・[Pro] Fix script warnings in the Master Audio integration with newer versions of MonoDevelop.
・[Pro] Fix to properly support Master Audio's Gapless Playback and Crossfading playlist features.
・[Pro] Reorganized the SECTR Audio integration script hierarchy to better match other integrations.
・[Pro] Fix Fabric integration scripts polluting the global namespace. The Fabric visors are now properly in the SonicBloom.Koreo.Players.Fabric namespace.

1.4.0 - Rhythm Game Demo! Master Audio Integration! FFT Audio Analysis! Expanded APIs! More responsive!

・[NEW] Rhythm Game Demo showing how to create a game with simple rhythm gameplay! This also provides an example of using the Event Delay functionality to overcome issues related to audio latency.
・[NEW] [Pro] Master Audio integration! Supports both Koreography Event triggering and the Music Time APIs.
・[NEW] [Pro] New FFT Audio Analysis method! This includes a new Payload type (Spectrum Payload) that is generated by the FFT analysis and can be queried at runtime for frequency spectrum information! Check out the new functionality by opening the Analysis Panel and clicking the FFT tab!
・[NEW] [Pro] Lyric Meta Event support added to the MIDI Converter.
・[NEW] The Koreography Editor no longer snaps at the beginning and end of a song to the edges of the screen; the waveform no longer jumps when pausing playback.
・[NEW] Lots of new functions added to the Music Time API! Timings can now be accessed in Samples, Seconds, or Beats, and, yes, there's a BPM accessor!
・[NEW] Tempo Sections now support smooth tempo transitions! If the new "Start New Measure" flag is unchecked, the time-in-beats will pick up where the previous Tempo Section left off. [Pro] The MIDI Converter now unchecks this option by default, properly matching the way MIDI tempo changes are handled.
・[NEW] Timing calculations use 'double' instead of 'float' for far greater precision.
・[NEW] Report to the user if a previous Koreography processing pass did not finish properly. This can occur if an exception is thrown from within a callback.
・[NEW] The Koreography Editor playhead can now be moved by beat as well as by measure (hold down the shift key while pressing left/right).
・[NEW] Support waveform rendering on Retina and HiDPI displays in the Koreography Editor (Unity 5.4+).
・[NEW] The waveform can now be "scratched" in the Koreography Editor by dragging with the middle mouse button during playback.
・[NEW] Koreographer's Music Players now have an IsPlaying property to quickly check the status of playback.
・[NEW] Koreographer's Music Players now support setting a target Koreographer component with which to interface.
・[NEW] Loaded Koreography can now be queried from a Koreographer component.
・[NEW] Improved the portability and performance of the KoreographyTrack event retrieval functions. Old versions marked as Obsolete.
・[NEW] Added APIs to the Koreography class to retrieve Sample Time and Sample Time Delta values that are concurrent with the current (or most recent) Koreography Processing pass.
・Major optimizations to the waveform rendering system resulting in less memory consumption and faster computation.
・Slightly faster zooming by optimizing the waveform cache system.
・Fix for unloading Koreography during a callback causing undefined/difficult to track down errors/issues.
・Fix Koreography Editor issues related to changing the AudioClip (especially when setting it to "None").
・Fix showing the Scene tab when adding Koreography to a field in the Inspector.
・Fix unexpected selection box appearing when clicking on controls in the Koreography Editor.
・Fix for unresponsive Transport Displays (LCD readouts) in the Koreography Editor when clicking to change mode.
・Fix drawing waveforms upside down.
・Fix calculating Curve Payload deltas when the first key was not at time 0.
・Fix Audio Visor logic that used the global Koreographer component, ignoring a specified target Koreographer component.
・[Pro] Fix RMS Audio Analysis results not immediately appearing in the Koreography Editor when generated.
・[Pro] Fix an issue where Audio Analysis would fail to mark a Koreography Track as dirty when generating events.
・Fix not resetting internal Koreography time tracking values when loading a Koreography.
・Customized the script execution order of Koreographer's Audio Players to get them to update early.
・No longer allow creating Koreography Events beyond the end of the AudioClip.
・Removed the version of the GetAllEventsInRange API that had been previously marked as Obsolete.
・Remove reliance upon 'decimal' type in Koreographer functions.

1.3.0 - Koreographer/Pro Packages; New Integrations, Demos, and UI Enhancements!

・[NEW] Koreographer now comes in two editions: "Koreographer" and "Koreographer Professional Edition". Owners of the original "Koreographer" package (release v1.2.0 and below) have been automatically upgraded to "Koreographer Professional Edition". All changes prior to v1.3.0 refer to the "Professional Edition". In the future, changes limited to the "Professional Edition" package will be marked with the "[Pro]" tag (as below).
・[NEW] [Pro] SECTR Audio integration! Supports both Koreography Event triggering and the Music Time APIs.
・[NEW] [Pro] Audio Analysis: RMS Payload Generation! Access by pressing the new "Analyze" button above the Waveform view in the Koreography Editor.
・[NEW] PlayMaker integration!
・[NEW] Karaoke Demo showing one way to use Koreographer to create sing-along lyrics! (Note: Included in packages downloaded from the Asset Store with Unity 5.0.0 and above.)
・[NEW] "Payload Peek" UI: OneOff event Payloads now appear above the event when either selected or the mouse is hovering over them!
・[NEW] We've overhauled the Waveform drawing algorithm to improve speed across all supported versions of Unity!
・[NEW] Icons for Koreographer Assets and Components to help differentiate between types!
・[NEW] Middle-mouse button "click-to-drag" support for scrolling the Waveform view!
・Support for Unity 4.7.0 in source code package.
・Change Simple/MultiMusicPlayer "Auto Play On Start" to "Auto Play On Awake" to better mimic default AudioSource approach.
・Add public EventDelayInSeconds property to Koreographer component to allow runtime delay configuration.
・Fix pasting from right-click menu overwriting selection rather than pasting at current mouse position (when not over an event).
・Fix a very serious bug in the Koreography Editor on Windows that caused a copied payload to paste into selected Koreography Events when a Ctrl button was pressed.
・Fix horizontal scrolling of the Koreography Editor Waveform by scrolling a mouse wheel while holding the Shift key.
・Fix remembering Waveform display setting after zooming in all the way in the Koreography Editor.
・Fix a bug in the CubeScaler script that resulted in not actually evaluating CurvePayloads.
・Fix SimpleMusicPlayer causing a Null Reference Exception if started without Koreography set.
・[Pro] Fix duplicate Koreography Event in the ColorSmooth demo Koreography Track.
・[Pro] Renamed MIDI Converter's "Replace Events in Track" button with more clear "Overwrite Events in Track".
・Various minor performance optimizations.

1.2.0 - Full Platform Support and Load/Save Optimizations!

・[NEW] Support for all Windows Phone 8 and Windows Store build targets! With these platforms joining the herd, Koreographer supports all platforms that Unity does (in Unity 4.5+)!
・[NEW] Massive simplification and optimizations to the KoreographyTrack serialization system! Hurray performance!
・[NEW] MIDI Converter can now Append/Replace Koreography Events to/in a Koreography Track!
・[NEW] MIDI Converter now exports Raw Note and Velocity values as IntPayloads rather than FloatPayloads.
・[NEW] MIDI Converter Event ID field now has a dropdown with Event IDs found in Koreography throughout the project.
・New/Load operations in the Koreography Editor no longer lead to directories in other projects!
・Added a tooltip for the MIDI Converter's loaded file, showing the full path of the file.
・Fix some payload modifications not registering with Undo in the Koreography Editor.
・Fix Gradient Undo in certain circumstances. Suffered the same issue as pre-1.1.0 AnimationCurves!
・Fix some script API documentation XML bugs.

1.1.0 - Core Optimizations and Integration Simplifications!

・[NEW] Add SampleRate field to Koreography. Please either remove and readd your AudioClip in the Koreography Editor or adjust this value directly in the inspector!
・[NEW] Integration with Tazman Audio's Fabric audio engine!
・[NEW] HUGE fixes for runtime memory performance. We tracked down a slew of minor allocations in the API that could add up quickly over time. We think we got most of them but if you notice any allocations occurring, please let us know!
・[NEW] AudioSourceVisor component. Enables simple Koreography Event triggering based on an associated AudioSource component (does not enable the Music Time API - use a Music Player if this is needed).
・[NEW] AnimationCurve undo support in the Koreography Editor.
・[NEW] Option to *not* auto-play music on Start() for SimpleMusicPlayer.
・[NEW] Option to *not* auto-play music on Start() for MultiMusicPlayer.
・[NEW] Detect when Unity's Audio System is disabled when using the Koreography Editor; offer to re-enable it.
・[NEW] Documentation for Koreographer's namespaces.
・[NEW] Core adjustments to make extending Koreographer easier by abstracting most of the "glue" visor code into the "VisorBase" class and removing AudioClip reference requirements on Koreography - matches now occur by name!
・Deprecate the "ProcessChoreography" function - use "ProcessKoreography" instead.
・Indent the "AudioClip" field in the Koreography Editor.
・Fix Koreography Events not triggering for a while after a pause due to Unity losing focus.
・Fix some tooltips in the MIDI Converter.
・Fix some documentation spelling/grammar.

1.0.1 - Documentation Updates!
・[NEW] Koreographer Quick Start Guide
・Unify some formatting across all documentation

1.0.0 - Initial Official Release!