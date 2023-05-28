// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using System;
using System.Linq;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// Conditions are used to selectively run actions. A condition is made of any number of Lua 
    /// conditions, quest conditions, accepted tags, and accepted game objects. In order for the 
    /// Condition to be true, all subconditions must be true.
    /// </summary>
    [System.Serializable]
    public class Condition
    {

        /// <summary>
        /// Conditional expressions in Lua code. The Condition is true only if all Lua conditions 
        /// evaluate to <c>true</c>.
        /// </summary>
        /// <example>
        /// [Lua code:] Variable["Gold"] > 50
        /// </example>
        public string[] luaConditions = new string[0];

        /// <summary>
        /// Quest conditions. The Condition is true only if all quest conditions are true.
        /// </summary>
        public QuestCondition[] questConditions = new QuestCondition[0];

        /// <summary>
        /// The accepted tags. The Condition is true only if the interactor's tag is in the list of accepted 
        /// tags, or if the list is empty.
        /// </summary>
        public string[] acceptedTags = new string[0];

        /// <summary>
        /// The accepted game objects. The Condition is true only if the interactor is in the list of
        /// accepted game objects, or if the list is empty.
        /// </summary>
        public GameObject[] acceptedGameObjects = new GameObject[0];

        [HideInInspector]
        public int luaWizardIndex = -1;

        public enum LastEvaluationValue { None, True, False }

        [HideInInspector]
        public LastEvaluationValue lastEvaluationValue = LastEvaluationValue.None;

        /// <summary>
        /// Indicates whether this Condition is true.
        /// </summary>
        public bool IsTrue(Transform interactor)
        {
            var result =
                LuaConditionsAreTrue() &&
                QuestConditionsAreTrue() &&
                IsAcceptedTag(interactor) &&
                IsAcceptedGameObject(interactor);
            lastEvaluationValue = (result == true) ? LastEvaluationValue.True : LastEvaluationValue.False;
            return result;
        }

        private bool LuaConditionsAreTrue()
        {
            if (luaConditions != null)
            {
                for (int i = 0; i < luaConditions.Length; i++)
                {
                    var luaCondition = luaConditions[i];
                    if (!Lua.IsTrue(luaCondition, DialogueDebug.logInfo)) return false;
                }
            }
            return true;
        }

        private bool QuestConditionsAreTrue()
        {
            if (questConditions != null)
            {
                for (int i = 0; i < questConditions.Length; i++)
                {
                    var questCondition = questConditions[i];
                    if ((questCondition != null) && !questCondition.IsTrue) return false;
                }
            }
            return true;
        }

        private bool IsAcceptedTag(Transform interactor)
        {
            if ((interactor == null) || (acceptedTags == null) || (acceptedTags.Length <= 0)) return true;
            return acceptedTags.Contains(interactor.tag);
        }

        private bool IsAcceptedGameObject(Transform interactor)
        {
            if ((interactor == null) || (acceptedGameObjects == null) || (acceptedGameObjects.Length <= 0)) return true;
            return acceptedGameObjects.Contains(interactor.gameObject);
        }

    }

    /// <summary>
    /// A quest condition checks the state of a quest. Question conditions are part of a Condition.
    /// </summary>
    [Serializable]
    public class QuestCondition
    {

        /// <summary>
        /// The name of the quest. If you are using the QuestLog class, this should be the name of an entry in the
        /// Lua table "Item[]". If the name is blank, there is no quest condition.
        /// </summary>
        public string questName = string.Empty;

        /// <summary>
        /// The allowable quest states for the condition to be true.
        /// </summary>
        [Tooltip("The allowable quest states for the condition to be true.")]
        [BitMask(typeof(QuestState))]
        [QuestState]
        public QuestState questState;

        [Tooltip("Check quest entry state.")]
        public bool checkQuestEntry = false;

        [QuestEntryPopup]
        public int entryNumber;

        /// <summary>
        /// The allowable quest entry states for the condition to be true.
        /// </summary>
        [Tooltip("If quest entry is specified, the allowable quest entry states for the condition to be true.")]
        [BitMask(typeof(QuestState))]
        [QuestState]
        public QuestState questEntryState;

        /// <summary>
        /// Indicates whether this QuestCondition is true.
        /// </summary>
        public bool IsTrue
        {
            get 
            {
                return
                    string.IsNullOrEmpty(questName) ||
                    (QuestLog.IsQuestInStateMask(questName, questState) &&
                    (!checkQuestEntry || QuestLog.IsQuestEntryInStateMask(questName, entryNumber, questEntryState)));
            }
        }

    }

}
