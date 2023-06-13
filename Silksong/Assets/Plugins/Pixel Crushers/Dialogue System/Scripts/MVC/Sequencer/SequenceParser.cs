// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using System.Collections.Generic;
using System.Text;
using System.IO;
using PixelCrushers.DialogueSystem.SequencerCommands;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// Custom exception for parser errors.
    /// </summary>
    public class ParserException : System.Exception
    {
        public ParserException(string message) : base(message) { }
    }

    /// <summary>
    /// Basic recursive descent parser for sequences.
    /// </summary>
    public class SequenceParser
    {
        private const int MaxSafeguard = 9999; // Limits loops to prevent infinite loop bugs.

        private int column; // Helps pinpoint location of syntax errors.
        private int row;

        /// <summary>
        /// Parses a sequence string into a list of sequencer command records.
        /// </summary>
        /// <param name="sequence">Sequence to parse.</param>
        /// <returns>A list of command records.</returns>
        public List<QueuedSequencerCommand> Parse(string sequence, bool throwExceptions = false)
        {
            var list = new List<QueuedSequencerCommand>();
            try
            {
                var reader = new StringReader(sequence);
                row = 1;
                column = 1;

                int safeguard = 0;
                while (reader.Peek() != -1 && safeguard < MaxSafeguard)
                {
                    safeguard++;
                    var command = ParseCommand(reader);
                    if (command != null) list.Add(command);
                }
            }
            catch (ParserException e)
            {
                if (DialogueDebug.logWarnings) Debug.LogWarning(DialogueDebug.Prefix + ": Syntax error '" + e.Message + "' at column " + column + " row " + row + " parsing: " + sequence);
                list.Clear();
                if (throwExceptions) throw e;
            }
            return list;
        }

        private QueuedSequencerCommand ParseCommand(StringReader reader)
        {
            ParseOptionalWhitespace(reader, true);

            CheckParseComment(reader);

            var required = false;
            var s = ParseWord(reader);
            if (string.Equals(s, SequencerKeywords.Required, System.StringComparison.OrdinalIgnoreCase) || string.Equals(s, SequencerKeywords.Require, System.StringComparison.OrdinalIgnoreCase))
            {
                required = true;
                ParseOptionalWhitespace(reader);
                s = ParseWord(reader);
            }
            var command = s;
            ParseOptionalWhitespace(reader);
            if (reader.Peek() == -1) return null;
            ParseOpenParen(reader);
            ParseOptionalWhitespace(reader);
            var parameters = ParseParameters(reader);
            ParseCloseParen(reader);
            ParseOptionalWhitespace(reader);
            float atTime;
            string atMessage;
            string sendMessage;
            ParsePostParameters(reader, out atTime, out atMessage, out sendMessage);
            ParseOptionalWhitespace(reader);
            if (!CheckParseComment(reader))
            {
                ParseSemicolonOrEnd(reader);
            }

            ParseOptionalWhitespace(reader);
            CheckParseComment(reader);

            return new QueuedSequencerCommand(command, parameters, atTime, atMessage, sendMessage, required);
        }

        private string ParseWord(StringReader reader, bool allowWhiteSpace = false)
        {
            var sb = new StringBuilder();
            int safeguard = 0;
            while (HasNextChar(reader) && safeguard < MaxSafeguard)
            {
                safeguard++;
                var c = (char)reader.Peek();
                if ((char.IsWhiteSpace(c) && !allowWhiteSpace) || c == '(' || c == ')' || c == ';' || c == '-')
                {
                    break;
                }
                else
                {
                    sb.Append(ReadNextChar(reader));
                }
            }
            return sb.ToString();
        }

        private void ParseOptionalWhitespace(StringReader reader, bool includingSemicolons = false)
        {
            int safeguard = 0;
            while ((IsNextCharWhiteSpace(reader) || (includingSemicolons && IsNextChar(reader, ';'))) && safeguard < MaxSafeguard)
            {
                safeguard++;
                ReadNextChar(reader);
            }
        }

        private bool IsNextCharWhiteSpace(StringReader reader)
        {
            return HasNextChar(reader) && char.IsWhiteSpace((char)reader.Peek());
        }

        private bool IsNextChar(StringReader reader, char requiredChar)
        {
            return HasNextChar(reader) && (char)reader.Peek() == requiredChar;
        }

        private bool IsNextCharNot(StringReader reader, char requiredChar)
        {
            return HasNextChar(reader) && (char)reader.Peek() != requiredChar;
        }

        private bool HasNextChar(StringReader reader)
        {
            return reader != null && reader.Peek() != -1;
        }

        private char ReadNextChar(StringReader reader)
        {
            var c = (char)reader.Read();
            if (c == '\n')
            {
                row++;
                column = 1;
            }
            else
            {
                column++;
            }
            return c;
        }

        private void ParseChar(StringReader reader, char requiredChar)
        {
            if (IsNextChar(reader, requiredChar))
            {
                ReadNextChar(reader);
            }
            else
            {
                throw new ParserException("Expected '" + requiredChar + "'");
            }
        }

        private void ParseOpenParen(StringReader reader)
        {
            ParseChar(reader, '(');
        }

        private void ParseCloseParen(StringReader reader)
        {
            ParseChar(reader, ')');
        }

        private string[] ParseParameters(StringReader reader)
        {
            var parameters = new List<string>();
            int safeguard = 0;
            while (IsNextCharNot(reader, ')') && safeguard < MaxSafeguard)
            {
                safeguard++;
                ParseOptionalWhitespace(reader);
                parameters.Add(ParseParameter(reader));
                if (IsNextChar(reader, ',')) ReadNextChar(reader);
            }
            return parameters.ToArray();
        }

        private string ParseParameter(StringReader reader)
        {
            var sb = new StringBuilder();
            int parenDepth = 0;
            int safeguard = 0;
            while (HasNextChar(reader) && safeguard < MaxSafeguard)
            {
                safeguard++;
                var c = (char)reader.Peek();
                if (parenDepth <= 0 && (c == ',' || c == ')'))
                {
                    break;
                }
                else
                {
                    var paramChar = ReadNextChar(reader);
                    sb.Append(paramChar);
                    if (paramChar == '(')
                    {
                        parenDepth++;
                    }
                    else if (paramChar == ')')
                    {
                        parenDepth--;
                    }
                }
            }
            var s = sb.ToString().Trim();
            return s;
        }

        private void ParsePostParameters(StringReader reader, out float atTime, out string atMessage, out string sendMessage)
        {
            atTime = 0;
            atMessage = string.Empty;
            sendMessage = string.Empty;
            if (IsNextChar(reader, '@'))
            {
                ParseAtSignModifier(reader, out atTime, out atMessage);
            }
            ParseOptionalWhitespace(reader);
            if (IsNextChar(reader, '-'))
            {
                ParseArrowModifier(reader, out sendMessage);
            }
        }

        private void ParseAtSignModifier(StringReader reader, out float atTime, out string atMessage)
        {
            atTime = 0;
            atMessage = string.Empty;
            if (IsNextChar(reader, '@'))
            {
                ReadNextChar(reader);
                ParseOptionalWhitespace(reader);
                var s = ParseWord(reader);
                if (string.Equals(s, "message", System.StringComparison.OrdinalIgnoreCase))
                {
                    ParseOptionalWhitespace(reader);
                    ParseChar(reader, '(');
                    ParseOptionalWhitespace(reader);
                    s = ParseWord(reader, true);
                    ParseChar(reader, ')');
                    atMessage = s;
                    atTime = string.IsNullOrEmpty(atMessage) ? 0 : 365f * 86400f; // One year -- essentially forever, since we're really waiting for the message.
                }
                else
                {
                    float value;
                    if (float.TryParse(s, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out value))
                    {
                        atTime = value;
                    }
                    else
                    {
                        throw new ParserException("Can't convert " + s + " to a number");
                    }
                }
            }
        }

        private void ParseArrowModifier(StringReader reader, out string sendMessage)
        {
            sendMessage = string.Empty;
            if (IsNextChar(reader, '-'))
            {
                ReadNextChar(reader);
                if (!IsNextChar(reader, '>'))
                {
                    throw new ParserException("Invalid modifier after command; expected @time, @Message(x), ->Message(x) or nothing");
                }
                else
                {
                    ReadNextChar(reader);
                    ParseOptionalWhitespace(reader);
                    var s = ParseWord(reader);
                    if (string.Equals(s, SequencerKeywords.Message, System.StringComparison.OrdinalIgnoreCase))
                    {
                        ParseOptionalWhitespace(reader);
                        ParseChar(reader, '(');
                        ParseOptionalWhitespace(reader);
                        s = ParseWord(reader, true);
                        sendMessage = s.Trim();
                        ParseChar(reader, ')');
                    }
                }
            }
        }

        private void ParseSemicolonOrEnd(StringReader reader)
        {
            if (!HasNextChar(reader) || (char)reader.Peek() == ';')
            {
                ReadNextChar(reader);
            }
            else
            {
                throw new ParserException("Expected semicolon or end of sequence");
            }
        }

        private bool CheckParseComment(StringReader reader)
        {
            if (!HasNextChar(reader) || (char)reader.Peek() == '/')
            {
                reader.Read();
                if (!HasNextChar(reader) || (char)reader.Peek() == '/')
                {
                    reader.ReadLine();
                    return true;
                }
            }
            return false;
        }

    }
}