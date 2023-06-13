// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// The current state of a conversation, which can also be thought of as the current position 
    /// in the dialogue tree.
    /// </summary>
    public class ConversationState
    {

        /// <summary>
        /// The subtitle of the current dialogue entry.
        /// </summary>
        public Subtitle subtitle;

        /// <summary>
        /// The NPC responses linked from the current dialogue entry. This array may be empty.
        /// Typically, a conversation state will have either NPC responses or PC responses but not
        /// both.
        /// </summary>
        public Response[] npcResponses;

        /// <summary>
        /// The PC responses linked from the current dialogue entry. This array may be empty.
        /// Typically, a conversation state will have either NPC responses or PC responses but not
        /// both.
        /// </summary>
        public Response[] pcResponses;

        /// <summary>
        /// Indicates whether the current state has any NPC responses.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has any NPC responses; otherwise, <c>false</c>.
        /// </value>
        public bool hasNPCResponse
        {
            get { return (npcResponses != null) && (npcResponses.Length > 0); }
        }

        /// <summary>
        /// Gets the first NPC response.
        /// </summary>
        /// <value>
        /// The first NPC response.
        /// </value>
        public Response firstNPCResponse
        {
            get { return hasNPCResponse ? npcResponses[0] : null; }
        }

        /// <summary>
        /// Indicates whether the current state has any PC responses.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has PC responses; otherwise, <c>false</c>.
        /// </value>
        public bool hasPCResponses
        {
            get { return (pcResponses != null) && (pcResponses.Length > 0); }
        }

        /// <summary>
        /// Indicates whether the current state has a PC auto-response, which means it has only a
        /// single response and that response does not specify "force menu."
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has PC auto response; otherwise, <c>false</c>.
        /// </value>
        public bool hasPCAutoResponse
        {
            get
            {
                if (pcResponses == null || pcResponses.Length == 0) return false;
                var hasAuto = false;
                for (int i = 0; i < pcResponses.Length; i++)
                {
                    if (pcResponses[i].formattedText.forceMenu) return false;
                    if (pcResponses[i].formattedText.forceAuto && pcResponses[i].enabled) hasAuto = true;
                }
                return hasAuto || (pcResponses.Length == 1);
            }
        }

        /// <summary>
        /// Indicates whether the current state has a PC response with an [auto] tag.
        /// </summary>
        public bool hasForceAutoResponse
        {
            get
            {
                if (pcResponses == null || pcResponses.Length == 0) return false;
                for (int i = 0; i < pcResponses.Length; i++)
                {
                    if (pcResponses[i].enabled && pcResponses[i].formattedText.forceAuto) return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Gets the PC auto response.
        /// </summary>
        /// <value>
        /// The PC auto response, or null if the state doesn't have one.
        /// </value>
        public Response pcAutoResponse
        {
            get
            {
                if (pcResponses == null || pcResponses.Length == 0) return null;
                for (int i = 0; i < pcResponses.Length; i++)
                {
                    if (pcResponses[i].enabled && pcResponses[i].formattedText.forceAuto) return pcResponses[i];
                }
                return pcResponses[0];
            }
        }

        /// <summary>
        /// Indicates whether this state has any responses (PC or NPC).
        /// </summary>
        /// <value><c>true</c> if this instance has any responses; otherwise, <c>false</c>.</value>
        public bool hasAnyResponses
        {
            get { return hasNPCResponse || hasPCResponses; }
        }

        /// <summary>
        /// Indicates whether this state corresponds to a group dialogue entry.
        /// </summary>
        /// <value>
        /// <c>true</c> if a group; otherwise, <c>false</c>.
        /// </value>
        public bool isGroup { get; set; }

        /// @cond FOR_V1_COMPATIBILITY
        public bool HasNPCResponse { get { return hasNPCResponse; } }
        public Response FirstNPCResponse { get { return firstNPCResponse; } }
        public bool HasPCResponses { get { return hasPCResponses; } }
        public bool HasPCAutoResponse { get { return hasPCAutoResponse; } }
        public Response PCAutoResponse { get { return pcAutoResponse; } }
        public bool HasAnyResponses { get { return hasAnyResponses; } }
        public bool IsGroup { get { return isGroup; } set { isGroup = value; } }
        /// @endcond

        /// <summary>
        /// Initializes a new ConversationState.
        /// </summary>
        /// <param name='subtitle'>
        /// Subtitle of the current dialogue entry.
        /// </param>
        /// <param name='npcResponses'>
        /// NPC responses.
        /// </param>
        /// <param name='pcResponses'>
        /// PC responses.
        /// </param>
        public ConversationState(Subtitle subtitle, Response[] npcResponses, Response[] pcResponses, bool isGroup = false)
        {
            this.subtitle = subtitle;
            this.npcResponses = npcResponses;
            this.pcResponses = pcResponses;
            this.isGroup = isGroup;
        }

        public DialogueEntry GetRandomNPCEntry()
        {
            return hasNPCResponse ? npcResponses[UnityEngine.Random.Range((int)0, (int)npcResponses.Length)].destinationEntry : null;

        }

    }

}
