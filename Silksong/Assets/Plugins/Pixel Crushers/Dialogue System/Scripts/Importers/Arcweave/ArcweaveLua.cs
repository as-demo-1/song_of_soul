#if USE_ARCWEAVE

using System;
using System.Collections.Generic;
using UnityEngine;
using Language.Lua;

namespace PixelCrushers.DialogueSystem.ArcweaveSupport
{

    /// <summary>
    /// Implements Arcscript built-in functions for the Dialogue System's Lua environment.
    /// Notes:
    /// - Uses LuaInterpreter.
    /// - Does not implement reset() or resetAll().
    /// </summary>
    [AddComponentMenu("")] // Use wrapper.
    public class ArcweaveLua : Saver
    {
        [Tooltip("Typically leave unticked so temporary Dialogue Managers don't unregister your functions.")]
        public bool unregisterOnDisable = false;

        [Tooltip("Support multiplayer Lua functions. Currently: visits() incorporates Variable['ActorIndex'].")]
        public bool multiplayer = false;

        #region Unity Entrypoints

        public override void OnEnable()
        {
            base.OnEnable();
            Lua.RegisterFunction(nameof(abs), null, SymbolExtensions.GetMethodInfo(() => abs(0)));
            Lua.RegisterFunction(nameof(sqr), null, SymbolExtensions.GetMethodInfo(() => sqr(0)));
            Lua.RegisterFunction(nameof(sqrt), null, SymbolExtensions.GetMethodInfo(() => sqrt(0)));
            Lua.RegisterFunction(nameof(random), null, SymbolExtensions.GetMethodInfo(() => random()));
            Lua.RegisterFunction(nameof(visits), this, SymbolExtensions.GetMethodInfo(() => visits(string.Empty)));
            Lua.environment.Register(nameof(roll), roll);
            Lua.environment.Register(nameof(show), show);
        }

        public override void OnDisable()
        {
            base.OnDisable();
            if (unregisterOnDisable)
            {
                Lua.UnregisterFunction(nameof(abs));
                Lua.UnregisterFunction(nameof(sqr));
                Lua.UnregisterFunction(nameof(sqrt));
                Lua.UnregisterFunction(nameof(random));
                Lua.UnregisterFunction(nameof(roll));
                Lua.UnregisterFunction(nameof(show));
            }
        }

        #endregion

        #region Arcweave Functions

        public static double abs(double n)
        {
            return Mathf.Abs((float)n);
        }

        public static double sqr(double n)
        {
            return (n * n);
        }

        public static double sqrt(double n)
        {
            return Mathf.Sqrt((float)n);
        }

        public static double random()
        {
            return UnityEngine.Random.value; // Note: Returns 1 inclusive, but Arcscript random() is 1 exclusive.
        }

        public static LuaValue roll(LuaValue[] values)
        {
            int m = (int)(values[0] as LuaNumber).Number;
            int n = (values.Length > 1 && values[1] is LuaNumber) ? (int)(values[1] as LuaNumber).Number : 1;
            double result = 0;
            for (int i = 0; i < (int)n; i++)
            {
                result += UnityEngine.Random.Range(1, (int)m + 1);
            }
            return new LuaNumber(result);
        }

        public static LuaValue show(LuaValue[] values)
        {
            var s = string.Empty;
            foreach (var value in values)
            {
                s += value.ToString();
            }
            return new LuaString(s);
        }

        #endregion

        #region visits()

        [Serializable]
        public class SaveData
        {
            public List<string> guids = new List<string>();
            public List<int> visits = new List<int>();
            public string lastTextGuid;

            public SaveData() { }

            public SaveData(Dictionary<string, int> visitsDict, string lastTextGuid)
            {
                foreach (var kvp in visitsDict)
                {
                    guids.Add(kvp.Key);
                    visits.Add(kvp.Value);
                }
                this.lastTextGuid = lastTextGuid;
            }
        }

        [Serializable]
        public class MultiplayerSaveData
        {
            public List<string> actorIndices = new List<string>();
            public List<SaveData> saveData = new List<SaveData>();
        }

