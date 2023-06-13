// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// This part of the Dialogue Editor window contains the screenplay export code.
    /// </summary>
    public static class ScreenplayExporter
    {
        private static bool omitNoneOrContinueEntries = false;
        private static string lastActorName = string.Empty;

        /// <summary>
        /// The main export method. Exports screenplay text files for each language.
        /// </summary>
        /// <param name="database">Source database.</param>
        /// <param name="filename">Target filename.</param>
        /// <param name="encodingType">Encoding type.</param>
        /// <param name="omitNoneSequenceEntries">Omit lines whose Sequence fields are set to None() or Continue().</param>
        public static void Export(DialogueDatabase database, string filename, EncodingType encodingType, bool omitNoneSequenceEntries)
        {
            if (database == null || string.IsNullOrEmpty(filename)) return;
            omitNoneOrContinueEntries = omitNoneSequenceEntries;
            lastActorName = string.Empty;
            var otherLanguages = FindOtherLanguages(database);
            ExportFile(database, string.Empty, filename, encodingType);
            foreach (var language in otherLanguages)
            {
                ExportFile(database, language, filename, encodingType);
            }
        }

        private static List<string> FindOtherLanguages(DialogueDatabase database)
        {
            var otherLanguages = new List<string>();
            foreach (var conversation in database.conversations)
            {
                foreach (var entry in conversation.dialogueEntries)
                {
                    foreach (var field in entry.fields)
                    {
                        if ((field.type == FieldType.Localization) &&
                            !otherLanguages.Contains(field.title) &&
                            !field.title.Contains(" "))
                        {
                            otherLanguages.Add(field.title);
                        }
                    }
                }
            }
            return otherLanguages;
        }

        private static void ExportFile(DialogueDatabase database, string language, string baseFilename, EncodingType encodingType)
        {
            var filename = string.IsNullOrEmpty(language) ? baseFilename
                : Path.GetDirectoryName(baseFilename) + "/" + Path.GetFileNameWithoutExtension(baseFilename) + "_" + language + ".csv";
            using (StreamWriter file = new StreamWriter(filename, false, EncodingTypeTools.GetEncoding(encodingType)))
            {
                ExportConversations(database, language, file);
            }
        }

        private static void ExportConversations(DialogueDatabase database, string language, StreamWriter file)
        {
            // Cache actor names:
            Dictionary<int, string> actorNames = new Dictionary<int, string>();

            // We need to know how many links go into each entry to know whether to show [id]:
            Dictionary<int, int> numLinksToEntry = new Dictionary<int, int>();

            // Track which entries we've already done:
            List<DialogueEntry> visited = new List<DialogueEntry>();

            // Export all conversations:
            foreach (var conversation in database.conversations)
            {
                file.WriteLine(string.Format("[{0}]\t{1}", conversation.id, conversation.Title.ToUpper()));
                file.WriteLine(string.Empty);
                if (!string.IsNullOrEmpty(conversation.Description))
                {
                    file.WriteLine(string.Format("\t{0}", conversation.Description));
                    file.WriteLine(string.Empty);
                }

                // Count num links going into each entry:
                foreach (var entry in conversation.dialogueEntries)
                {
                    numLinksToEntry[entry.id] = 0;
                }
                foreach (var entry in conversation.dialogueEntries)
                {
                    foreach (var link in entry.outgoingLinks)
                    {
                        if (link.destinationConversationID == entry.conversationID)
                        {
                            numLinksToEntry[link.destinationDialogueID]++;
                        }
                    }
                }

                // Export depth first, starting from <START> node:
                ExportSubtree(database, language, actorNames, numLinksToEntry, visited, conversation.GetFirstDialogueEntry(), 0, file);

                // Add orphan nodes at the end:
                foreach (var e in conversation.dialogueEntries)
                {
                    if (!visited.Contains(e))
                    {
                        ExportSubtree(database, language, actorNames, numLinksToEntry, visited, e, -1, file);
                    }
                }

                file.WriteLine(string.Empty);
                file.WriteLine("---page---");
                file.WriteLine(string.Empty);
            }
        }

        private static void ExportSubtree(DialogueDatabase database, string language, Dictionary<int, string> actorNames, Dictionary<int, int> numLinksToEntry, List<DialogueEntry> visited, DialogueEntry entry, int siblingIndex, StreamWriter file)
        {
            if (entry == null) return;
            visited.Add(entry);
            if (entry.id > 0)
            {
                var omit = omitNoneOrContinueEntries && (entry.Sequence == "None()" || entry.Sequence == "Continue()");
                var show = !omit;

                // Write this entry (the root of the subtree).

                // Write entry ID if necessary:
                if (siblingIndex == -1)
                {
                    if (show) file.WriteLine(string.Format("\tUnconnected entry [{0}]:", entry.id));
                    if (show) file.WriteLine(string.Empty);
                }
                else if ((siblingIndex == 0 && !string.IsNullOrEmpty(entry.conditionsString)) ||
                    (siblingIndex > 0)  ||
                    (numLinksToEntry.ContainsKey(entry.id) && numLinksToEntry[entry.id] > 1))
                {
                    if (string.IsNullOrEmpty(entry.conditionsString))
                    {
                        if (show) file.WriteLine(string.Format("\tEntry [{0}]:", entry.id));
                    }
                    else
                    {
                        if (show) file.WriteLine(string.Format("\tEntry [{0}]: ({1})", entry.id, entry.conditionsString));
                    }
                    if (show) file.WriteLine(string.Empty);
                }
                if (!actorNames.ContainsKey(entry.ActorID))
                {
                    Actor actor = database.GetActor(entry.ActorID);
                    actorNames.Add(entry.ActorID, (actor != null) ? actor.Name.ToUpper() : "ACTOR");
                }
                if (show)
                {
                    var actorName = actorNames[entry.ActorID];
                    if (actorName != lastActorName)
                    {
                        lastActorName = actorName;
                        file.WriteLine(string.Format("\t\t\t\t{0}", actorName));
                    }
                }

                var description = Field.LookupValue(entry.fields, "Description");
                if (!string.IsNullOrEmpty(description))
                {
                    if (show) file.WriteLine(string.Format("\t\t\t({0})", description));
                }
                var lineText = string.IsNullOrEmpty(language) ? entry.subtitleText : Field.LookupValue(entry.fields, language);
                if (entry.isGroup)
                {
                    // Group entries use Title:
                    lineText = Field.LookupValue(entry.fields, "Title");
                    lineText = !string.IsNullOrEmpty(lineText) ? ("(" + lineText + ")") : "(Group entry; no dialogue)";
                }
                if (show) file.WriteLine(string.Format("\t\t{0}", lineText));
                if (show) file.WriteLine(string.Empty);
            }

            // Handle link summary:
            if (entry.outgoingLinks.Count == 0)
            {
                file.WriteLine("\t\t\t\t[END]");
                file.WriteLine(string.Empty);
            }
            else if (entry.outgoingLinks.Count > 1)
            {
                var s = "\tResponses: ";
                var first = true;
                for (int i = 0; i < entry.outgoingLinks.Count; i++)
                {
                    if (!first) s += ", ";
                    first = false;
                    var link = entry.outgoingLinks[i];
                    if (link.destinationConversationID == entry.conversationID)
                    {
                        s += "[" + link.destinationDialogueID + "]";
                    }
                    else
                    {
                        var destConversation = database.GetConversation(link.destinationConversationID);
                        if (destConversation != null)
                        {
                            s += "[" + destConversation.Title.ToUpper() + ":" + link.destinationDialogueID + "]";
                        }
                        else
                        {
                            s += "[Other Conversation]";
                        }
                    }
                }
                file.WriteLine(s);
                file.WriteLine(string.Empty);
            }

            // Follow each outgoing link as a subtree:
            for (int i = 0; i < entry.outgoingLinks.Count; i++)
            {
                var child = database.GetDialogueEntry(entry.outgoingLinks[i]);
                if (!visited.Contains(child))
                {
                    ExportSubtree(database, language, actorNames, numLinksToEntry, visited, child, i, file);
                }
            }
        }

    }
}
