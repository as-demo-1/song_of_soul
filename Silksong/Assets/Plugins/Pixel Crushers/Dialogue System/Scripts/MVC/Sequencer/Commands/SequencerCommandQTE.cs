// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.DialogueSystem.SequencerCommands
{

    /// <summary>
    /// Implements sequencer command: "QTE(index, duration, luaVariable, luaValue)", which presents
    /// a timed opportunity to perform a Quick Time Event.
    /// 
    /// Arguments:
    /// -# The index number of the QTE indicator. (QTE indicators are defined the dialogue UI.)
    /// -# The duration to display the QTE indicator.
    /// -# The Lua variable to set if the QTE is triggered.
    /// -# The value to set the variable to. If not trigger, the variable is set to a blank string.
    /// </summary>
    [AddComponentMenu("")] // Hide from menu.
    public class SequencerCommandQTE : SequencerCommand
    {

        private int index;
        private float stopTime;
        private string button;
        private string variableName;
        private string variableQTEValue;
        private FieldType variableType;

        public void Start()
        {
            index = GetParameterAsInt(0);
            DialogueManager.dialogueUI.ShowQTEIndicator(index);
            button = (index < DialogueManager.displaySettings.inputSettings.qteButtons.Length)
                ? DialogueManager.displaySettings.inputSettings.qteButtons[index]
                : null;
            float duration = GetParameterAsFloat(1);
            stopTime = DialogueTime.time + duration;
            variableName = GetParameter(2);
            variableQTEValue = GetParameter(3);
            variableType = GetVariableType();
            if (DialogueDebug.logInfo) Debug.Log(string.Format("{0}: Sequencer: QTE(index={1}, {2}sec, {3}, {4})", new System.Object[] { DialogueDebug.Prefix, index, duration, variableName, variableQTEValue }));
            Lua.Run(string.Format("Variable[\"{0}\"] = \"\"", new System.Object[] { variableName }), DialogueDebug.logInfo);
        }

        private FieldType GetVariableType()
        {
            float temp;
            if ((string.Equals(variableQTEValue, "true", System.StringComparison.OrdinalIgnoreCase)) ||
                (string.Equals(variableQTEValue, "false", System.StringComparison.OrdinalIgnoreCase)))
            {
                return FieldType.Boolean;
            }
            else if (float.TryParse(variableQTEValue, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out temp))
            {
                return FieldType.Number;
            }
            else
            {
                return FieldType.Text;
            }
        }

        public void Update()
        {
            if (!string.IsNullOrEmpty(button) && DialogueManager.getInputButtonDown(button))
            {
                if (DialogueDebug.logInfo) Debug.Log("Dialogue System: Sequencer: QTE(" + GetParameters() + ") triggered. Setting " + variableName + " to " + variableQTEValue);
                if (variableType == FieldType.Boolean)
                {
                    DialogueLua.SetVariable(variableName, Tools.StringToBool(variableQTEValue));
                }
                else
                {
                    DialogueLua.SetVariable(variableName, ValueAsString(variableType, variableQTEValue));
                }
                DialogueManager.instance.SendMessage(DialogueSystemMessages.OnConversationContinueAll, SendMessageOptions.DontRequireReceiver);
                Stop();
            }
            else if (DialogueTime.time >= stopTime)
            {
                Stop();
            }
        }

        private string ValueAsString(FieldType fieldType, string fieldValue)
        {
            switch (fieldType)
            {
                case FieldType.Actor:
                case FieldType.Item:
                case FieldType.Location:
                case FieldType.Number: return string.IsNullOrEmpty(fieldValue) ? "0" : fieldValue;
                case FieldType.Boolean: return string.IsNullOrEmpty(fieldValue) ? "false" : fieldValue.ToLower();
                default: return fieldValue; // Don't use DialogueLua.ValueAsString, since that would add quotes around text.
            }
        }

        public void OnDestroy()
        {
            DialogueManager.dialogueUI.HideQTEIndicator(index);
        }

    }

}
