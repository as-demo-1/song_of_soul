// Copyright (c) Pixel Crushers. All rights reserved.

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// A link from one DialogueEntry (the origin) to another (the destination). Every dialogue 
    /// entry has a list of outgoing links. Note that links can cross into different conversations. 
    /// A link holds the origin conversation and dialogue entry, and the destination conversation 
    /// and dialogue entry.
    /// </summary>
    [System.Serializable]
    public class Link
    {

        /// <summary>
        /// The origin conversation ID.
        /// </summary>
        public int originConversationID = 0;

        /// <summary>
        /// The origin dialogue ID.
        /// </summary>
        public int originDialogueID = 0;

        /// <summary>
        /// The destination conversation ID.
        /// </summary>
        public int destinationConversationID = 0;

        /// <summary>
        /// The destination dialogue ID.
        /// </summary>
        public int destinationDialogueID = 0;

        /// <summary>
        /// When <c>true</c>, specifies that the link crosses conversations. This field comes from 
        /// Chat Mapper but is currently unused in the dialogue system because the same information 
        /// is contained the conversation IDs.
        /// </summary>
        public bool isConnector = false;

        /// <summary>
        /// The priority of the link. Higher priority links are evaluated first.
        /// </summary>
        public ConditionPriority priority = ConditionPriority.Normal;

        /// <summary>
        /// Initializes a new Link.
        /// </summary>
        public Link() { }

        /// <summary>
        /// Initializes a new Link copied from a Chat Mapper link.
        /// </summary>
        /// <param name='chatMapperLink'>
        /// The Chat Mapper link.
        /// </param>
        public Link(ChatMapper.Link chatMapperLink)
        {
            if (chatMapperLink != null)
            {
                //---Updated to handle pre-1.3 XML files. Was: originConversationID = chatMapperLink.OriginConvoID;
                originConversationID = ((chatMapperLink.OriginConvoID == 0) && (chatMapperLink.ConversationID > 0)) ? chatMapperLink.ConversationID : chatMapperLink.OriginConvoID;
                originDialogueID = chatMapperLink.OriginDialogID;
                //---Updated to handle pre-1.3 XML files. Was: destinationConversationID = chatMapperLink.DestinationConvoID;
                destinationConversationID = ((chatMapperLink.DestinationConvoID == 0) && (chatMapperLink.ConversationID > 0)) ? chatMapperLink.ConversationID : chatMapperLink.DestinationConvoID;
                destinationDialogueID = chatMapperLink.DestinationDialogID;
                isConnector = chatMapperLink.IsConnector;
            }
        }


        /// <summary>
        /// Copy constructor.
        /// </summary>
        /// <param name="sourceLink">Source link.</param>
        public Link(Link sourceLink)
        {
            this.originConversationID = sourceLink.originConversationID;
            this.originDialogueID = sourceLink.originDialogueID;
            this.destinationConversationID = sourceLink.destinationConversationID;
            this.destinationDialogueID = sourceLink.destinationDialogueID;
            this.isConnector = sourceLink.isConnector;
            this.priority = sourceLink.priority;
        }

        /// <summary>
        /// Initializes a new Link.
        /// </summary>
        /// <param name="originConversationID">Origin conversation ID.</param>
        /// <param name="originDialogueID">Origin dialogue ID.</param>
        /// <param name="destinationConversationID">Destination conversation ID.</param>
        /// <param name="destinationDialogueID">Destination dialogue ID.</param>
        public Link(int originConversationID, int originDialogueID, int destinationConversationID, int destinationDialogueID)
        {
            this.originConversationID = originConversationID;
            this.originDialogueID = originDialogueID;
            this.destinationConversationID = destinationConversationID;
            this.destinationDialogueID = destinationDialogueID;
        }

    }

}
