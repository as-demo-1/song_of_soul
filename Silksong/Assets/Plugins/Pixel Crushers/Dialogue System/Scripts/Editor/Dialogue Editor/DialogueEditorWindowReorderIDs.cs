// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem.DialogueEditor
{

    /// <summary>
    /// This part of the Dialogue Editor window handles the Conversations tab's Reorder IDs options.
    /// </summary>
    public partial class DialogueEditorWindow
    {

        private bool reorderIDsDepthFirst = true;

        private void ConfirmReorderIDsThisConversation()
        {
            if (EditorUtility.DisplayDialog("Reorder IDs", "Are you sure you want to reorder dialogue entry ID numbers in this conversation?", "OK", "Cancel"))
            {
                ReorderIDsThisConversationNow();
            }
        }

        private void ReorderIDsThisConversationNow()
        {
            var currentConv = currentConversation;
            ReorderIDsInConversation(currentConversation);
            ResetConversationSection();
            OpenConversation(currentConv);
        }

        private void ConfirmReorderIDsAllConversations()
        {
            if (!EditorUtility.DisplayDialog("Reorder IDs", "Are you sure you want to reorder dialogue entry ID numbers in ALL conversations?", "OK", "Cancel")) return;
            var currentConv = currentConversation;
            ReorderIDsAllConversations();
            ResetConversationSection();
            OpenConversation(currentConv);
        }

        public void ReorderIDsAllConversations()
        {
            if (database == null) return;
            foreach (var conversation in database.conversations)
            {
                OpenConversation(conversation);
                InitializeDialogueTree();
                ReorderIDsThisConversationNow();
            }
            ResetDialogueTreeSection();
        }

        private void ReorderIDsInConversation(Conversation conversation)
        {
            if (!CheckAllEntryIDsUnique(conversation)) return;
            if (reorderIDsDepthFirst)
            {
                ReorderIDsInConversationDepthFirst(conversation);
            }
            else
            {
                ReorderIDsInConversationAlternateMethod(conversation);
            }
        }

        private bool CheckAllEntryIDsUnique(Conversation conversation)
        {
            var processedIDs = new HashSet<int>();
            var conflictedIDs = new List<int>();
            foreach (var entry in conversation.dialogueEntries)
            {
                if (processedIDs.Contains(entry.id) && !conflictedIDs.Contains(entry.id)) conflictedIDs.Add(entry.id);
                processedIDs.Add(entry.id);
            }
            if (conflictedIDs.Count == 1)
            {
                Debug.LogWarning($"Dialogue System: Can't reorder '{conversation.Title}'. This internal ID is used by more than one entry: {conflictedIDs[0]}. Resolve this conflict first. Menu > Show > Show Node IDs may help.");
            }
            else if (conflictedIDs.Count > 1)
            {
                string s = $"Dialogue System: Can't reorder '{conversation.Title}'. These internal IDs are used by more than one entry:";
                foreach (var id in conflictedIDs)
                {
                    s += $" {id}";
                }
                s += ". Resolve this conflict first. Menu > Show > Show Node IDs may help.";
                Debug.LogWarning(s);
            }
            return conflictedIDs.Count > 0;
        }

        #region Reorder IDs Depth First

        private void ReorderIDsInConversationDepthFirst(Conversation conversation)
        {
            if (conversation == null) return;
            try
            {
                EditorUtility.DisplayProgressBar("Reordering IDs", conversation.Title, 0);

                // Determine new order:
                var newIDs = new Dictionary<int, int>();
                int nextID = 0;
                DetermineNewEntryID(conversation, dialogueTree, newIDs, ref nextID);

                // Include orphans:
                foreach (var entry in conversation.dialogueEntries)
                {
                    if (newIDs.ContainsKey(entry.id)) continue;
                    newIDs.Add(entry.id, nextID);
                    nextID++;
                }

                if (debug)
                {
                    var s = conversation.Title + " new IDs:\n";
                    foreach (var kvp in newIDs)
                    {
                        s += kvp.Key + " --> " + kvp.Value + "\n";
                    }
                    Debug.Log(s);
                }

                // Change IDs:
                int tempOffset = 100000;
                foreach (var kvp in newIDs)
                {
                    ChangeEntryIDEverywhere(conversation.id, kvp.Key, kvp.Value + tempOffset);
                }
                foreach (var kvp in newIDs)
                {
                    ChangeEntryIDEverywhere(conversation.id, kvp.Value + tempOffset, kvp.Value);
                }

                // Sort entries:
                conversation.dialogueEntries.Sort((x, y) => x.id.CompareTo(y.id));

            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private void DetermineNewEntryID(Conversation conversation, DialogueNode node, Dictionary<int, int> newIDs, ref int nextID)
        {
            if (conversation == null || node == null || node.entry.conversationID != conversation.id) return;
            newIDs.Add(node.entry.id, nextID);
            nextID++;
            for (int i = 0; i < node.children.Count; i++)
            {
                var child = node.children[i];
                if (child == null) continue;
                if (newIDs.ContainsKey(child.entry.id)) continue;
                DetermineNewEntryID(conversation, child, newIDs, ref nextID);
            }
        }

        private void ChangeEntryIDEverywhere(int conversationID, int oldID, int newID)
        {
            for (int c = 0; c < database.conversations.Count; c++)
            {
                var conversation = database.conversations[c];
                for (int e = 0; e < conversation.dialogueEntries.Count; e++)
                {
                    var entry = conversation.dialogueEntries[e];
                    if (conversation.id == conversationID && entry.id == oldID)
                    {
                        entry.id = newID;
                    }
                    for (int i = 0; i < entry.outgoingLinks.Count; i++)
                    {
                        var link = entry.outgoingLinks[i];
                        if (link.originConversationID == conversationID && link.originDialogueID == oldID) link.originDialogueID = newID;
                        if (link.destinationConversationID == conversationID && link.destinationDialogueID == oldID) link.destinationDialogueID = newID;
                    }
                }
            }
        }

        #endregion

        #region Reorder IDs Alternate Method (Reorder parents before convergence nodes)

        private void ReorderIDsInConversationAlternateMethod(Conversation conversation)
        {
            if (conversation == null) return;
            try
            {
                EditorUtility.DisplayProgressBar("Reordering IDs", conversation.Title, 0);

                var newIDs = ReorderNodes(conversation);

                // Include orphans:
                int nextID = 0;
                foreach (var id in newIDs.Keys) // Find the next available ID.
                {
                    if (id > nextID) nextID = id;
                }
                nextID++;
                foreach (var entry in conversation.dialogueEntries)
                {
                    if (newIDs.ContainsKey(entry.id)) continue;
                    newIDs.Add(entry.id, nextID);
                    nextID++;
                }

                if (debug)
                {
                    var s = conversation.Title + " new IDs:\n";
                    foreach (var kvp in newIDs)
                    {
                        s += kvp.Key + " --> " + kvp.Value + "\n";
                    }
                    Debug.Log(s);
                }

                // Change IDs:
                int tempOffset = 100000;
                foreach (var kvp in newIDs)
                {
                    ChangeEntryIDEverywhere(conversation.id, kvp.Key, kvp.Value + tempOffset);
                }
                foreach (var kvp in newIDs)
                {
                    ChangeEntryIDEverywhere(conversation.id, kvp.Value + tempOffset, kvp.Value);
                }

                // Sort entries:
                conversation.dialogueEntries.Sort((x, y) => x.id.CompareTo(y.id));

            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private Dictionary<int, int> ReorderNodes(Conversation conversation)
        {
            //there's only one starting node and its always the node at index 0 in the node list; what happens if more items are added to the list hasn't been tested
            List<DialogueEntry> startingNodes = new List<DialogueEntry>();
            startingNodes.Add(conversation.GetFirstDialogueEntry());

            //a branch is a unique sequence of nodes that begins with the starting node, and ends either at a dead end, or when it loops back to a node that it has already visited
            List<List<DialogueEntry>> branches = new List<List<DialogueEntry>>();
            foreach (DialogueEntry node in startingNodes)
            {
                List<DialogueEntry> startingBranch = new List<DialogueEntry>();
                branches.AddRange(RecursiveCreateNewBranch(conversation, node, startingBranch, new List<List<DialogueEntry>>()));
            }

            //this creates a series of debug messages listing the contents of each node
            /*
            foreach(List<DialogueEntry> branch in branches)
            {
                string debugString = "Branch: " + branch[0].id;
                for (int i = 1; i < branch.Count; i++)
                {
                    debugString += ", " + branch[i].id;
                }
                Debug.Log(debugString);
            }
            /**/

            //this dictionary will be used to update all outgoing links once everything's ID potentially changes
            Dictionary<int, int> idReplaceDict = new Dictionary<int, int>();

            //the ID starts at 0 (<START>) and counts up. It only needs to be stored once or it will potentially repeat itself
            int id = 0;
            //this list is used to prevent re-assigning nodes new IDs if they're visited again
            List<DialogueEntry> assignedNodes = new List<DialogueEntry>();
            List<string> idChangeMessages = new List<string>();
            //follow each branch until it gets to a convergence point
            //a convergence point is defined as what happens when two branches link to the same node, at the same "depth", from different nodes
            //this means that when branches merge, the nodes after the merge will all have higher ID numbers than the nodes that link into it
            for (int i = 0; i < branches.Count; i++)
            {
                List<DialogueEntry> currentBranch = branches[i];
                for (int depth = 0; depth < currentBranch.Count; depth++)
                {
                    DialogueEntry currentNode = currentBranch[depth];
                    //if a node is already assigned, nodes underneath it in the branch might still be unnassigned, so we'll look deeper on this branch just in case
                    if (assignedNodes.Contains(currentNode))
                    {
                        continue;
                    }
                    //if a node contains the same target at the same depth as another branch, that is a convergence point
                    //at that point go on to the next branch
                    //branches only check "later" branches for convergence points, so eventually every node will be reached
                    bool convergencePoint = false;
                    if (startingNodes.Contains(currentNode) == false)
                    {
                        for (int bi = i + 1; bi < branches.Count; bi++)
                        {
                            List<DialogueEntry> branch = branches[bi];
                            if (branch.Contains(currentNode) == true)
                            {
                                int targetIndex = branch.IndexOf(currentNode);
                                //make sure that the current branch isn't identical up to this point
                                if (currentBranch.Count <= targetIndex)
                                {
                                    convergencePoint = true;
                                }
                                else
                                {
                                    for (int d = 0; d < targetIndex; d++)
                                    {
                                        if (currentBranch[d] != branch[d])
                                        {
                                            convergencePoint = true;
                                            break;
                                        }
                                    }
                                }
                                if (convergencePoint == true)
                                {
                                    //Debug.Log(currentNode.id + " is a convergence point, branch " + i + " depth " + depth + " converges with branch " + bi + " at depth " + targetIndex);
                                    break;
                                }
                            }
                        }
                    }
                    if (convergencePoint == true)
                    {
                        break;
                    }
                    //add the number to the ID dict, and record the change in the console
                    idReplaceDict.Add(currentNode.id, id);
                    AddStringToIdChangeMessages(currentNode.id, id, false, idChangeMessages);
                    //currentNode.id = id;
                    id++;
                    assignedNodes.Add(currentNode);
                }
            }
            foreach (DialogueEntry node in conversation.dialogueEntries)
            {
                if (assignedNodes.Contains(node) == false)
                {
                    idReplaceDict.Add(node.id, id);
                    AddStringToIdChangeMessages(node.id, id, true, idChangeMessages);
                    //node.id = id;
                    id++;
                    assignedNodes.Add(node);
                }
            }
            conversation.dialogueEntries.Sort(GetLowestID);

            ////post the id change messages to console
            //foreach (string message in idChangeMessages)
            //{
            //    if (message == null)
            //    {
            //        continue;
            //    }
            //    Debug.Log(message);
            //}

            //foreach (DialogueEntry node in conversation.dialogueEntries)
            //{
            //    for (int i = 0; i < node.outgoingLinks.Count; i++)
            //    {
            //        var link = node.outgoingLinks[i];
            //        if (link.destinationConversationID != conversation.id) continue; // Ignore cross-conversation links.
            //        //Debug.Log(node.outgoingLinks[i]);
            //        node.outgoingLinks[i].destinationDialogueID = idReplaceDict[node.outgoingLinks[i].destinationDialogueID];
            //    }
            //}

            return idReplaceDict;
        }

        //all functions past this point exist to support ReorderNodes()

        //this is a helper function to make sure that the ID change messages display in order based on their original ID
        //it also ensures that any IDs that don't exist, won't have a message
        private List<string> AddStringToIdChangeMessages(int oldID, int newID, bool hanging, List<string> messages)
        {
            string newMessage = "";
            if (oldID == newID)
            {
                newMessage = oldID + ": unchanged";
            }
            else
            {
                newMessage = oldID + ": change to " + newID;
            }
            if (hanging == true)
            {
                newMessage += " (hanging node: not accessible from start node)";
            }
            while (messages.Count <= oldID)
            {
                messages.Add(null);
            }
            messages[oldID] = newMessage;
            return messages;
        }

        //int step; //(Debugging)

        //this function is called every time there's a "fork" in the graph, requiring a new branch to be created
        //the branchNode is where the branch "forks", it includes a single node path from beginning to end, including what is "behind" that branchNode
        private List<List<DialogueEntry>> RecursiveCreateNewBranch(Conversation conversation, DialogueEntry branchNode, List<DialogueEntry> existingBranch, List<List<DialogueEntry>> existingBranches)
        {
            List<List<DialogueEntry>> outList = existingBranches;
            existingBranch.Add(branchNode);

            //this produces debug messages that explain each step that involves a new branch being created
            /*
            //step++;
            string debugString = existingBranch[0].id + "";
            for (int i = 1; i < existingBranch.Count; i++)
            {
                debugString += ", " + existingBranch[i].id;
            }
            //Debug.Log("Step " + step + ": create new branch, branch: " + debugString + "  outgoing links: " + branchNode.outgoingLinks.Count + " branches count: " + outList.Count);
            /**/

            if (branchNode.outgoingLinks != null)
            {
                int firstInConversationLinkIndex = 0;

                //if an outgoing link loops back to a node that already is included in the branch, end that branch
                //the first outgoing link doesn't need to create a new branch, since it just continues the existing branch
                if (branchNode.outgoingLinks.Count > 0)
                {
                    List<DialogueEntry> fullBranch = existingBranch;
                    List<List<DialogueEntry>> additionalBranches = new List<List<DialogueEntry>>();
                    for (int i = 0; i < branchNode.outgoingLinks.Count; i++)
                    {
                        if (branchNode.outgoingLinks[i].destinationConversationID != conversation.id) continue; // Ignore cross-conversation links.
                        firstInConversationLinkIndex = i;
                        if (existingBranch.Contains(GetNodeByID(conversation, branchNode.outgoingLinks[i].destinationDialogueID)) == false)
                        {
                            List<DialogueEntry> copyOfExistingBranch = new List<DialogueEntry>();
                            copyOfExistingBranch.AddRange(existingBranch);
                            List<List<DialogueEntry>> newBranchesCreated;
                            fullBranch = RecursiveContinueBranch(conversation, GetNodeByID(conversation, branchNode.outgoingLinks[0].destinationDialogueID), 
                                copyOfExistingBranch, out newBranchesCreated);
                            additionalBranches.AddRange(newBranchesCreated);
                        }
                        break;
                    }
                    outList.Add(fullBranch);
                    outList.AddRange(additionalBranches);
                }
                else
                {
                    outList.Add(existingBranch);
                }
                //any further outgoing links will create a new branch
                for (int i = (firstInConversationLinkIndex + 1); i < branchNode.outgoingLinks.Count; i++)
                {
                    if (branchNode.outgoingLinks[i].destinationConversationID != conversation.id) continue;
                    if (existingBranch.Contains(GetNodeByID(conversation, branchNode.outgoingLinks[i].destinationDialogueID)) == false)
                    {
                        List<DialogueEntry> copyOfExistingBranch = new List<DialogueEntry>();
                        copyOfExistingBranch.AddRange(existingBranch);
                        outList = RecursiveCreateNewBranch(conversation, GetNodeByID(conversation, branchNode.outgoingLinks[i].destinationDialogueID), copyOfExistingBranch, outList);
                    }
                }
            }
            return outList;
        }

        //this function is used to continue a branch that already exists
        //branchNode in this case is just the node currently occupied
        private List<DialogueEntry> RecursiveContinueBranch(Conversation conversation, DialogueEntry branchNode, List<DialogueEntry> existingBranch, out List<List<DialogueEntry>> newBranchesCreated)
        {
            newBranchesCreated = new List<List<DialogueEntry>>();
            existingBranch.Add(branchNode);

            //this produces debug messages that explain each step of the branch being made
            /*
            step++;
            string debugString = existingBranch[0].id +"";
            for (int i = 1; i < existingBranch.Count; i++)
            {
                debugString += ", " + existingBranch[i].id;
            }
            //Debug.Log("Step " + step + ": continue branch, branch: " + debugString + "  outgoing links: " + branchNode.outgoingLinks.Count);
            */


            List<DialogueEntry> outList = existingBranch;
            if (branchNode.outgoingLinks == null)
            {
                return outList;
            }
            else if (branchNode.outgoingLinks.Count == 0)
            {
                return outList;
            }
            //if an outgoing link loops back to a node that already is included in the branch, end that branch
            //the first outgoing link doesn't need to create a new branch, since it just continues the existing branch
            int oldBranchSize = existingBranch.Count;
            if (existingBranch.Contains(GetNodeByID(conversation, branchNode.outgoingLinks[0].destinationDialogueID)) == false)
            {
                List<DialogueEntry> copyOfExistingBranch = new List<DialogueEntry>();
                copyOfExistingBranch.AddRange(existingBranch);
                List<List<DialogueEntry>> additionalNewBranchesCreated;
                outList = RecursiveContinueBranch(conversation, GetNodeByID(conversation, branchNode.outgoingLinks[0].destinationDialogueID), 
                    copyOfExistingBranch, out additionalNewBranchesCreated);
                newBranchesCreated.AddRange(additionalNewBranchesCreated);
            }
            //any further outgoing links will create a new branch
            for (int i = 1; i < branchNode.outgoingLinks.Count; i++)
            {
                if (existingBranch.Contains(GetNodeByID(conversation, branchNode.outgoingLinks[i].destinationDialogueID)) == false)
                {
                    List<DialogueEntry> copyOfExistingBranch = new List<DialogueEntry>();
                    copyOfExistingBranch.AddRange(existingBranch);
                    newBranchesCreated.AddRange(RecursiveCreateNewBranch(conversation, GetNodeByID(conversation, branchNode.outgoingLinks[i].destinationDialogueID), copyOfExistingBranch, new List<List<DialogueEntry>>()));
                }
            }
            return outList;
        }

        private DialogueEntry GetNodeByID(Conversation conversation, int targetID)
        {
            DialogueEntry outNode = null;
            outNode = conversation.dialogueEntries.Find(node => node.id == targetID);
            if (outNode == null)
            {
                //if there's an invalid outgoing link, this error will post to console, and then immediately be followed by a null reference exception
                //the outgoing links will have to be fixed before ReorderNodes() will work
                Debug.LogError("Dialogue System: A dialogue entry has an outgoing link to entry ID " + targetID + " but no entry in conversation '" +
                    conversation.Title + "' has that ID.");
            }
            return outNode;
        }

        public int GetLowestID(DialogueEntry n1, DialogueEntry n2)
        {
            return n1.id - n2.id;
        }

        #endregion

    }

}
