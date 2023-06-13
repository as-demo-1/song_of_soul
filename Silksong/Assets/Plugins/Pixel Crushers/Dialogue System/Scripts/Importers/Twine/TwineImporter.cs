#if USE_TWINE
// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace PixelCrushers.DialogueSystem.Twine
{

    /// <summary>
    /// Imports Twine 2 Harlowe stories exported from Twison
    /// into dialogue database conversations.
    /// </summary>
    public class TwineImporter
    {

        #region Convert

        protected DialogueDatabase database { get; set; }
        protected Template template { get; set; }

        public virtual void ConvertStoryToConversation(DialogueDatabase database, Template template, TwineStory story, int actorID, int conversantID, bool splitPipesIntoEntries, bool useTwineNodePositions = false)
        {
            this.database = database;
            this.template = template;

            // Get/create conversation:
            var conversation = database.GetConversation(story.name);
            if (conversation == null)
            {
                conversation = template.CreateConversation(template.GetNextConversationID(database), story.name);
                database.conversations.Add(conversation);
            }
            conversation.ActorID = actorID;
            conversation.ConversantID = conversantID;

            // Reset to just <START> node:
            conversation.dialogueEntries.Clear();
            var startEntry = template.CreateDialogueEntry(0, conversation.id, "START");
            conversation.dialogueEntries.Add(startEntry);

            // Find the highest pid:
            int highestPid = 0;
            foreach (var passage in story.passages)
            {
                highestPid = Mathf.Max(highestPid, SafeConvert.ToInt(passage.pid));
            }


            // Add passages as nodes:
            var isFirstPassage = true;
            var allHooks = new Dictionary<TwinePassage, List<TwineHook>>();
            foreach (var passage in story.passages)
            {
                var entryID = SafeConvert.ToInt(passage.pid);
                if (entryID == 0) entryID = ++highestPid;
                var entry = template.CreateDialogueEntry(entryID, conversation.id, passage.name);
                if (useTwineNodePositions)
                {
                    SetEntryPosition(entry, passage.position);
                    if (isFirstPassage)
                    {
                        isFirstPassage = false;
                        SetEntryPosition(startEntry, new TwinePosition(Mathf.Max(1, passage.position.x - DialogueEntry.CanvasRectWidth / 4f), Mathf.Max(1, passage.position.y - 1.5f * DialogueEntry.CanvasRectHeight)));
                    }
                }
                int entryActorID, entryConversantID;
                string dialogueText, sequence, conditions, script, description;
                List<TwineHook> hooks;
                ExtractParticipants(passage.text, actorID, conversantID, false, out dialogueText, out entryActorID, out entryConversantID);
                ExtractSequenceConditionsScriptDescription(ref dialogueText, out sequence, out conditions, out script, out description);
                ExtractHooks(ref dialogueText, out hooks);
                allHooks.Add(passage, hooks);
                dialogueText = RemoveAllLinksFromText(dialogueText);
                ExtractMacros(ref dialogueText, ref entry);
                dialogueText = ReplaceFormatting(dialogueText);
                entry.DialogueText = dialogueText.Trim();
                entry.ActorID = entryActorID;
                entry.ConversantID = conversantID;
                entry.Sequence = AppendCode(entry.Sequence, sequence);
                string falseConditionAction;
                CheckConditionsForPassthrough(conditions, out conditions, out falseConditionAction);
                entry.conditionsString = AppendCode(entry.conditionsString, conditions);
                entry.falseConditionAction = falseConditionAction;
                entry.userScript = AppendCode(entry.userScript, script);
                Field.SetValue(entry.fields, DialogueSystemFields.Description, description);
                conversation.dialogueEntries.Add(entry);
            }

            // Link startnode:
            var startnodeID = SafeConvert.ToInt(story.startnode);
            startEntry.outgoingLinks.Add(new Link(conversation.id, startEntry.id, conversation.id, startnodeID));

            // Link nodes:
            int linkNum = 0;
            foreach (var passage in story.passages)
            {
                if (passage.links == null) continue;
                var originID = SafeConvert.ToInt(passage.pid);
                var originEntry = conversation.GetDialogueEntry(originID);
                foreach (var link in passage.links)
                {
                    if (link == null) continue;
                    var willLinkInHook = IsLinkInHooks(link.link, allHooks[passage]);
                    var linkedPassageID = SafeConvert.ToInt(link.pid);
                    //-- Save for potential future use: var destinationPassageEntry = conversation.GetDialogueEntry(link.link) ?? conversation.GetDialogueEntry(RemoveFormatting(link.link));
                    if (IsLinkImplicit(link))
                    {
                        // Link passages directly with implicit links (with parens around name):
                        if (!willLinkInHook)
                        {
                            originEntry.outgoingLinks.Add(new Link(conversation.id, originID, conversation.id, linkedPassageID));
                        }
                    }
                    else
                    {
                        // Check if there's a node that's a repeat of the link (and, if so, don't add a link entry):
                        var linkRepeatEntry = conversation.GetDialogueEntry(link.name);
                        if (linkRepeatEntry != null && linkRepeatEntry.ActorID == conversation.ActorID)
                        {
                            // Link links to node for that link (to allow Script: etc in link), so do nothing.
                            var linkEntry = linkRepeatEntry;
                            if (useTwineNodePositions)
                            {
                                SetEntryPosition(linkEntry, new TwinePosition(passage.position.x + DialogueEntry.CanvasRectWidth / 4f + (linkNum * (DialogueEntry.CanvasRectWidth + 8)), passage.position.y + (1.5f * DialogueEntry.CanvasRectHeight)));
                                linkNum++;
                            }
                            int linkActorID, linkConversantID;
                            string linkDialogueText, sequence, conditions, script, description;
                            ExtractParticipants(link.name, actorID, conversantID, true, out linkDialogueText, out linkActorID, out linkConversantID);
                            ExtractSequenceConditionsScriptDescription(ref linkDialogueText, out sequence, out conditions, out script, out description);
                            linkEntry.DialogueText = ReplaceFormatting(linkDialogueText);
                            linkEntry.ActorID = linkActorID;
                            linkEntry.ConversantID = linkConversantID;
                            linkEntry.Sequence = sequence;
                            linkEntry.conditionsString = AppendCode(linkEntry.conditionsString, conditions);
                            linkEntry.userScript = AppendCode(linkEntry.userScript, script);
                            originEntry.outgoingLinks.Add(new Link(conversation.id, originID, conversation.id, linkEntry.id));
                        }
                        else
                        {
                            // Otherwise add a link entry between passages:
                            var linkEntryTitle = GetLinkEntryTitle(link.name, originID);
                            var linkEntry = conversation.GetDialogueEntry(linkEntryTitle);
                            if (linkEntry == null)
                            {
                                linkEntry = template.CreateDialogueEntry(++highestPid, conversation.id, linkEntryTitle);
                                if (useTwineNodePositions)
                                {
                                    SetEntryPosition(linkEntry, new TwinePosition(passage.position.x + DialogueEntry.CanvasRectWidth / 4f + (linkNum * (DialogueEntry.CanvasRectWidth + 8)), passage.position.y + (1.5f * DialogueEntry.CanvasRectHeight)));
                                    linkNum++;
                                }
                                int linkActorID, linkConversantID;
                                string linkDialogueText, sequence, conditions, script, description;
                                ExtractParticipants(link.name, actorID, conversantID, true, out linkDialogueText, out linkActorID, out linkConversantID);
                                ExtractSequenceConditionsScriptDescription(ref linkDialogueText, out sequence, out conditions, out script, out description);
                                linkEntry.DialogueText = ReplaceFormatting(linkDialogueText);
                                linkEntry.ActorID = linkActorID;
                                linkEntry.ConversantID = linkConversantID;
                                linkEntry.Sequence = sequence;
                                linkEntry.conditionsString = AppendCode(linkEntry.conditionsString, conditions);
                                linkEntry.userScript = AppendCode(linkEntry.userScript, script);
                            }
                            conversation.dialogueEntries.Add(linkEntry);
                            if (!willLinkInHook)
                            {
                                originEntry.outgoingLinks.Add(new Link(conversation.id, originID, conversation.id, linkEntry.id));
                            }
                            linkEntry.outgoingLinks.Add(new Link(conversation.id, linkEntry.id, conversation.id, linkedPassageID));
                        }
                    }
                }
            }

            // Link hooks:
            foreach (var passage in story.passages)
            {
                var passageID = SafeConvert.ToInt(passage.pid);
                var passageEntry = conversation.GetDialogueEntry(passageID);
                var rectOffset = Vector2.zero;
                var passageHooks = allHooks[passage];
                foreach (var hook in passageHooks)
                {
                    int hookActorID, hookConversantID;
                    ExtractParticipants(hook.text, passageEntry.ActorID, passageEntry.ConversantID, true, out hook.text, out hookActorID, out hookConversantID);
                    var conditions = hook.prefix.StartsWith("(if:") ? ConvertIfMacro(hook.prefix) : string.Empty;
                    if (hook.links.Count == 0)
                    {
                        var linkEntry = conversation.GetDialogueEntry(GetLinkEntryTitle(hook.text, passageID));
                        if (linkEntry != null) linkEntry.conditionsString = conditions;
                    }
                    else
                    {
                        foreach (var link in hook.links)
                        {
                            var linkEntry = conversation.GetDialogueEntry(GetLinkEntryTitle(link, passageID));
                            if (!string.IsNullOrEmpty(hook.text))
                            {
                                // Hook still has text, so make the text an intermediate entry:
                                var midEntry = template.CreateDialogueEntry(++highestPid, conversation.id, hook.text);
                                if (useTwineNodePositions)
                                {
                                    SetEntryPosition(linkEntry, new TwinePosition(passage.position.x + DialogueEntry.CanvasRectWidth / 4f + (linkNum * (DialogueEntry.CanvasRectWidth + 8)), passage.position.y + (1.5f * DialogueEntry.CanvasRectHeight)));
                                    linkNum++;
                                }
                                midEntry.DialogueText = hook.text;
                                midEntry.ActorID = hookActorID;
                                midEntry.ConversantID = hookConversantID;

                                string falseConditionAction;
                                CheckConditionsForPassthrough(conditions, out conditions, out falseConditionAction);
                                midEntry.conditionsString = AppendCode(midEntry.conditionsString, conditions);
                                midEntry.falseConditionAction = falseConditionAction;

                                conversation.dialogueEntries.Add(midEntry);
                                passageEntry.outgoingLinks.Add(new Link(conversation.id, passageEntry.id, conversation.id, midEntry.id));
                            }
                            else
                            {
                                // Otherwise link directly from passage to link entry:
                                linkEntry.conditionsString = conditions;
                                passageEntry.outgoingLinks.Add(new Link(conversation.id, passageEntry.id, conversation.id, linkEntry.id));
                            }
                        }
                    }
                }
            }

            // Split pipes:
            if (splitPipesIntoEntries)
            {
                conversation.SplitPipesIntoEntries();
            }
        }

        protected virtual void SetEntryPosition(DialogueEntry entry, TwinePosition position)
        {
            entry.canvasRect = new Rect(position.x, position.y, DialogueEntry.CanvasRectWidth, DialogueEntry.CanvasRectHeight);
        }

        protected string GetLinkEntryTitle(string linkName, int originPassageID)
        {
            // Include ID to make links with same names unique.
            return linkName + " Link " + originPassageID;
        }

        protected virtual void ExtractParticipants(string text, int actorID, int conversantID, bool isLinkEntry,
            out string dialogueText, out int entryActorID, out int entryConversantID)
        {
            ExtractActor(text, actorID, conversantID, isLinkEntry, out dialogueText, out entryActorID);
            if (entryActorID == -1) entryActorID = isLinkEntry ? actorID : conversantID;
            entryConversantID = (entryActorID == actorID) ? conversantID : actorID;
        }

        protected virtual void ExtractActor(string text, int actorID, int conversantID, bool isLinkEntry,
            out string dialogueText, out int entryActorID)
        {
            entryActorID = isLinkEntry ? actorID : conversantID;
            dialogueText = text;
            var colonPos = text.IndexOf(':');
            if (colonPos != -1)
            {
                var potentialActorName = text.Substring(0, colonPos);
                var remainder = text.Substring(colonPos + 1).TrimStart(new char[] { ' ', '\n', '\t' });
                var actor = database.GetActor(potentialActorName);
                if (actor != null)
                {
                    entryActorID = actor.id;
                    dialogueText = remainder;
                }
            }
            dialogueText = dialogueText.Trim();
        }

        protected virtual void ExtractSequenceConditionsScriptDescription(ref string text, out string sequence, out string conditions, out string script, out string description)
        {
            ExtractBlock("Sequence:", ref text, out sequence);
            ExtractBlock("Conditions:", ref text, out conditions);
            ExtractBlock("Script:", ref text, out script);
            ExtractBlock("Description:", ref text, out description);
        }

        protected virtual void ExtractBlock(string heading, ref string text, out string block)
        {
            var index = text.IndexOf(heading);
            if (index != -1)
            {
                var blockIndex = index + heading.Length;
                var sequenceIndex = FindBlockIndex(text, blockIndex, "Sequence:");
                var conditionsIndex = FindBlockIndex(text, blockIndex, "Conditions:");
                var scriptIndex = FindBlockIndex(text, blockIndex, "Script:");
                var descriptionIndex = FindBlockIndex(text, blockIndex, "Description:");
                var rindex = Mathf.Min(sequenceIndex, Mathf.Min(conditionsIndex, scriptIndex));
                block = text.Substring(blockIndex, rindex - blockIndex).Trim();
                var remaining = text.Substring(0, index);
                if (rindex < text.Length) remaining += text.Substring(rindex);
                text = remaining.Trim();
            }
            else
            {
                block = string.Empty;
            }
        }

        protected int FindBlockIndex(string text, int startIndex, string heading)
        {
            var index = text.IndexOf(heading, startIndex);
            return (index == -1) ? text.Length : index;
        }

        protected void CheckConditionsForPassthrough(string originalConditions, out string conditions, out string falseConditionAction)
        {
            var passthrough = false;
            if (!string.IsNullOrEmpty(originalConditions) && originalConditions.StartsWith("(passthrough)"))
            {
                passthrough = true;
                conditions = originalConditions.Substring("(passthrough)".Length);
            }
            else
            {
                conditions = originalConditions;
            }
            falseConditionAction = passthrough ? "Passthrough" : "Block";
        }

        protected string AppendCode(string block, string extra)
        {
            if (string.IsNullOrEmpty(extra)) return block;
            if (string.IsNullOrEmpty(block)) return extra;
            return block + ";\n" + extra;
        }

        #endregion

        #region Links and Hooks

        protected const string LinkRegexPattern = @"\[\[.*?\]\]";
        protected static Regex LinkRegex = new Regex(LinkRegexPattern);
        protected static Regex PrefixedHookRegex = new Regex(@"\(.+\)\[.+\]");

        public class TwineHook
        {
            public string prefix; // (part) before hooks such as (if: x==y).
            public string text; // Remaining text after extracting links.
            public List<string> links;
            public TwineHook(string prefix, string text, List<string> links)
            { this.prefix = prefix; this.text = text; this.links = links; }
        }

        protected virtual void ExtractHooks(ref string text, out List<TwineHook> hooks)
        {
            hooks = new List<TwineHook>();

            // Extract (...)[...]:
            var matches = PrefixedHookRegex.Matches(text);
            foreach (var match in matches.Cast<Match>().Reverse())
            {
                var index = match.Value.IndexOf(")[");
                if (index == -1) continue;
                var prefix = match.Value.Substring(0, index + 1);
                var hookText = match.Value.Substring(index + 2, match.Length - (prefix.Length + 2)).Trim();
                var isLink = hookText.StartsWith("[");
                if (hookText.StartsWith("[")) hookText = hookText.Substring(1);
                if (hookText.EndsWith("]")) hookText = hookText.Substring(0, hookText.Length - 1);
                List<string> links;
                ExtractLinksFromText(ref hookText, out links);
                hookText = ReplaceFormatting(hookText);
                hooks.Add(new TwineHook(prefix, hookText, links));

                var newlinePos = match.Index + match.Length;
                var hasNewline = 0 <= newlinePos && newlinePos < text.Length && text[newlinePos] == '\n';

                string replacement = string.Empty;
                if (!(isLink || string.IsNullOrEmpty(hookText)))
                {
                    var condition = ConvertIfMacro(prefix).Replace("\"", "\\\"");
                    var cleanHookText = hookText.Replace("\"", "\\\"");
                    if (hasNewline) cleanHookText += "\\n";
                    replacement = "[lua(Conditional(\"" + condition + "\", \"" + cleanHookText + "\"))]";
                }

                text = Replace(text, match.Index, match.Length + (hasNewline ? 1 : 0), replacement);
            }
        }

        protected virtual void ExtractLinksFromText(ref string text, out List<string> links)
        {
            links = new List<string>();
            var matches = LinkRegex.Matches(text);
            foreach (var match in matches.Cast<Match>().Reverse())
            {
                links.Add(match.Value.Substring(2, match.Value.Length - 4));
                text = Replace(text, match.Index, match.Length, string.Empty);
            }
            text = text.Trim();
        }

        protected virtual string RemoveAllLinksFromText(string text)
        {
            return Regex.Replace(text, LinkRegexPattern, string.Empty).Trim();
        }

        protected bool IsLinkInHooks(string link, List<TwineHook> hooks)
        {
            foreach (var hook in hooks)
            {
                if (hook.links.Contains(link)) return true;
            }
            return false;
        }

        protected bool IsLinkImplicit(TwineLink link)
        {
            return (link.name.Length > 2) && (link.name[0] == '(') && (link.name[link.name.Length - 1] == ')');
        }

        #endregion

        #region Formatting

        protected virtual string RemoveFormatting(string s)
        {
            var result = Regex.Replace(s, @"\/\/.*?\/\/|\'\'.*?\'\'|\*\*.*?\*\*|\*.*?\*", string.Empty);
            result = Regex.Replace(result, @"==>|=><=|<==", string.Empty);
            return result.Trim();
        }

        protected virtual string ReplaceFormatting(string s)
        {
            // Replace formatting codes:
            s = ReplaceFormattingCode(s, "//", "<i>", "</i>");
            s = ReplaceFormattingCode(s, "''", "<b>", "</b>");
            s = ReplaceFormattingCode(s, "**", "<b>", "</b>");
            s = ReplaceFormattingCode(s, "*", "<i>", "</i>");

            // Replace variables:
            s = ReplaceVariables(s);

            return s;
        }

        protected virtual string ReplaceFormattingCode(string s, string formatCode, string richCodeOpen, string richCodeClose)
        {
            int safeguard = 0;
            while (s.Contains(formatCode) && safeguard++ < 999)
            {
                var index = s.IndexOf(formatCode);
                var nextIndex = (index + formatCode.Length < s.Length) ? s.IndexOf(formatCode, index + 2) : -1;
                if (nextIndex == -1) break; // Not paired, so stop.
                s = s.Substring(0, index) + richCodeOpen +
                    s.Substring(index + formatCode.Length, nextIndex - (index + formatCode.Length)) + richCodeClose +
                    s.Substring(nextIndex + formatCode.Length);
            }
            return s;
        }

        protected static Regex GlobalVariableRegex = new Regex(@"\$\w+");
        protected static Regex LocalVariableRegex = new Regex(@"_\w+");

        protected virtual string ReplaceVariables(string s)
        {
            // Replace $x with Variable["x"]:
            var matches = GlobalVariableRegex.Matches(s);
            foreach (var match in matches.Cast<Match>().Reverse())
            {
                var varTag = "[var=" + match.Value.Substring(1) + "]";
                s = Replace(s, match.Index, match.Length, varTag);
            }

            // Replace _x with [lua(_x)]:
            matches = LocalVariableRegex.Matches(s);
            foreach (var match in matches.Cast<Match>().Reverse())
            {
                var varTag = "[lua(" + match.Value.Substring(1) + ")]";
                s = Replace(s, match.Index, match.Length, varTag);
            }

            return s;
        }

        protected string Replace(string s, int index, int length, string replacement)
        {
            var builder = new System.Text.StringBuilder();
            builder.Append(s.Substring(0, index));
            builder.Append(replacement);
            builder.Append(s.Substring(index + length));
            return builder.ToString();
        }

        #endregion

        #region Macros

        protected static Regex MacroRegex = new Regex(@"\(\w+:.+\)");

        protected void ExtractMacros(ref string s, ref DialogueEntry entry)
        {
            var matches = MacroRegex.Matches(s);
            foreach (var match in matches.Cast<Match>().Reverse())
            {
                entry.userScript = AppendCode(entry.userScript, ConvertMacro(match.Value));
                s = Replace(s, match.Index, match.Length, string.Empty);
            }
            s.Trim();
        }

        protected string ConvertMacro(string macro)
        {
            if (string.IsNullOrEmpty(macro)) return macro;
            if (macro.StartsWith("(set:"))
            {
                return ConvertSetMacro(macro);
            }
            else
            {
                Debug.LogWarning("This Twine macro is not supported yet: " + macro);
                return "UnhandledTwineMacro(" + macro + ")";
            }
        }

        protected string ConvertSetMacro(string macro)
        {
            var s = macro.Trim();
            s = s.Substring(0, s.Length - 1); // Remove last paren.
            var tokens = s.Split(' ');
            if (tokens.Length < 4) return macro;
            var lua = string.Empty;
            var startNewExpression = true;
            for (int i = 1; i < tokens.Length; i++)
            {
                var token = tokens[i];
                if (token == "to") token = "=";
                if (startNewExpression)
                {
                    if (!string.IsNullOrEmpty(lua)) lua += ";\n";
                    lua += ConvertVariableToLua(token);
                    startNewExpression = false;
                }
                else
                {
                    if (token.EndsWith(","))
                    {
                        token = token.Substring(0, token.Length - 1);
                        startNewExpression = true;
                    }
                    lua += " " + ConvertVariableToLua(token);
                }
            }
            return lua;
        }

        protected string ConvertIfMacro(string macro)
        {
            var s = macro.Trim();
            s = s.Substring(0, s.Length - 1); // Remove last paren.

            // Insert space in case there's no space between if: and condition:
            var colonPos = s.IndexOf(':');
            if (colonPos != -1 && !s.Contains(": ")) s = s.Substring(0, colonPos + 1) + ' ' + s.Substring(colonPos + 1);

            var tokens = s.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length < 4) return macro;
            // Indices: [0](if [1]$var [2]is [3+]condition
            var lua = ConvertVariableToLua(tokens[1]) + " ==";
            for (int i = 3; i < tokens.Length; i++)
            {
                lua += " " + ConvertVariableToLua(tokens[i]);
            }
            return lua;
        }

        protected string ConvertVariableToLua(string variable)
        {
            if (variable.StartsWith("$"))
            {
                return "Variable[\"" + variable.Substring(1) + "\"]";
            }
            else if (variable.StartsWith("_"))
            {
                return variable.Substring(1);
            }
            else
            {
                return variable;
            }
        }

        #endregion

    }
}
#endif
