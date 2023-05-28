#if USE_ARTICY
// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.DialogueSystem.Articy
{

    /// <summary>
    /// Implements articy:expresso functions.
    /// </summary>
    [AddComponentMenu("")] // Use wrapper.
    public class ArticyLuaFunctions : MonoBehaviour
    {
        private static bool s_registered = false;

#if UNITY_2019_3_OR_NEWER && UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void InitStaticVariables()
        {
            s_registered = false;
        }
#endif

        private void OnEnable()
        {
            if (s_registered) return;
            s_registered = true;
            Lua.RegisterFunction("getObj", this, SymbolExtensions.GetMethodInfo(() => getObj(string.Empty)));
            Lua.RegisterFunction("getObject", this, SymbolExtensions.GetMethodInfo(() => getObj(string.Empty)));
            Lua.RegisterFunction("getProp", this, SymbolExtensions.GetMethodInfo(() => getProp(string.Empty, string.Empty)));
            Lua.RegisterFunction("setProp", this, SymbolExtensions.GetMethodInfo(() => setProp(string.Empty, string.Empty, default(object))));
        }

        private void OnConversationLine(Subtitle subtitle)
        {
            var speaker = "\"Actor[\\\"" + DialogueLua.StringToTableIndex(subtitle.speakerInfo.nameInDatabase) + "\\\"]\"";
            var self = "\"Dialog[" + subtitle.dialogueEntry.id + "]\""; // Note that Dialog[#] only has SimStatus to conserve memory. getProp() uses special case to get entry fields.
            Lua.Run("speaker = " + speaker + "; self = " + self, DialogueDebug.logInfo);
        }

        public static string getObj(string objectName)
        {
            var db = DialogueManager.MasterDatabase;
            var actor = db.actors.Find(x => string.Equals(objectName, x.Name) || string.Equals(objectName, x.LookupValue("Technical Name")) || string.Equals(objectName, x.LookupValue("Articy Id")));
            if (actor != null) return "Actor[\"" + DialogueLua.StringToTableIndex(actor.Name) + "\"]";
            var item = db.items.Find(x => string.Equals(objectName, x.Name) || string.Equals(objectName, x.LookupValue("Technical Name")) || string.Equals(objectName, x.LookupValue("Articy Id")));
            if (item != null) return "Item[\"" + DialogueLua.StringToTableIndex(item.Name) + "\"]";
            var location = db.locations.Find(x => string.Equals(objectName, x.Name) || string.Equals(objectName, x.LookupValue("Technical Name")) || string.Equals(objectName, x.LookupValue("Articy Id")));
            if (location != null) return "Location[\"" + DialogueLua.StringToTableIndex(location.Name) + "\"]";
            var conversation = db.conversations.Find(x => string.Equals(objectName, x.Title) || string.Equals(objectName, x.LookupValue("Technical Name")) || string.Equals(objectName, x.LookupValue("Articy Id")));
            if (conversation != null) return "Conversation[\"" + conversation.id + "\"]";
            if (objectName.StartsWith("Dialog[")) return objectName;
            return null;
        }

        public static object getProp(string objectIdentifier, string propertyName)
        {
            if (string.IsNullOrEmpty(objectIdentifier) || string.IsNullOrEmpty(propertyName)) return string.Empty;
            if (objectIdentifier.StartsWith("Dialog[") && DialogueManager.isConversationActive)
            {
                // Handle Dialog[#] specially:
                var entryID = Tools.StringToInt(objectIdentifier.Substring(7, objectIdentifier.Length - 8));
                var conversationID = DialogueManager.currentConversationState.subtitle.dialogueEntry.conversationID;
                if (string.Equals("SimStatus", propertyName)) return DialogueLua.GetSimStatus(conversationID, entryID);
                var entry = DialogueManager.masterDatabase.GetDialogueEntry(conversationID, entryID);
                if (entry == null) return string.Empty;
                var field = Field.Lookup(entry.fields, propertyName);
                if (field == null) return string.Empty;
                if (field.type == FieldType.Number) return Tools.StringToFloat(field.value);
                else if (field.type == FieldType.Boolean) return Tools.StringToBool(field.value);
                else return field.value;
            }
            var result = Lua.Run("return " + objectIdentifier + "." + DialogueLua.StringToTableIndex(GetShortPropertyName(propertyName)), DialogueDebug.logInfo);
            if (result.isBool)
            {
                return result.asBool;
            }
            else if (result.isNumber)
            {
                return result.asInt;
            }
            else
            {
                return result.asString;
            }
        }

        public static void setProp(string objectIdentifier, string propertyName, object value)
        {
            string rightSide;
            if (value == null)
            {
                rightSide = "nil";
            }
            else if (value.GetType() == typeof(string))
            {
                rightSide = "\"" + value.ToString() + "\"";
            }
            else if (value.GetType() == typeof(bool))
            {
                rightSide = value.ToString().ToLower();
            }
            else
            {
                rightSide = value.ToString();
            }
            Lua.Run(objectIdentifier + "." + GetShortPropertyName(propertyName) + " = " + rightSide, DialogueDebug.logInfo);
        }

        private static string GetShortPropertyName(string propertyName)
        {
            // In articy, custom feature properties include the feature name.
            // In DS, they don't. Remove the feature name if present.
            if (propertyName.Contains("."))
            {
                var lastIndex = propertyName.LastIndexOf('.');
                return propertyName.Substring(lastIndex + 1);
            }
            else
            {
                return propertyName;
            }
        }
    }
}
#endif
