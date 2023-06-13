// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// This part of the Dialogue Editor window contains the voiceover script export code.
    /// </summary>
    public static class VoiceoverScriptExporter
    {

        /// <summary>
        /// The main export method. Exports voiceover scripts to a CSV file for each language.
        /// </summary>
        /// <param name="database">Source database.</param>
        /// <param name="filename">Target CSV filename.</param>
        /// <param name="exportActors">If set to <c>true</c> export actors.</param>
        public static void Export(DialogueDatabase database, string filename, bool exportActors, EntrytagFormat entrytagFormat, EncodingType encodingType)
        {
            if (database == null || string.IsNullOrEmpty(filename)) return;
            var otherLanguages = FindOtherLanguages(database);
            ExportFile(database, string.Empty, filename, exportActors, entrytagFormat, encodingType);
            foreach (var language in otherLanguages)
            {
                ExportFile(database, language, filename, exportActors, entrytagFormat, encodingType);
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

        private static void ExportFile(DialogueDatabase database, string language, string baseFilename, bool exportActors, EntrytagFormat entrytagFormat, EncodingType encodingType)
        {
            var filename = string.IsNullOrEmpty(language) ? baseFilename
                : Path.GetDirectoryName(baseFilename) + "/" + Path.GetFileNameWithoutExtension(baseFilename) + "_" + language + ".csv";
            using (StreamWriter file = new StreamWriter(filename, false, EncodingTypeTools.GetEncoding(encodingType)))
            {
                ExportDatabaseProperties(database, file);
                if (exportActors) ExportActors(database, file);
                ExportConversations(database, language, entrytagFormat, file);
            }
        }

        private static void ExportDatabaseProperties(DialogueDatabase database, StreamWriter file)
        {
            file.WriteLine("Database," + CleanField(database.name));
            file.WriteLine("Author," + CleanField(database.author));
            file.WriteLine("Version," + CleanField(database.version));
            file.WriteLine("Description," + CleanField(database.description));
        }

        private static void ExportActors(DialogueDatabase database, StreamWriter file)
        {
            file.WriteLine(string.Empty);
            file.WriteLine("---Actors---");
            file.WriteLine("Name,Description");
            foreach (var actor in database.actors)
            {
                file.WriteLine(CleanField(actor.Name) + "," + CleanField(actor.LookupValue("Description")));
            }
        }

        private static void ExportConversations(DialogueDatabase database, string language, EntrytagFormat entrytagFormat, StreamWriter file)
        {
            file.WriteLine(string.Empty);
            file.WriteLine("---Conversations---");

            // Cache actor names:
            Dictionary<int, string> actorNames = new Dictionary<int, string>();

            // Export all conversations:
            foreach (var conversation in database.conversations)
            {
                file.WriteLine(string.Empty);
                file.WriteLine(string.Format("Conversation {0},{1}", conversation.id, CleanField(conversation.Title)));
                file.WriteLine(string.Format("Description,{0}", CleanField(conversation.Description)));
                StringBuilder sb = new StringBuilder("entrytag,Actor,Description,");
                sb.Append(string.IsNullOrEmpty(language) ? "Dialogue Text" : CleanField(language));
                file.WriteLine(sb.ToString());
                foreach (var entry in conversation.dialogueEntries)
                {
                    if (entry.id > 0)
                    {
                        sb = new StringBuilder();
                        if (!actorNames.ContainsKey(entry.ActorID))
                        {
                            Actor actor = database.GetActor(entry.ActorID);
                            actorNames.Add(entry.ActorID, (actor != null) ? CleanField(actor.Name) : "ActorNotFound");
                        }
                        string actorName = actorNames[entry.ActorID];
                        string description = Field.LookupValue(entry.fields, "Description");
                        string entrytag = database.GetEntrytag(conversation, entry, entrytagFormat);
                        var lineText = string.IsNullOrEmpty(language) ? entry.subtitleText : Field.LookupValue(entry.fields, language);
                        sb.AppendFormat("{0},{1},{2},{3}", CleanField(entrytag), CleanField(actorName), CleanField(description), CleanField(lineText));
                        file.WriteLine(sb.ToString());
                    }
                }
            }
        }

        private static string CleanField(string s)
        {
            return CSVExporter.CleanField(s);
        }

    }

}
