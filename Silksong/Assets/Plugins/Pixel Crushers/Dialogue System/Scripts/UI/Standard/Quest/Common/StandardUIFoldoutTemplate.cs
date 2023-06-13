// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// This component hooks up the elements of a Standard UI quest group template.
    /// Add it to your quest group template and assign the properties.
    /// </summary>
    [AddComponentMenu("")] // Use wrapper.
    public class StandardUIFoldoutTemplate : StandardUIContentTemplate
    {

        public UnityEngine.UI.Button foldoutButton;

        public UITextField foldoutText;

        public RectTransform interiorPanel;

        protected StandardUIInstancedContentManager contentManager { get; set; }

        public virtual void Awake()
        {
            if (foldoutButton == null && DialogueDebug.logWarnings) Debug.LogWarning("Dialogue System: Foldout Button is unassigned.", this);
            if (UITextField.IsNull(foldoutText) && DialogueDebug.logWarnings) Debug.LogWarning("Dialogue System: Foldout Text is unassigned.", this);
            if (interiorPanel == null && DialogueDebug.logWarnings) Debug.LogWarning("Dialogue System: Interior Panel is unassigned.", this);
        }

        public void Assign(string text, bool expanded)
        {
            if (contentManager == null) contentManager = new StandardUIInstancedContentManager();
            contentManager.Clear();
            name = text;
            foldoutText.text = text;
            interiorPanel.gameObject.SetActive(expanded);
        }

        public void ToggleInterior()
        {
            interiorPanel.gameObject.SetActive(!interiorPanel.gameObject.activeSelf);
        }

    }

}
