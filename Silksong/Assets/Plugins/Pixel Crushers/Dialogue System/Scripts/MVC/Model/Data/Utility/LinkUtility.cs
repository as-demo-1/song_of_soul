// Copyright (c) Pixel Crushers. All rights reserved.

using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// A static tool class for working with links (see Link).
    /// </summary>
    public static class LinkUtility
    {

        /// <summary>
        /// Sorts the outgoing links in a Conversation by ConditionPriority.
        /// </summary>
        /// <param name='database'>
        /// The dialogue database. Since links can span different conversations, this method needs
        /// access to the entire database of all conversations.
        /// </param>
        /// <param name='conversation'>
        /// The conversation to sort.
        /// </param>
        public static void SortOutgoingLinks(DialogueDatabase database, Conversation conversation)
        {
            if (conversation != null)
            {
                foreach (DialogueEntry dialogueEntry in conversation.dialogueEntries)
                {
                    SortOutgoingLinks(database, dialogueEntry);
                }
            }
        }

        /// <summary>
        /// Sorts the outgoing links in a DialogueEntry by ConditionPriority.
        /// </summary>
        /// <param name='database'>
        /// The dialogue database. Since links can span different conversations, this method needs
        /// access to the entire database of all conversations.
        /// </param>
        /// <param name='dialogueEntry'>
        /// The dialogue entry to sort.
        /// </param>
        public static void SortOutgoingLinks(DialogueDatabase database, DialogueEntry dialogueEntry)
        {
            if ((database != null) && (dialogueEntry != null))
            {
                foreach (Link link in dialogueEntry.outgoingLinks)
                {
                    DialogueEntry linkEntry = database.GetDialogueEntry(link);
                    if (linkEntry != null)
                    {
                        link.priority = linkEntry.conditionPriority;
                    }
                }
                dialogueEntry.outgoingLinks.Sort(new PrioritySorter());
            }
        }

        /// <summary>
        /// Compares the ConditionPriority values of two Link assets. Used to sort links by priority.
        /// </summary>
        public class PrioritySorter : IComparer<Link>
        {
            public int Compare(Link link1, Link link2)
            {
                return ((link1 != null) && (link2 != null)) ? link2.priority.CompareTo(link1.priority) : 0;
            }
        }

        /// <summary>
        /// Returns whether the DialogueEntry passes through to evaluate children when its 
        /// condition is false.
        /// </summary>
        /// <returns>
        /// <c>true</c> if this instance is passthrough on false; otherwise, <c>false</c>.
        /// </returns>
        /// <param name='entry'>
        /// The DialogueEntry to check.
        /// </param>
        public static bool IsPassthroughOnFalse(DialogueEntry entry)
        {
            return string.Equals(entry.falseConditionAction, "Passthrough");
        }

    }

}
