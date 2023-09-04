// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// NPC setup wizard.
    /// </summary>
    public class NPCSetupWizard : EditorWindow
    {

        [MenuItem("Tools/Pixel Crushers/Dialogue System/Wizards/NPC Setup Wizard", false, 1)]
        public static void Init()
        {
            (EditorWindow.GetWindow(typeof(NPCSetupWizard), false, "NPC Setup") as NPCSetupWizard).minSize = new Vector2(700, 500);
        }

        // Private fields for the window:

        private enum Stage
        {
            SelectNPC,
            SelectDB,
            Actor,
            Interaction,
            Actions,
            Persistence,
            Review
        };

        private Stage stage = Stage.SelectNPC;

        private string[] stageLabels = new string[] { "NPC", "Database", "Actor", "Interaction", "Actions", "Persistence", "Review" };

        private const float ToggleWidth = 16;

        private GameObject npcObject = null;

        private DialogueDatabase database = null;
        private DialogueSystemController dialogueManager = null;

        private Vector2 scrollPosition = Vector2.zero;

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
            if (npcObject == null) stage = Stage.SelectNPC;
            switch (stage)
            {
                case Stage.SelectNPC: DrawSelectNPCStage(); break;
                case Stage.SelectDB: DrawSelectDBStage(); break;
                case Stage.Actor: DrawActorStage(); break;
                case Stage.Interaction: DrawTargetingStage(); break;
                case Stage.Actions: DrawActionsStage(); break;
                case Stage.Persistence: DrawPersistenceStage(); break;
                case Stage.Review: DrawReviewStage(); break;
            }
            EditorGUILayout.EndScrollView();
            if ((npcObject != null) && GUI.changed) EditorUtility.SetDirty(npcObject);
        }

        private void DrawSelectNPCStage()
        {
            EditorGUILayout.LabelField("Select NPC Object", EditorStyles.boldLabel);
            EditorWindowTools.StartIndentedSection();
            EditorGUILayout.HelpBox("This wizard will help you configure a Non Player Character (NPC) or other interactable object to work with the Dialogue System. First, assign the NPC's GameObject below.", MessageType.Info);
            npcObject = EditorGUILayout.ObjectField("NPC Object", npcObject, typeof(GameObject), true) as GameObject;
            EditorWindowTools.EndIndentedSection();
            DrawNavigationButtons(false, (npcObject != null), false);
        }

        private void DrawSelectDBStage()
        {
            EditorGUILayout.LabelField("Select Dialogue Database", EditorStyles.boldLabel);
            EditorWindowTools.StartIndentedSection();
            if (dialogueManager == null)
            {
                dialogueManager = GameObjectUtility.FindFirstObjectByType<DialogueSystemController>();
            }
            if (database == null && dialogueManager != null)
            {
                database = dialogueManager.initialDatabase;
            }
            if (dialogueManager == null || dialogueManager.initialDatabase == null || database != dialogueManager.initialDatabase)
            {
                EditorGUILayout.HelpBox("Assign the dialogue database that the NPC will use for conversations and barks.", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("Assign the dialogue database that the NPC will use for conversations and barks. Since your scene has a Dialogue Manager, by default we'll use the database assigned to the Dialogue Manager.", MessageType.Info);
            }
            DialogueDatabase newDatabase = EditorGUILayout.ObjectField("Dialogue Database", database, typeof(DialogueDatabase), true) as DialogueDatabase;
            if (newDatabase != database)
            {
                database = newDatabase;
            }
            if (dialogueManager != null && database != null && dialogueManager.initialDatabase != database)
            {
                EditorGUILayout.HelpBox("This is not the initial database assigned to the Dialogue Manager. Remember to load this database at runtime before using the NPC. You can use the Extra Databases component or DialogueManager.AddDatabase() in script. Also make sure the internal IDs are unique across databases. You can use the Unique ID Tool to do this.", MessageType.Warning);
            }
            EditorWindowTools.EndIndentedSection();
            DrawNavigationButtons(true, (database != null), false);
        }

        private Editor dialogueActorEditor = null;

        private void DrawActorStage()
        {

            EditorGUILayout.LabelField("Dialogue Actor Component (Optional)", EditorStyles.boldLabel);
            EditorWindowTools.StartIndentedSection();
            EditorGUILayout.HelpBox("A Dialogue Actor component lets you specify which actor in the database this GameObject is associated with if the GameObject doesn't have the same name as the actor. You can also set actor-specific UI settings.", MessageType.Info);
            var dialogueActor = npcObject.GetComponent<DialogueActor>();
            var hasDialogueActor = dialogueActor != null;
            if (dialogueActor == null) dialogueActorEditor = null;
            var useDialogueActor = EditorGUILayout.Toggle("Use Dialogue Actor", hasDialogueActor);
            if (useDialogueActor && !hasDialogueActor)
            {
                dialogueActor = npcObject.AddComponent(TypeUtility.GetWrapperType(typeof(PixelCrushers.DialogueSystem.DialogueActor))) as DialogueActor;
                hasDialogueActor = true;
            }
            else if (!useDialogueActor && hasDialogueActor)
            {
                DestroyImmediate(dialogueActor);
                dialogueActor = null;
                hasDialogueActor = false;
                dialogueActorEditor = null;
            }
            if (hasDialogueActor)
            {
                if (dialogueActorEditor == null) dialogueActorEditor = Editor.CreateEditor(dialogueActor);
                dialogueActorEditor.OnInspectorGUI();
            }

            EditorWindowTools.DrawHorizontalLine();
            var defaultCameraAngle = npcObject.GetComponent<DefaultCameraAngle>();
            var hasDefaultCameraAngle = defaultCameraAngle != null;
            EditorGUILayout.BeginHorizontal();
            var useDefaultCameraAngle = EditorGUILayout.Toggle("Override Default Camera Angle", hasDefaultCameraAngle);
            EditorGUILayout.HelpBox("The default camera angle is 'Closeup'. You can override it here.", MessageType.None);
            EditorGUILayout.EndHorizontal();
            if (useDefaultCameraAngle && !hasDefaultCameraAngle)
            {
                defaultCameraAngle = npcObject.AddComponent(TypeUtility.GetWrapperType(typeof(PixelCrushers.DialogueSystem.DefaultCameraAngle))) as DefaultCameraAngle;
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
                EditorGUILayout.HelpBox("Specify the default camera angle for this NPC.", MessageType.None);
                EditorGUILayout.EndHorizontal();
            }

            DrawBarkGroupSection();
            if (GUILayout.Button("Select NPC", GUILayout.Width(100))) Selection.activeGameObject = npcObject;
            EditorWindowTools.EndIndentedSection();
            DrawNavigationButtons(true, (database != null), false);
        }

        private Editor barkOnIdleEditor = null;

        private bool DrawBarkOnIdleSection()
        {
            EditorGUILayout.BeginHorizontal();
            BarkOnIdle barkOnIdle = npcObject.GetComponentInChildren<BarkOnIdle>();
            bool hasBarkOnIdle = EditorGUILayout.Toggle((barkOnIdle != null), GUILayout.Width(ToggleWidth));
            EditorGUILayout.LabelField("NPC barks on a timed basis", EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();
            if (hasBarkOnIdle)
            {
                if (barkOnIdle == null) barkOnIdle = npcObject.AddComponent(TypeUtility.GetWrapperType(typeof(PixelCrushers.DialogueSystem.BarkOnIdle))) as BarkOnIdle;
                if (barkOnIdleEditor == null) barkOnIdleEditor = Editor.CreateEditor(barkOnIdle);
                barkOnIdleEditor.OnInspectorGUI();
            }
            else
            {
                DestroyImmediate(barkOnIdle);
                barkOnIdleEditor = null;
            }
            return hasBarkOnIdle;
        }

        private bool DrawBarkUISection()
        {
            EditorWindowTools.DrawHorizontalLine();
            EditorGUILayout.BeginHorizontal();
            IBarkUI barkUI = npcObject.GetComponentInChildren(typeof(IBarkUI)) as IBarkUI;
            bool hasBarkUI = (barkUI != null);
            var dialogueActor = npcObject.GetComponent<DialogueActor>();
            if (!hasBarkUI)
            {
                dialogueActor = npcObject.GetComponent<DialogueActor>();
                if (dialogueActor != null)
                {
                    hasBarkUI = (dialogueActor.barkUISettings.barkUI != null);
                    if (!hasBarkUI)
                    {
                        dialogueActor.barkUISettings.barkUI = dialogueActor.GetComponentInChildren<AbstractBarkUI>();
                        hasBarkUI = (dialogueActor.barkUISettings.barkUI != null);
                    }
                }
            }
            EditorGUILayout.LabelField("Bark UI", EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();
            EditorWindowTools.StartIndentedSection();
            if (hasBarkUI)
            {
                EditorGUILayout.HelpBox("The NPC has a bark UI, so it will be able to display barks.", MessageType.None);
            }
            else
            {
                if (dialogueActor != null)
                {
                    EditorGUILayout.HelpBox("The NPC needs a bark UI to be able to display barks. Assign a bark UI instance or bark UI prefab below. This will be assigned to the NPC's Dialogue Actor component.", MessageType.Info);
                    EditorGUILayout.BeginHorizontal();
                    dialogueActor.barkUISettings.barkUI = EditorGUILayout.ObjectField("Bark UI", dialogueActor.barkUISettings.barkUI, typeof(AbstractBarkUI), true) as AbstractBarkUI;
                    if (GUILayout.Button("Add Basic Bark UI", GUILayout.Width(180)))
                    {
                        var basicBarkUI = npcObject.AddComponent<PixelCrushers.DialogueSystem.UnityGUI.UnityBarkUI>();
                        dialogueActor.barkUISettings.barkUI = basicBarkUI;
                    }
                    EditorGUILayout.EndHorizontal();
                }
                else
                {
                    EditorGUILayout.HelpBox("The NPC needs a bark UI to be able to display barks. You can add a Dialogue Actor component, which will allow you to assign a bark UI prefab that will be instantiated at runtime, or click Basic Bark UI to add very basic bark UI.", MessageType.Info);
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("Add Dialogue Actor", GUILayout.Width(180)))
                    {
                        npcObject.AddComponent<PixelCrushers.DialogueSystem.UnityGUI.UnityBarkUI>();
                    }
                    if (GUILayout.Button("Add Basic Bark UI", GUILayout.Width(180)))
                    {
                        npcObject.AddComponent<PixelCrushers.DialogueSystem.UnityGUI.UnityBarkUI>();
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorWindowTools.EndIndentedSection();
            return hasBarkUI;
        }

        private void DrawBarkGroupSection()
        {
            EditorWindowTools.DrawHorizontalLine();
            EditorGUILayout.LabelField("Bark Group", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("If an NPC is a member of a bark group, then when one member barks, the others will hide their barks to reduce audiovisual clutter.", MessageType.None);
            var barkGroupMember = npcObject.GetComponent<BarkGroupMember>();
            var hasBarkGroupMember = barkGroupMember != null;

            var wantsBarkGroupMember = EditorGUILayout.Toggle("Is Bark Group Member", hasBarkGroupMember);
            if (wantsBarkGroupMember && !hasBarkGroupMember)
            {
                barkGroupMember = npcObject.AddComponent(TypeUtility.GetWrapperType(typeof(PixelCrushers.DialogueSystem.BarkGroupMember))) as BarkGroupMember;
                hasBarkGroupMember = true;
            }
            else if (!wantsBarkGroupMember && hasBarkGroupMember)
            {
                DestroyImmediate(barkGroupMember);
                hasBarkGroupMember = false;
            }
            if (hasBarkGroupMember)
            {
                barkGroupMember.groupId = EditorGUILayout.TextField(new GUIContent("Group", "Name of group this NPC belongs to. Can contain [lua] and [var] markup tags."), barkGroupMember.groupId);
            }
        }

        private Editor dialogueSystemTriggerEditor = null;

        private void DrawActionsStage()
        {
            EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);
            EditorWindowTools.StartIndentedSection();
            EditorGUILayout.LabelField("Dialogue System Trigger", EditorStyles.boldLabel);
            var dialogueSystemTrigger = npcObject.GetComponent<DialogueSystemTrigger>();
            EditorGUILayout.HelpBox("A Dialogue System Trigger runs actions when an event occurs, such as when the player uses the NPC.", MessageType.None);
            var hasDialogueSystemTrigger = (dialogueSystemTrigger != null);
            hasDialogueSystemTrigger = EditorGUILayout.Toggle("Dialogue System Trigger", hasDialogueSystemTrigger);
            if (hasDialogueSystemTrigger && dialogueSystemTrigger == null)
            {
                dialogueSystemTrigger = npcObject.AddComponent(TypeUtility.GetWrapperType(typeof(PixelCrushers.DialogueSystem.DialogueSystemTrigger))) as DialogueSystemTrigger;
            }
            else if (!hasDialogueSystemTrigger && dialogueSystemTrigger != null)
            {
                DestroyImmediate(dialogueSystemTrigger);
                dialogueSystemTrigger = null;
                hasDialogueSystemTrigger = false;
                dialogueSystemTriggerEditor = null;
            }
            if (hasDialogueSystemTrigger)
            {
                if (dialogueSystemTriggerEditor == null) dialogueSystemTriggerEditor = Editor.CreateEditor(dialogueSystemTrigger);
                dialogueSystemTriggerEditor.OnInspectorGUI();
            }

            EditorWindowTools.DrawHorizontalLine();
            EditorGUILayout.LabelField("Dialogue System Events", EditorStyles.boldLabel);
            var dialogueSystemEvents = npcObject.GetComponent<DialogueSystemEvents>();
            if (dialogueSystemEvents != null)
            {
                EditorGUILayout.HelpBox("A Dialogue System Events component is a handy way to configure activity such as disabling components and setting animator states when Dialogue System events occur. To configure Dialogue System Events in the Inspector view, click Select NPC.", MessageType.None);
            }
            else
            {
                EditorGUILayout.HelpBox("A Dialogue System Events component is a handy way to configure activity such as disabling components and setting animator states when Dialogue System events occur.", MessageType.None);
            }
            var hasDialogueSystemEvents = (dialogueSystemEvents != null);
            hasDialogueSystemEvents = EditorGUILayout.Toggle("Dialogue System Events", dialogueSystemEvents);
            if (hasDialogueSystemEvents && dialogueSystemEvents == null)
            {
                dialogueSystemEvents = npcObject.AddComponent(TypeUtility.GetWrapperType(typeof(PixelCrushers.DialogueSystem.DialogueSystemEvents))) as DialogueSystemEvents;
            }
            else if (!hasDialogueSystemEvents && dialogueSystemEvents != null)
            {
                DestroyImmediate(dialogueSystemEvents);
                dialogueSystemEvents = null;
                hasDialogueSystemEvents = false;
            }

            EditorWindowTools.DrawHorizontalLine();
            DrawBarkOnIdleSection();

            if (GUILayout.Button("Select NPC", GUILayout.Width(100))) Selection.activeGameObject = npcObject;
            EditorWindowTools.EndIndentedSection();
            DrawNavigationButtons(true, true, false);
        }

        private enum Dimension { In2D, In3D }
        private Dimension dimension = Dimension.In3D;
        private bool lookedUpDimension = false;
        private Editor usableEditor = null;

        private void DrawTargetingStage()
        {
            EditorGUILayout.LabelField("Interaction", EditorStyles.boldLabel);
            EditorWindowTools.StartIndentedSection();
            EditorGUILayout.HelpBox("If your player will be configured to use interaction components such as Selector or Proximity Selector, you can configure this NPC to be usable by the player.", MessageType.Info);
            if (!lookedUpDimension)
            {
                lookedUpDimension = true;
                dimension = (npcObject.GetComponent<Collider2D>() != null) ? Dimension.In2D : Dimension.In3D;
            }
            dimension = (Dimension)EditorGUILayout.EnumPopup(new GUIContent("Scene Is...", "Is this NPC in a 2D or 3D scene?"), dimension);

            var usable = npcObject.GetComponent<Usable>();
            var hasUsable = (usable != null);
            hasUsable = EditorGUILayout.Toggle("Usable By Player", hasUsable);
            if (usable == null && hasUsable)
            {
                usable = npcObject.AddComponent(TypeUtility.GetWrapperType(typeof(PixelCrushers.DialogueSystem.Usable))) as Usable;
            }
            else if (usable != null && !hasUsable)
            {
                DestroyImmediate(usable);
                usable = null;
                usableEditor = null;
            }
            if (usable != null)
            {
                if (usableEditor == null) usableEditor = Editor.CreateEditor(usable);
                usableEditor.OnInspectorGUI();
            }

            var hasTriggerCollider = false;
            var hasNontriggerCollider = false;
            if (dimension == Dimension.In2D)
            {
                var colliders = npcObject.GetComponents<Collider2D>();
                foreach (var collider in colliders)
                {
                    if (collider.isTrigger) hasTriggerCollider = true; else hasNontriggerCollider = true;
                }
            }
            else
            {
                var colliders = npcObject.GetComponents<Collider>();
                foreach (var collider in colliders)
                {
                    if (collider.isTrigger) hasTriggerCollider = true; else hasNontriggerCollider = true;
                }
            }

            if (hasNontriggerCollider)
            {
                EditorGUILayout.HelpBox("This NPC has a collider. If you want to edit its properties, click Select NPC.", MessageType.None);
            }
            else
            {
                if (hasUsable)
                {
                    EditorGUILayout.HelpBox("This NPC has a Usable component. It also needs a collider so the player's Selector or Proximity Selector can detect it.", MessageType.None);
                }
                else
                {
                    EditorGUILayout.HelpBox("If this NPC will take action on collision enter/exit, it needs a collider.", MessageType.None);
                }
                hasTriggerCollider = EditorGUILayout.Toggle("Add Collider", hasTriggerCollider);
                if (hasTriggerCollider)
                {
                    if (dimension == Dimension.In2D)
                    {
                        npcObject.AddComponent<CircleCollider2D>();
                        var rb = npcObject.GetComponent<Rigidbody2D>();
                        if (rb == null)
                        {
                            rb = npcObject.AddComponent<Rigidbody2D>();
                            rb.isKinematic = true;
                        }
                    }
                    else
                    {
                        npcObject.AddComponent<CapsuleCollider>();
                        var rb = npcObject.GetComponent<Rigidbody>();
                        if (rb == null)
                        {
                            rb = npcObject.AddComponent<Rigidbody>();
                            rb.isKinematic = true;
                        }
                    }
                }
            }

            if (hasTriggerCollider)
            {
                EditorGUILayout.HelpBox("This NPC has a trigger collider. If you want to edit its properties, click Select NPC.", MessageType.None);
            }
            else
            {
                EditorGUILayout.HelpBox("If this NPC will take action on trigger enter/exit, it needs a trigger collider.", MessageType.None);
                hasTriggerCollider = EditorGUILayout.Toggle("Add Trigger Collider", hasTriggerCollider);
                if (hasTriggerCollider)
                {
                    if (dimension == Dimension.In2D)
                    {
                        var circleCollider = npcObject.AddComponent<CircleCollider2D>();
                        circleCollider.isTrigger = true;
                        circleCollider.radius = 1.5f;
                        var rb = npcObject.GetComponent<Rigidbody2D>();
                        if (rb == null)
                        {
                            rb = npcObject.AddComponent<Rigidbody2D>();
                            rb.isKinematic = true;
                        }
                    }
                    else
                    {
                        SphereCollider sphereCollider = npcObject.AddComponent<SphereCollider>();
                        sphereCollider.isTrigger = true;
                        sphereCollider.radius = 1.5f;
                        var rb = npcObject.GetComponent<Rigidbody>();
                        if (rb == null)
                        {
                            rb = npcObject.AddComponent<Rigidbody>();
                            rb.isKinematic = true;
                        }
                    }
                }
            }
            if (GUILayout.Button("Select NPC", GUILayout.Width(100))) Selection.activeGameObject = npcObject;
            EditorWindowTools.EndIndentedSection();
            DrawNavigationButtons(true, true, false);
        }

        //private Editor positionSaverEditor = null;

        private void DrawPersistenceStage()
        {
            EditorGUILayout.LabelField("Persistence", EditorStyles.boldLabel);
            var positionSaver = npcObject.GetComponent<PositionSaver>();
            EditorWindowTools.StartIndentedSection();
            EditorGUILayout.HelpBox("The NPC can be configured to record its position in the Dialogue System's Lua environment so it will be preserved when saving and loading games.", MessageType.Info);
            EditorGUILayout.BeginHorizontal();
            bool hasPositionSaver = EditorGUILayout.Toggle((positionSaver != null), GUILayout.Width(ToggleWidth));
            EditorGUILayout.LabelField("NPC records position for saved games", EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();
            if (hasPositionSaver)
            {
                if (positionSaver == null) positionSaver = npcObject.AddComponent(TypeUtility.GetWrapperType(typeof(PixelCrushers.PositionSaver))) as PositionSaver;
            }
            else
            {
                DestroyImmediate(positionSaver);
                positionSaver = null;
                //positionSaverEditor = null;
                hasPositionSaver = false;
            }
            if (hasPositionSaver)
            {
                //if (positionSaverEditor == null) positionSaverEditor = Editor.CreateEditor(positionSaver);
                //positionSaverEditor.OnInspectorGUI();
                positionSaver.key = EditorGUILayout.TextField(new GUIContent("Save Under Key", "Each NPC's position saver needs a unique key."), positionSaver.key);
                positionSaver.appendSaverTypeToKey = EditorGUILayout.Toggle(new GUIContent("Append _PositionSaver", "Append '_PositionSaver' to the key to distinguish it from other Saver components on the NPC."), positionSaver.appendSaverTypeToKey);
                positionSaver.saveAcrossSceneChanges = EditorGUILayout.Toggle(new GUIContent("Save In Scene Changes", "Tick to remember position when changing scenes. Untick only to remember when saving and loading games in NPC's scene (which saves memory)."), positionSaver.saveAcrossSceneChanges);
            }
            if (GUILayout.Button("Select NPC", GUILayout.Width(100))) Selection.activeGameObject = npcObject;
            EditorWindowTools.EndIndentedSection();
            DrawNavigationButtons(true, true, false);
        }

        private void DrawReviewStage()
        {
            EditorGUILayout.LabelField("Review", EditorStyles.boldLabel);
            EditorWindowTools.StartIndentedSection();
            EditorGUILayout.HelpBox("Your NPC is ready! Below is a summary of your NPC's configuration.\n\nNPC configuration can sometimes be complex. This wizard can't anticipate every possible character design. If your NPC doesn't behave the way you expect, please examine the components on the NPC and make adjustments as necessary.", MessageType.Info);
            var dialogueActor = npcObject.GetComponent<DialogueActor>();
            if (dialogueActor != null)
            {
                EditorGUILayout.LabelField("Dialogue Actor: " + dialogueActor.actor);
            }
            if (npcObject.GetComponent<Usable>() != null)
            {
                EditorGUILayout.LabelField("Usable: Yes");
            }
            var dialogueSystemTrigger = npcObject.GetComponentInChildren<DialogueSystemTrigger>();
            if (dialogueSystemTrigger != null)
            {
                EditorGUILayout.LabelField("Dialogue System Trigger: " + GetDialogueSystemTriggerSummary(dialogueSystemTrigger) + " " + dialogueSystemTrigger.trigger);
            }
            BarkOnIdle barkOnIdle = npcObject.GetComponentInChildren<BarkOnIdle>();
            if (barkOnIdle != null)
            {
                EditorGUILayout.LabelField(string.Format("Timed Bark: '{0}' ({1}) every {2}-{3} seconds", barkOnIdle.conversation, barkOnIdle.barkOrder, barkOnIdle.minSeconds, barkOnIdle.maxSeconds));
            }
            PositionSaver positionSaver = npcObject.GetComponentInChildren<PositionSaver>();
            EditorGUILayout.LabelField(string.Format("Save Position: {0}", (positionSaver != null) ? "Yes" : "No"));
            EditorWindowTools.EndIndentedSection();
            DrawNavigationButtons(true, true, true);
        }

        private string GetDialogueSystemTriggerSummary(DialogueSystemTrigger trigger)
        {
            if (trigger == null) return string.Empty;
            if (!string.IsNullOrEmpty(trigger.conversation)) return "Conversation '" + trigger.conversation + "'";
            if (trigger.barkSource == DialogueSystemTrigger.BarkSource.Conversation && !string.IsNullOrEmpty(trigger.barkConversation)) return "Cark conversation '" + trigger.barkConversation + "'";
            if (trigger.barkSource == DialogueSystemTrigger.BarkSource.Text && !string.IsNullOrEmpty(trigger.barkText)) return "Bark '" + trigger.barkText + "'";
            if (!string.IsNullOrEmpty(trigger.sequence)) return "Play sequence '" + trigger.sequence + "'";
            if (!string.IsNullOrEmpty(trigger.luaCode)) return "Run Lua '" + trigger.luaCode + "'";
            if (!string.IsNullOrEmpty(trigger.questName))
            {
                if (trigger.setQuestState && trigger.setQuestEntryState) return "Set quest '" + trigger.questName + "' " + trigger.questState + " & entry " + trigger.questEntryNumber + " " + trigger.questEntryState;
                if (trigger.setQuestState) return "Set quest '" + trigger.questName + "' " + trigger.questState;
                if (trigger.setQuestEntryState && trigger.setQuestEntryState) return "Set quest '" + trigger.questName + "' entry " + trigger.questEntryNumber + " " + trigger.questEntryState;
            }
            return string.Empty;
        }

    }

}
