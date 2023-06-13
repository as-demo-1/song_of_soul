// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// A dialogue entry is a single line of dialogue in a Conversation, with a list of links to 
    /// possible responses (see Link). The dialogue entries in a conversation form a dialogue tree,
    /// where earlier entries branch out using links to point to later entries. Technically, the
    /// conversation forms a directed graph, not a true tree, because links can also loop back to
    /// earlier entries. 
    /// 
    /// A dialogue entry has a speaker (the "actor") and a listener (the "conversant").
    /// </summary>
    [System.Serializable]
    public class DialogueEntry
    {

        /// <summary>
        /// The dialogue entry ID. Links reference dialogue entries by ID.
        /// </summary>
        public int id = 0;

        /// <summary>
        /// The dialogue entry's field list. (See Field)
        /// </summary>
        public List<Field> fields = null;

        /// <summary>
        /// The ID of the conversation that this dialogue entry belongs to.
        /// </summary>
        public int conversationID = 0;

        /// <summary>
        /// <c>true</c> if this is the root (first) entry in a conversation; otherwise 
        /// <c>false</c>.
        /// </summary>
        public bool isRoot = false;

        /// <summary>
        /// <c>true</c> if this entry is an empty line used to group a subtree of dialogue entries.
        /// </summary>
        public bool isGroup = false;

        /// <summary>
        /// Currently unused by the dialogue system, this is the nodeColor value defined in Chat 
        /// Mapper.
        /// </summary>
        public string nodeColor = null;

        /// <summary>
        /// Currently unused by the dialogue system, this is the delay value to use in Chat 
        /// Mapper's conversation simulator.
        /// </summary>
        public bool delaySimStatus = false;

        /// <summary>
        /// Specifies how to proceed when encountering a link whose entry's condition is false. 
        /// Valid values are "Block" and "Passthrough". If the value is "Block", no further links 
        /// are considered. If the value is "Passthrough", the dialogue system ignores this link 
        /// and continues evaluating the next link.
        /// </summary>
        public string falseConditionAction = null;

        /// <summary>
        /// The priority of the link. In a dialogue entry, links are evaluated in priority order 
        /// from High to Low.
        /// </summary>
        public ConditionPriority conditionPriority = ConditionPriority.Normal;

        /// <summary>
        /// The list of outgoing links from this dialogue entry.
        /// </summary>
        public List<Link> outgoingLinks = new List<Link>();

        /// <summary>
        /// A Lua conditional expression. When evaluating links, the dialogue system will only 
        /// present links whose conditions are true, empty, or <c>null</c>.
        /// </summary>
        public string conditionsString = null;

        /// <summary>
        /// Lua code to run when this dialogue entry is spoken. If userScript is <c>null</c> or
        /// empty, it's ignored.
        /// </summary>
        public string userScript = null;

        /// <summary>
        /// 
        /// </summary>
        public UnityEngine.Events.UnityEvent onExecute = new UnityEngine.Events.UnityEvent();

        public const float CanvasRectWidth = 160f;
        public const float CanvasRectHeight = 30f;

        /// <summary>
        /// The position of this entry on the Dialogue Editor's node canvas.
        /// </summary>
        public Rect canvasRect = new Rect(0, 0, CanvasRectWidth, CanvasRectHeight);

        public const string SceneEventGuidFieldName = "EventGuid";

        /// <summary>
        /// Gets or sets the ID of the line's actor (speaker).
        /// </summary>
        /// <value>
        /// The ID of the actor.
        /// </value>
        public int ActorID
        {
            get { return Field.LookupInt(fields, "Actor"); }
            set { Field.SetValue(fields, "Actor", value.ToString(), FieldType.Actor); }
        }

        /// <summary>
        /// Gets or sets the ID of the line's conversant (listener).
        /// </summary>
        /// <value>
        /// The ID of the conversant.
        /// </value>
        public int ConversantID
        {
            get { return Field.LookupInt(fields, "Conversant"); }
            set { Field.SetValue(fields, "Conversant", value.ToString(), FieldType.Actor); }
        }

        /// <summary>
        /// Gets or sets the title of the dialogue entry, which is an optional field typically only
        /// used internally by the developer.
        /// </summary>
        /// <value>
        /// The title of the dialogue entry.
        /// </value>
        public string Title
        {
            get { return Field.LookupLocalizedValue(fields, "Title"); }
            set { Field.SetValue(fields, "Title", value); }
        }

        /// <summary>
        /// Gets or sets the menu text, which is the text displayed when offering a list of player 
        /// responses. Menu text can be used to present a shortened or paraphrased version of the 
        /// full dialogue text. If menuText is null, the dialogue system uses dialogueText when 
        /// offering player responses.
        /// </summary>
        /// <value>
        /// The menu text. The actual field used depends on the current Localization. If using the
        /// default language, the field is "Menu Text". Otherwise the field is "Menu Text LN",
        /// where LN is the current language.
        /// </value>
        public string currentMenuText
        {
            get { return Field.FieldValue(GetCurrentMenuTextField()); }
            set { Field field = GetCurrentMenuTextField(); if (field != null) field.value = value; }
        }

        /// <summary>
        /// Gets the current menu text field: the localized field if using localization and the
        /// localized field exists; otherwise the default field.
        /// </summary>
        /// <returns>
        /// The current menu text field.
        /// </returns>
        private Field GetCurrentMenuTextField()
        {
            return Field.AssignedField(fields, Field.LocalizedTitle("Menu Text")) ?? Field.Lookup(fields, "Menu Text");
        }

        /// <summary>
        /// Gets or sets the default menu text ("Menu Text").
        /// </summary>
        /// <value>
        /// The default menu text.
        /// </value>
        public string MenuText
        {
            get { return Field.LookupValue(fields, "Menu Text"); }
            set { Field.SetValue(fields, "Menu Text", value); }
        }

        /// <summary>
        /// Gets or sets the localized menu text (e.g., "Menu Text LN", where LN is the current language).
        /// </summary>
        /// <value>
        /// The localized menu text.
        /// </value>
        public string currentLocalizedMenuText
        {
            get { return Field.LookupValue(fields, Field.LocalizedTitle("Menu Text")); }
            set { Field.SetValue(fields, Field.LocalizedTitle("Menu Text"), value); }
        }

        /// <summary>
        /// Gets or sets the dialogue text, which is the subtitle text displayed when a line is 
        /// spoken.
        /// </summary>
        /// <value>
        /// The dialogue text. The actual field used depends on the current Localization. If using the
        /// default language, the field is "Dialogue Text". Otherwise the field is "LN", where LN is
        /// the current language.
        /// </value>
        public string currentDialogueText
        {
            get { return Field.FieldValue(GetCurrentDialogueTextField()); }
            set { Field field = GetCurrentDialogueTextField(); if (field != null) field.value = value; }
        }

        /// <summary>
        /// Gets the current dialogue text field: the localized field if using localization and the
        /// localized field exists; otherwise the default field.
        /// </summary>
        /// <returns>
        /// The current dialogue text field.
        /// </returns>
        private Field GetCurrentDialogueTextField()
        {
            if (string.IsNullOrEmpty(Localization.language)) return Field.Lookup(fields, "Dialogue Text");
            return Field.AssignedField(fields, Localization.language) ?? Field.Lookup(fields, "Dialogue Text");
        }

        /// <summary>
        /// Gets or sets the default dialogue text ("Dialogue Text").
        /// </summary>
        /// <value>
        /// The default dialogue text.
        /// </value>
        public string DialogueText
        {
            get { return Field.LookupValue(fields, "Dialogue Text"); }
            set { Field.SetValue(fields, "Dialogue Text", value); }
        }

        /// <summary>
        /// Gets or sets the localized dialogue text ("LN", where LN is the current language).
        /// </summary>
        /// <value>
        /// The localized dialogue text.
        /// </value>
        public string currentLocalizedDialogueText
        {
            get { return Field.LookupValue(fields, Localization.language); }
            set { Field.SetValue(fields, Localization.language, value); }
        }

        /// <summary>
        /// Gets the subtitle text, which is normally the dialogue text. If the dialogue text
        /// isn't assigned, the menu text is used.
        /// </summary>
        /// <value>
        /// The subtitle text.
        /// </value>
        public string subtitleText
        {
            get { return string.IsNullOrEmpty(currentDialogueText) ? currentMenuText : currentDialogueText; }
        }

        /// <summary>
        /// Gets the response button menu text, which is normally the menu text. If the menu text
        /// isn't assigned, the dialogue text is used.
        /// </summary>
        /// <value>
        /// The response button text.
        /// </value>
        public string responseButtonText
        {
            get { return string.IsNullOrEmpty(currentMenuText) ? currentDialogueText : currentMenuText; }
        }

        /// <summary>
        /// Gets or sets the sequence string. This is a custom field, and is used to specify 
        /// Sequencer commands to play as the line is spoken. These sequencer commands can 
        /// include animation, lip sync, audio, camera work, etc. (See @ref sequencer)
        /// </summary>
        /// <value>
        /// The sequencer commands to play. 
        /// </value>
        /// <remarks>
        /// Prior to version 1.0.6, the Dialogue System used the VideoFile field. Use the
        /// Sequence field instead now.
        /// </remarks>
        public string currentSequence
        {
            get { return Field.FieldValue(GetCurrentSequenceField()); }
            set { SetCurrentSequenceField(value); } // Was: Field field = GetCurrentSequenceField(); if (field != null) field.value = value;
        }

        /// <summary>
        /// Gets the current sequence field: the localized field if using localization and the
        /// localized field exists; otherwise the default field.
        /// </summary>
        /// <returns>
        /// The current sequence field.
        /// </returns>
        private Field GetCurrentSequenceField()
        {
            return Field.AssignedField(fields, Field.LocalizedTitle("Sequence")) ?? Field.Lookup(fields, "Sequence");
        }

        private void SetCurrentSequenceField(string value)
        {
            Field field = GetCurrentSequenceField();
            if ((field == null) && Localization.isDefaultLanguage)
            {
                fields.Add(new Field("Sequence", value, FieldType.Text));
            }
            else
            {
                if (field != null) field.value = value;
            }
        }

        /// <summary>
        /// Gets or sets the default, non-localized sequence ("Sequence"). Note that this is 
        /// different from the DialogueManager display settings Default Sequence, which is 
        /// the sequence used whenever a dialogue entry doesn't have a sequence defined.
        /// </summary>
        /// <value>
        /// The default sequence.
        /// </value>
        public string Sequence
        {
            get { return Field.LookupValue(fields, "Sequence"); }
            set { SetTextField("Sequence", value); }
        }

        /// <summary>
        /// Gets or sets the localized sequence (e.g., "Sequence LN", where LN is the current language).
        /// </summary>
        /// <value>
        /// The localized sequence.
        /// </value>
        public string currentLocalizedSequence
        {
            get { return Field.LookupValue(fields, Field.LocalizedTitle("Sequence")); }
            set { SetTextField(Field.LocalizedTitle("Sequence"), value); }
        }

        /// <summary>
        /// Gets or sets the guid corresponding to an entry in the current scene's
        /// DialogueSystemSceneEvents list, or blank if none.
        /// </summary>
        public string sceneEventGuid
        {
            get { return Field.LookupValue(fields, SceneEventGuidFieldName); }
            set { SetTextField(SceneEventGuidFieldName, value); }
        }

        /// <summary>
        /// Sets the sequence field, adding it if it doesn't already exist.
        /// </summary>
        /// <param name='title'>
        /// Title of the sequence field to use (default or localized).
        /// </param>
        /// <param name='value'>
        /// Value to assign to the sequence field.
        /// </param>
        private void SetTextField(string title, string value)
        {
            Field field = Field.Lookup(fields, title);
            if (field != null)
            {
                field.value = value;
            }
            else
            {
                fields.Add(new Field(title, value, FieldType.Text));
            }
        }

        /// <summary>
        /// Indicates whether this entry has a response menu sequence.
        /// </summary>
        /// <returns><c>true</c> if it has a response menu sequence; otherwise, <c>false</c>.</returns>
        public bool HasResponseMenuSequence()
        {
            return Field.FieldExists(fields, "Response Menu Sequence");
        }

        /// <summary>
        /// Gets or sets the response menu sequence string. This is a custom field, and is used 
        /// to specify Sequencer commands to play as the line is spoken. These sequencer commands 
        /// can include animation, lip sync, audio, camera work, etc. (See @ref sequencer)
        /// </summary>
        /// <value>
        /// The sequencer commands to play. 
        /// </value>
        public string currentResponseMenuSequence
        {
            get { return Field.FieldValue(GetCurrentResponseMenuSequenceField()); }
            set { SetCurrentResponseMenuSequenceField(value); }
        }

        /// <summary>
        /// Gets the current response menu sequence field: the localized field if using localization 
        /// and the localized field exists; otherwise the default field.
        /// </summary>
        /// <returns>
        /// The current response menu sequence field.
        /// </returns>
        private Field GetCurrentResponseMenuSequenceField()
        {
            return Field.AssignedField(fields, Field.LocalizedTitle("Response Menu Sequence")) ?? Field.Lookup(fields, "Response Menu Sequence");
        }

        private void SetCurrentResponseMenuSequenceField(string value)
        {
            Field field = GetCurrentSequenceField();
            if ((field == null) && Localization.isDefaultLanguage)
            {
                fields.Add(new Field("Response Menu Sequence", value, FieldType.Text));
            }
            else
            {
                if (field != null) field.value = value;
            }
        }

        /// <summary>
        /// Gets or sets the default, non-localized response menu sequence ("Response Menu Sequence"). 
        /// </summary>
        /// <value>
        /// The default response menu sequence.
        /// </value>
        public string ResponseMenuSequence
        {
            get { return Field.LookupValue(fields, "Response Menu Sequence"); }
            set { SetTextField("Response Menu Sequence", value); }
        }

        /// <summary>
        /// Gets or sets the localized response menu sequence (e.g., "Response Menu Sequence LN", 
        /// where LN is the current language).
        /// </summary>
        /// <value>
        /// The localized sequence.
        /// </value>
        public string currentLocalizedResponseMenuSequence
        {
            get { return Field.LookupValue(fields, Field.LocalizedTitle("Response Menu Sequence")); }
            set { SetTextField(Field.LocalizedTitle("Response Menu Sequence"), value); }
        }

        /// <summary>
        /// Gets or sets the video file string. This field is no longer used for cutscene sequences!
        /// </summary>
        /// <value>
        /// Whatever you want. This is no longer used for cutscene sequences.
        /// </value>
        /// <remarks>
        /// Prior to version 1.0.6, the Dialogue System used this field for cutscene sequences.
        /// It now uses the custom Sequence field.
        /// </remarks>
        public string VideoFile
        {
            get { return Field.LookupValue(fields, "Video File"); }
            set { Field.SetValue(fields, "Video File", value); }
        }

        /// <summary>
        /// Gets or sets the audio files string. This field is defined in Chat Mapper but doesn't 
        /// do anything in the Dialogue System. Instead, the Sequence field should contain 
        /// Sequencer commands to do things like play audio.
        /// </summary>
        /// <value>
        /// The audio files string.
        /// </value>
        public string AudioFiles
        {
            get { return Field.LookupValue(fields, "Audio Files"); }
            set { Field.SetValue(fields, "Audio Files", value); }
        }

        /// <summary>
        /// Gets or sets the animation files string. This field is defined in Chat Mapper, and 
        /// doesn't do anything in the dialogue system. Instead, the Sequence field should contain
        /// Sequencer commands to do things like play animations.
        /// </summary>
        /// <value>
        /// The animation files string.
        /// </value>
        public string AnimationFiles
        {
            get { return Field.LookupValue(fields, "Animation Files"); }
            set { Field.SetValue(fields, "Animation Files", value); }
        }

        /// <summary>
        /// Gets or sets the lipsync files string. This field is defined in Chat Mapper, and 
        /// doesn't do anything in the Dialogue System. Instead, the videoFile field should contain
        /// Sequencer commands to do things like play lip sync animation and audio.
        /// </summary>
        /// <value>
        /// The lip sync files string.
        /// </value>
        public string LipsyncFiles
        {
            get { return Field.LookupValue(fields, "Lipsync Files"); }
            set { Field.SetValue(fields, "Lipsync Files", value); }
        }

        /// <summary>
        /// Initializes a new DialogueEntry.
        /// </summary>
        public DialogueEntry() { }

        /// <summary>
        /// Initializes a new DialogueEntry copied from a Chat Mapper DialogEntry.
        /// </summary>
        /// <param name='chatMapperDialogEntry'>
        /// The Chat Mapper dialog entry to copy.
        /// </param>
        public DialogueEntry(ChatMapper.DialogEntry chatMapperDialogEntry)
        {
            if (chatMapperDialogEntry != null)
            {
                id = chatMapperDialogEntry.ID;
                fields = Field.CreateListFromChatMapperFields(chatMapperDialogEntry.Fields);
                UseCanvasRectField();
                //--- Removed in Chat Mapper 1.7: conversationID = chatMapperDialogEntry.ConversationID;
                isRoot = chatMapperDialogEntry.IsRoot;
                isGroup = chatMapperDialogEntry.IsGroup;
                if (isGroup) Sequence = ""; //"Continue()";
                nodeColor = chatMapperDialogEntry.NodeColor;
                delaySimStatus = chatMapperDialogEntry.DelaySimStatus;
                falseConditionAction = chatMapperDialogEntry.FalseCondtionAction;
                conditionPriority = ConditionPriorityUtility.StringToConditionPriority(chatMapperDialogEntry.ConditionPriority);
                foreach (var chatMapperLink in chatMapperDialogEntry.OutgoingLinks)
                {
                    outgoingLinks.Add(new Link(chatMapperLink));
                }
                conditionsString = chatMapperDialogEntry.ConditionsString;
                userScript = chatMapperDialogEntry.UserScript;
            }
        }

        /// <summary>
        /// If the dialogue entry has a canvasRect field, extract it and use it to set the canvas rect position.
        /// </summary>
        public void UseCanvasRectField()
        {
            var canvasRectField = Field.Lookup(fields, "canvasRect");
            if (canvasRectField != null && !string.IsNullOrEmpty(canvasRectField.value))
            {
                var values = canvasRectField.value.Split(';');
                var canvasX = (values.Length >= 1) ? Tools.StringToFloat(values[0]) : 0;
                var canvasY = (values.Length >= 2) ? Tools.StringToFloat(values[1]) : 0;
                if (canvasX > 0 && canvasY > 0) canvasRect = new Rect(canvasX, canvasY, canvasRect.width, canvasRect.height);
                fields.Remove(canvasRectField);
            }
        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        /// <param name="sourceEntry">Source entry.</param>
        public DialogueEntry(DialogueEntry sourceEntry)
        {
            this.id = sourceEntry.id;
            this.fields = Field.CopyFields(sourceEntry.fields);
            this.conversationID = sourceEntry.conversationID;
            this.isRoot = sourceEntry.isRoot;
            this.isGroup = sourceEntry.isGroup;
            this.nodeColor = sourceEntry.nodeColor;
            this.delaySimStatus = sourceEntry.delaySimStatus;
            this.falseConditionAction = sourceEntry.falseConditionAction;
            this.conditionPriority = ConditionPriority.Normal;
            this.outgoingLinks = CopyLinks(sourceEntry.outgoingLinks);
            this.conditionsString = sourceEntry.conditionsString;
            this.userScript = sourceEntry.userScript;
            this.canvasRect = sourceEntry.canvasRect;
        }

        private List<Link> CopyLinks(List<Link> sourceLinks)
        {
            List<Link> links = new List<Link>();
            foreach (var sourceLink in sourceLinks)
            {
                links.Add(new Link(sourceLink));
            }
            return links;
        }

    }

}
