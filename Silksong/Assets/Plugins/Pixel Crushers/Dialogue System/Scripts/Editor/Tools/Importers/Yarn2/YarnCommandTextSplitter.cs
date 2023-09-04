#if USE_YARN2

/*

The MIT License (MIT)

Copyright (c) 2015-2017 Secret Lab Pty. Ltd. and Yarn Spinner contributors.

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

*/

using System.Collections.Generic;
using Antlr4.Runtime;

// NOTE: Copied from Yarn's Github repo:
//      - YarnSpinner/YarnSpinner.LanguageServer/src/Server/Utils/CommandTextSplitter.cs
//      - https://github.com/YarnSpinnerTool/YarnSpinner/blob/adf1cb113d965a5144285abcccdde23a54becb00/YarnSpinner.LanguageServer/src/Server/Utils/CommandTextSplitter.cs

namespace PixelCrushers.DialogueSystem.Yarn
{
    public static class CommandTextSplitter
    {
        public class CommandTextItem {
            public CommandTextItem(string text, int offset)
            {
                Text = text;
                Offset = offset;
            }

            public string Text { get; set; }
            public int Offset { get; set; }
        }

        /// <summary>
        /// Splits input into a number of non-empty sub-strings, separated
        /// by whitespace, and grouping double-quoted strings into a single
        /// sub-string.
        /// </summary>
        /// <param name="input">The string to split.</param>
        /// <returns>A collection of sub-strings.</returns>
        /// <remarks>
        /// This method behaves similarly to the <see
        /// cref="string.Split(char[], StringSplitOptions)"/> method with
        /// the <see cref="StringSplitOptions"/> parameter set to <see
        /// cref="StringSplitOptions.RemoveEmptyEntries"/>, with the
        /// following differences:
        ///
        /// <list type="bullet">
        /// <item>Text that appears inside a pair of double-quote
        /// characters will not be split.</item>
        ///
        /// <item>Text that appears after a double-quote character and
        /// before the end of the input will not be split (that is, an
        /// unterminated double-quoted string will be treated as though it
        /// had been terminated at the end of the input.)</item>
        ///
        /// <item>When inside a pair of double-quote characters, the string
        /// <c>\\</c> will be converted to <c>\</c>, and the string
        /// <c>\"</c> will be converted to <c>"</c>.</item>
        /// </list>
        /// </remarks>
        public static IReadOnlyList<CommandTextItem> SplitCommandText(string input, bool addBackInTheQuotes = false)
        {
            var reader = new System.IO.StringReader(input.Normalize());

            int c;

            int currentComponentOffset = 0;

            int position = 0;

            var results = new List<CommandTextItem>();
            var currentComponent = new System.Text.StringBuilder();

            while ((c = reader.Read()) != -1)
            {
                if (char.IsWhiteSpace((char)c))
                {
                    if (currentComponent.Length > 0)
                    {
                        // We've reached the end of a run of visible
                        // characters. Add this run to the result list and
                        // prepare for the next one.
                        results.Add(new CommandTextItem(currentComponent.ToString(), currentComponentOffset));
                        currentComponent.Clear();

                        currentComponentOffset = position + 1;
                    }
                    else
                    {
                        // We encountered a whitespace character, but
                        // didn't have any characters queued up. Skip this
                        // character.
                        currentComponentOffset = position + 1;
                    }

                    position += 1;
                    continue;
                }
                else if (c == '\"')
                {
                    // We've entered a quoted string!
                    while (true)
                    {
                        c = reader.Read();
                        if (c == -1)
                        {
                            // Oops, we ended the input while parsing a
                            // quoted string! Dump our current word
                            // immediately and return.
                            results.Add(new CommandTextItem(currentComponent.ToString(), currentComponentOffset));
                            return results;
                        }
                        else if (c == '\\')
                        {
                            // Possibly an escaped character!
                            var next = reader.Peek();
                            if (next == '\\' || next == '\"')
                            {
                                // It is! Skip the \ and use the character
                                // after it.
                                reader.Read();
                                currentComponent.Append((char)next);
                            }
                            else
                            {
                                // Oops, an invalid escape. Add the \ and
                                // whatever is after it.
                                currentComponent.Append((char)c);
                            }
                        }
                        else if (c == '\"')
                        {
                            // The end of a string!
                            break;
                        }
                        else
                        {
                            // Any other character. Add it to the buffer.
                            currentComponent.Append((char)c);
                        }
                    }

                    var output = addBackInTheQuotes ? $"\"{currentComponent.ToString()}\"" : currentComponent.ToString();

                    var bork = new CommandTextItem(output, currentComponentOffset);
                    results.Add(bork);
                    // results.Add(new CommandTextItem(currentComponent.ToString(), currentComponentOffset));
                    currentComponent.Clear();
                    currentComponentOffset = position + 1;
                }
                else
                {
                    currentComponent.Append((char)c);
                }

                position += 1;
            }

            if (currentComponent.Length > 0)
            {
                results.Add(new CommandTextItem(currentComponent.ToString(), currentComponentOffset));
            }

            return results;
        }
    }
}

#endif