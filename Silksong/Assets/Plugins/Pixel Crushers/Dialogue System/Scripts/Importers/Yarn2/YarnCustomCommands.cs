#if USE_YARN2

// WARNING: Do not modify this file, it is automatically generated on every Yarn project import.
//          To add/change behavior, subclass YarnCustomCommands.
//          Generated on: 11/24/2021 1:49:32 PM

using System;
using System.Collections.Generic;
using UnityEngine;
using Yarn;
using Yarn.Markup;
using Yarn.Unity;

namespace PixelCrushers.DialogueSystem.Yarn
{
    public class YarnCustomCommands : MonoBehaviour
    {
        // Can't pass an array of values from Lua to C#, only primitives.
        // So arguments to string.Format functions must be individually added to a list
        // by calling AddStringFormatArgument().
        private Dictionary<int, Dictionary<int, List<string>>> _stringFormatArgumentMap = new Dictionary<int, Dictionary<int, List<string>>>();

        // Data members used to implement Yarn's built-in functions
        private Dictionary<string, int> _visitedCount = new Dictionary<string, int>();

        public Dialogue Dialogue { get; private set; } = new Dialogue(new MemoryVariableStore());
        private static bool isRegistered = false;
        private static bool didIRegister = false;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]

        static void Init()
        {
            isRegistered = false;
            didIRegister = false;
        }

        public virtual void OnEnable()
        {
            if (!isRegistered)
            {
                isRegistered = true;
                didIRegister = true;
                RegisterFunctions();
            }
        }

        public virtual void OnDisable()
        {
            if (isRegistered && didIRegister)
            {
                isRegistered = false;
                didIRegister = false;
                UnregisterFunctions();
            }
        }

        public virtual void RegisterFunctions()
        {
            // Debug.Log("YarnCustomCommands::OnEnable()");
            // These calls register your custom commands. You must replace two sections of each register call:
            // <MethodName>: Replace this with your custom command implementation's method call
            // <Parameters>: Replace this with a list of arguments, of the proper type, to complete your method call
            //               It is fine to use the default value for argument types, e.g.:
            //                  BoolValue: false
            //                  FloatValue: 0F
            //                  None: null or 0
            //                  StringValue: string.Empty

            // Yarn built-in functions
            Lua.RegisterFunction(BuiltInFunction.Visited.LuaName(), this, SymbolExtensions.GetMethodInfo(() => Visited(string.Empty)));
            Lua.RegisterFunction(BuiltInFunction.VisitedCount.LuaName(), this, SymbolExtensions.GetMethodInfo(() => VisitedCount(string.Empty)));
            Lua.RegisterFunction(BuiltInFunction.Random.LuaName(), this, SymbolExtensions.GetMethodInfo(() => Random()));
            Lua.RegisterFunction(BuiltInFunction.RandomRange.LuaName(), this, SymbolExtensions.GetMethodInfo(() => RandomRange(0D, 0D)));
            Lua.RegisterFunction(BuiltInFunction.Dice.LuaName(), this, SymbolExtensions.GetMethodInfo(() => Dice(0D)));
            Lua.RegisterFunction(BuiltInFunction.Round.LuaName(), this, SymbolExtensions.GetMethodInfo(() => Round(0D)));
            Lua.RegisterFunction(BuiltInFunction.RoundPlaces.LuaName(), this, SymbolExtensions.GetMethodInfo(() => RoundPlaces(0D, 0D)));
            Lua.RegisterFunction(BuiltInFunction.Floor.LuaName(), this, SymbolExtensions.GetMethodInfo(() => Floor(0D)));
            Lua.RegisterFunction(BuiltInFunction.Ceil.LuaName(), this, SymbolExtensions.GetMethodInfo(() => Ceil(0D)));
            Lua.RegisterFunction(BuiltInFunction.Inc.LuaName(), this, SymbolExtensions.GetMethodInfo(() => Inc(0D)));
            Lua.RegisterFunction(BuiltInFunction.Dec.LuaName(), this, SymbolExtensions.GetMethodInfo(() => Dec(0D)));
            Lua.RegisterFunction(BuiltInFunction.Decimal.LuaName(), this, SymbolExtensions.GetMethodInfo(() => Decimal(0D)));
            Lua.RegisterFunction(BuiltInFunction.Int.LuaName(), this, SymbolExtensions.GetMethodInfo(() => Int(0D)));

            // Yarn's built-in commands
            Lua.RegisterFunction(BuiltInCommand.Stop.LuaName(), this, SymbolExtensions.GetMethodInfo(() => StopConversation()));

            // Dialogue System's built-in commands
            Lua.RegisterFunction(BuiltInCommand.ClearAndAddStringFormatArg.LuaName(), this, SymbolExtensions.GetMethodInfo(() => ClearAndAddStringFormatArgument(0D, 0D, string.Empty)));
            Lua.RegisterFunction(BuiltInCommand.AddStringFormatArg.LuaName(), this, SymbolExtensions.GetMethodInfo(() => AddStringFormatArgument(0D, 0D, string.Empty)));
            Lua.RegisterFunction(BuiltInCommand.StopConversation.LuaName(), this, SymbolExtensions.GetMethodInfo(() => StopConversation()));
        }

