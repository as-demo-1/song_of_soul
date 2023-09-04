using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// This part of the Dialogue Editor window contains the language text export code.
    /// </summary>
    public static class LanguageTextExporter
    {

        private static HashSet<string> languages = new HashSet<string>();

        private const int MaxLineLength = 80;

        /// <summary>
        /// The main export method. Exports one text file for each language.
        /// </summary>
        /// <param name="database">Source database.</param>
        /// <param name="filename">Target filename for default text.</param>
        public static void Export(DialogueDatabase database, string filename, EncodingType encodingType)
        {
            FindAllLanguages(database);
            ExportLanguage(database, string.Empty, filename, encodingType);
            var originalLanguage = Localization.language;
            foreach (var language in languages)
            {
                Localization.language = language;
                ExportLanguage(database, language, filename, encodingType);
            }
            Localization.language = originalLanguage;
        }

        private static void FindAllLanguages(DialogueDatabase database)
        {
            languages.Clear();
            foreach (var conversation in database.conversations)
            {
                foreach (var entry in conversation.dialogueEntries)
                {
                    foreach (var field in entry.fields)
                    {
                        if ((field.type == FieldType.Localization) &&
                            !field.title.Contains(" "))
                        {
                            languages.Add(field.title);
                        }
                    }
                }
            }
        }

        private static void ExportLanguage(DialogueDatabase database, string language, string baseFilename, EncodingType encodingType)
        {
            var filename = string.IsNullOrEmpty(language) ? baseFilename
                : Path.GetDirectoryName(baseFilename) + "/" + Path.GetFileNameWithoutExtension(baseFilename) + "_" + language + ".txt";
            var lines = CharsToLines(GetUniqueCharacters(database));
            File.WriteAllLines(filename, lines);
            //using (StreamWriter file = new StreamWriter(filename, false, EncodingTypeTools.GetEncoding(encodingType)))
            //{
            //    ExportTextFields(database, file);
            //}
        }

        /// <summary>
        /// Gets all unique characters in database for current language.
        /// Assumes current localization language is already set.
        /// </summary>
        /// <returns>Array of lines of characters.</returns>
        private static HashSet<char> GetUniqueCharacters(DialogueDatabase database)
        {
            var uniqueChars = new HashSet<char>();

            // Actors:
            foreach (var actor in database.actors)
            {
                AddUniqueChars(uniqueChars, actor.localizedName);
                AddUniqueChars(uniqueChars, actor.LookupLocalizedValue("Description"));
            }

            // Locations:
            foreach (var location in database.locations)
            {
                AddUniqueChars(uniqueChars, location.localizedName);
                AddUniqueChars(uniqueChars, location.LookupLocalizedValue("Description"));
            }

            // Items/Quests:
            foreach (var item in database.items)
            {
                if (item.IsItem)
                {
                    AddUniqueChars(uniqueChars, item.localizedName);
                    AddUniqueChars(uniqueChars, item.LookupLocalizedValue("Description"));
                }
                else
                {
                    AddUniqueChars(uniqueChars, item.localizedName);
                    AddUniqueChars(uniqueChars, item.LookupLocalizedValue("Description"));
                    AddUniqueChars(uniqueChars, item.LookupLocalizedValue("Display Name"));
                    AddUniqueChars(uniqueChars, item.LookupLocalizedValue("Group"));
                    AddUniqueChars(uniqueChars, item.LookupLocalizedValue("Success Description"));
                    AddUniqueChars(uniqueChars, item.LookupLocalizedValue("Failure Description"));
                    var entryCount = item.LookupInt("Entry Count");
                    for (int i = 0; i < entryCount; i++)
                    {
                        AddUniqueChars(uniqueChars, item.LookupLocalizedValue("Entry " + (i + 1)));
                    }
                }
            }

            // Export all conversations:
            foreach (var conversation in database.conversations)
            {
                foreach (var entry in conversation.dialogueEntries)
                {
                    if (entry.id > 0)
                    {
                        AddUniqueChars(uniqueChars, entry.currentMenuText);
                        AddUniqueChars(uniqueChars, entry.currentDialogueText);
                        AddUniqueChars(uniqueChars, entry.userScript);
                    }
                }
            }

            return uniqueChars;
        }

        private static void AddUniqueChars(HashSet<char> uniqueChars, string s)
        {
            if (string.IsNullOrEmpty(s) || uniqueChars == null) return;
            for (int i = 0; i < s.Length; i++)
            {
                uniqueChars.Add(s[i]);
            }
        }

        private static string[] CharsToLines(HashSet<char> chars)
        {
            var sortedChars = chars.OrderBy(x => x);
            var sb = new StringBuilder(chars.Count);
            foreach (var c in sortedChars)
            {
                sb.Append(c);
            }
            var s = sb.ToString();
            return new List<string>(Enumerable.Range(0, s.Length / MaxLineLength)
                .Select(i => s.Substring(i * MaxLineLength, MaxLineLength))).ToArray();
        }

        //private static void ExportTextFields(DialogueDatabase database, StreamWriter file)
        //{
        //    // Actors:
        //    foreach (var actor in database.actors)
        //    {
        //        file.WriteLine(actor.localizedName + "," + actor.LookupLocalizedValue("Description"));
        //    }
        //    file.WriteLine(string.Empty);

        //    // Locations:
        //    foreach (var location in database.locations)
        //    {
        //        file.WriteLine(location.localizedName + "," + location.LookupLocalizedValue("Description"));
        //    }
        //    file.WriteLine(string.Empty);

        //    // Items/Quests:
        //    foreach (var item in database.items)
        //    {
        //        if (item.IsItem)
        //        {
        //            file.WriteLine(item.localizedName + "," + item.LookupLocalizedValue("Description"));
        //        }
        //        else
        //        {
        //            file.WriteLine(item.localizedName + "," + item.LookupLocalizedValue("Description") + "," +
        //                item.LookupLocalizedValue("Display Name") + "," + item.LookupLocalizedValue("Group") + "," +
        //                item.LookupLocalizedValue("Success Description") + "," + item.LookupLocalizedValue("Failure Description"));
        //            var entryCount = item.LookupInt("Entry Count");
        //            for (int i = 0; i < entryCount; i++)
        //            {
        //                file.WriteLine(item.LookupLocalizedValue("Entry " + (i + 1)));
        //            }
        //        }
        //    }
        //    file.WriteLine(string.Empty);

        //    // Export all conversations:
        //    foreach (var conversation in database.conversations)
        //    {
        //        foreach (var entry in conversation.dialogueEntries)
        //        {
        //            if (entry.id > 0)
        //            {
        //                file.WriteLine(entry.currentMenuText + "," + entry.currentDialogueText + "," + entry.userScript);
        //            }
        //        }
        //    }
        //}

    }

}