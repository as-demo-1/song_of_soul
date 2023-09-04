// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;
using System;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// This custom property drawer for DialogueSystemTriggerEvent reorders the 
    /// popup for trigger events. As the Dialogue System has grown, trigger 
    /// events were added to the end rather than reordering the enum (which
    /// would break serialization in existing projects).
    /// </summary>
    [CustomPropertyDrawer(typeof(DialogueSystemTriggerEventAttribute))]
    public class DialogueSystemTriggerEventDrawer : PropertyDrawer
    {

        private static string[] triggerNames = {
            "None",					// 0
			"On Use",				// 1
			"On Start",				// 2
			"On Enable",			// 3
			"On Disable",			// 4
			"On Destroy",			// 5

			"On Trigger Enter",		// 6
			"On Trigger Exit",		// 7
			"On Collision Enter",	// 8
			"On Collision Exit",	// 9

            "On Bark Start",        // 10
			"On Bark End",			// 11
            "On Conversation Start",// 12
			"On Conversation End",	// 13
            "On Sequence Start",    // 14
			"On Sequence End",		// 15

            "On Save Data Applied"  // 16
		};

        private static int NameIndexToEnumValueIndex(int nameIndex)
        {
            switch (nameIndex)
            {
                case 1:
                    return 5; // DialogueSystemTriggerEvent.OnUse;
                case 2:
                    return 4; // DialogueSystemTriggerEvent.OnStart;
                case 3:
                    return 6; // DialogueSystemTriggerEvent.OnEnable;
                case 4:
                    return 8; // DialogueSystemTriggerEvent.OnDisable;
                case 5:
                    return 9; // DialogueSystemTriggerEvent.OnDestroy;
                case 6:
                    return 3; // DialogueSystemTriggerEvent.OnTriggerEnter;
                case 7:
                    return 7; // DialogueSystemTriggerEvent.OnTriggerExit;
                case 8:
                    return 11; // DialogueSystemTriggerEvent.OnCollisionEnter;
                case 9:
                    return 12; // DialogueSystemTriggerEvent.OnCollisionExit;
                case 10:
                    return 13; // DialogueSystemTriggerEvent.OnBarkStart;
                case 11:
                    return 0; // DialogueSystemTriggerEvent.OnBarkEnd;
                case 12:
                    return 14; // DialogueSystemTriggerEvent.OnConversationStart;
                case 13:
                    return 1; // DialogueSystemTriggerEvent.OnConversationEnd;
                case 14:
                    return 15; // DialogueSystemTriggerEvent.OnSequenceStart;
                case 15:
                    return 2; // DialogueSystemTriggerEvent.OnSequenceEnd;
                case 16:
                    return 16; //DialogueSystemTriggerEvent.OnSaveDataApplied;
                default:
                    return 10; // DialogueSystemTriggerEvent.None;
            }
        }

        private static int EnumValueIndexToNameIndex(int enumValueIndex)
        {
            switch (enumValueIndex)
            {
                case 5: // DialogueSystemTriggerEvent.OnUse:
                    return 1;
                case 4: // DialogueSystemTriggerEvent.OnStart:
                    return 2;
                case 6: // DialogueSystemTriggerEvent.OnEnable:
                    return 3;
                case 8: // DialogueSystemTriggerEvent.OnDisable:
                    return 4;
                case 9: // DialogueSystemTriggerEvent.OnDestroy:
                    return 5;
                case 3: // DialogueSystemTriggerEvent.OnTriggerEnter:
                    return 6;
                case 7: // DialogueSystemTriggerEvent.OnTriggerExit:
                    return 7;
                case 11: // DialogueSystemTriggerEvent.OnCollisionEnter:
                    return 8;
                case 12: // DialogueSystemTriggerEvent.OnCollisionExit:
                    return 9;
                case 0: // DialogueSystemTriggerEvent.OnBarkEnd:
                    return 11;
                case 1: // DialogueSystemTriggerEvent.OnConversationEnd:
                    return 13;
                case 2: // DialogueSystemTriggerEvent.OnSequenceEnd:
                    return 15;
                case 13: // DialogueSystemTriggerEvent.OnBarkStart:
                    return 10;
                case 14: // DialogueSystemTriggerEvent.OnConversationStart:
                    return 12;
                case 15: // DialogueSystemTriggerEvent.OnSequenceStart:
                    return 14;
                case 16: // DialogueSystemTriggerEvent.OnSaveDataApplied:
                    return 16;
                default:
                    return 0; // DialogueSystemTriggerEvent.None
            }
        }

        public static int DialogueSystemTriggerEventToEnumValueIndex(DialogueSystemTriggerEvent trigger)
        {
            switch (trigger)
            {
                case DialogueSystemTriggerEvent.OnBarkEnd:
                    return 0;
                case DialogueSystemTriggerEvent.OnConversationEnd:
                    return 1;
                case DialogueSystemTriggerEvent.OnSequenceEnd:
                    return 2;
                case DialogueSystemTriggerEvent.OnTriggerEnter:
                    return 3;
                case DialogueSystemTriggerEvent.OnStart:
                    return 4;
                case DialogueSystemTriggerEvent.OnUse:
                    return 5;
                case DialogueSystemTriggerEvent.OnEnable:
                    return 6;
                case DialogueSystemTriggerEvent.OnTriggerExit:
                    return 7;
                case DialogueSystemTriggerEvent.OnDisable:
                    return 8;
                case DialogueSystemTriggerEvent.OnDestroy:
                    return 9;
                case DialogueSystemTriggerEvent.None:
                    return 10;
                case DialogueSystemTriggerEvent.OnCollisionEnter:
                    return 11;
                case DialogueSystemTriggerEvent.OnCollisionExit:
                    return 12;
                case DialogueSystemTriggerEvent.OnBarkStart:
                    return 13;
                case DialogueSystemTriggerEvent.OnConversationStart:
                    return 14;
                case DialogueSystemTriggerEvent.OnSequenceStart:
                    return 15;
                case DialogueSystemTriggerEvent.OnSaveDataApplied:
                    return 16;
                default:
                    return 10;
            }
        }

        public static DialogueSystemTriggerEvent EnumValueIndexToDialogueSystemTriggerEvent(int enumValueIndex)
        {
            switch (enumValueIndex)
            {
                case 0:
                    return DialogueSystemTriggerEvent.OnBarkEnd;
                case 1:
                    return DialogueSystemTriggerEvent.OnConversationEnd;
                case 2:
                    return DialogueSystemTriggerEvent.OnSequenceEnd;
                case 3:
                    return DialogueSystemTriggerEvent.OnTriggerEnter;
                case 4:
                    return DialogueSystemTriggerEvent.OnStart;
                case 5:
                    return DialogueSystemTriggerEvent.OnUse;
                case 6:
                    return DialogueSystemTriggerEvent.OnEnable;
                case 7:
                    return DialogueSystemTriggerEvent.OnTriggerExit;
                case 8:
                    return DialogueSystemTriggerEvent.OnDisable;
                case 9:
                    return DialogueSystemTriggerEvent.OnDestroy;
                case 10:
                    return DialogueSystemTriggerEvent.None;
                case 11:
                    return DialogueSystemTriggerEvent.OnCollisionEnter;
                case 12:
                    return DialogueSystemTriggerEvent.OnCollisionExit;
                case 13:
                    return DialogueSystemTriggerEvent.OnBarkStart;
                case 14:
                    return DialogueSystemTriggerEvent.OnConversationStart;
                case 15:
                    return DialogueSystemTriggerEvent.OnSequenceStart;
                case 16:
                    return DialogueSystemTriggerEvent.OnSaveDataApplied;
                default:
                    return DialogueSystemTriggerEvent.None;
            }
        }

        public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
        {
            EditorGUI.BeginProperty(position, GUIContent.none, prop);
            if (label != GUIContent.none)
            {
                position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            }

            var nameIndex = EnumValueIndexToNameIndex(prop.enumValueIndex);
            var newNameIndex = EditorGUI.Popup(position, nameIndex, triggerNames);
            if (newNameIndex != nameIndex)
            {
                prop.enumValueIndex = NameIndexToEnumValueIndex(newNameIndex);
            }

            EditorGUI.EndProperty();
        }

        public static DialogueSystemTriggerEvent LayoutPopup(string label, DialogueSystemTriggerEvent trigger)
        {
            var nameIndex = EnumValueIndexToNameIndex(DialogueSystemTriggerEventToEnumValueIndex(trigger));
            var newNameIndex = EditorGUILayout.Popup(label, nameIndex, triggerNames);
            if (newNameIndex != nameIndex)
            {
                return EnumValueIndexToDialogueSystemTriggerEvent(NameIndexToEnumValueIndex(newNameIndex));
            }
            else
            {
                return trigger;
            }
        }

        public static bool IsEnableOrStartEnumIndex(int enumValueIndex)
        {
            int indexOnEnable = Array.IndexOf(Enum.GetValues(typeof(DialogueSystemTriggerEvent)), DialogueSystemTriggerEvent.OnEnable);
            int indexOnStart = Array.IndexOf(Enum.GetValues(typeof(DialogueSystemTriggerEvent)), DialogueSystemTriggerEvent.OnStart);
            return enumValueIndex == indexOnEnable || enumValueIndex == indexOnStart;
        }

    }

}