        public virtual void UnregisterFunctions()
        {
            // Debug.Log("YarnCustomCommands::OnDisable()");
            // Note: If this script is on your Dialogue Manager & the Dialogue Manager is configured
            // as Don't Destroy On Load (on by default), don't unregister Lua functions.

            // Yarn built-in functions
            Lua.UnregisterFunction(BuiltInFunction.Visited.LuaName());
            Lua.UnregisterFunction(BuiltInFunction.VisitedCount.LuaName());
            Lua.UnregisterFunction(BuiltInFunction.Random.LuaName());
            Lua.UnregisterFunction(BuiltInFunction.RandomRange.LuaName());
            Lua.UnregisterFunction(BuiltInFunction.Dice.LuaName());
            Lua.UnregisterFunction(BuiltInFunction.Round.LuaName());
            Lua.UnregisterFunction(BuiltInFunction.RoundPlaces.LuaName());
            Lua.UnregisterFunction(BuiltInFunction.Floor.LuaName());
            Lua.UnregisterFunction(BuiltInFunction.Ceil.LuaName());
            Lua.UnregisterFunction(BuiltInFunction.Inc.LuaName());
            Lua.UnregisterFunction(BuiltInFunction.Dec.LuaName());
            Lua.UnregisterFunction(BuiltInFunction.Decimal.LuaName());
            Lua.UnregisterFunction(BuiltInFunction.Int.LuaName());

            // Yarn's built-in commands
            Lua.UnregisterFunction(BuiltInCommand.Stop.LuaName());

            // Dialogue System's built-in commands
            Lua.UnregisterFunction(BuiltInCommand.ClearAndAddStringFormatArg.LuaName());
            Lua.UnregisterFunction(BuiltInCommand.AddStringFormatArg.LuaName());
            Lua.UnregisterFunction(BuiltInCommand.StopConversation.LuaName());
        }

        public virtual void OnConversationLine(Subtitle subtitle)
        {
            // Debug.Log($"YarnCustomCommands::OnConversationLine(c_id: {subtitle.dialogueEntry.conversationID}, d_id: {subtitle.dialogueEntry.id})");
            if (!string.IsNullOrEmpty(subtitle.formattedText.text?.Trim()))
            {
                var text = FormatText(
                    subtitle.dialogueEntry.conversationID,
                    subtitle.dialogueEntry.id,
                    subtitle.formattedText.text);

                var markupResult = Dialogue.ParseMarkup(text);
                subtitle.formattedText.text = ParseMarkup(markupResult);
            }
        }

        public virtual void OnConversationResponseMenu(Response[] responses)
        {
            // Debug.Log($"YarnCustomCommands::OnConversationResponseMenu()");
            foreach (Response response in responses)
            {
                if (!string.IsNullOrEmpty(response.formattedText.text?.Trim()))
                {
                    var text = FormatText(
                        response.destinationEntry.conversationID,
                        response.destinationEntry.id,
                        response.formattedText.text);

                    var markupResult = Dialogue.ParseMarkup(text);
                    response.formattedText.text = ParseMarkup(markupResult);
                }
            }
        }

        public virtual void OnConversationEnd(Transform actor)
        {
            // Debug.Log($"YarnCustomCommands::OnConversationEnd() - {DialogueManager.instance.LastConversationStarted}");
            ClearConversationStringArgumentsMap(DialogueManager.instance.lastConversationID);

            // NOTE: Why would this ever be null, I could see if it's OnConversationStart()
            //       Should I remove the null check?
            var conversationName = DialogueManager.instance.LastConversationStarted;
            if (conversationName != null)
            {
                int visitCnt = 0;
                _visitedCount.TryGetValue(conversationName, out visitCnt);
                _visitedCount[conversationName] = visitCnt + 1;
                // Debug.Log($"YarnCustomCommands::OnConversationEnd() - updated visited count to: {_visitedCount[conversationName]}");
            }
        }

        // Override this method to add custom handling for markup attributes associated with a line.
        // This default implementation simply strips all markup removed from the line.
        public virtual string ParseMarkup(MarkupParseResult markupResult) => markupResult.Text;

        // Calls string.Format (if there are any args) on the text, then runs it through Yarn's format function logic.
        private string FormatText(int conversationId, int dialogueEntryId, string text)
        {
            // Debug.Log($"YarnCustomCommands::FormatText({conversationId}, {dialogueEntryId}, \"{text}\")");

            // Grab the string, then format it if necessary.
            // We know this if there are format arguments for the dialogue entry in string arguments map.
            var formattedText = text;

            if (_stringFormatArgumentMap.ContainsKey(conversationId) && _stringFormatArgumentMap[conversationId].ContainsKey(dialogueEntryId))
            {
                var args = _stringFormatArgumentMap[conversationId][dialogueEntryId];
                if (args.Count > 0) formattedText = string.Format(formattedText, args.ToArray());
            }

            return formattedText;
        }

