// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.DialogueSystem.SequencerCommands
{

    /// <summary>
    /// Implements sequencer command: TextInput(textFieldUI, label, luaVariableName[, maxLength[, clear]]).
    /// 
    /// - textFieldUI: the name of GameObject with an ITextFieldUI.
    /// - label: the label text, or var=varName to use a variable value as the label.
    /// - luaVariableName: where to store the input
    /// - maxLength: max length of input to accept
    /// - clear: optional; specifies to start with an empty string instead of variable value.
    /// </summary>
    [AddComponentMenu("")] // Hide from menu.
    public class SequencerCommandTextInput : SequencerCommand
    {

        private ITextFieldUI textFieldUI = null;
        private string variableName = string.Empty;
        private bool acceptedText = false;

        /// <summary>
        /// Start the sequence and its corresponding text field UI.
        /// </summary>
        public void Start()
        {
            Transform textFieldUIObject = FindTextFieldUIObject();
            if (textFieldUIObject != null)
            {
                bool currentlyActive = textFieldUIObject.gameObject.activeSelf;
                if (!currentlyActive) textFieldUIObject.gameObject.SetActive(true);
                textFieldUI = textFieldUIObject.GetComponent(typeof(ITextFieldUI)) as ITextFieldUI;
                if (!currentlyActive) textFieldUIObject.gameObject.SetActive(false);
            }
            string labelText = GetParameter(1);
            variableName = GetParameter(2);
            int maxLength = GetParameterAsInt(3);
            bool clearField = string.Equals(GetParameter(4), "clear");
            if (DialogueDebug.logInfo) Debug.Log(string.Format("{0}: Sequencer: TextInput({1}, {2}, {3}, {4})", new System.Object[] { DialogueDebug.Prefix, Tools.GetObjectName(textFieldUIObject), labelText, variableName, maxLength }));
            if (string.IsNullOrEmpty(variableName))
            {
                if (DialogueDebug.logWarnings) Debug.Log(string.Format("{0}: Sequencer: TextInput({1}): The third parameter must be the name of a Dialogue System variable.", new System.Object[] { DialogueDebug.Prefix, GetParameters() }));
                Stop();
            }
            else if (textFieldUI == null)
            {
                if (DialogueDebug.logWarnings) Debug.Log(string.Format("{0}: Sequencer: TextInput(): Text Field UI not found on a GameObject '{1}'. Did you specify the correct GameObject name?", new System.Object[] { DialogueDebug.Prefix, GetParameter(0) }));
                Stop();
            }
            else
            {
                if (labelText.StartsWith("var="))
                {
                    labelText = DialogueLua.GetVariable(labelText.Substring(4)).asString;
                }
                string variableValue = clearField ? string.Empty : DialogueLua.GetVariable(variableName).asString;
                textFieldUI.StartTextInput(labelText, variableValue, maxLength, OnAcceptedText);
            }
        }

        private Transform FindTextFieldUIObject() // Gives preference to name match in current dialogue UI.
        {
            var uiScript = DialogueManager.dialogueUI as MonoBehaviour;
            if (uiScript != null)
            {
                var obj = FindChildRecursive(uiScript.transform, GetParameter(0));
                if (obj != null)
                {
                    return obj;
                }
            }
            return GetSubject(0);
        }

        private Transform FindChildRecursive(Transform t, string childName)
        {
            if (t != null && t.gameObject.activeInHierarchy)
            {
                if (string.Equals(t.name, childName)) return t;
                foreach (Transform child in t)
                {
                    var result = FindChildRecursive(child, childName);
                    if (result != null) return result;
                }
            }
            return null;
        }

        /// <summary>
        /// When the text field UI calls our OnAcceptedText delegate, record the value into the Lua variable and
        /// stop this sequence.
        /// </summary>
        /// <param name="text">Text.</param>
        public void OnAcceptedText(string text)
        {
            if (!acceptedText)
            {
                var variable = DialogueManager.masterDatabase.GetVariable(variableName);
                if (variable != null && variable.Type == FieldType.Number)
                {
                    // If the variable is a number, convert to float:
                    var number = Tools.StringToFloat(text);
                    DialogueLua.SetVariable(variableName, number);
                }
                else
                {
                    DialogueLua.SetVariable(variableName, text);
                    //---Was: Lua.Run(string.Format("Variable[\"{0}\"] = \"{1}\"", new System.Object[] { variableName, DialogueLua.DoubleQuotesToSingle(text) }));
                }
            }
            acceptedText = true;
            Stop();
        }

        /// <summary>
        /// Finishes this sequence. If we haven't accepted text yet, tell the text field UI to cancel.
        /// </summary>
        public void OnDestroy()
        {
            if (!acceptedText && (textFieldUI != null)) textFieldUI.CancelTextInput();
        }
    }

}
