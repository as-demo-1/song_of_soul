// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace PixelCrushers.DialogueSystem
{

    public enum SequenceSyntaxState { Unchecked, Valid, Error }

    public delegate void SetupGenericMenuDelegate(GenericMenu menu);
    public delegate bool TryDragAndDropDelegate(UnityEngine.Object obj, ref string sequence);

    /// <summary>
    /// This class provides a custom drawer for Sequence fields.
    /// </summary>
    public static class SequenceEditorTools
    {

        /// <summary>
        /// Assign delegate handler to check extra drag-n-drop options.
        /// </summary>
        public static TryDragAndDropDelegate tryDragAndDrop = null;

        /// <summary>
        /// Assign handler(s) to add extra menu items to the Sequence dropdown menu.
        /// </summary>
        public static event SetupGenericMenuDelegate customSequenceMenuSetup = null;

        /// <summary>
        /// Add text to the currently-edited sequence. Typically called from a
        /// customSequenceMenuSetup handler.
        /// </summary>
        public static void AddText(string text)
        {
            if (!string.IsNullOrEmpty(queuedText))
            {
                queuedText += ";\n";
            }
            queuedText += text;
        }

        private static string queuedText = string.Empty;

        private enum MenuResult
        {
            Unselected, DefaultSequence, Delay, DefaultCameraAngle, UpdateTracker, RandomizeNextEntry, None, Continue, ContinueTrue, ContinueFalse, OtherCommand
        }

        private static MenuResult menuResult = MenuResult.Unselected;

        private enum AudioDragDropCommand { AudioWait, Audio, SALSA, LipSync, Nothing }

        private static AudioDragDropCommand audioDragDropCommand = AudioDragDropCommand.AudioWait;

        private enum GameObjectDragDropCommand { Camera, DOF, SetActiveTrue, SetActiveFalse, Nothing }

        private static GameObjectDragDropCommand gameObjectDragDropCommand = GameObjectDragDropCommand.Camera;

        private static GameObjectDragDropCommand alternateGameObjectDragDropCommand = GameObjectDragDropCommand.SetActiveTrue;

        private enum ComponentDragDropCommand { SetEnabledTrue, SetEnabledFalse, Nothing }

        private static ComponentDragDropCommand componentDragDropCommand = ComponentDragDropCommand.SetEnabledTrue;

        private static ComponentDragDropCommand alternateComponentDragDropCommand = ComponentDragDropCommand.SetEnabledTrue;

        private static string otherCommandName = string.Empty;

        [Serializable]
        private class DragDropCommands
        {
            public AudioDragDropCommand audioDragDropCommand;
            public GameObjectDragDropCommand gameObjectDragDropCommand;
            public GameObjectDragDropCommand alternateGameObjectDragDropCommand;
        }

        public static string SaveDragDropCommands()
        {
            var commands = new DragDropCommands();
            commands.audioDragDropCommand = audioDragDropCommand;
            commands.gameObjectDragDropCommand = gameObjectDragDropCommand;
            commands.alternateGameObjectDragDropCommand = alternateGameObjectDragDropCommand;
            return JsonUtility.ToJson(commands);
        }

        public static void RestoreDragDropCommands(string s)
        {
            if (string.IsNullOrEmpty(s)) return;
            var commands = JsonUtility.FromJson<DragDropCommands>(s);
            audioDragDropCommand = commands.audioDragDropCommand;
            gameObjectDragDropCommand = commands.gameObjectDragDropCommand;
            alternateGameObjectDragDropCommand = commands.alternateGameObjectDragDropCommand;
        }

        public static string DrawLayout(GUIContent guiContent, string sequence, ref Rect rect, DialogueEntry entry = null, Field field = null)
        {
            var syntaxState = SequenceSyntaxState.Unchecked;
            return DrawLayout(guiContent, sequence, ref rect, ref syntaxState, entry, field);
        }

        public static string DrawLayout(GUIContent guiContent, string sequence, ref Rect rect, ref SequenceSyntaxState syntaxState, DialogueEntry entry = null, Field field = null)
        {
            if (!string.IsNullOrEmpty(queuedText))
            {
                if (!string.IsNullOrEmpty(sequence)) sequence += ";\n";
                sequence += queuedText;
                queuedText = string.Empty;
                GUI.changed = true;
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(guiContent);

            if (entry != null && field != null && DialogueEditor.DialogueEditorWindow.instance != null)
            {
                DialogueEditor.DialogueEditorWindow.instance.DrawAISequence(entry, field);
            }

            EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(sequence));
            if (GUILayout.Button(new GUIContent("Check", "Check sequence for errors."), EditorStyles.miniButton, GUILayout.Width(52)))
            {
                syntaxState = CheckSyntax(sequence);
            }
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginChangeCheck();

            if (GUILayout.Button("+", EditorStyles.miniButton, GUILayout.Width(26)))
            {
                DrawContextMenu(sequence);
            }
            EditorGUILayout.EndHorizontal();
            if (menuResult != MenuResult.Unselected)
            {
                sequence = ApplyMenuResult(menuResult, sequence);
                menuResult = MenuResult.Unselected;
            }

            SetSyntaxStateGUIColor(syntaxState);

            var newSequence = EditorGUILayout.TextArea(sequence);

            ClearSyntaxStateGUIColor();
            if (!string.Equals(newSequence, sequence))
            {
                sequence = newSequence;
                GUI.changed = true;
            }

            switch (Event.current.type)
            {
                case EventType.Repaint:
                    rect = GUILayoutUtility.GetLastRect();
                    break;
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (rect.Contains(Event.current.mousePosition))
                    {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                        if (Event.current.type == EventType.DragPerform)
                        {
                            DragAndDrop.AcceptDrag();
                            foreach (var obj in DragAndDrop.objectReferences)
                            {
                                if (tryDragAndDrop != null && tryDragAndDrop(obj, ref sequence))
                                {
                                    GUI.changed = true;
                                }
                                else if (obj is AudioClip)
                                {
                                    // Drop audio clip according to selected audio command:
                                    var currentAudioCommand = GetCurrentAudioCommand();
                                    if (string.IsNullOrEmpty(currentAudioCommand)) continue;
                                    var clip = obj as AudioClip;
                                    var path = AssetDatabase.GetAssetPath(clip);
                                    if (path.Contains("Resources"))
                                    {
                                        sequence = AddCommandToSequence(sequence, currentAudioCommand + "(" + GetResourceName(path) + ")");
                                        GUI.changed = true;
                                    }
                                    else if (currentAudioCommand == "LipSync")
                                    {
                                        sequence = AddCommandToSequence(sequence, currentAudioCommand + "(" + System.IO.Path.GetFileNameWithoutExtension(path) + ")");
                                        GUI.changed = true;
                                    }
                                    else
                                    {
                                        EditorUtility.DisplayDialog("Not in Resources Folder", "To use drag-n-drop, audio clips must be located in the hierarchy of a Resources folder.", "OK");
                                    }
                                }
                                else if (obj is GameObject)
                                {
                                    // Drop GameObject.
                                    var go = obj as GameObject;
                                    if (sequence.EndsWith("("))
                                    {
                                        // If sequence ends in open paren, add GameObject and close:
                                        sequence += go.name + ")";
                                    }
                                    else
                                    {
                                        // Drop GameObject according to selected GameObject command:
                                        var command = Event.current.alt ? alternateGameObjectDragDropCommand : gameObjectDragDropCommand;
                                        var currentGameObjectCommand = GetCurrentGameObjectCommand(command, go.name);
                                        if (string.IsNullOrEmpty(currentGameObjectCommand)) continue;
                                        sequence = AddCommandToSequence(sequence, currentGameObjectCommand);
                                    }
                                    GUI.changed = true;
                                }
                                else if (obj is Component)
                                {
                                    // Drop component.
                                    var component = obj as Component;
                                    var go = component.gameObject;
                                    if (sequence.EndsWith("("))
                                    {
                                        // If sequence ends in open paren, add component and close:
                                        sequence += component.GetType().Name + ")";
                                    }
                                    else
                                    {
                                        // Drop component according to selected component command:
                                        var command = Event.current.alt ? alternateComponentDragDropCommand : componentDragDropCommand;
                                        var currentComponentCommand = GetCurrentComponentCommand(command, component.GetType().Name, go.name);
                                        if (string.IsNullOrEmpty(currentComponentCommand)) continue;
                                        sequence = AddCommandToSequence(sequence, currentComponentCommand);
                                    }
                                    GUI.changed = true;
                                }
                            }
                        }
                    }
                    break;
            }

            // If content changed, reset syntax check state:
            if (EditorGUI.EndChangeCheck()) syntaxState = SequenceSyntaxState.Unchecked;

            return sequence;
        }

        private static void DrawContextMenu(string sequence)
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Help/Overview..."), false, OpenURL, "https://www.pixelcrushers.com/dialogue_system/manual2x/html/cutscene_sequences.html");
            menu.AddItem(new GUIContent("Help/Command Reference..."), false, OpenURL, "https://www.pixelcrushers.com/dialogue_system/manual2x/html/sequencer_command_reference.html");
            menu.AddSeparator("");
            menu.AddDisabledItem(new GUIContent("Shortcuts:"));
            menu.AddItem(new GUIContent("Include Dialogue Manager's Default Sequence"), false, SetMenuResult, MenuResult.DefaultSequence);
            menu.AddItem(new GUIContent("Delay for subtitle length"), false, SetMenuResult, MenuResult.Delay);
            menu.AddItem(new GUIContent("Cut to speaker's default camera angle"), false, SetMenuResult, MenuResult.DefaultCameraAngle);
            menu.AddItem(new GUIContent("Update quest tracker"), false, SetMenuResult, MenuResult.UpdateTracker);
            menu.AddItem(new GUIContent("Randomize next entry"), false, SetMenuResult, MenuResult.RandomizeNextEntry);
            menu.AddItem(new GUIContent("None (null command with zero duration)"), false, SetMenuResult, MenuResult.None);
            menu.AddItem(new GUIContent("Continue/Simulate continue button click"), false, SetMenuResult, MenuResult.Continue);
            menu.AddItem(new GUIContent("Continue/Enable continue button"), false, SetMenuResult, MenuResult.ContinueTrue);
            menu.AddItem(new GUIContent("Continue/Disable continue button"), false, SetMenuResult, MenuResult.ContinueFalse);
            menu.AddItem(new GUIContent("Audio Drag-n-Drop/Help..."), false, ShowSequenceEditorAudioHelp, null);
            menu.AddItem(new GUIContent("Audio Drag-n-Drop/Use AudioWait()"), audioDragDropCommand == AudioDragDropCommand.AudioWait, SetAudioDragDropCommand, AudioDragDropCommand.AudioWait);
            menu.AddItem(new GUIContent("Audio Drag-n-Drop/Use Audio()"), audioDragDropCommand == AudioDragDropCommand.Audio, SetAudioDragDropCommand, AudioDragDropCommand.Audio);
            menu.AddItem(new GUIContent("Audio Drag-n-Drop/Use SALSA() (3rd party)"), audioDragDropCommand == AudioDragDropCommand.SALSA, SetAudioDragDropCommand, AudioDragDropCommand.SALSA);
            menu.AddItem(new GUIContent("Audio Drag-n-Drop/Use LipSync() (3rd party)"), audioDragDropCommand == AudioDragDropCommand.LipSync, SetAudioDragDropCommand, AudioDragDropCommand.LipSync);
            menu.AddItem(new GUIContent("Audio Drag-n-Drop/(Do Nothing)"), audioDragDropCommand == AudioDragDropCommand.Nothing, SetAudioDragDropCommand, AudioDragDropCommand.Nothing);
            menu.AddItem(new GUIContent("GameObject Drag-n-Drop/Help..."), false, ShowSequenceEditorGameObjectHelp, null);
            menu.AddItem(new GUIContent("GameObject Drag-n-Drop/Default/Use Camera()"), gameObjectDragDropCommand == GameObjectDragDropCommand.Camera, SetGameObjectDragDropCommand, GameObjectDragDropCommand.Camera);
            menu.AddItem(new GUIContent("GameObject Drag-n-Drop/Default/Use DOF()"), gameObjectDragDropCommand == GameObjectDragDropCommand.DOF, SetGameObjectDragDropCommand, GameObjectDragDropCommand.DOF);
            menu.AddItem(new GUIContent("GameObject Drag-n-Drop/Default/SetActive(GameObject,true)"), gameObjectDragDropCommand == GameObjectDragDropCommand.SetActiveTrue, SetGameObjectDragDropCommand, GameObjectDragDropCommand.SetActiveTrue);
            menu.AddItem(new GUIContent("GameObject Drag-n-Drop/Default/SetActive(GameObject,false)"), gameObjectDragDropCommand == GameObjectDragDropCommand.SetActiveFalse, SetGameObjectDragDropCommand, GameObjectDragDropCommand.SetActiveFalse);
            menu.AddItem(new GUIContent("GameObject Drag-n-Drop/Default/(Do Nothing)"), gameObjectDragDropCommand == GameObjectDragDropCommand.Nothing, SetGameObjectDragDropCommand, GameObjectDragDropCommand.Nothing);
            menu.AddItem(new GUIContent("GameObject Drag-n-Drop/Alt-Key/Use Camera()"), alternateGameObjectDragDropCommand == GameObjectDragDropCommand.Camera, SetAlternateGameObjectDragDropCommand, GameObjectDragDropCommand.Camera);
            menu.AddItem(new GUIContent("GameObject Drag-n-Drop/Alt-Key/Use DOF()"), alternateGameObjectDragDropCommand == GameObjectDragDropCommand.DOF, SetAlternateGameObjectDragDropCommand, GameObjectDragDropCommand.DOF);
            menu.AddItem(new GUIContent("GameObject Drag-n-Drop/Alt-Key/SetActive(GameObject,true)"), alternateGameObjectDragDropCommand == GameObjectDragDropCommand.SetActiveTrue, SetAlternateGameObjectDragDropCommand, GameObjectDragDropCommand.SetActiveTrue);
            menu.AddItem(new GUIContent("GameObject Drag-n-Drop/Alt-Key/SetActive(GameObject,false)"), alternateGameObjectDragDropCommand == GameObjectDragDropCommand.SetActiveFalse, SetAlternateGameObjectDragDropCommand, GameObjectDragDropCommand.SetActiveFalse);
            menu.AddItem(new GUIContent("GameObject Drag-n-Drop/Alt-Key/(Do Nothing)"), alternateGameObjectDragDropCommand == GameObjectDragDropCommand.Nothing, SetAlternateGameObjectDragDropCommand, GameObjectDragDropCommand.Nothing);
            menu.AddItem(new GUIContent("Component Drag-n-Drop/Default/SetEnabled(Component,true,GameObject)"), componentDragDropCommand == ComponentDragDropCommand.SetEnabledTrue, SetComponentDragDropCommand, ComponentDragDropCommand.SetEnabledTrue);
            menu.AddItem(new GUIContent("Component Drag-n-Drop/Default/SetEnabled(Component,false,GameObject)"), componentDragDropCommand == ComponentDragDropCommand.SetEnabledFalse, SetComponentDragDropCommand, ComponentDragDropCommand.SetEnabledFalse);
            menu.AddItem(new GUIContent("Component Drag-n-Drop/Default/(Do Nothing)"), componentDragDropCommand == ComponentDragDropCommand.Nothing, SetComponentDragDropCommand, ComponentDragDropCommand.Nothing);
            menu.AddItem(new GUIContent("Component Drag-n-Drop/Alt-Key/SetEnabled(Component,true,GameObject)"), alternateComponentDragDropCommand == ComponentDragDropCommand.SetEnabledTrue, SetAlternateComponentDragDropCommand, ComponentDragDropCommand.SetEnabledTrue);
            menu.AddItem(new GUIContent("Component Drag-n-Drop/Alt-Key/SetEnabled(Component,false,GameObject)"), alternateComponentDragDropCommand == ComponentDragDropCommand.SetEnabledFalse, SetAlternateComponentDragDropCommand, ComponentDragDropCommand.SetEnabledFalse);
            menu.AddItem(new GUIContent("Component Drag-n-Drop/Alt-Key/(Do Nothing)"), alternateComponentDragDropCommand == ComponentDragDropCommand.Nothing, SetAlternateComponentDragDropCommand, ComponentDragDropCommand.Nothing);
            AddAllSequencerCommands(menu);
            AddAllShortcuts(menu);
            if (customSequenceMenuSetup != null) customSequenceMenuSetup(menu);
            menu.ShowAsContext();
        }

        private static void OpenURL(object url)
        {
            Application.OpenURL(url as string);
        }

        private static void ShowSequenceEditorAudioHelp(object data)
        {
            EditorUtility.DisplayDialog("Audio Drag & Drop Help", "Select an item in this Audio submenu to specify which command to add when dragging an audio clip onto the Sequence field. Audio clips must be in a Resources folder.\n\nAudio commands can use AssetBundles and Addressables, but not with this drag-n-drop feature.\n\nIf using LipSync(), to use drag-n-drop the LipSync data file and audio file must be named the same, and you must drag the audio file into the Sequence field, but the audio file doesn't have to be in Resources.", "OK");
        }

        private static void SetAudioDragDropCommand(object data)
        {
            audioDragDropCommand = (AudioDragDropCommand)data;
        }

        private static string GetCurrentAudioCommand()
        {
            switch (audioDragDropCommand)
            {
                case AudioDragDropCommand.Nothing:
                    return string.Empty;
                case AudioDragDropCommand.Audio:
                    return "Audio";
                case AudioDragDropCommand.SALSA:
                    return "SALSA";
                case AudioDragDropCommand.LipSync:
                    return "LipSync";
                default:
                    return "AudioWait";
            }
        }

        private static void ShowSequenceEditorGameObjectHelp(object data)
        {
            EditorUtility.DisplayDialog("GameObject Drag & Drop Help", "Select an item in this GameObject submenu to specify which command to add when dragging a GameObject onto the Sequence field.", "OK");
        }

        private static void SetGameObjectDragDropCommand(object data)
        {
            gameObjectDragDropCommand = (GameObjectDragDropCommand)data;
        }

        private static void SetAlternateGameObjectDragDropCommand(object data)
        {
            alternateGameObjectDragDropCommand = (GameObjectDragDropCommand)data;
        }

        private static string GetCurrentGameObjectCommand(GameObjectDragDropCommand command, string goName)
        {
            if (string.IsNullOrEmpty(goName)) return string.Empty;
            switch (command)
            {
                default:
                case GameObjectDragDropCommand.Camera:
                    return "Camera(default," + goName + ")";
                case GameObjectDragDropCommand.DOF:
                    return "DOF(" + goName + ")";
                case GameObjectDragDropCommand.SetActiveTrue:
                    return "SetActive(" + goName + ",true)";
                case GameObjectDragDropCommand.SetActiveFalse:
                    return "SetActive(" + goName + ",false)";
                case GameObjectDragDropCommand.Nothing:
                    return string.Empty;
            }
        }

        private static void SetComponentDragDropCommand(object data)
        {
            componentDragDropCommand = (ComponentDragDropCommand)data;
        }

        private static void SetAlternateComponentDragDropCommand(object data)
        {
            alternateComponentDragDropCommand = (ComponentDragDropCommand)data;
        }

        private static string GetCurrentComponentCommand(ComponentDragDropCommand command, string componentName, string goName)
        {
            if (string.IsNullOrEmpty(componentName)) return string.Empty;
            switch (command)
            {
                default:
                case ComponentDragDropCommand.SetEnabledTrue:
                    return "SetEnabled(" + componentName + ",true," + goName + ")";
                case ComponentDragDropCommand.SetEnabledFalse:
                    return "SetEnabled(" + componentName + ",false," + goName + ")";
                case ComponentDragDropCommand.Nothing:
                    return string.Empty;
            }
        }

        private static void SetMenuResult(object data)
        {
            menuResult = (MenuResult)data;
        }

        private static string ApplyMenuResult(MenuResult menuResult, string sequence)
        {
            GUI.changed = true;
            var newCommand = GetMenuResultCommand(menuResult);
            if (string.IsNullOrEmpty(newCommand))
            {
                return sequence;
            }
            else
            {
                return AddCommandToSequence(sequence, newCommand);
            }
        }

        private static string GetMenuResultCommand(MenuResult menuResult)
        {
            switch (menuResult)
            {
                case MenuResult.DefaultSequence:
                    return "{{default}}";
                case MenuResult.Delay:
                    return "Delay({{end}})";
                case MenuResult.DefaultCameraAngle:
                    return "Camera(default)";
                case MenuResult.UpdateTracker:
                    return "UpdateTracker()";
                case MenuResult.RandomizeNextEntry:
                    return "RandomizeNextEntry()";
                case MenuResult.None:
                    return "None()";
                case MenuResult.Continue:
                    return "Continue()";
                case MenuResult.ContinueTrue:
                    return "SetContinueMode(true)";
                case MenuResult.ContinueFalse:
                    return "SetContinueMode(false)";
                case MenuResult.OtherCommand:
                    return otherCommandName;
                default:
                    return string.Empty;
            }
        }

        private static string AddCommandToSequence(string sequence, string newCommand)
        {
            var s = sequence;
            if (!string.IsNullOrEmpty(sequence) && !sequence.TrimEnd().EndsWith(";"))
            {
                s += ";\n";
            }
            else if (!string.IsNullOrEmpty(sequence) && !sequence.EndsWith("\n"))
            {
                s += "\n";
            }
            return s + newCommand;
        }

        private static string GetResourceName(string path)
        {
            if (string.IsNullOrEmpty(path)) return string.Empty;
            var index = path.IndexOf("Resources/");
            if (index == -1) return string.Empty;
            var s = path.Substring(index + "Resources/".Length);
            index = s.LastIndexOf(".");
            if (index != -1) s = s.Substring(0, index);
            return s;
        }

        private static string[] InternalSequencerCommands =
        {
            "None",
            "AnimatorController",
            "AnimatorBool",
            "AnimatorInt",
            "AnimatorFloat",
            "AnimatorTrigger",
            "AnimatorPlay",
            "Audio",
            "ClearSubtitleText",
            "Continue",
            "SendMessage",
            "SetActive",
            "SetEnabled",
            "SetPanel",
            "SetMenuPanel",
            "SetDialoguePanel",
            "SetPortrait",
            "SetTimeout",
            "SetContinueMode",
            "Continue",
            "SetVariable",
            "ShowAlert",
            "UpdateTracker",
            "RandomizeNextEntry",
                    };

        private static void AddAllSequencerCommands(GenericMenu menu)
        {
            var list = new List<string>(InternalSequencerCommands);
            var assemblies = RuntimeTypeUtility.GetAssemblies();
            for (int i = 0; i < assemblies.Length; i++)
            {
                var assembly = assemblies[i];
                try
                {
                    foreach (var type in assembly.GetTypes().Where(t => typeof(PixelCrushers.DialogueSystem.SequencerCommands.SequencerCommand).IsAssignableFrom(t)))
                    {
                        var commandName = type.Name.Substring("SequencerCommand".Length);
                        list.Add(commandName);
                    }
                }
                catch (System.Exception) { }
            }
            list.Sort();
            for (int i = 0; i < list.Count; i++)
            {
                menu.AddItem(new GUIContent("All Sequencer Commands/" + list[i]), false, StartSequencerCommand, list[i]);
            }
        }

        private static void AddAllShortcuts(GenericMenu menu)
        {
            menu.AddItem(new GUIContent("Shortcuts/Help..."), false, OpenURL, "https://www.pixelcrushers.com/dialogue_system/manual2x/html/cutscene_sequences.html#shortcuts");
            var list = new List<string>();
            var allSequencerShortcuts = GameObjectUtility.FindObjectsByType<SequencerShortcuts>();
            foreach (var sequencerShortcuts in allSequencerShortcuts)
            {
                foreach (var shortcut in sequencerShortcuts.shortcuts)
                {
                    list.Add(@"{{" + shortcut.shortcut + @"}}");
                }
            }
            list.Sort();
            for (int i = 0; i < list.Count; i++)
            {
                menu.AddItem(new GUIContent("Shortcuts/" + list[i]), false, StartOtherCommand, list[i]);
            }
        }

        private static void StartSequencerCommand(object data)
        {

            otherCommandName = (string)data + "(";
            SetMenuResult(MenuResult.OtherCommand);
        }

        private static void StartOtherCommand(object data)
        {

            otherCommandName = (string)data;
            SetMenuResult(MenuResult.OtherCommand);
        }

        public static SequenceSyntaxState CheckSyntax(string sequence)
        {
            if (string.IsNullOrEmpty(sequence)) return SequenceSyntaxState.Valid;
            // Add fake values for special {{...}} keywords:
            var sequenceToCheck = sequence.
                Replace("{{default}}", "None()").
                Replace("{{end}}", "0");
            sequenceToCheck = Regex.Replace(sequenceToCheck, @"\{\{.+\}\}", string.Empty);
            if (string.IsNullOrEmpty(sequenceToCheck) ||
                string.IsNullOrEmpty(sequenceToCheck.Replace(";", string.Empty).Trim()))
            {
                return SequenceSyntaxState.Valid;
            }
            var parser = new SequenceParser();
            var result = parser.Parse(sequenceToCheck);
            return (result == null || result.Count == 0) ? SequenceSyntaxState.Error : SequenceSyntaxState.Valid;
        }

        public static void SetSyntaxStateGUIColor(SequenceSyntaxState syntaxState)
        {
            switch (syntaxState)
            {
                case SequenceSyntaxState.Valid:
                    GUI.color = Color.green;
                    break;
                case SequenceSyntaxState.Error:
                    GUI.color = Color.red;
                    break;
            }
        }

        public static void ClearSyntaxStateGUIColor()
        {
            GUI.color = Color.white;
        }

    }

}
