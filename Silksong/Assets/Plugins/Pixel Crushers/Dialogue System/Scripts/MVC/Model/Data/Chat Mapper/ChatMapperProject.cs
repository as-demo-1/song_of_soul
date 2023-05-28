// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem.ChatMapper
{

    /// <summary>
    /// Defines an XML-serializable Chat Mapper project. See http://www.chatmapper.com/
    /// The Dialogue System's Chat Mapper Converter tool converts Chat Mapper XML output
    /// to the system's internal DialogueDatabase format.
    /// </summary>
    [XmlRoot("ChatMapperProject")]
    public class ChatMapperProject
    {
        [XmlAttribute("Title")]
        public string Title;

        [XmlAttribute("Version")]
        public string Version;

        [XmlAttribute("Author")]
        public string Author;

        [XmlAttribute("EmphasisColor1Label")]
        public string EmphasisColor1Label = string.Empty;
        [XmlAttribute("EmphasisColor1")]
        public string EmphasisColor1;
        [XmlAttribute("EmphasisStyle1")]
        public string EmphasisStyle1;

        [XmlAttribute("EmphasisColor2Label")]
        public string EmphasisColor2Label = string.Empty;
        [XmlAttribute("EmphasisColor2")]
        public string EmphasisColor2;
        [XmlAttribute("EmphasisStyle2")]
        public string EmphasisStyle2;

        [XmlAttribute("EmphasisColor3Label")]
        public string EmphasisColor3Label = string.Empty;
        [XmlAttribute("EmphasisColor3")]
        public string EmphasisColor3;
        [XmlAttribute("EmphasisStyle3")]
        public string EmphasisStyle3;

        [XmlAttribute("EmphasisColor4Label")]
        public string EmphasisColor4Label = string.Empty;
        [XmlAttribute("EmphasisColor4")]
        public string EmphasisColor4;
        [XmlAttribute("EmphasisStyle4")]
        public string EmphasisStyle4;

        public string Description;
        public string UserScript;
        public Assets Assets;

        /// <summary>
        /// Creates a DialogueDatabase from the ChatMapperProject.
        /// </summary>
        /// <returns>The dialogue database.</returns>
        public DialogueDatabase ToDialogueDatabase()
        {
            return ChatMapperToDialogueDatabase.ConvertToDialogueDatabase(this);
        }

    }

    /// <summary>
    /// Contains all chat mapper elements (e.g., actors, locations, dialogue entries, etc).
    /// </summary>
    [System.Serializable]
    public class Assets
    {
        [XmlArray("Actors")]
        [XmlArrayItem("Actor")]
        public List<Actor> Actors = new List<Actor>();

        [XmlArray("Items")]
        [XmlArrayItem("Item")]
        public List<Item> Items = new List<Item>();

        [XmlArray("Locations")]
        [XmlArrayItem("Location")]
        public List<Location> Locations = new List<Location>();

        [XmlArray("Conversations")]
        [XmlArrayItem("Conversation")]
        public List<Conversation> Conversations = new List<Conversation>();

        [XmlArray("UserVariables")]
        [XmlArrayItem("UserVariable")]
        public List<UserVariable> UserVariables = new List<UserVariable>();
    }

    /// <summary>
    /// Defines a Chat Mapper Actor.
    /// </summary>
    [System.Serializable]
    public class Actor
    {
        [XmlAttribute("ID")]
        public int ID;
        [XmlArray("Fields")]
        [XmlArrayItem("Field")]
        public List<Field> Fields = new List<Field>();
    }

    /// <summary>
    /// Defines a Chat Mapper Item.
    /// </summary>
    public class Item
    {
        [XmlAttribute("ID")]
        public int ID;
        [XmlArray("Fields")]
        [XmlArrayItem("Field")]
        public List<Field> Fields = new List<Field>();
    }

    /// <summary>
    /// Defines a Chat Mapper Location.
    /// </summary>
    public class Location
    {
        [XmlAttribute("ID")]
        public int ID;
        [XmlArray("Fields")]
        [XmlArrayItem("Field")]
        public List<Field> Fields = new List<Field>();
    }

    /// <summary>
    /// Defines a Chat Mapper Conversation. A conversation is composed of a collection of dialog 
    /// entries.
    /// </summary>
    public class Conversation
    {
        [XmlAttribute("ID")]
        public int ID;
        [XmlAttribute("NodeColor")]
        public string NodeColor;
        [XmlAttribute("LockedMode")]
        public string LockedMode;
        [XmlArray("Fields")]
        [XmlArrayItem("Field")]
        public List<Field> Fields = new List<Field>();
        [XmlArray("DialogEntries")]
        [XmlArrayItem("DialogEntry")]
        public List<DialogEntry> DialogEntries = new List<DialogEntry>();
    }

    /// <summary>
    /// Defines a Chat Mapper DialogEntry. A dialog entry is a line in a conversation, delivered by 
    /// a speaker (referred to as Actor in the dialog entry) to a listener (referred to as 
    /// Conversant).
    /// </summary>
    public class DialogEntry
    {
        [XmlAttribute("ID")]
        public int ID;
        //--- Removed in Chat Mapper 1.7: [XmlAttribute("ConversationID")]
        //--- Removed: public int ConversationID;
        [XmlAttribute("IsRoot")]
        public bool IsRoot;
        [XmlAttribute("IsGroup")]
        public bool IsGroup;
        [XmlAttribute("NodeColor")]
        public string NodeColor;
        [XmlAttribute("DelaySimStatus")]
        public bool DelaySimStatus;
        [XmlAttribute("FalseCondtionAction")]
        public string FalseCondtionAction;
        [XmlAttribute("ConditionPriority")]
        public string ConditionPriority;
        [XmlArray("Fields")]
        [XmlArrayItem("Field")]
        public List<Field> Fields = new List<Field>();
        [XmlArray("OutgoingLinks")]
        [XmlArrayItem("Link")]
        public List<Link> OutgoingLinks = new List<Link>();
        public string ConditionsString;
        public string UserScript;
    }

    /// <summary>
    /// Defines a Chat Mapper Link. A link connects a DialogEntry to another DialogEntry. Links can
    /// cross conversations.
    /// </summary>
    public class Link
    {
        [XmlAttribute("ConversationID")]
        public int ConversationID;
        [XmlAttribute("OriginConvoID")]
        public int OriginConvoID;
        [XmlAttribute("DestinationConvoID")]
        public int DestinationConvoID;
        [XmlAttribute("OriginDialogID")]
        public int OriginDialogID;
        [XmlAttribute("DestinationDialogID")]
        public int DestinationDialogID;
        [XmlAttribute("IsConnector")]
        public bool IsConnector;
    }

    /// <summary>
    /// Defines a Chat Mapper user variable.
    /// </summary>
    public class UserVariable
    {
        [XmlArray("Fields")]
        [XmlArrayItem("Field")]
        public List<Field> Fields = new List<Field>();
    }

    /// <summary>
    /// Defines a Chat Mapper field.  Most Chat Mapper assets have a collection of fields.
    /// </summary>
    public class Field
    {
        [XmlAttribute("Hint")]
        public string Hint;
        [XmlAttribute("Type")]
        public string Type;
        public string Title;
        public string Value;
    }

}
