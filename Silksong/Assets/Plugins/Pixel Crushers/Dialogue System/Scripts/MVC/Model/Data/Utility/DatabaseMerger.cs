// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// This static utility class merges the contents of a dialogue database into another
    /// dialogue database.
    /// </summary>
    public static class DatabaseMerger
    {

        public enum ConflictingIDRule
        {
            /// <summary>
            /// If the same ID exists in both databases, replace the original one with the new one.
            /// </summary>
            ReplaceConflictingIDs,

            /// <summary>
            /// Add assets even if IDs conflict.
            /// </summary>
            AllowConflictingIDs,

            /// <summary>
            /// Assign new IDs to assets from source database.
            /// </summary>
            AssignUniqueIDs
        };

        private class NewIDs
        {
            public bool destinationHasPlayerActor = false;
            public bool destinationHasAlertVariable = false;
            public Dictionary<int, int> actor = new Dictionary<int, int>();
            public Dictionary<int, int> item = new Dictionary<int, int>();
            public Dictionary<int, int> location = new Dictionary<int, int>();
            public Dictionary<int, int> variable = new Dictionary<int, int>();
            public Dictionary<int, int> conversation = new Dictionary<int, int>();
        }

        public static void Merge(DialogueDatabase destination, DialogueDatabase source, ConflictingIDRule conflictingIDRule,
                                 bool mergeProperties, bool mergeActors, bool mergeItems, bool mergeLocations,
                                 bool mergeVariables, bool mergeConversations)
        {
            Merge(destination, source, conflictingIDRule, mergeProperties, true, mergeActors, mergeItems, mergeLocations, mergeVariables, mergeConversations);
        }


        /// <summary>
        /// Merges a source database into a destination database. Note that if the destination database
        /// has an actor marked IsPlayer, then the source database will use this actor instead of
        /// any IsPlayer actors in the source database. Similarly, only one Alert variable will be added.
        /// This variation allows selective merge of only certain types of assets.
        /// </summary>
        /// <param name="destination">Destination.</param>
        /// <param name="source">Source.</param>
        /// <param name="conflictingIDRule">Specifies how to handle conflicting IDs.</param>
        public static void Merge(DialogueDatabase destination, DialogueDatabase source, ConflictingIDRule conflictingIDRule,
                             bool mergeProperties, bool mergeEmphases, bool mergeActors, bool mergeItems, bool mergeLocations,
                             bool mergeVariables, bool mergeConversations)
        {
            if ((destination != null) && (source != null))
            {
                switch (conflictingIDRule)
                {
                    case ConflictingIDRule.ReplaceConflictingIDs:
                        MergeReplaceConflictingIDs(destination, source, mergeProperties, mergeEmphases, mergeActors, mergeItems, mergeLocations, mergeVariables, mergeConversations);
                        break;
                    case ConflictingIDRule.AllowConflictingIDs:
                        MergeAllowConflictingIDs(destination, source, mergeProperties, mergeEmphases, mergeActors, mergeItems, mergeLocations, mergeVariables, mergeConversations);
                        break;
                    case ConflictingIDRule.AssignUniqueIDs:
                        MergeAssignUniqueIDs(destination, source, mergeProperties, mergeEmphases, mergeActors, mergeItems, mergeLocations, mergeVariables, mergeConversations);
                        break;
                    default:
                        Debug.LogError(string.Format("{0}: Internal error. Unsupported merge type: {1}", new System.Object[] { DialogueDebug.Prefix, conflictingIDRule }));
                        break;
                }
            }
        }

        /// <summary>
        /// Merges a source database into a destination database. Note that if the destination database
        /// has an actor marked IsPlayer, then the source database will use this actor instead of
        /// any IsPlayer actors in the source database. Similarly, only one Alert variable will be added.
        /// </summary>
        /// <param name="destination">Destination.</param>
        /// <param name="source">Source.</param>
        /// <param name="conflictingIDRule">Specifies how to handle conflicting IDs.</param>
        public static void Merge(DialogueDatabase destination, DialogueDatabase source, ConflictingIDRule conflictingIDRule)
        {
            Merge(destination, source, conflictingIDRule, true, true, true, true, true, true);
        }

        /// <summary>
        /// Merges the database properties, assigning any values that are blank in the destination.
        /// </summary>
        /// <param name="destination">Destination.</param>
        /// <param name="source">Source.</param>
        private static void MergeDatabaseProperties(DialogueDatabase destination, DialogueDatabase source, bool mergeEmphases)
        {
            if (string.IsNullOrEmpty(destination.author)) destination.author = source.author;
            if (string.IsNullOrEmpty(destination.version)) destination.version = source.version;
            if (string.IsNullOrEmpty(destination.description)) destination.description = source.description;
            if (!string.IsNullOrEmpty(source.globalUserScript))
            {
                if (string.IsNullOrEmpty(destination.globalUserScript))
                {
                    destination.globalUserScript = source.description;
                }
                else
                {
                    destination.globalUserScript = string.Format("{0}; {1}", new System.Object[] { destination.globalUserScript, source.globalUserScript });
                }
            }
            if (mergeEmphases)
            {
                if (source.emphasisSettings.Length > destination.emphasisSettings.Length)
                {
                    destination.emphasisSettings = new EmphasisSetting[source.emphasisSettings.Length];
                }
                for (int i = 0; i < source.emphasisSettings.Length; i++)
                {
                    var srcEm = source.emphasisSettings[i];
                    destination.emphasisSettings[i] = new EmphasisSetting(srcEm.color, srcEm.bold, srcEm.italic, srcEm.underline);
                }
            }
        }

        #region Replace Conflicting IDs

        /// <summary>
        /// Merge, replacing the destination's assets with the source's assets when they have the same IDs.
        /// </summary>
        private static void MergeReplaceConflictingIDs(DialogueDatabase destination, DialogueDatabase source,
                                                     bool mergeProperties, bool mergeEmphases, bool mergeActors, bool mergeItems, bool mergeLocations,
                                                     bool mergeVariables, bool mergeConversations)
        {
            if (mergeProperties) MergeDatabaseProperties(destination, source, mergeEmphases);
            if (mergeActors) MergeActorsReplaceConflictingIDs(destination, source);
            if (mergeItems) MergeItemsReplaceConflictingIDs(destination, source);
            if (mergeLocations) MergeLocationsReplaceConflictingIDs(destination, source);
            if (mergeVariables) MergeVariablesReplaceConflictingIDs(destination, source);
            if (mergeConversations) MergeConversationsReplaceConflictingIDs(destination, source);
        }

        private static void MergeActorsReplaceConflictingIDs(DialogueDatabase destination, DialogueDatabase source)
        {
            foreach (Actor actor in source.actors)
            {
                destination.actors.RemoveAll(x => x.id == actor.id);
                destination.actors.Add(actor);
            }
        }

        private static void MergeItemsReplaceConflictingIDs(DialogueDatabase destination, DialogueDatabase source)
        {
            foreach (Item item in source.items)
            {
                destination.items.RemoveAll(x => x.id == item.id);
                destination.items.Add(item);
            }
        }

        private static void MergeLocationsReplaceConflictingIDs(DialogueDatabase destination, DialogueDatabase source)
        {
            foreach (Location location in source.locations)
            {
                destination.locations.RemoveAll(x => x.id == location.id);
                destination.locations.Add(location);
            }
        }

        private static void MergeVariablesReplaceConflictingIDs(DialogueDatabase destination, DialogueDatabase source)
        {
            foreach (Variable variable in source.variables)
            {
                destination.variables.RemoveAll(x => x.id == variable.id);
                destination.variables.Add(variable);
            }
        }

        private static void MergeConversationsReplaceConflictingIDs(DialogueDatabase destination, DialogueDatabase source)
        {
            foreach (Conversation conversation in source.conversations)
            {
                destination.conversations.RemoveAll(x => x.id == conversation.id);
                destination.conversations.Add(conversation);
            }
        }

        #endregion

        #region Allow Conflicting IDs

        private static void MergeAllowConflictingIDs(DialogueDatabase destination, DialogueDatabase source,
                                                     bool mergeProperties, bool mergeEmphases, bool mergeActors, bool mergeItems, bool mergeLocations,
                                                     bool mergeVariables, bool mergeConversations)
        {
            if (mergeProperties) MergeDatabaseProperties(destination, source, mergeEmphases);
            if (mergeActors) MergeActorsAllowConflictingIDs(destination, source);
            if (mergeItems) MergeItemsAllowConflictingIDs(destination, source);
            if (mergeLocations) MergeLocationsAllowConflictingIDs(destination, source);
            if (mergeVariables) MergeVariablesAllowConflictingIDs(destination, source);
            if (mergeConversations) MergeConversationsAllowConflictingIDs(destination, source);
        }

        private static void MergeActorsAllowConflictingIDs(DialogueDatabase destination, DialogueDatabase source)
        {
            foreach (Actor actor in source.actors)
            {
                destination.actors.Add(actor);
            }
        }

        private static void MergeItemsAllowConflictingIDs(DialogueDatabase destination, DialogueDatabase source)
        {
            foreach (Item item in source.items)
            {
                destination.items.Add(item);
            }
        }

        private static void MergeLocationsAllowConflictingIDs(DialogueDatabase destination, DialogueDatabase source)
        {
            foreach (Location location in source.locations)
            {
                destination.locations.Add(location);
            }
        }

        private static void MergeVariablesAllowConflictingIDs(DialogueDatabase destination, DialogueDatabase source)
        {
            foreach (Variable variable in source.variables)
            {
                destination.variables.Add(variable);
            }
        }

        private static void MergeConversationsAllowConflictingIDs(DialogueDatabase destination, DialogueDatabase source)
        {
            foreach (Conversation conversation in source.conversations)
            {
                var existingByTitle = destination.conversations.Find(c => string.Equals(c.Title, conversation.Title));
                if (existingByTitle != null)
                {
                    var copy = new Conversation(conversation);
                    copy.Title = conversation.Title + " Copy";
                    destination.conversations.Add(copy);
                }
                else
                {
                    destination.conversations.Add(conversation);
                }
            }
        }

        #endregion

        #region Assign Unique IDs

        /// <summary>
        /// Merges a source database into a destination database by first determining new IDs, 
        /// then assigning those IDs as it copies assets over.
        /// </summary>
        /// <param name="destination">Destination database.</param>
        /// <param name="source">Source database.</param>
        private static void MergeAssignUniqueIDs(DialogueDatabase destination, DialogueDatabase source,
                                                 bool mergeProperties, bool mergeEmphases, bool mergeActors, bool mergeItems, bool mergeLocations,
                                                 bool mergeVariables, bool mergeConversations)
        {
            if (mergeProperties) MergeDatabaseProperties(destination, source, mergeEmphases);
            NewIDs newIDs = new NewIDs();
            GetNewActorIDs(destination, source, newIDs);
            GetNewItemIDs(destination, source, newIDs);
            GetNewLocationIDs(destination, source, newIDs);
            GetNewVariableIDs(destination, source, newIDs);
            GetNewConversationIDs(destination, source, newIDs);
            if (mergeActors) MergeActors(destination, source, newIDs);
            if (mergeItems) MergeItems(destination, source, newIDs);
            if (mergeLocations) MergeLocations(destination, source, newIDs);
            if (mergeVariables) MergeVariables(destination, source, newIDs);
            if (mergeConversations) MergeConversations(destination, source, newIDs);
        }

        private static void GetNewActorIDs(DialogueDatabase destination, DialogueDatabase source, NewIDs newIDs)
        {
            int highestID = destination.baseID - 1;
            foreach (var actor in destination.actors)
            {
                highestID = Mathf.Max(actor.id, highestID);
                if (actor.IsPlayer) newIDs.destinationHasPlayerActor = true;
            }
            int newID = highestID + 1;
            foreach (var actor in source.actors)
            {
                if (!(actor.IsPlayer && newIDs.destinationHasPlayerActor))
                {
                    if (destination.actors.Find(x => string.Equals(x.Name, actor.Name)) == null)
                    {
                        newIDs.actor[actor.id] = newID;
                        newID++;
                    }
                }
            }
        }

        private static void GetNewItemIDs(DialogueDatabase destination, DialogueDatabase source, NewIDs newIDs)
        {
            int highestID = destination.baseID - 1;
            foreach (var item in destination.items)
            {
                highestID = Mathf.Max(item.id, highestID);
            }
            int newID = highestID + 1;
            foreach (var item in source.items)
            {
                if (destination.items.Find(x => string.Equals(x.Name, item.Name)) == null)
                {
                    newIDs.item[item.id] = newID;
                    newID++;
                }
            }
        }

        private static void GetNewLocationIDs(DialogueDatabase destination, DialogueDatabase source, NewIDs newIDs)
        {
            int highestID = destination.baseID - 1;
            foreach (var location in destination.locations)
            {
                highestID = Mathf.Max(location.id, highestID);
            }
            int newID = highestID + 1;
            foreach (var location in source.locations)
            {
                if (destination.locations.Find(x => string.Equals(x.Name, location.Name)) == null)
                {
                    newIDs.location[location.id] = newID;
                    newID++;
                }
            }
        }

        private static void GetNewVariableIDs(DialogueDatabase destination, DialogueDatabase source, NewIDs newIDs)
        {
            int highestID = destination.baseID - 1;
            foreach (var variable in destination.variables)
            {
                highestID = Mathf.Max(variable.id, highestID);
                if (string.Equals(variable.Name, "Alert")) newIDs.destinationHasAlertVariable = true;
            }
            int newID = highestID + 1;
            foreach (var variable in source.variables)
            {
                if (!(string.Equals(variable.Name, "Alert") && newIDs.destinationHasAlertVariable))
                {
                    if (destination.variables.Find(x => string.Equals(x.Name, variable.Name)) == null)
                    {
                        newIDs.variable[variable.id] = newID;
                        newID++;
                    }
                }
            }
        }

        private static void GetNewConversationIDs(DialogueDatabase destination, DialogueDatabase source, NewIDs newIDs)
        {
            int highestID = destination.baseID - 1;
            foreach (var conversation in destination.conversations)
            {
                highestID = Mathf.Max(conversation.id, highestID);
            }
            int newID = highestID + 1;
            foreach (var conversation in source.conversations)
            {
                newIDs.conversation[conversation.id] = newID;
                newID++;
            }
        }

        private static void ConvertFieldIDs(List<Field> fields, NewIDs newIDs)
        {
            foreach (var field in fields)
            {
                int oldID = Tools.StringToInt(field.value);
                switch (field.type)
                {
                    case FieldType.Actor:
                        if (newIDs.actor.ContainsKey(oldID)) field.value = newIDs.actor[oldID].ToString();
                        break;
                    case FieldType.Item:
                        if (newIDs.item.ContainsKey(oldID)) field.value = newIDs.item[oldID].ToString();
                        break;
                    case FieldType.Location:
                        if (newIDs.location.ContainsKey(oldID)) field.value = newIDs.location[oldID].ToString();
                        break;
                }
            }
        }

        private static void MergeActors(DialogueDatabase destination, DialogueDatabase source, NewIDs newIDs)
        {
            foreach (var actor in source.actors)
            {
                if (newIDs.actor.ContainsKey(actor.id))
                {
                    Actor newActor = new Actor(actor);
                    newActor.id = newIDs.actor[actor.id];
                    ConvertFieldIDs(newActor.fields, newIDs);
                    destination.actors.Add(newActor);
                }
            }
        }

        private static void MergeItems(DialogueDatabase destination, DialogueDatabase source, NewIDs newIDs)
        {
            foreach (var item in source.items)
            {
                if (newIDs.item.ContainsKey(item.id))
                {
                    Item newItem = new Item(item);
                    newItem.id = newIDs.item[item.id];
                    ConvertFieldIDs(newItem.fields, newIDs);
                    destination.items.Add(newItem);
                }
            }
        }

        private static void MergeLocations(DialogueDatabase destination, DialogueDatabase source, NewIDs newIDs)
        {
            foreach (var location in source.locations)
            {
                if (newIDs.location.ContainsKey(location.id))
                {
                    Location newLocation = new Location(location);
                    newLocation.id = newIDs.location[location.id];
                    ConvertFieldIDs(newLocation.fields, newIDs);
                    destination.locations.Add(newLocation);
                }
            }
        }

        private static void MergeVariables(DialogueDatabase destination, DialogueDatabase source, NewIDs newIDs)
        {
            foreach (var variable in source.variables)
            {
                if (newIDs.variable.ContainsKey(variable.id))
                {
                    Variable newVariable = new Variable(variable);
                    newVariable.id = newIDs.variable[variable.id];
                    ConvertFieldIDs(newVariable.fields, newIDs);
                    destination.variables.Add(newVariable);
                }
            }
        }

        private static void MergeConversations(DialogueDatabase destination, DialogueDatabase source, NewIDs newIDs)
        {
            foreach (var conversation in source.conversations)
            {
                if (newIDs.conversation.ContainsKey(conversation.id))
                {
                    Conversation newConversation = new Conversation(conversation);
                    newConversation.id = newIDs.conversation[conversation.id];
                    var existingByTitle = destination.conversations.Find(c => string.Equals(c.Title, conversation.Title));
                    if (existingByTitle != null) newConversation.Title = conversation.Title + " Copy";
                    ConvertFieldIDs(newConversation.fields, newIDs);
                    foreach (DialogueEntry newEntry in newConversation.dialogueEntries)
                    {
                        newEntry.conversationID = newConversation.id;
                        foreach (var newLink in newEntry.outgoingLinks)
                        {
                            if (newIDs.conversation.ContainsKey(newLink.originConversationID)) newLink.originConversationID = newIDs.conversation[newLink.originConversationID];
                            if (newIDs.conversation.ContainsKey(newLink.destinationConversationID)) newLink.destinationConversationID = newIDs.conversation[newLink.destinationConversationID];
                        }
                    }
                    destination.conversations.Add(newConversation);
                }
            }
        }

        #endregion

    }

}
