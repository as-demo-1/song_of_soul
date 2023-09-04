// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// Contains information about a conversation participant that the dialogue UI or Sequencer may
    /// need. CharacterInfo also contains a static list of mappings of transforms to actor names.
    /// A GameObject can register that its transform corresponds to a specific actor. This is 
    /// typically done automatically by the OverrideActorName component.
    /// </summary>
    public class CharacterInfo
    {

        /// <summary>
        /// The actor ID of the character.
        /// </summary>
        public int id;

        /// <summary>
        /// The name of the actor as defined in the dialogue database.
        /// </summary>
        public string nameInDatabase;

        /// <summary>
        /// The type of the character (PC or NPC).
        /// </summary>
        public CharacterType characterType;

        /// <summary>
        /// Indicates whether this character is a player (PC).
        /// </summary>
        /// <value>
        /// <c>true</c> if this is a player; otherwise, <c>false</c> for an NPC.
        /// </value>
        public bool isPlayer { get { return characterType == CharacterType.PC; } }

        /// <summary>
        /// Indicates whether this character is an NPC.
        /// </summary>
        /// <value>
        /// <c>true</c> if this is an NPC; otherwise, <c>false</c> for a player.
        /// </value>
        public bool isNPC { get { return characterType == CharacterType.NPC; } }

        /// <summary>
        /// The transform of the character's GameObject.
        /// </summary>
        public Transform transform;

        /// <summary>
        /// The portrait image of the character.
        /// </summary>
        public Sprite portrait;

        /// <summary>
        /// Gets the character's name.
        /// </summary>
        /// <value>
        /// If the character info has been provided a non-null transform, this property returns the
        /// name of the transform's game object. Otherwise it returns the name of the actor in the
        /// dialogue database.
        /// </value>
        public string Name { get; set; }

        /// @cond FOR_V1_COMPATIBILITY
        public bool IsPlayer { get { return isPlayer; } }
        public bool IsNPC { get { return isNPC; } }
        /// @endcond

        /// <summary>
        /// Initializes a new CharacterInfo.
        /// </summary>
        /// <param name='actorID'>
        /// Actor ID.
        /// </param>
        /// <param name='actorName'>
        /// Name of the actor as defined in the dialogue database.
        /// </param>
        /// <param name='transform'>
        /// Transform.
        /// </param>
        /// <param name='characterType'>
        /// Character type.
        /// </param>
        /// <param name='portrait'>
        /// Portrait.
        /// </param>
        public CharacterInfo(int id, string nameInDatabase, Transform transform, CharacterType characterType, Sprite portrait)
        {
            this.id = id;
            this.nameInDatabase = nameInDatabase;
            this.characterType = characterType;
            this.portrait = portrait;
            this.transform = transform;
            if ((transform == null) && !string.IsNullOrEmpty(nameInDatabase))
            {
                GameObject go = SequencerTools.FindSpecifier(nameInDatabase, true);
                if (go != null) this.transform = go.transform;
            }
            var dialogueActor = DialogueActor.GetDialogueActorComponent(transform);
            if (dialogueActor == null)
            {
                Name = GetLocalizedDisplayNameInDatabase(nameInDatabase);
            }
            else
            {
                Name = dialogueActor.GetActorName();
                var actor = DialogueManager.masterDatabase.GetActor(dialogueActor.actor);
                var dialogueActorPortrait = dialogueActor.GetPortraitSprite();
                if (dialogueActorPortrait != null)
                {
                    this.portrait = dialogueActorPortrait;
                }
                else if (actor != null)
                {
                    if (portrait == null) this.portrait = actor.GetPortraitSprite();
                }
            }
        }

        public static string GetLocalizedDisplayNameInDatabase(string nameInDatabase)
        {
            var result = DialogueLua.GetLocalizedActorField(nameInDatabase, "Display Name").asString;
            if (string.IsNullOrEmpty(result) || string.Equals(result, "nil"))
            {
                result = DialogueLua.GetLocalizedActorField(nameInDatabase, "Name").asString;
            }
            if (string.IsNullOrEmpty(result) || string.Equals(result, "nil"))
            {
                result = nameInDatabase;
            }
            return FormattedText.ParseCode(result);
        }

        /// <summary>
        /// Gets the pic override portrait. Dialogue text can include <c>[pic=#]</c> tags.
        /// This number corresponds to the actor's portrait (if picNum == 1) or 
        /// alternatePortraits (if picNum >= 2).
        /// </summary>
        /// <returns>The pic override portrait.</returns>
        /// <param name="picNum">Pic number.</param>
        public Sprite GetPicOverride(int picNum)
        {
            if (picNum < 2) return portrait;
            int alternatePortraitIndex = picNum - 2;
            Actor actor = DialogueManager.masterDatabase.GetActor(id);
            return ((actor != null) && (alternatePortraitIndex < actor.alternatePortraits.Count))
                ? UITools.CreateSprite(actor.alternatePortraits[alternatePortraitIndex])
                : ((actor != null) && (alternatePortraitIndex < actor.spritePortraits.Count)) ? actor.spritePortraits[alternatePortraitIndex]
                : portrait;
        }

        #region Static Transform to Actor Mapping Code

        private static Dictionary<string, Transform> registeredActorTransforms = new Dictionary<string, Transform>();

#if UNITY_2019_3_OR_NEWER && UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void InitStaticVariables()
        {
            registeredActorTransforms = new Dictionary<string, Transform>();
        }
#endif

        /// <summary>
        /// Associates a transform with an actor name. Typically called automatically by DialogueActor.
        /// </summary>
        public static void RegisterActorTransform(string actorName, Transform actorTransform)
        {
            if (string.IsNullOrEmpty(actorName) || (actorTransform == null)) return;
            if (registeredActorTransforms.ContainsKey(actorName))
            {
                if (DialogueDebug.logInfo) Debug.LogWarning("Dialogue System: Registering transform " + actorTransform.name + " as actor '" + actorName + "' but another transform is already registered. Overwriting with new transform.", actorTransform);
                registeredActorTransforms[actorName] = actorTransform;
            }
            else
            {
                if (DialogueDebug.logInfo) Debug.Log("Dialogue System: Registering transform " + actorTransform.name + " as actor '" + actorName + "'.", actorTransform);
                registeredActorTransforms.Add(actorName, actorTransform);
            }
        }

        /// <summary>
        /// Unregisters a transform from an actor name. Typically called automatically by DialogueActor when disabled.
        /// </summary>
        public static void UnregisterActorTransform(string actorName, Transform actorTransform)
        {
            if (string.IsNullOrEmpty(actorName) || (actorTransform == null)) return;
            if (registeredActorTransforms.ContainsKey(actorName))
            {
                if (DialogueDebug.logInfo) Debug.Log("Dialogue System: Unregistering transform " + actorTransform.name + " from actor '" + actorName + "'.", actorTransform);
                registeredActorTransforms.Remove(actorName);
            }
        }

        /// <summary>
        /// Gets the transform associated with an actor name, if any.
        /// </summary>
        public static Transform GetRegisteredActorTransform(string actorName)
        {
            return registeredActorTransforms.ContainsKey(actorName) ? registeredActorTransforms[actorName] : null;
        }

        /// <summary>
        /// Returns a list of all transforms registered by RegisterActorTransform, including transforms
        /// registered by DialogueActor.
        /// </summary>
        /// <remarks>This method generates a new List when called.</remarks>
        public static List<Transform> GetAllRegisteredActorTransforms()
        {
            return new List<Transform>(registeredActorTransforms.Values);
        }

        #endregion

    }

}