        private void ClearConversationStringArgumentsMap(int conversationId)
        {
            // Debug.Log($"YarnCustomCommands::ClearConversationStringArgumentsMap({conversationId})");
            var cId = DialogueManager.instance.lastConversationID;
            if (_stringFormatArgumentMap.ContainsKey(cId))
            {
                // Probably unnecessary to do before clearing the map,
                // but let's just do it anyway.
                foreach (var argsEntry in _stringFormatArgumentMap[cId]) argsEntry.Value.Clear();
                _stringFormatArgumentMap.Clear();
            }
        }

        // Call this method with the first argument for string.Format. This clears the list before adding anything,
        // making sure that no extra args are present from previous runs. This is extra defensive, as all string arg
        // lists associated with a conversation are cleared on conversation start and end but hey, can't hurt to be careful!
        public void ClearAndAddStringFormatArgument(double conversationId, double dialogueEntryId, string arg)
        {
            // Debug.Log($"YarnCustomCommands::ClearAndAddStringFormatArgument({conversationId}, {dialogueEntryId}, \"{arg}\")");
            var cId = (int)conversationId;
            var dId = (int)dialogueEntryId;

            if (_stringFormatArgumentMap.ContainsKey(cId) && _stringFormatArgumentMap[cId].ContainsKey(dId)) _stringFormatArgumentMap[cId][dId].Clear();
            AddStringFormatArgument(conversationId, dialogueEntryId, arg);
        }

        public void AddStringFormatArgument(double conversationId, double dialogueEntryId, string arg)
        {
            // Debug.Log($"YarnCustomCommands::AddStringFormatArgument({conversationId}, {dialogueEntryId}, \"{arg}\")");
            var cId = (int)conversationId;
            var dId = (int)dialogueEntryId;

            if (!_stringFormatArgumentMap.ContainsKey(cId)) _stringFormatArgumentMap[cId] = new Dictionary<int, List<string>>();
            if (!_stringFormatArgumentMap[cId].ContainsKey(dId)) _stringFormatArgumentMap[cId][dId] = new List<string>();
            _stringFormatArgumentMap[cId][dId].Add(arg);
        }

        public void StopConversation()
        {
            // Debug.Log($"YarnCustomCommands::StopConversation()");
            DialogueManager.instance.StopConversation();
        }

        // Built-in Method Implementations
        // See: https://github.com/YarnSpinnerTool/YarnSpinner-Unity/blob/8a117dc5775a160b33e24a9bd3ba5c964a074c93/Runtime/Commands/DefaultActions.cs
        public bool Visited(string conversationName) => VisitedCount(conversationName) > 0;

        public int VisitedCount(string conversationName)
        {
            // Debug.Log($"YarnCustomCommands::VisitedCount('{conversationName}')");
            int visitCnt = 0;
            _visitedCount.TryGetValue(conversationName, out visitCnt);
            return visitCnt;
        }

        public float Random() => RandomRange(0, 1);
        public float RandomRange(double min, double max) => UnityEngine.Random.Range((float)min, (float)max);
        public int Dice(double sides) => UnityEngine.Random.Range(1, (int)sides + 1);
        public float Round(double value) => RoundPlaces(value, 0);
        public float RoundPlaces(double value, double digits) => (float)Math.Round(value, (int)digits);
        public int Floor(double value) => Mathf.FloorToInt((float)value);
        public int Ceil(double value) => Mathf.CeilToInt((float)value);
        public int Inc(double value) => Decimal(value) != 0 ? Mathf.CeilToInt((float)value) : (int)value + 1;
        public int Dec(double value) => Decimal(value) != 0 ? Mathf.FloorToInt((float)value) : (int)value - 1;
        public float Decimal(double value) => (float)value - Int((float)value);
        public float Int(double value) => (int)Math.Truncate(value);

        // These do nothing, they're just placeholders so that Yarn's editor UI doesn't raise warnings
        // for undefined commands in case they're used in any scripts. Gotta keep Yarn happy, yo.
        [YarnCommand("seq")]
        public void SeqPlaceholder(string sequence) { }
        [YarnCommand("sequence")]
        public void SequencePlaceholder(string sequence) { }

        // These methods do nothing, but are implemented as placeholders
        // in case functionality needs to be added in the future.
        public virtual void OnConversationStart(Transform actor) { }
        public virtual void OnConversationCancelled(Transform actor) { }
        public virtual void OnPrepareConversationLine(DialogueEntry entry) { }
        public virtual void OnConversationLineEnd(Subtitle subtitle) { }
        public virtual void OnConversationLineCancelled(Subtitle subtitle) { }
        public virtual void OnConversationTimeout() { }
        public virtual void OnLinkedConversationStart(Transform actor) { }
        public virtual void OnTextChange(UnityEngine.UI.Text text) { }

        public virtual void OnBarkStart(Transform actor) { }
        public virtual void OnBarkEnd(Transform actor) { }
        public virtual void OnBarkLine(Subtitle subtitle) { }
    }
}

#endif
