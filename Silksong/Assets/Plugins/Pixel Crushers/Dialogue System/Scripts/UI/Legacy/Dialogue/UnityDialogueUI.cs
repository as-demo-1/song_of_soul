using UnityEngine;

namespace PixelCrushers.DialogueSystem.UnityGUI
{

    /// <summary>
    /// This component implements IDialogueUI using Unity GUI. It's based on AbstractDialogueUI
    /// and compiles the Unity GUI versions of the controls defined in UnitySubtitleControls, 
    /// UnityResponseMenuControls, etc.
    ///
    /// To use this component, build a GUI layout (or use a pre-built one in the Prefabs folder)
    /// and assign the GUI control properties. You can save a UnityDialogueUI as a prefab and 
    /// assign the prefab or an instance to the DialogueManager.
    /// </summary>
    [AddComponentMenu("")] // Deprecated
    public class UnityDialogueUI : AbstractDialogueUI
    {

        /// <summary>
        /// The GUI root.
        /// </summary>
        public GUIRoot guiRoot;

        /// <summary>
        /// The dialogue controls.
        /// </summary>
        public UnityDialogueControls dialogue;

        /// <summary>
        /// The QTE (Quick Time Event) indicators.
        /// </summary>
        public GUIControl[] qteIndicators;

        /// <summary>
        /// The alert message controls.
        /// </summary>
        public UnityAlertControls alert;

        private UnityUIRoot unityUIRoot;

        private UnityQTEControls unityQTEControls;

        public override AbstractUIRoot uiRootControls
        {
            get { return unityUIRoot; }
        }

        public override AbstractDialogueUIControls dialogueControls
        {
            get { return dialogue; }
        }

        public override AbstractUIQTEControls qteControls
        {
            get { return unityQTEControls; }
        }

        public override AbstractUIAlertControls alertControls
        {
            get { return alert; }
        }

        /// <summary>
        /// Sets up the component.
        /// </summary>
        public override void Awake()
        {
            base.Awake();
            FindControls();
        }

        public override void Open()
        {
            base.Open();
            dialogue.responseMenu.Hide();
        }

        /// <summary>
        /// Makes sure we have a GUIRoot and logs warnings if any critical controls are unassigned.
        /// </summary>
        private void FindControls()
        {
            if (guiRoot == null) guiRoot = GetComponentInChildren<GUIRoot>();
            unityUIRoot = new UnityUIRoot(guiRoot);
            unityQTEControls = new UnityQTEControls(qteIndicators);
            SetupContinueButton(dialogue.npcSubtitle.continueButton);
            SetupContinueButton(dialogue.pcSubtitle.continueButton);
            SetupContinueButton(alert.continueButton);
            if (DialogueDebug.logErrors)
            {
                if (guiRoot == null) Debug.LogError(string.Format("{0}: UnityDialogueUI can't find GUIRoot and won't be able to display dialogue.", new System.Object[] { DialogueDebug.Prefix }));
                if (DialogueDebug.logWarnings)
                {
                    if (dialogue.npcSubtitle.line == null) Debug.LogWarning(string.Format("{0}: UnityDialogueUI NPC Subtitle Line needs to be assigned.", new System.Object[] { DialogueDebug.Prefix }));
                    if (dialogue.pcSubtitle.line == null) Debug.LogWarning(string.Format("{0}: UnityDialogueUI PC Subtitle Line needs to be assigned.", new System.Object[] { DialogueDebug.Prefix }));
                    if (dialogue.responseMenu.buttons.Length == 0) Debug.LogWarning(string.Format("{0}: UnityDialogueUI Response buttons need to be assigned.", new System.Object[] { DialogueDebug.Prefix }));
                    if (alert.line == null) Debug.LogWarning(string.Format("{0}: UnityDialogueUI Alert Line needs to be assigned.", new System.Object[] { DialogueDebug.Prefix }));
                }
            }
        }

        private void SetupContinueButton(GUIButton continueButton)
        {
            if (continueButton != null)
            {
                if (string.IsNullOrEmpty(continueButton.message) || string.Equals(continueButton.message, "OnClick")) continueButton.message = "OnContinue";
                if (continueButton.target == null) continueButton.target = this.transform;
            }
        }

    }

}
