// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// Player setup wizard.
    /// </summary>
    public class PlayerSetupWizard : EditorWindow
    {

        [MenuItem("Tools/Pixel Crushers/Dialogue System/Wizards/Player Setup Wizard", false, 1)]
        public static void Init()
        {
            (EditorWindow.GetWindow(typeof(PlayerSetupWizard), false, "Player Setup") as PlayerSetupWizard).minSize = new Vector2(700, 500);
        }

        // Private fields for the window:

        private enum Stage
        {
            SelectPC,
            Actor,
            Control,
            Camera,
            Targeting,
            Transition,
            Persistence,
            Review
        };

        private Stage stage = Stage.SelectPC;

        private string[] stageLabels = new string[] { "Player", "Actor", "Control", "Camera", "Targeting", "Transition", "Persistence", "Review" };

        private const float ToggleWidth = 16;

        private GameObject pcObject = null;

        private bool setEventsFlag = false;
        private bool setEnabledFlag = false;

        private Vector2 scrollPosition = Vector2.zero;

        private void OnEnable()
        {
            pcObject = GameObject.FindGameObjectWithTag("Player");
        }

        /// <summary>
        /// Draws the window.
        /// </summary>
        void OnGUI()
        {
            DrawProgressIndicator();
            DrawCurrentStage();
        }

        private void DrawProgressIndicator()
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Toolbar((int)stage, stageLabels, GUILayout.Width(700));
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup();
            EditorWindowTools.DrawHorizontalLine();
        }

        private void DrawNavigationButtons(bool backEnabled, bool nextEnabled, bool nextCloses)
        {
            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Close", GUILayout.Width(100)))
            {
                this.Close();
            }
            else if (backEnabled && GUILayout.Button("Back", GUILayout.Width(100)))
            {
                stage--;
            }
            else
            {
                EditorGUI.BeginDisabledGroup(!nextEnabled);
                if (GUILayout.Button(nextCloses ? "Finish" : "Next", GUILayout.Width(100)))
                {
                    if (nextCloses)
                    {
                        Close();
                    }
                    else
                    {
                        stage++;
                    }
                }
                EditorGUI.EndDisabledGroup();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.LabelField(string.Empty, GUILayout.Height(2));
        }

        private void DrawCurrentStage()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            if (pcObject == null) stage = Stage.SelectPC;
            switch (stage)
            {
                case Stage.SelectPC: DrawSelectPCStage(); break;
                case Stage.Actor: DrawActorStage(); break;
                case Stage.Control: DrawControlStage(); break;
                case Stage.Camera: DrawCameraStage(); break;
                case Stage.Targeting: DrawTargetingStage(); break;
                case Stage.Transition: DrawTransitionStage(); break;
                case Stage.Persistence: DrawPersistenceStage(); break;
                case Stage.Review: DrawReviewStage(); break;
            }
            EditorGUILayout.EndScrollView();
            if ((pcObject != null) && GUI.changed) EditorUtility.SetDirty(pcObject);
        }

        private void DrawSelectPCStage()
        {
            EditorGUILayout.LabelField("Select Player Object", EditorStyles.boldLabel);
            EditorWindowTools.StartIndentedSection();
            EditorGUILayout.HelpBox("This wizard will help you configure a Player object to work with the Dialogue System. First, assign the Player's GameObject below. Assign a scene GameObject, not a prefab.\n\nThis wizard is optional. If you prefer, you can set up the player yourself in the Inspector view.", MessageType.Info);
            pcObject = EditorGUILayout.ObjectField("Player Object", pcObject, typeof(GameObject), true) as GameObject;
            EditorWindowTools.EndIndentedSection();
            DrawNavigationButtons(false, (pcObject != null), false);
        }

        private Editor dialogueActorEditor = null;

        private void DrawActorStage()
        {
            EditorGUILayout.LabelField("Dialogue Actor Component (Optional)", EditorStyles.boldLabel);
            EditorWindowTools.StartIndentedSection();
            EditorGUILayout.HelpBox("A Dialogue Actor component lets you specify which actor in the database this GameObject is associated with if the GameObject doesn't have the same name as the actor.", MessageType.Info);
            var dialogueActor = pcObject.GetComponent<DialogueActor>();
            var hasDialogueActor = dialogueActor != null;
            var useDialogueActor = EditorGUILayout.Toggle("Use Dialogue Actor", hasDialogueActor);
            if (useDialogueActor && !hasDialogueActor)
            {
                dialogueActor = pcObject.AddComponent(TypeUtility.GetWrapperType(typeof(PixelCrushers.DialogueSystem.DialogueActor))) as DialogueActor;
                hasDialogueActor = true;
            }
            else if (!useDialogueActor && hasDialogueActor)
            {
                DestroyImmediate(dialogueActor);
                hasDialogueActor = false;
                dialogueActorEditor = null;
            }
            if (hasDialogueActor)
            {
                if (dialogueActorEditor == null) dialogueActorEditor = Editor.CreateEditor(dialogueActor);
                dialogueActorEditor.OnInspectorGUI();
            }

            EditorWindowTools.DrawHorizontalLine();
            var defaultCameraAngle = pcObject.GetComponent<DefaultCameraAngle>();
            var hasDefaultCameraAngle = defaultCameraAngle != null;
            EditorGUILayout.BeginHorizontal();
            var useDefaultCameraAngle = EditorGUILayout.Toggle("Override Default Angle", hasDefaultCameraAngle);
            EditorGUILayout.HelpBox("The default camera angle is 'Closeup'. You can override it here.", MessageType.None);
            EditorGUILayout.EndHorizontal();
            if (useDefaultCameraAngle && !hasDefaultCameraAngle)
            {
                defaultCameraAngle = pcObject.AddComponent(TypeUtility.GetWrapperType(typeof(PixelCrushers.DialogueSystem.DefaultCameraAngle))) as DefaultCameraAngle;
                hasDefaultCameraAngle = true;
            }
            else if (!useDefaultCameraAngle && hasDefaultCameraAngle)
            {
                DestroyImmediate(defaultCameraAngle);
                defaultCameraAngle = null;
                hasDefaultCameraAngle = false;
            }
            if (hasDefaultCameraAngle)
            {
                EditorGUILayout.BeginHorizontal();
                defaultCameraAngle.cameraAngle = EditorGUILayout.TextField("Angle", defaultCameraAngle.cameraAngle);
                EditorGUILayout.HelpBox("Specify the default camera angle for the player.", MessageType.None);
                EditorGUILayout.EndHorizontal();
            }


            if (GUILayout.Button("Select Player", GUILayout.Width(100))) Selection.activeGameObject = pcObject;
            EditorWindowTools.EndIndentedSection();
            DrawNavigationButtons(true, true, false);
        }

        private enum ControlStyle
        {
            SimpleThirdPerson,
            FollowMouseClicks,
            Custom
        };

        private Editor simpleControllerEditor = null;
        private Editor navMouseClickEditor = null;

        private void DrawControlStage()
        {
            EditorGUILayout.LabelField("Control", EditorStyles.boldLabel);
            EditorWindowTools.StartIndentedSection();
            var simpleController = pcObject.GetComponent<PixelCrushers.DialogueSystem.Demo.SimpleController>();
            var navigateOnMouseClick = pcObject.GetComponent<PixelCrushers.DialogueSystem.Demo.NavigateOnMouseClick>();
            ControlStyle controlStyle = (simpleController != null)
                ? ControlStyle.SimpleThirdPerson
                : (navigateOnMouseClick != null)
                    ? ControlStyle.FollowMouseClicks
                    : ControlStyle.Custom;
            EditorGUILayout.HelpBox("How will the player control movement? The Dialogue System provides some simple control components to help you start prototyping quickly, but they are NOT intended for production use. Select Custom to provide your own control components instead of using the Dialogue System's.", MessageType.Info);
            controlStyle = (ControlStyle)EditorGUILayout.EnumPopup("Control", controlStyle);
            switch (controlStyle)
            {
                case ControlStyle.SimpleThirdPerson:
                    EditorGUILayout.HelpBox("This control script is only intended for quick prototyping.", MessageType.Info);
                    DestroyImmediate(navigateOnMouseClick);
                    navMouseClickEditor = null;
                    DrawSimpleControllerSection(simpleController ?? pcObject.AddComponent(TypeUtility.GetWrapperType(typeof(PixelCrushers.DialogueSystem.Demo.SimpleController))) as Demo.SimpleController);
                    break;
                case ControlStyle.FollowMouseClicks:
                    EditorGUILayout.HelpBox("This control script is only intended for quick prototyping.", MessageType.Info);
                    DestroyImmediate(simpleController);
                    simpleControllerEditor = null;
                    DrawNavigateOnMouseClickSection(navigateOnMouseClick ?? pcObject.AddComponent(TypeUtility.GetWrapperType(typeof(PixelCrushers.DialogueSystem.Demo.NavigateOnMouseClick))) as Demo.NavigateOnMouseClick);
                    break;
                default:
                    DestroyImmediate(simpleController);
                    DestroyImmediate(navigateOnMouseClick);
                    navMouseClickEditor = null;
                    simpleControllerEditor = null;
                    break;
            }
            if (GUILayout.Button("Select Player", GUILayout.Width(100))) Selection.activeGameObject = pcObject;
            EditorWindowTools.EndIndentedSection();
            DrawNavigationButtons(true, true, false);
        }

        private void DrawSimpleControllerSection(PixelCrushers.DialogueSystem.Demo.SimpleController simpleController)
        {
            EditorWindowTools.StartIndentedSection();
            if (simpleControllerEditor == null) simpleControllerEditor = Editor.CreateEditor(simpleController);
            simpleControllerEditor.OnInspectorGUI();
            EditorWindowTools.EndIndentedSection();
        }

        private void DrawNavigateOnMouseClickSection(PixelCrushers.DialogueSystem.Demo.NavigateOnMouseClick navigateOnMouseClick)
        {
            EditorWindowTools.StartIndentedSection();
            if (navMouseClickEditor == null) navMouseClickEditor = Editor.CreateEditor(navigateOnMouseClick);
            navMouseClickEditor.OnInspectorGUI();
            EditorWindowTools.EndIndentedSection();
        }

        private void DrawCameraStage()
        {
            EditorGUILayout.LabelField("Camera", EditorStyles.boldLabel);
            EditorWindowTools.StartIndentedSection();
            EditorGUILayout.HelpBox("You can add a smooth follow camera for quick prototyping. This script will make the camera follow the player.", MessageType.Info);
            UnityEngine.Camera playerCamera = pcObject.GetComponentInChildren<UnityEngine.Camera>() ?? UnityEngine.Camera.main;
            var smoothCamera = (playerCamera != null) ? playerCamera.GetComponent<PixelCrushers.DialogueSystem.Demo.SmoothCameraWithBumper>() : null;
            EditorGUILayout.BeginHorizontal();
            bool useSmoothCamera = EditorGUILayout.Toggle((smoothCamera != null), GUILayout.Width(ToggleWidth));
            EditorGUILayout.LabelField("Use Smooth Follow Camera", EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();
            if (useSmoothCamera)
            {
                if (playerCamera == null)
                {
                    GameObject playerCameraObject = new GameObject("Player Camera");
                    playerCameraObject.transform.parent = pcObject.transform;
                    playerCamera = playerCameraObject.AddComponent<UnityEngine.Camera>();
                    playerCamera.tag = "MainCamera";
                }
                smoothCamera = playerCamera.GetComponentInChildren<PixelCrushers.DialogueSystem.Demo.SmoothCameraWithBumper>() ?? playerCamera.gameObject.AddComponent(TypeUtility.GetWrapperType(typeof(PixelCrushers.DialogueSystem.Demo.SmoothCameraWithBumper))) as Demo.SmoothCameraWithBumper;
                EditorWindowTools.StartIndentedSection();
                if (smoothCamera.target == null)
                {
                    EditorGUILayout.HelpBox("Specify the transform (usually the head) that the camera should follow.", MessageType.Info);
                }
                smoothCamera.target = EditorGUILayout.ObjectField("Target", smoothCamera.target, typeof(Transform), true) as Transform;
                EditorWindowTools.EndIndentedSection();
            }
            else
            {
                DestroyImmediate(smoothCamera);
            }
            if (GUILayout.Button("Select Camera", GUILayout.Width(100))) Selection.activeObject = playerCamera;
            EditorWindowTools.EndIndentedSection();
            DrawNavigationButtons(true, true, false);
        }

        private void DrawTargetingStage()
        {
            EditorGUILayout.LabelField("Targeting", EditorStyles.boldLabel);
            EditorWindowTools.StartIndentedSection();
            EditorGUILayout.HelpBox("The Dialogue System includes an optional interaction system that you can use to start conversations and initiate other activity. If you want to use it, you can set it up below.", MessageType.Info);
            SelectorType selectorType = GetSelectorType();
            if (selectorType == SelectorType.None) EditorGUILayout.HelpBox("Specify how the player will target NPCs to trigger conversations and barks.", MessageType.Info);
            selectorType = (SelectorType)EditorGUILayout.EnumPopup("Target NPCs By", selectorType);
            switch (selectorType)
            {
                case SelectorType.Proximity:
                    DrawProximitySelector();
                    break;
                case SelectorType.CenterOfScreen:
                case SelectorType.MousePosition:
                case SelectorType.CustomPosition:
                    DrawSelector(selectorType);
                    break;
                default:
                    DrawNoSelector();
                    break;
            }
            EditorWindowTools.EndIndentedSection();
            EditorWindowTools.DrawHorizontalLine();
            if (GUILayout.Button("Select Player", GUILayout.Width(100))) Selection.activeGameObject = pcObject;
            DrawNavigationButtons(true, true, false);
        }

        private enum SelectorType
        {
            CenterOfScreen,
            MousePosition,
            Proximity,
            CustomPosition,
            None,
        };

        private enum MouseButtonChoice
        {
            LeftMouseButton,
            RightMouseButton
        }

        private SelectorType GetSelectorType()
        {
            if (pcObject.GetComponent<ProximitySelector>() != null)
            {
                return SelectorType.Proximity;
            }
            else
            {
                Selector selector = pcObject.GetComponent<Selector>();
                if (selector != null)
                {
                    switch (selector.selectAt)
                    {
                        case Selector.SelectAt.CenterOfScreen:
                            return SelectorType.CenterOfScreen;
                        case Selector.SelectAt.MousePosition:
                            return SelectorType.MousePosition;
                        default:
                            return SelectorType.CustomPosition;
                    }
                }
                else
                {
                    return SelectorType.None;
                }
            }
        }

        private void DrawNoSelector()
        {
            DestroyImmediate(pcObject.GetComponent<Selector>());
            DestroyImmediate(pcObject.GetComponent<ProximitySelector>());
            EditorWindowTools.StartIndentedSection();
            EditorGUILayout.HelpBox("The player will not use a Dialogue System-provided targeting component.", MessageType.None);
            EditorWindowTools.EndIndentedSection();
        }

        private void DrawProximitySelector()
        {
            DestroyImmediate(pcObject.GetComponent<Selector>());
            ProximitySelector proximitySelector = pcObject.GetComponent<ProximitySelector>() ?? pcObject.AddComponent(TypeUtility.GetWrapperType(typeof(PixelCrushers.DialogueSystem.ProximitySelector))) as ProximitySelector;
            EditorWindowTools.StartIndentedSection();
            EditorGUILayout.HelpBox("The player can target usable objects (e.g., conversations on NPCs) when inside their trigger areas. Click Select Player to customize the Proximity Selector.", MessageType.None);
            DrawSelectorUIPosition();
            proximitySelector.useKey = (KeyCode)EditorGUILayout.EnumPopup("'Use' Key", proximitySelector.useKey);
            proximitySelector.useButton = EditorGUILayout.TextField("'Use' Button", proximitySelector.useButton);
            EditorWindowTools.EndIndentedSection();
        }

        private void DrawSelector(SelectorType selectorType)
        {
            DestroyImmediate(pcObject.GetComponent<ProximitySelector>());
            Selector selector = pcObject.GetComponent<Selector>() ?? pcObject.AddComponent(TypeUtility.GetWrapperType(typeof(PixelCrushers.DialogueSystem.Selector))) as Selector;
            EditorWindowTools.StartIndentedSection();
            switch (selectorType)
            {
                case SelectorType.CenterOfScreen:
                    EditorGUILayout.HelpBox("Usable objects in the center of the screen will be targeted.", MessageType.None);
                    selector.selectAt = Selector.SelectAt.CenterOfScreen;
                    break;
                case SelectorType.MousePosition:
                    EditorGUILayout.HelpBox("Usable objects under the mouse cursor will be targeted. Specify which mouse button activates the targeted object.", MessageType.None);
                    selector.selectAt = Selector.SelectAt.MousePosition;
                    MouseButtonChoice mouseButtonChoice = string.Equals(selector.useButton, "Fire2") ? MouseButtonChoice.RightMouseButton : MouseButtonChoice.LeftMouseButton;
                    mouseButtonChoice = (MouseButtonChoice)EditorGUILayout.EnumPopup("Select With", mouseButtonChoice);
                    selector.useButton = (mouseButtonChoice == MouseButtonChoice.RightMouseButton) ? "Fire2" : "Fire1";
                    break;
                default:
                case SelectorType.CustomPosition:
                    EditorGUILayout.HelpBox("Usable objects will be targeted at a custom screen position. You are responsible for setting the Selector component's CustomPosition property.", MessageType.None);
                    selector.selectAt = Selector.SelectAt.CustomPosition;
                    break;
            }
            if (selector.reticle != null)
            {
                selector.reticle.inRange = EditorGUILayout.ObjectField("In-Range Reticle", selector.reticle.inRange, typeof(Texture2D), false) as Texture2D;
                selector.reticle.outOfRange = EditorGUILayout.ObjectField("Out-of-Range Reticle", selector.reticle.outOfRange, typeof(Texture2D), false) as Texture2D;
            }
            DrawSelectorUIPosition();
            selector.useKey = (KeyCode)EditorGUILayout.EnumPopup("'Use' Key", selector.useKey);
            selector.useButton = EditorGUILayout.TextField("'Use' Button", selector.useButton);
            EditorGUILayout.HelpBox("Click Select Player to customize the Selector.", MessageType.None);
            EditorWindowTools.EndIndentedSection();
        }

        private enum SelectorUIPositionType
        {
            TopOfScreen,
            OnSelectionTarget
        }

        private void DrawSelectorUIPosition()
        {
            SelectorFollowTarget selectorFollowTarget = pcObject.GetComponent<SelectorFollowTarget>();
            SelectorUIPositionType position = (selectorFollowTarget != null) ? SelectorUIPositionType.OnSelectionTarget : SelectorUIPositionType.TopOfScreen;
            EditorGUILayout.HelpBox("Specify where the current selection message will be displayed.", MessageType.None);
            position = (SelectorUIPositionType)EditorGUILayout.EnumPopup("Message Position", position);
            if (position == SelectorUIPositionType.TopOfScreen)
            {
                DestroyImmediate(selectorFollowTarget);
            }
            else
            {
                if (selectorFollowTarget == null) pcObject.AddComponent(TypeUtility.GetWrapperType(typeof(PixelCrushers.DialogueSystem.SelectorFollowTarget)));
            }
        }

        private void DrawTransitionStage()
        {
            EditorGUILayout.LabelField("Gameplay/Conversation Transition", EditorStyles.boldLabel);
            EditorWindowTools.StartIndentedSection();
            DialogueSystemEvents events = pcObject.GetComponent<DialogueSystemEvents>();
            SetComponentEnabledOnDialogueEvent setEnabled = pcObject.GetComponent<SetComponentEnabledOnDialogueEvent>();
            setEventsFlag = setEventsFlag || (events != null);
            setEnabledFlag = setEnabledFlag || (setEnabled != null);
            if (!(setEventsFlag || setEnabledFlag)) EditorGUILayout.HelpBox("Gameplay components, such as movement and camera control, may interfere with conversations. If you want to disable gameplay components during conversations, tick the checkbox below. There are many ways to disable gameplay components. You can add a Dialogue System Events component and configure it in the inspector, or add Dialogue System Triggers set to OnConversationStart and OnConversationEnd.", MessageType.None);

            EditorGUILayout.BeginHorizontal();
            setEventsFlag = EditorGUILayout.Toggle(setEventsFlag, GUILayout.Width(ToggleWidth));
            EditorGUILayout.LabelField("Add Dialogue System Events component", EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();

            if (!setEventsFlag && setEnabledFlag)
            {
                EditorGUILayout.BeginHorizontal();
                setEnabledFlag = EditorGUILayout.Toggle(setEnabledFlag, GUILayout.Width(ToggleWidth));
                EditorGUILayout.LabelField("Add Set Component Enabled On Dialogue Event component", EditorStyles.boldLabel);
                EditorGUILayout.EndHorizontal();
            }

            DrawDisableControlsSection();
            DrawDialogueSystemEventsSection();

            DrawShowCursorSection();
            if (GUILayout.Button("Select Player", GUILayout.Width(100))) Selection.activeGameObject = pcObject;
            EditorWindowTools.EndIndentedSection();
            DrawNavigationButtons(true, true, false);
        }

        private void DrawDisableControlsSection()
        {
            EditorWindowTools.StartIndentedSection();
            SetComponentEnabledOnDialogueEvent enabler = FindConversationEnabler();
            if (setEnabledFlag)
            {
                if (enabler == null) enabler = pcObject.AddComponent(TypeUtility.GetWrapperType(typeof(PixelCrushers.DialogueSystem.SetComponentEnabledOnDialogueEvent))) as SetComponentEnabledOnDialogueEvent;
                enabler.trigger = DialogueEvent.OnConversation;
                enabler.onStart = GetPlayerControls(enabler.onStart, Toggle.False);
                enabler.onEnd = GetPlayerControls(enabler.onEnd, Toggle.True);
                ShowDisabledComponents(enabler.onStart);
            }
            else
            {
                DestroyImmediate(enabler);
            }
            EditorWindowTools.EndIndentedSection();
        }

        private void DrawDialogueSystemEventsSection()
        {
            EditorWindowTools.StartIndentedSection();
            DialogueSystemEvents events = FindDialogueSystemEvents();
            if (setEventsFlag)
            {
                if (events == null) events = pcObject.AddComponent(TypeUtility.GetWrapperType(typeof(PixelCrushers.DialogueSystem.DialogueSystemEvents))) as DialogueSystemEvents;
                ShowDialogueSystemEvents(events);
            }
            else
            {
                    DestroyImmediate(events);
            }
            EditorWindowTools.EndIndentedSection();
        }

        private SetComponentEnabledOnDialogueEvent FindConversationEnabler()
        {
            foreach (var component in pcObject.GetComponents<SetComponentEnabledOnDialogueEvent>())
            {
                if (component.trigger == DialogueEvent.OnConversation) return component;
            }
            return null;
        }

        private DialogueSystemEvents FindDialogueSystemEvents()
        {
            return pcObject.GetComponent<DialogueSystemEvents>();
        }

        private void ShowDisabledComponents(SetComponentEnabledOnDialogueEvent.SetComponentEnabledAction[] actionList)
        {
            EditorGUILayout.LabelField("The following components will be disabled during conversations:");
            EditorWindowTools.StartIndentedSection();
            foreach (SetComponentEnabledOnDialogueEvent.SetComponentEnabledAction action in actionList)
            {
                if (action.target != null)
                {
                    EditorGUILayout.LabelField(action.target.GetType().Name);
                }
            }
            EditorWindowTools.EndIndentedSection();
            EditorGUILayout.LabelField("If you need to disable additional components, click Select Player and manually add them.");
        }

        private SetComponentEnabledOnDialogueEvent.SetComponentEnabledAction[] GetPlayerControls(SetComponentEnabledOnDialogueEvent.SetComponentEnabledAction[] oldList, Toggle state)
        {
            List<SetComponentEnabledOnDialogueEvent.SetComponentEnabledAction> actions = new List<SetComponentEnabledOnDialogueEvent.SetComponentEnabledAction>();
            if (oldList != null)
            {
                actions.AddRange(oldList);
            }
            foreach (var component in pcObject.GetComponents<MonoBehaviour>())
            {
                if (IsPlayerControlComponent(component) && !IsInActionList(actions, component))
                {
                    AddToActionList(actions, component, state);
                }
            }
            var smoothCamera = pcObject.GetComponentInChildren<PixelCrushers.DialogueSystem.Demo.SmoothCameraWithBumper>();
            if (smoothCamera == null) smoothCamera = UnityEngine.Camera.main.GetComponent<PixelCrushers.DialogueSystem.Demo.SmoothCameraWithBumper>();
            if ((smoothCamera != null) && !IsInActionList(actions, smoothCamera))
            {
                AddToActionList(actions, smoothCamera, state);
            }
            actions.RemoveAll(a => ((a == null) || (a.target == null)));
            return actions.ToArray();
        }

        private void ShowDialogueSystemEvents(DialogueSystemEvents events)
        {
            EditorGUILayout.HelpBox("Click 'Select Player' to inspect the player GameObject. Then assign components to the OnConversationStart() and OnConversationEnd() events, disabling them in OnConversationStart() and re-enabling them in OnConversationEnd().\n\nFor example, you may want to assign the player's movement control component(s) and the camera's player control component(s).", MessageType.None);
        }

        private bool IsPlayerControlComponent(MonoBehaviour component)
        {
            return (component is Selector) ||
                    (component is ProximitySelector) ||
                    (component is PixelCrushers.DialogueSystem.Demo.SimpleController) ||
                    (component is PixelCrushers.DialogueSystem.Demo.NavigateOnMouseClick);
        }

        private bool IsInActionList(List<SetComponentEnabledOnDialogueEvent.SetComponentEnabledAction> actions, MonoBehaviour component)
        {
            return (actions.Find(a => (a.target == component)) != null);
        }

        private void AddToActionList(List<SetComponentEnabledOnDialogueEvent.SetComponentEnabledAction> actions, MonoBehaviour component, Toggle state)
        {
            SetComponentEnabledOnDialogueEvent.SetComponentEnabledAction newAction = new SetComponentEnabledOnDialogueEvent.SetComponentEnabledAction();
            newAction.state = state;
            newAction.target = component;
            actions.Add(newAction);
        }

        private void DrawShowCursorSection()
        {
            EditorWindowTools.DrawHorizontalLine();
            ShowCursorOnConversation showCursor = pcObject.GetComponent<ShowCursorOnConversation>();
            bool showCursorFlag = (showCursor != null);
            if (!showCursorFlag) EditorGUILayout.HelpBox("If regular gameplay hides the mouse cursor, tick Show Mouse Cursor to enable it during conversations.", MessageType.None);
            EditorGUILayout.BeginHorizontal();
            showCursorFlag = EditorGUILayout.Toggle(showCursorFlag, GUILayout.Width(ToggleWidth));
            EditorGUILayout.LabelField("Show mouse cursor during conversations", EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();
            if (showCursorFlag)
            {
                if (showCursor == null) showCursor = pcObject.AddComponent(TypeUtility.GetWrapperType(typeof(PixelCrushers.DialogueSystem.ShowCursorOnConversation))) as ShowCursorOnConversation;
            }
            else
            {
                DestroyImmediate(showCursor);
            }
        }

        private Editor positionSaverEditor = null;

        private void DrawPersistenceStage()
        {
            EditorGUILayout.LabelField("Persistence", EditorStyles.boldLabel);
            var positionSaver = pcObject.GetComponent<PositionSaver>();
            EditorWindowTools.StartIndentedSection();
            if (positionSaver == null) EditorGUILayout.HelpBox("The player can be configured to record its position in the Dialogue System's Lua environment so it will be preserved when saving and loading games.", MessageType.Info);
            EditorGUILayout.BeginHorizontal();
            bool hasPersistentPosition = EditorGUILayout.Toggle((positionSaver != null), GUILayout.Width(ToggleWidth));
            EditorGUILayout.LabelField("Player records position for saved games", EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();
            if (hasPersistentPosition)
            {
                if (positionSaver == null)
                {
                    positionSaver = pcObject.AddComponent(TypeUtility.GetWrapperType(typeof(PixelCrushers.PositionSaver))) as PositionSaver;
                    positionSaver.key = "Player";
                }
                if (positionSaverEditor == null) positionSaverEditor = Editor.CreateEditor(positionSaver);
                positionSaverEditor.OnInspectorGUI();
            }
            else
            {
                DestroyImmediate(positionSaver);
                positionSaver = null;
                positionSaverEditor = null;
            }
            EditorWindowTools.EndIndentedSection();
            DrawNavigationButtons(true, true, false);
        }

        private void DrawReviewStage()
        {
            EditorGUILayout.LabelField("Review", EditorStyles.boldLabel);
            EditorWindowTools.StartIndentedSection();
            EditorGUILayout.HelpBox("Your Player is ready! Below is a summary of the configuration.\n\nPlayer configuration can sometimes be complex. This wizard can't anticipate every possible character design. If your player doesn't behave the way you expect, " +
                "please examine the components on the player and make adjustments as necessary. Often, you may need to add additional gameplay components, such as your own controller or camera scripts, to the Set Component Enabled On Dialogue Event component in the Gameplay/Conversation Transition section.", MessageType.Info);
            var simpleController = pcObject.GetComponent<PixelCrushers.DialogueSystem.Demo.SimpleController>();
            var navigateOnMouseClick = pcObject.GetComponent<PixelCrushers.DialogueSystem.Demo.NavigateOnMouseClick>();
            if (simpleController != null)
            {
                EditorGUILayout.LabelField("Control: Third-Person Shooter Style");
            }
            else if (navigateOnMouseClick != null)
            {
                EditorGUILayout.LabelField("Control: Follow Mouse Clicks");
            }
            else
            {
                EditorGUILayout.LabelField("Control: Custom");
            }
            switch (GetSelectorType())
            {
                case SelectorType.CenterOfScreen: EditorGUILayout.LabelField("Targeting: Center of Screen"); break;
                case SelectorType.CustomPosition: EditorGUILayout.LabelField("Targeting: Custom Position (you must set Selector.CustomPosition)"); break;
                case SelectorType.MousePosition: EditorGUILayout.LabelField("Targeting: Mouse Position"); break;
                case SelectorType.Proximity: EditorGUILayout.LabelField("Targeting: Proximity"); break;
                default: EditorGUILayout.LabelField("Targeting: None"); break;
            }
            SetComponentEnabledOnDialogueEvent enabler = FindConversationEnabler();
            if (enabler != null) ShowDisabledComponents(enabler.onStart);
            ShowCursorOnConversation showCursor = pcObject.GetComponentInChildren<ShowCursorOnConversation>();
            if (showCursor != null) EditorGUILayout.LabelField("Show Cursor During Conversations: Yes");
            var positionSaver = pcObject.GetComponentInChildren<PositionSaver>();
            EditorGUILayout.LabelField(string.Format("Save Position: {0}", (positionSaver != null) ? "Yes" : "No"));
            EditorWindowTools.EndIndentedSection();
            DrawNavigationButtons(true, true, true);
        }

    }

}