        protected Dictionary<string, Dictionary<string, int>> visitsDicts = new Dictionary<string, Dictionary<string, int>>();
        protected Dictionary<string, string> lastTextGuids = new Dictionary<string, string>();

        protected virtual string GetActorIndex()
        {
            return multiplayer ? DialogueLua.GetVariable("ActorIndex").asString : "Player"; 
        }

        protected virtual string GetLastTextGuid(string actorIndex)
        {
            if (!lastTextGuids.ContainsKey(actorIndex))
            {
                lastTextGuids.Add(actorIndex, string.Empty);
            }
            return lastTextGuids[actorIndex];
        }

        protected virtual Dictionary<string, int> GetVisitsDict(string actorIndex)
        {
            if (!visitsDicts.ContainsKey(actorIndex))
            {
                visitsDicts.Add(actorIndex, new Dictionary<string, int>());
            }
            return visitsDicts[actorIndex];
        }

        protected virtual void OnConversationLine(Subtitle subtitle)
        {
            if (string.IsNullOrEmpty(subtitle.formattedText.text)) return;
            var lastTextGuid = Field.LookupValue(subtitle.dialogueEntry.fields, "Guid");
            if (string.IsNullOrEmpty(lastTextGuid)) return;
            var actorIndex = GetActorIndex();
            lastTextGuids[actorIndex] = lastTextGuid;
            var visitsDict = GetVisitsDict(actorIndex);
            if (visitsDict.ContainsKey(lastTextGuid))
            {
                visitsDict[lastTextGuid]++;
            }
            else
            {
                visitsDict.Add(lastTextGuid, 1);
            }
        }

        public double visits(string id)
        {
            int count;
            var actorIndex = GetActorIndex();
            var guid = !string.IsNullOrEmpty(id) ? id : GetLastTextGuid(actorIndex);
            var visitsDict = GetVisitsDict(actorIndex);
            return visitsDict.TryGetValue(guid, out count) ? count : 0;
        }

        public override string RecordData()
        {
            if (!multiplayer)
            {
                var actorIndex = GetActorIndex();
                var data = new SaveData(GetVisitsDict(actorIndex), GetLastTextGuid(actorIndex));
                return SaveSystem.Serialize(data);
            }
            else
            {
                var multiplayerData = new MultiplayerSaveData();
                foreach (var kvp in visitsDicts)
                {
                    var actorIndex = kvp.Key;
                    var visitsDict = kvp.Value;
                    var data = new SaveData(visitsDict, GetLastTextGuid(actorIndex));
                    multiplayerData.actorIndices.Add(actorIndex);
                    multiplayerData.saveData.Add(data);
                }
                return SaveSystem.Serialize(multiplayerData);
            }
        }

        public override void ApplyData(string s)
        {
            if (string.IsNullOrEmpty(s)) return;
            if (!multiplayer)
            {
                var data = SaveSystem.Deserialize<SaveData>(s);
                if (data == null) return;
                var actorIndex = GetActorIndex();
                var visitsDict = GetVisitsDict(actorIndex);
                visitsDict.Clear();
                for (int i = 0; i < Mathf.Min(data.guids.Count, data.visits.Count); i++)
                {
                    visitsDict.Add(data.guids[i], data.visits[i]);
                }
                lastTextGuids[actorIndex] = data.lastTextGuid;
            }
            else
            {
                var multiplayerData = SaveSystem.Deserialize<MultiplayerSaveData>(s);
                if (multiplayerData == null) return;
                for (int i = 0; i < Mathf.Min(multiplayerData.actorIndices.Count, multiplayerData.saveData.Count); i++)
                {
                    var actorIndex = multiplayerData.actorIndices[i];
                    var data = multiplayerData.saveData[i];
                    var visitsDict = GetVisitsDict(actorIndex);
                    visitsDict.Clear();
                    for (int j = 0; j < Mathf.Min(data.guids.Count, data.visits.Count); j++)
                    {
                        visitsDict.Add(data.guids[j], data.visits[j]);
                    }
                    lastTextGuids[actorIndex] = data.lastTextGuid;
                }
            }
        }

        #endregion

    }
}

#endif
