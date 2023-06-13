// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// A dialogue database asset. A dialogue database is a collection of data records necessary
    /// to run dialogue, such as actors and conversations. This is a ScriptableObject so it can be 
    /// edited, serialized, and assigned in Unity.
    /// </summary>
    public class DialogueDatabase : ScriptableObject
    {

        /// <summary>
        /// The version of the database, typically only used internally by the developer.
        /// </summary>
        public string version;

        /// <summary>
        /// The author, typically only used internally by the developer.
        /// </summary>
        public string author;

        /// <summary>
        /// The description of the database, typically only used internally by the developer.
        /// </summary>
        public string description;

        /// <summary>
        /// The global Lua user script.
        /// </summary>
        public string globalUserScript;

        /// <summary>
        /// The number of emphasis settings supported by Chat Mapper and the Dialogue System.
        /// </summary>
        public const int NumEmphasisSettings = 4;

        /// <summary>
        /// A Chat Mapper project defines four emphasis settings (text styles) for formatting lines 
        /// of dialogue. This array contains those emphasis settings.
        /// </summary>
        public EmphasisSetting[] emphasisSettings = new EmphasisSetting[NumEmphasisSettings];

        /// <summary>
        /// Assign IDs of assets (actors, items, conversations, etc.) from this base value.
        /// Useful for multiple databases.
        /// </summary>
        public int baseID = 1;

        /// <summary>
        /// The actors in the database.
        /// </summary>
        public List<Actor> actors = new List<Actor>();

        /// <summary>
        /// The items in the database. You can use these to track in-game items, or to track quests using the QuestLog class.
        /// </summary>
        public List<Item> items = new List<Item>();

        /// <summary>
        /// The locations in the database.
        /// </summary>
        public List<Location> locations = new List<Location>();

        /// <summary>
        /// The variables in the database.
        /// </summary>
        public List<Variable> variables = new List<Variable>();

        /// <summary>
        /// The conversations in the database.
        /// </summary>
        public List<Conversation> conversations = new List<Conversation>();

        [System.Serializable]
        public class SyncInfo
        {
            public bool syncActors = false;
            public bool syncItems = false;
            public bool syncLocations = false;
            public bool syncVariables = false;
            public DialogueDatabase syncActorsDatabase = null;
            public DialogueDatabase syncItemsDatabase = null;
            public DialogueDatabase syncLocationsDatabase = null;
            public DialogueDatabase syncVariablesDatabase = null;
        }

        public void ResetEmphasisSettings()
        {
            emphasisSettings[0] = new EmphasisSetting(Color.white, false, false, false);
            emphasisSettings[1] = new EmphasisSetting(Color.red, false, false, false);
            emphasisSettings[2] = new EmphasisSetting(Color.green, false, false, false);
            emphasisSettings[3] = new EmphasisSetting(Color.blue, false, false, false);
        }

        public SyncInfo syncInfo = new SyncInfo();

        /// <summary>
        /// Each database now stores a copy of its template.
        /// </summary>
        public string templateJson = string.Empty;

        // Cache dictionary by asset name to speed up searches:
        private Dictionary<string, Actor> actorNameCache = null;
        private Dictionary<string, Item> itemNameCache = null;
        private Dictionary<string, Location> locationNameCache = null;
        private Dictionary<string, Variable> variableNameCache = null;
        private Dictionary<string, Conversation> conversationTitleCache = null;

        /// <summary>
        /// Gets the ID of the first player character in the actor list.
        /// </summary>
        /// <value>
        /// The player ID.
        /// </value>
        public int playerID
        {
            get
            {
                Actor player = actors.Find(a => a.IsPlayer);
                return (player != null) ? player.id : 0;
            }
        }

        /// <summary>
        /// Determines whether an actor is a player character.
        /// </summary>
        /// <returns>
        /// <c>true</c> if actorID is the ID of a player; otherwise, <c>false</c>.
        /// </returns>
        /// <param name='actorID'>
        /// An actor ID.
        /// </param>
        public bool IsPlayerID(int actorID)
        {
            Actor actor = actors.Find(a => a.id == actorID);
            return (actor != null) ? actor.IsPlayer : false;
        }

        /// <summary>
        /// Determines whether an actor is a player character.
        /// </summary>
        /// <returns><c>true</c> if the named actor is a player; otherwise, <c>false</c>.</returns>
        /// <param name="actorName">Actor name in the database.</param>
        public bool IsPlayer(string actorName)
        {
            Actor actor = GetActor(actorName);
            return (actor != null) && actor.IsPlayer;
        }

        /// <summary>
        /// Gets the type of the character (PC or NPC) of an actor.
        /// </summary>
        /// <returns>
        /// The character type (PC or NPC)
        /// </returns>
        /// <param name='actorID'>
        /// The Actor ID to check.
        /// </param>
        /// <remarks>
        /// The comparison is based on the value of playerID, not the actor's IsPlayer field. If more than one actor's
        /// IsPlayer field is true, this will only identify the first one.
        /// </remarks>
        public CharacterType GetCharacterType(int actorID)
        {
            return IsPlayerID(actorID) ? CharacterType.PC : CharacterType.NPC;
        }

        #region Cache

        private void SetupCaches()
        {
            if (actorNameCache == null) actorNameCache = CreateCache<Actor>(actors);
            if (itemNameCache == null) itemNameCache = CreateCache<Item>(items);
            if (locationNameCache == null) locationNameCache = CreateCache<Location>(locations);
            if (variableNameCache == null) variableNameCache = CreateCache<Variable>(variables);
            if (conversationTitleCache == null) conversationTitleCache = CreateCache<Conversation>(conversations);
        }

        private Dictionary<string, T> CreateCache<T>(List<T> assets) where T : Asset
        {
            var useTitle = typeof(T) == typeof(Conversation);
            var cache = new Dictionary<string, T>();
            if (Application.isPlaying) // Only build cache at runtime so Dialogue Editor doesn't have to worry about updating it.
            {
                for (int i = 0; i < assets.Count; i++)
                {
                    var asset = assets[i];
                    var key = useTitle ? (asset as Conversation).Title : asset.Name;
                    if (!cache.ContainsKey(key)) cache.Add(key, asset);
                }
            }
            return cache;
        }

        /// <summary>
        /// Dialogue databases maintain a quick lookup cache of assets by Name (by Title
        /// for conversations). If you change asset Names/Titles manually, call this
        /// method to sync the cache with the new values.
        /// </summary>
        public void ResetCache()
        {
            actorNameCache = null;
            itemNameCache = null;
            locationNameCache = null;
            variableNameCache = null;
            conversationTitleCache = null;
        }

        #endregion

        /// <summary>
        /// Gets the actor by name.
        /// </summary>
        /// <returns>
        /// The actor.
        /// </returns>
        /// <param name='actorName'>
        /// The actor's name (the value of the Name field).
        /// </param>
        public Actor GetActor(string actorName)
        {
            if (string.IsNullOrEmpty(actorName)) return null;
            SetupCaches();
            return actorNameCache.ContainsKey(actorName) ? actorNameCache[actorName] : actors.Find(a => string.Equals(a.Name, actorName));
        }

        /// <summary>
        /// Gets the actor associated with an actor ID.
        /// </summary>
        /// <returns>
        /// The actor.
        /// </returns>
        /// <param name='id'>
        /// The actor ID.
        /// </param>
        public Actor GetActor(int id)
        {
            return actors.Find(a => a.id == id);
        }

        /// <summary>
        /// Gets the item by name.
        /// </summary>
        /// <returns>The item.</returns>
        /// <param name="itemName">The item's name (the value of the Name field).</param>
        public Item GetItem(string itemName)
        {
            if (string.IsNullOrEmpty(itemName)) return null;
            SetupCaches();
            return itemNameCache.ContainsKey(itemName) ? itemNameCache[itemName] : items.Find(i => string.Equals(i.Name, itemName));
        }

        /// <summary>
        /// Gets the item associated with an item ID.
        /// </summary>
        /// <returns>
        /// The item.
        /// </returns>
        /// <param name='id'>
        /// The item ID.
        /// </param>
        public Item GetItem(int id)
        {
            return items.Find(i => i.id == id);
        }

        /// <summary>
        /// Gets the location by name.
        /// </summary>
        /// <returns>The location.</returns>
        /// <param name="locationName">The location's name (value of the Name field).</param>
        public Location GetLocation(string locationName)
        {
            if (string.IsNullOrEmpty(locationName)) return null;
            SetupCaches();
            return locationNameCache.ContainsKey(locationName) ? locationNameCache[locationName] : locations.Find(l => string.Equals(l.Name, locationName));
        }

        /// <summary>
        /// Gets the location associated with a location ID.
        /// </summary>
        /// <returns>
        /// The location.
        /// </returns>
        /// <param name='id'>
        /// The location ID.
        /// </param>
        public Location GetLocation(int id)
        {
            return locations.Find(l => l.id == id);
        }

        /// <summary>
        /// Gets the variable by name.
        /// </summary>
        /// <returns>The variable.</returns>
        /// <param name="variableName">Variable name.</param>
        public Variable GetVariable(string variableName)
        {
            if (string.IsNullOrEmpty(variableName)) return null;
            SetupCaches();
            return variableNameCache.ContainsKey(variableName) ? variableNameCache[variableName] : variables.Find(v => string.Equals(v.Name, variableName));
        }

        /// <summary>
        /// Gets the variable by ID.
        /// </summary>
        /// <returns>The variable.</returns>
        /// <param name="id">ID.</param>
        public Variable GetVariable(int id)
        {
            return variables.Find(v => v.id == id);
        }

        /// <summary>
        /// Adds a Conversation to the database.
        /// </summary>
        /// <param name='conversation'>
        /// The conversation to add.
        /// </param>
        public void AddConversation(Conversation conversation)
        {
            SetupCaches();
            var title = conversation.Title;
            //--- Removed for speed: if (DialogueDebug.logInfo) Debug.Log("Dialogue System: Add Conversation: " + title);
            if (!conversationTitleCache.ContainsKey(title)) conversationTitleCache.Add(title, conversation);
            conversations.Add(conversation);
            LinkUtility.SortOutgoingLinks(this, conversation);
        }

        /// <summary>
        /// Retrieves a Conversation by its Title field.
        /// </summary>
        /// <returns>
        /// The conversation matching the specified title.
        /// </returns>
        /// <param name='conversationTitle'>
        /// The title of the conversation.
        /// </param>
        public Conversation GetConversation(string conversationTitle)
        {
            if (string.IsNullOrEmpty(conversationTitle)) return null;
            SetupCaches();
            return conversationTitleCache.ContainsKey(conversationTitle) ? conversationTitleCache[conversationTitle] : conversations.Find(c => string.Equals(c.Title, conversationTitle));
        }

        /// <summary>
        /// Retrieves a Conversation by its ID.
        /// </summary>
        /// <returns>
        /// The conversation with the specified ID.
        /// </returns>
        /// <param name='conversationID'>
        /// The Conversation ID.
        /// </param>
        public Conversation GetConversation(int conversationID)
        {
            return conversations.Find(c => c.id == conversationID);
        }

        /// <summary>
        /// Gets a dialogue entry by its conversation and dialogue entry IDs.
        /// </summary>
        /// <returns>The dialogue entry.</returns>
        /// <param name="conversationID">Conversation ID.</param>
        /// <param name="dialogueEntryID">Dialogue entry ID.</param>
        public DialogueEntry GetDialogueEntry(int conversationID, int dialogueEntryID)
        {
            Conversation conversation = GetConversation(conversationID);
            return (conversation != null) ? conversation.GetDialogueEntry(dialogueEntryID) : null;
        }

        /// <summary>
        /// Follows a Link and returns the destination DialogueEntry.
        /// </summary>
        /// <returns>
        /// The dialogue entry that the link points to.
        /// </returns>
        /// <param name='link'>
        /// The link to follow.
        /// </param>
        public DialogueEntry GetDialogueEntry(Link link)
        {
            if (link != null)
            {
                Conversation conversation = GetConversation(link.destinationConversationID);
                if ((conversation != null) && (conversation.dialogueEntries != null))
                {
                    return conversation.dialogueEntries.Find(e => e.id == link.destinationDialogueID);
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the text of the dialogue entry that a link points to.
        /// </summary>
        /// <returns>
        /// The text of the link's destination dialogue entry, or <c>null</c> if the link is invalid.
        /// </returns>
        /// <param name='link'>
        /// The link to follow.
        /// </param>
        public string GetLinkText(Link link)
        {
            DialogueEntry entry = GetDialogueEntry(link);
            return (entry == null) ? string.Empty : entry.responseButtonText;
        }

        /// <summary>
        /// Add the contents of another database to this database.
        /// </summary>
        /// <param name='database'>
        /// The database to copy.
        /// </param>
        public void Add(DialogueDatabase database)
        {
            if (database != null)
            {
                AddEmphasisSettings(database.emphasisSettings);
                AddGlobalUserScript(database);

                // Only add assets that aren't already in the database (as determined by name/ID as appropriate for the asset type):
                //--- Optimized to use cache:
                //foreach (var actor in database.actors)
                //{
                //    if (!Contains(this, actor)) actors.Add(actor);
                //}
                //foreach (var item in database.items)
                //{
                //    if (!Contains(this, item)) items.Add(item);
                //}
                //foreach (var location in database.locations)
                //{
                //    if (!Contains(this, location)) locations.Add(location);
                //}
                //foreach (var variable in database.variables)
                //{
                //    if (!Contains(this, variable)) variables.Add(variable);
                //}
                //foreach (var conversation in database.conversations)
                //{
                //    if (!Contains(this, conversation)) conversations.Add(conversation);
                //}
                SetupCaches();
                AddAssets<Actor>(actors, database.actors, actorNameCache);
                AddAssets<Item>(items, database.items, itemNameCache);
                AddAssets<Location>(locations, database.locations, locationNameCache);
                AddAssets<Variable>(variables, database.variables, variableNameCache);
                AddAssets<Conversation>(conversations, database.conversations, conversationTitleCache);
            }
        }

        private void AddAssets<T>(List<T> myAssets, List<T> assetsToAdd, Dictionary<string, T> cache) where T : Asset
        {
            var useTitle = typeof(T) == typeof(Conversation);
            for (int i = 0; i < assetsToAdd.Count; i++)
            {
                var asset = assetsToAdd[i];
                var key = useTitle ? (asset as Conversation).Title : asset.Name;
                if (!cache.ContainsKey(key))
                {
                    cache.Add(key, asset);
                    myAssets.Add(asset);
                    //--- Removed for speed: if (DialogueDebug.logInfo) Debug.Log("Dialogue System: Add " + typeof(T).Name + ": " + key);
                }
            }
        }

        private void AddEmphasisSettings(EmphasisSetting[] newEmphasisSettings)
        {
            if ((emphasisSettings == null) || (emphasisSettings.Length < NumEmphasisSettings))
            {
                emphasisSettings = newEmphasisSettings;
            }
            else if (newEmphasisSettings != null)
            {
                emphasisSettings = new EmphasisSetting[newEmphasisSettings.Length];
                for (int i = 0; i < newEmphasisSettings.Length; i++)
                {
                    if ((emphasisSettings[i] == null) || (emphasisSettings[i].IsEmpty))
                    {
                        emphasisSettings[i] = newEmphasisSettings[i];
                    }
                }
            }
        }

        private void AddGlobalUserScript(DialogueDatabase database)
        {
            if (!string.IsNullOrEmpty(globalUserScript))
            {
                if (!string.IsNullOrEmpty(database.globalUserScript))
                {
                    globalUserScript = string.Format("{0}; {1}", new System.Object[] { globalUserScript, database.globalUserScript });
                }
            }
            else if (!string.IsNullOrEmpty(database.globalUserScript))
            {
                globalUserScript = database.globalUserScript;
            }
        }

        /// <summary>
        /// Removes from this database all content that's in the specified database.
        /// </summary>
        /// <param name='database'>
        /// The database containing assets to remove from this database.
        /// </param>
        public void Remove(DialogueDatabase database)
        {
            if (database != null)
            {
                //--- Optimized to use cache:
                //actors.RemoveAll(x => database.actors.Contains(x));
                //items.RemoveAll(x => database.items.Contains(x));
                //locations.RemoveAll(x => database.locations.Contains(x));
                //variables.RemoveAll(x => database.variables.Contains(x));
                //conversations.RemoveAll(x => database.conversations.Contains(x));

                SetupCaches();
                RemoveAssets<Actor>(actors, database.actors, actorNameCache);
                RemoveAssets<Item>(items, database.items, itemNameCache);
                RemoveAssets<Location>(locations, database.locations, locationNameCache);
                RemoveAssets<Variable>(variables, database.variables, variableNameCache);
                RemoveAssets<Conversation>(conversations, database.conversations, conversationTitleCache);
            }
        }

        /// <summary>
        /// Removes from this database all content that's in the specified database.
        /// </summary>
        /// <param name='database'>
        /// The database containing assets to remove from this database.
        /// </param>
        /// <param name='keep'>
        /// List of databases containing items that should be kept.
        /// </param>
        public void Remove(DialogueDatabase database, List<DialogueDatabase> keep)
        {
            if (database != null)
            {
                //--- Optimized to use cache:
                //actors.RemoveAll(x => (database.actors.Contains(x) && !Contains(keep, x)));
                //items.RemoveAll(x => (database.items.Contains(x) && !Contains(keep, x)));
                //locations.RemoveAll(x => (database.locations.Contains(x) && !Contains(keep, x)));
                //variables.RemoveAll(x => (database.variables.Contains(x) && !Contains(keep, x)));
                //conversations.RemoveAll(x => (database.conversations.Contains(x) && !Contains(keep, x)));

                SetupCaches();
                RemoveAssets<Actor>(actors, database.actors, actorNameCache, keep);
                RemoveAssets<Item>(items, database.items, itemNameCache, keep);
                RemoveAssets<Location>(locations, database.locations, locationNameCache, keep);
                RemoveAssets<Variable>(variables, database.variables, variableNameCache, keep);
                RemoveAssets<Conversation>(conversations, database.conversations, conversationTitleCache, keep);
            }
        }

        private void RemoveAssets<T>(List<T> myAssets, List<T> assetsToRemove, Dictionary<string, T> cache) where T : Asset
        {
            var useTitle = typeof(T) == typeof(Conversation);
            for (int i = 0; i < assetsToRemove.Count; i++)
            {
                var asset = assetsToRemove[i];
                var key = useTitle ? (asset as Conversation).Title : asset.Name;
                if (cache.ContainsKey(key))
                {
                    myAssets.Remove(cache[key]);
                    cache.Remove(key);
                }
            }
        }

        private void RemoveAssets<T>(List<T> myAssets, List<T> assetsToRemove, Dictionary<string, T> cache, List<DialogueDatabase> keep) where T : Asset
        {
            var useTitle = typeof(T) == typeof(Conversation);
            for (int i = 0; i < assetsToRemove.Count; i++)
            {
                var asset = assetsToRemove[i];
                if (Contains(keep, asset)) continue;
                var key = useTitle ? (asset as Conversation).Title : asset.Name;
                if (cache.ContainsKey(key))
                {
                    myAssets.Remove(cache[key]);
                    cache.Remove(key);
                }
            }
        }

        /// <summary>
        /// Removes all assets from this database.
        /// </summary>
        public void Clear()
        {
            actors.Clear();
            items.Clear();
            locations.Clear();
            variables.Clear();
            conversations.Clear();
            ResetCache();
        }

        #region Sync

        /// <summary>
        /// Syncs all assets from their source databases if assigned in syncInfo.
        /// </summary>
        public void SyncAll()
        {
            SyncActors();
            SyncItems();
            SyncLocations();
            SyncVariables();
        }

        /// <summary>
        /// Syncs the actors from syncInfo.syncActorsDatabase.
        /// </summary>
        public void SyncActors()
        {
            ResetCache();
            if (!syncInfo.syncActors || syncInfo.syncActorsDatabase == null || syncInfo.syncActorsDatabase == this) return;

            // Identify actors in sync DB that have been renamed:
            var renamedActors = new Dictionary<string, string>(); // <old, new> Lua indices
            foreach (Actor sourceActor in syncInfo.syncActorsDatabase.actors)
            {
                var myActor = GetActor(sourceActor.id);
                if (myActor != null && myActor.Name != sourceActor.Name)
                {
                    renamedActors.Add(DialogueLua.StringToTableIndex(myActor.Name), DialogueLua.StringToTableIndex(sourceActor.Name));
                }
            }

            // Copy actors from sync DB to this DB, replacing those in this DB that have same ID:
            actors.RemoveAll(x => (syncInfo.syncActorsDatabase.GetActor(x.id) != null));
            for (int i = 0; i < syncInfo.syncActorsDatabase.actors.Count; i++)
            {
                actors.Insert(i, new Actor(syncInfo.syncActorsDatabase.actors[i]));
            }

            // Update any renamed actors in dialogue entry Conditions and Scripts:
            foreach (Conversation conversation in conversations)
            {
                foreach (DialogueEntry entry in conversation.dialogueEntries)
                {
                    foreach (var kvp in renamedActors)
                    {
                        var oldLuaIndex = kvp.Key;
                        var newLuaIndex = kvp.Value;
                        if (entry.conditionsString.Contains(oldLuaIndex))
                        {
                            entry.conditionsString = ReplaceLuaIndex(entry.conditionsString, "Actor", oldLuaIndex, newLuaIndex);
                        }
                        if (entry.userScript.Contains(oldLuaIndex))
                        {
                            entry.userScript = ReplaceLuaIndex(entry.userScript, "Actor", oldLuaIndex, newLuaIndex);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Syncs the items from syncInfo.syncItemsDatabase.
        /// </summary>
        public void SyncItems()
        {
            ResetCache();
            if (!syncInfo.syncItems || syncInfo.syncItemsDatabase == null || syncInfo.syncItemsDatabase == this) return;

            // Identify items in sync DB that have been renamed:
            var renamedItems = new Dictionary<string, string>(); // <old, new> Lua indices
            foreach (Item sourceItem in syncInfo.syncItemsDatabase.items)
            {
                var myItem = GetItem(sourceItem.id);
                if (myItem != null && myItem.Name != sourceItem.Name)
                {
                    renamedItems.Add(DialogueLua.StringToTableIndex(myItem.Name), DialogueLua.StringToTableIndex(sourceItem.Name));
                }
            }

            // Copy items from sync DB to this DB, replacing those in this DB that have same ID:
            items.RemoveAll(x => (syncInfo.syncItemsDatabase.GetItem(x.id) != null));
            for (int i = 0; i < syncInfo.syncItemsDatabase.items.Count; i++)
            {
                items.Insert(i, new Item(syncInfo.syncItemsDatabase.items[i]));
            }

            // Update any renamed items in dialogue entry Conditions and Scripts:
            foreach (Conversation conversation in conversations)
            {
                foreach (DialogueEntry entry in conversation.dialogueEntries)
                {
                    foreach (var kvp in renamedItems)
                    {
                        var oldLuaIndex = kvp.Key;
                        var newLuaIndex = kvp.Value;
                        if (entry.conditionsString.Contains(oldLuaIndex))
                        {
                            entry.conditionsString = ReplaceLuaIndex(entry.conditionsString, "Item", oldLuaIndex, newLuaIndex);
                        }
                        if (entry.userScript.Contains(oldLuaIndex))
                        {
                            entry.userScript = ReplaceLuaIndex(entry.userScript, "Item", oldLuaIndex, newLuaIndex);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Syncs the locations from syncInfo.syncLocationsDatabase.
        /// </summary>
        public void SyncLocations()
        {
            ResetCache();
            if (!syncInfo.syncLocations || syncInfo.syncLocationsDatabase == null || syncInfo.syncLocationsDatabase == this) return;

            // Identify locations in sync DB that have been renamed:
            var renamedLocations = new Dictionary<string, string>(); // <old, new> Lua indices
            foreach (Location sourceLocation in syncInfo.syncLocationsDatabase.locations)
            {
                var myLocation = GetLocation(sourceLocation.id);
                if (myLocation != null && myLocation.Name != sourceLocation.Name)
                {
                    renamedLocations.Add(DialogueLua.StringToTableIndex(myLocation.Name), DialogueLua.StringToTableIndex(sourceLocation.Name));
                }
            }

            // Copy locations from sync DB to this DB, replacing those in this DB that have same ID:
            locations.RemoveAll(x => (syncInfo.syncLocationsDatabase.GetLocation(x.id) != null));
            for (int i = 0; i < syncInfo.syncLocationsDatabase.locations.Count; i++)
            {
                locations.Insert(i, new Location(syncInfo.syncLocationsDatabase.locations[i]));
            }

            // Update any renamed locations in dialogue entry Conditions and Scripts:
            foreach (Conversation conversation in conversations)
            {
                foreach (DialogueEntry entry in conversation.dialogueEntries)
                {
                    foreach (var kvp in renamedLocations)
                    {
                        var oldLuaIndex = kvp.Key;
                        var newLuaIndex = kvp.Value;
                        if (entry.conditionsString.Contains(oldLuaIndex))
                        {
                            entry.conditionsString = ReplaceLuaIndex(entry.conditionsString, "Location", oldLuaIndex, newLuaIndex);
                        }
                        if (entry.userScript.Contains(oldLuaIndex))
                        {
                            entry.userScript = ReplaceLuaIndex(entry.userScript, "Location", oldLuaIndex, newLuaIndex);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Syncs the variables from syncInfo.syncVariablesDatabase.
        /// </summary>
        public void SyncVariables()
        {
            ResetCache();
            if (!syncInfo.syncVariables || syncInfo.syncVariablesDatabase == null || syncInfo.syncVariablesDatabase == this) return;

            // Identify variables in sync DB that have been renamed:
            var renamedVariables = new Dictionary<string, string>(); // <old, new> Lua indices
            foreach (Variable sourceVariable in syncInfo.syncVariablesDatabase.variables)
            {
                var myVariable = GetVariable(sourceVariable.id);
                if (myVariable != null && myVariable.Name != sourceVariable.Name)
                {
                    renamedVariables.Add(DialogueLua.StringToTableIndex(myVariable.Name), DialogueLua.StringToTableIndex(sourceVariable.Name));
                }
            }

            // Copy variables from sync DB to this DB, replacing those in this DB that have same ID:
            variables.RemoveAll(x => (syncInfo.syncVariablesDatabase.GetVariable(x.id) != null));
            for (int i = 0; i < syncInfo.syncVariablesDatabase.variables.Count; i++)
            {
                variables.Insert(i, new Variable(syncInfo.syncVariablesDatabase.variables[i]));
            }

            // Update any renamed variables in dialogue entry Conditions and Scripts:
            foreach (Conversation conversation in conversations)
            {
                foreach (DialogueEntry entry in conversation.dialogueEntries)
                {
                    foreach (var kvp in renamedVariables)
                    {
                        var oldLuaIndex = kvp.Key;
                        var newLuaIndex = kvp.Value;
                        if (entry.conditionsString.Contains(oldLuaIndex))
                        {
                            entry.conditionsString = ReplaceLuaIndex(entry.conditionsString, "Variable", oldLuaIndex, newLuaIndex);
                        }
                        if (entry.userScript.Contains(oldLuaIndex))
                        {
                            entry.userScript = ReplaceLuaIndex(entry.userScript, "Variable", oldLuaIndex, newLuaIndex);
                        }
                    }
                }
            }
        }

        private string ReplaceLuaIndex(string luaCode, string tableName, string oldLuaIndex, string newLuaIndex)
        {
            return luaCode.Replace(tableName + "[\"" + oldLuaIndex + "\"]", tableName + "[\"" + newLuaIndex + "\"]").
                    Replace(tableName + "['" + oldLuaIndex + "']", tableName + "['" + newLuaIndex + "']");
        }

        #endregion

        /// <summary>
        /// Checks if a list of assets contains an asset with a specified name.
        /// </summary>
        /// <returns>
        /// <c>true</c> if an asset matches the specified name.
        /// </returns>
        /// <param name='assetList'>
        /// The list of assets to search.
        /// </param>
        /// <param name='assetName'>
        /// The name to search for.
        /// </param>
        /// <typeparam name='T'>
        /// The type of asset.
        /// </typeparam>
        public static bool ContainsName<T>(List<T> assetList, string assetName) where T : Asset
        {
            return (assetList != null) && (assetList.Find(x => string.Equals(x.Name, assetName)) != null);
        }

        /// <summary>
        /// Checks if a list of assets contains an asset with a specified ID.
        /// </summary>
        /// <returns>
        /// <c>true</c> if an asset matches the specified ID.
        /// </returns>
        /// <param name='assetList'>
        /// The list of assets to search.
        /// </param>
        /// <param name='assetID'>
        /// The ID to search for.
        /// </param>
        /// <typeparam name='T'>
        /// The type of asset.
        /// </typeparam>
        public static bool ContainsID<T>(List<T> assetList, int assetID) where T : Asset
        {
            return (assetList != null) && (assetList.Find(x => (x.id == assetID)) != null);
        }

        /// <summary>
        /// Checks is a list of conversations contains a conversation with a specified title.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the title is found.
        /// </returns>
        /// <param name='conversations'>
        /// The conversations to search.
        /// </param>
        /// <param name='title'>
        /// The title to search for.
        /// </param>
        public static bool ContainsTitle(List<Conversation> conversations, string title)
        {
            return (conversations != null) && (conversations.Find(x => string.Equals(x.Title, title)) != null);
        }

        /// <summary>
        /// Checks if a database contains an asset. Depending on the asset's type, it will check by
        /// name or title. This might not be an exact match -- for example, two conversations could 
        /// have the same title, in which case either one would return true.
        /// </summary>
        /// <param name='database'>
        /// The database to search.
        /// </param>
        /// <param name='asset'>
        /// The asset to search for.
        /// </param>
        public static bool Contains(DialogueDatabase database, Asset asset)
        {
            if (asset == null)
            {
                return false;
            }
            //--- Optimized to use cache:
            //else if (asset is Actor)
            //{
            //    return ContainsName<Actor>(database.actors, asset.Name);
            //}
            //else if (asset is Item)
            //{
            //    return ContainsName<Item>(database.items, asset.Name);
            //}
            //else if (asset is Location)
            //{
            //    return ContainsName<Location>(database.locations, asset.Name);
            //}
            //else if (asset is Variable)
            //{
            //    return ContainsName<Variable>(database.variables, asset.Name);
            //}
            //else if (asset is Conversation)
            //{
            //    return ContainsTitle(database.conversations, (asset as Conversation).Title);
            //}
            database.SetupCaches();
            if (asset is Actor)
            {
                return database.actorNameCache.ContainsKey(asset.Name);
            }
            else if (asset is Item)
            {
                return database.itemNameCache.ContainsKey(asset.Name);
            }
            else if (asset is Location)
            {
                return database.locationNameCache.ContainsKey(asset.Name);
            }
            else if (asset is Variable)
            {
                return database.variableNameCache.ContainsKey(asset.Name);            
            }
            else if (asset is Conversation)
            {
                return database.conversationTitleCache.ContainsKey((asset as Conversation).Title);
            }
            else
            {
                Debug.LogError(string.Format("{0}: Unexpected asset type {1}", new System.Object[] { DialogueDebug.Prefix, asset.GetType().Name }));
                return false;
            }
        }

        /// <summary>
        /// Checks if a list of databases contains an asset.
        /// </summary>
        /// <param name='databaseList'>
        /// The list of databases to check.
        /// </param>
        /// <param name='asset'>
        /// The asset to search for.
        /// </param>
        public static bool Contains(List<DialogueDatabase> databaseList, Asset asset)
        {
            foreach (DialogueDatabase database in databaseList)
            {
                if (Contains(database, asset)) return true;
            }
            return false;
        }

        public const string InvalidEntrytag = "invalid_entrytag";
        public const string VoiceOverFileFieldName = DialogueSystemFields.VoiceOverFile;

        public delegate string GetCustomEntrytagDelegate(Conversation conversation, DialogueEntry entry);
        public static GetCustomEntrytagDelegate getCustomEntrytag = null;


        //--- Was manually specifying invalid filename chars: private static Regex entrytagRegex = new Regex("[;:,!\'\"\t\r\n\\/\\?\\[\\] ]");
        private static Regex entrytagRegex = new Regex(string.Format("[{0}]", Regex.Escape(" " + new string(System.IO.Path.GetInvalidFileNameChars()) + new string(System.IO.Path.GetInvalidPathChars()))));
        private static Regex actorNameLineNumberEntrytagRegex = new Regex("[,\"\t\r\n\\/<>?]");


        /// <summary>
        /// Gets the entrytag string of a dialogue entry.
        /// </summary>
        /// <param name="conversation">Dialogue entry's conversation.</param>
        /// <param name="entry">Dialogue entry.</param>
        /// <param name="entrytagFormat">Entrytag format.</param>
        /// <returns>Entrytag.</returns>
        public string GetEntrytag(Conversation conversation, DialogueEntry entry, EntrytagFormat entrytagFormat)
        {
            if (conversation == null || entry == null) return InvalidEntrytag;
            Actor actor = null;
            switch (entrytagFormat)
            {
                case EntrytagFormat.ActorName_ConversationID_EntryID:
                    actor = GetActor(entry.ActorID);
                    if (actor == null) return InvalidEntrytag;
                    return string.Format("{0}_{1}_{2}", entrytagRegex.Replace(actor.Name, "_"), conversation.id, entry.id);
                case EntrytagFormat.ConversationTitle_EntryID:
                    return string.Format("{0}_{1}", entrytagRegex.Replace(conversation.Title, "_"), entry.id);
                case EntrytagFormat.ActorNameLineNumber:
                    actor = GetActor(entry.ActorID);
                    if (actor == null) return InvalidEntrytag;
                    int lineNumber = (conversation.id * 500) + entry.id;
                    return string.Format("{0}{1}", actorNameLineNumberEntrytagRegex.Replace(actor.Name, "_"), lineNumber);
                case EntrytagFormat.ConversationID_ActorName_EntryID:
                    actor = GetActor(entry.ActorID);
                    if (actor == null) return InvalidEntrytag;
                    return string.Format("{0}_{1}_{2}", conversation.id, entrytagRegex.Replace(actor.Name, "_"), entry.id);
                case EntrytagFormat.ActorName_ConversationTitle_EntryDescriptor:
                    actor = GetActor(entry.ActorID);
                    if (actor == null) { return InvalidEntrytag; }
                    var entryDesc = !string.IsNullOrEmpty(entry.Title) ? entry.Title
                        : (!string.IsNullOrEmpty(entry.currentMenuText) ? entry.currentMenuText : entry.id.ToString());
                    return string.Format("{0}_{1}_{2}", entrytagRegex.Replace(actor.Name, "_"), entrytagRegex.Replace(conversation.Title, "_"), entrytagRegex.Replace(entryDesc, "_"));
                case EntrytagFormat.VoiceOverFile:
                    if (entry == null) return InvalidEntrytag;
                    var voiceOverFile = Field.LookupValue(entry.fields, VoiceOverFileFieldName);
                    return (voiceOverFile == null) ? InvalidEntrytag : entrytagRegex.Replace(voiceOverFile, "_");
                case EntrytagFormat.Title:
                    if (entry == null) return InvalidEntrytag;
                    var title = Field.LookupValue(entry.fields, DialogueSystemFields.Title);
                    return (title == null) ? InvalidEntrytag : entrytagRegex.Replace(title, "_");
                case EntrytagFormat.Custom:
                    return (getCustomEntrytag != null) ? getCustomEntrytag(conversation, entry) : InvalidEntrytag;
                default:
                    return InvalidEntrytag;
            }
        }

        /// <summary>
        /// Gets the entrytag string of a dialogue entry.
        /// </summary>
        /// <param name="conversationID">Dialogue entry's conversation.</param>
        /// <param name="dialogueEntryID">Dialogue entry.</param>
        /// <param name="entrytagFormat">Entrytag format.</param>
        /// <returns>Entrytag.</returns>
        public string GetEntrytag(int conversationID, int dialogueEntryID, EntrytagFormat entrytagFormat)
        {
            var conversation = GetConversation(conversationID);
            var entry = (conversation != null) ? conversation.GetDialogueEntry(dialogueEntryID) : null;
            return GetEntrytag(conversation, entry, entrytagFormat);
        }

        /// <summary>
        /// Gets the entrytaglocal string (localized version) of a dialogue entry.
        /// </summary>
        /// <param name="conversation">Dialogue entry's conversation.</param>
        /// <param name="entry">Dialogue entry.</param>
        /// <param name="entrytagFormat">Entrytag format.</param>
        /// <returns>Localized entrytag.</returns>
        public string GetEntrytaglocal(Conversation conversation, DialogueEntry entry, EntrytagFormat entrytagFormat)
        {
            return GetEntrytag(conversation, entry, entrytagFormat) + "_" + Localization.language;
        }

        /// <summary>
        /// Gets the entrytaglocal string (localized version) of a dialogue entry.
        /// </summary>
        /// <param name="conversationID">Dialogue entry's conversation.</param>
        /// <param name="dialogueEntryID">Dialogue entry.</param>
        /// <param name="entrytagFormat">Entrytag format.</param>
        /// <returns>Localized entrytag.</returns>
        public string GetEntrytaglocal(int conversationID, int dialogueEntryID, EntrytagFormat entrytagFormat)
        {
            var conversation = GetConversation(conversationID);
            var entry = (conversation != null) ? conversation.GetDialogueEntry(dialogueEntryID) : null;
            return GetEntrytaglocal(conversation, entry, entrytagFormat);
        }
    }

}
