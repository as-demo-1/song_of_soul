// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;
using System;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// This custom property drawer for DialogueTriggerEvent reorders the 
    /// popup for trigger events. As the Dialogue System has grown, trigger 
    /// events were added to the end rather than reordering the enum (which
    /// would break serialization in existing projects).
    /// </summary>
    [CustomPropertyDrawer(typeof(DialogueTriggerEventAttribute))]
    public class DialogueTriggerEventDrawer : PropertyDrawer
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

			"On Bark End",			// 10
			"On Conversation End",	// 11
			"On Sequence End"		// 12
		};

        private static int NameIndexToEnumValueIndex(int nameIndex)
        {
            switch (nameIndex)
            {
                case 1:
                    return 5; // DialogueTriggerEvent.OnUse;
                case 2:
                    return 4; // DialogueTriggerEvent.OnStart;
                case 3:
                    return 6; // DialogueTriggerEvent.OnEnable;
                case 4:
                    return 8; // DialogueTriggerEvent.OnDisable;
                case 5:
                    return 9; // DialogueTriggerEvent.OnDestroy;
                case 6:
                    return 3; // DialogueTriggerEvent.OnTriggerEnter;
                case 7:
                    return 7; // DialogueTriggerEvent.OnTriggerExit;
                case 8:
                    return 11; // DialogueTriggerEvent.OnCollisionEnter;
                case 9:
                    return 12; // DialogueTriggerEvent.OnCollisionExit;
                case 10:
                    return 0; // DialogueTriggerEvent.OnBarkEnd;
                case 11:
                    return 1; // DialogueTriggerEvent.OnConversationEnd;
                case 12:
                    return 2; // DialogueTriggerEvent.OnSequenceEnd;
                default:
                    return 10; // DialogueTriggerEvent.None;
            }
        }

        private static int EnumValueIndexToNameIndex(int enumValueIndex)
        {
            switch (enumValueIndex)
            {
                case 5: // DialogueTriggerEvent.OnUse:
                    return 1;
                case 4: // DialogueTriggerEvent.OnStart:
                    return 2;
                case 6: // DialogueTriggerEvent.OnEnable:
                    return 3;
                case 8: // DialogueTriggerEvent.OnDisable:
                    return 4;
                case 9: // DialogueTriggerEvent.OnDestroy:
                    return 5;
                case 3: // DialogueTriggerEvent.OnTriggerEnter:
                    return 6;
                case 7: // DialogueTriggerEvent.OnTriggerExit:
                    return 7;
                case 11: // DialogueTriggerEvent.OnCollisionEnter:
                    return 8;
                case 12: // DialogueTriggerEvent.OnCollisionExit:
                    return 9;
                case 0: // DialogueTriggerEvent.OnBarkEnd:
                    return 10;
                case 1: // DialogueTriggerEvent.OnConversationEnd:
                    return 11;
                case 2: // DialogueTriggerEvent.OnSequenceEnd:
                    return 12;
                default:
                    return 0; // DialogueTriggerEvent.None
            }
        }

        public static int DialogueTriggerEventToEnumValueIndex(DialogueTriggerEvent trigger)
        {
            switch (trigger)
            {
                case DialogueTriggerEvent.OnBarkEnd:
                    return 0;
                case DialogueTriggerEvent.OnConversationEnd:
                    return 1;
                case DialogueTriggerEvent.OnSequenceEnd:
                    return 2;
                case DialogueTriggerEvent.OnTriggerEnter:
                    return 3;
                case DialogueTriggerEvent.OnStart:
                    return 4;
                case DialogueTriggerEvent.OnUse:
                    return 5;
                case DialogueTriggerEvent.OnEnable:
                    return 6;
                case DialogueTriggerEvent.OnTriggerExit:
                    return 7;
                case DialogueTriggerEvent.OnDisable:
                    return 8;
                case DialogueTriggerEvent.OnDestroy:
                    return 9;
                case DialogueTriggerEvent.None:
                    return 10;
                case DialogueTriggerEvent.OnCollisionEnter:
                    return 11;
                case DialogueTriggerEvent.OnCollisionExit:
                    return 12;
                default:
                    return 10;
            }
        }

        public static DialogueTriggerEvent EnumValueIndexToDialogueTriggerEvent(int enumValueIndex)
        {
            switch (enumValueIndex)
            {
                case 0:
                    return DialogueTriggerEvent.OnBarkEnd;
                case 1:
                    return DialogueTriggerEvent.OnConversationEnd;
                case 2:
                    return DialogueTriggerEvent.OnSequenceEnd;
                case 3:
                    return DialogueTriggerEvent.OnTriggerEnter;
                case 4:
                    return DialogueTriggerEvent.OnStart;
                case 5:
                    return DialogueTriggerEvent.OnUse;
                case 6:
                    return DialogueTriggerEvent.OnEnable;
                case 7:
                    return DialogueTriggerEvent.OnTriggerExit;
                case 8:
                    return DialogueTriggerEvent.OnDisable;
                case 9:
                    return DialogueTriggerEvent.OnDestroy;
                case 10:
                    return DialogueTriggerEvent.None;
                case 11:
                    return DialogueTriggerEvent.OnCollisionEnter;
                case 12:
                    return DialogueTriggerEvent.OnCollisionExit;
                default:
                    return DialogueTriggerEvent.None;
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

        public static DialogueTriggerEvent LayoutPopup(string label, DialogueTriggerEvent trigger)
        {
            var nameIndex = EnumValueIndexToNameIndex(DialogueTriggerEventToEnumValueIndex(trigger));
            var newNameIndex = EditorGUILayout.Popup(label, nameIndex, triggerNames);
            if (newNameIndex != nameIndex)
            {
                return EnumValueIndexToDialogueTriggerEvent(NameIndexToEnumValueIndex(newNameIndex));
            }
            else
            {
                return trigger;
            }
        }

        public static bool IsEnableOrStartEnumIndex(int enumValueIndex)
        {
            int indexOnEnable = Array.IndexOf(Enum.GetValues(typeof(DialogueTriggerEvent)), DialogueTriggerEvent.OnEnable);
            int indexOnStart = Array.IndexOf(Enum.GetValues(typeof(DialogueTriggerEvent)), DialogueTriggerEvent.OnStart);
            return enumValueIndex == indexOnEnable || enumValueIndex == indexOnStart;
        }

    }

}
