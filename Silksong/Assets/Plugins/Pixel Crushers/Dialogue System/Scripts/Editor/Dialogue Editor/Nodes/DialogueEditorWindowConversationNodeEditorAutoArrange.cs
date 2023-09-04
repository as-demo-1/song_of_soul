using UnityEngine;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem.DialogueEditor
{

    /// <summary>
    /// This part of the Dialogue Editor window handles the auto-arrange
    /// feature for the conversation node editor.
    /// </summary>
    public partial class DialogueEditorWindow
    {

        private const float AutoWidthBetweenNodes = 20f;
        private const float AutoHeightBetweenNodes = 20f;

        private const float AutoStartX = 20f;
        private const float AutoStartY = 20f;

        private float canvasRectWidth { get { return canvasRectWidthMultiplier * DialogueEntry.CanvasRectWidth; } }
        private float canvasRectHeight { get { return DialogueEntry.CanvasRectHeight; } }

        [SerializeField]
        private int canvasRectWidthMultiplier = 1;

        public enum AutoArrangeStyle { Vertically, VerticallyOld, Horizontally }

        private void CheckNodeWidths()
        {
            if (startEntry == null) return;
            if (!Mathf.Approximately(startEntry.canvasRect.width, canvasRectWidth))
            {
                foreach (var entry in currentConversation.dialogueEntries)
                {
                    var rect = entry.canvasRect;
                    var midX = rect.x + (rect.width / 2);
                    entry.canvasRect = new Rect(midX - (canvasRectWidth / 2), rect.y, canvasRectWidth, rect.height);
                }
            }
        }

        private void SetNodeWidthMultiplier(object data)
        {
            canvasRectWidthMultiplier = (int)data;
            CheckNodeWidths();
            ResetDialogueTreeSection();
            ResetConversationNodeSection();
        }

        private void CheckNodeArrangement()
        {
            if (startEntry == null) return;
            if ((startEntry.canvasRect.x == 0) && (startEntry.canvasRect.y == 0)) AutoArrangeNodes(!prefs.addNewNodesToRight);
        }

        private void ConfirmAndAutoArrangeNodes(AutoArrangeStyle style)
        {
            AutoArrangeNodes(style);
        }

        public void AutoArrangeNodes(bool vertically)
        {
            AutoArrangeNodes(vertically ? AutoArrangeStyle.Vertically : AutoArrangeStyle.Horizontally);
        }

        public void AutoArrangeNodes(AutoArrangeStyle style)
        {
            InitializeDialogueTree();
            var tree = new List<List<DialogueEntry>>();
            ArrangeGatherChildren(dialogueTree, 0, tree);
            ArrangeTree(tree, style);
            ArrangeOrphans(style != AutoArrangeStyle.Horizontally);
            SetDatabaseDirty("Auto-Arrange Nodes");
        }

        private void ArrangeGatherChildren(DialogueNode node, int level, List<List<DialogueEntry>> tree)
        {
            if (node == null) return;
            var skip = multinodeSelection.nodes.Count > 1 && !multinodeSelection.nodes.Contains(node.entry);
            while (tree.Count <= level)
            {
                tree.Add(new List<DialogueEntry>());
            }
            if (!(skip || tree[level].Contains(node.entry))) tree[level].Add(node.entry);
            if (node.hasFoldout)
            {
                for (int i = 0; i < node.children.Count; i++)
                {
                    var child = node.children[i];
                    ArrangeGatherChildren(child, level + 1, tree);
                }
            }
        }

        private float GetTreeWidth(List<List<DialogueEntry>> tree)
        {
            float maxWidth = 0;
            for (int i = 0; i < tree.Count; i++)
            {
                var level = tree[i];
                float levelWidth = level.Count * (canvasRectWidth + AutoWidthBetweenNodes);
                maxWidth = Mathf.Max(maxWidth, levelWidth);
            }
            return maxWidth;
        }

        private float GetTreeHeight(List<List<DialogueEntry>> tree)
        {
            float maxHeight = 0;
            for (int i = 0; i < tree.Count; i++)
            {
                var level = tree[i];
                float levelHeight = level.Count * (canvasRectHeight + AutoHeightBetweenNodes);
                maxHeight = Mathf.Max(maxHeight, levelHeight);
            }
            return maxHeight;
        }

        private void ArrangeTree(List<List<DialogueEntry>> tree, AutoArrangeStyle style)
        {
            if (style == AutoArrangeStyle.Horizontally)
            {
                float treeHeight = GetTreeHeight(tree);
                float y = AutoStartY;
                if (orphans.Count > 0) y += canvasRectHeight + AutoHeightBetweenNodes;
                float x = AutoStartX;
                for (int level = 0; level < tree.Count; level++)
                {
                    ArrangeLevel(tree[level], x, y, 0, treeHeight, false);
                    x += canvasRectWidth + AutoWidthBetweenNodes;
                }
            }
            else
            {
                if (currentConversation == null || currentConversation.dialogueEntries == null || currentConversation.dialogueEntries.Count == 0) return;
                if (style == AutoArrangeStyle.VerticallyOld || (multinodeSelection != null && multinodeSelection.nodes.Count > 1))
                {
                    // Use old algorithm if specified or for subsections of conversation tree:
                    float treeWidth = GetTreeWidth(tree);
                    float x = AutoStartX;
                    if (orphans.Count > 0) x += canvasRectWidth + AutoWidthBetweenNodes;
                    float y = AutoStartY;
                    for (int level = 0; level < tree.Count; level++)
                    {
                        ArrangeLevel(tree[level], x, y, treeWidth, 0, true);
                        y += canvasRectHeight + AutoHeightBetweenNodes;
                    }
                }
                else
                {
                    // Use new algorithm provided by digiwombat [Fairmoon Museum]:
                    CalculatePositions(currentConversation.dialogueEntries[0], 0, 0);
                    visited.Clear();
                    subtreeVisited.Clear();
                    subTreeWidths.Clear();
                }
            }
        }


        #region digiwombat's contribution (Thank you!)

        private float HorizontalSpacing => canvasRectWidth * 0.3f; // The horizontal spacing between nodes
        private float VerticalSpacing => canvasRectHeight + 24; // The vertical spacing between nodes

        private HashSet<DialogueEntry> visited = new HashSet<DialogueEntry>();
        private HashSet<DialogueEntry> subtreeVisited = new HashSet<DialogueEntry>();
        private Dictionary<DialogueEntry, float> subTreeWidths = new Dictionary<DialogueEntry, float>();

        // Calculate the positions of all nodes recursively
        private void CalculatePositions(DialogueEntry node, int level, float offset)
        {
            if (node == null) return;

            // If the node has been visited before, return
            if (visited.Contains(node)) return;

            // Mark the node as visited
            visited.Add(node);

            // Calculate the width of the subtree rooted at this node
            float subtreeWidth = GetSubtreeWidth(node);

            node.canvasRect = new Rect(0, 0, canvasRectWidth, canvasRectHeight);
            // Set the X position of this node to be the center of its subtree
            node.canvasRect.x = offset + subtreeWidth / 2;

            // Set the Y position of this node to be based on its level
            node.canvasRect.y = level * (canvasRectHeight + VerticalSpacing) + 50;
            // Recursively calculate the positions of the child nodes
            float childOffset = offset;
            foreach (Link childLink in node.outgoingLinks)
            {
                if (childLink.destinationConversationID != currentConversation.id)
                {
                    continue;
                }
                DialogueEntry child = currentConversation.GetDialogueEntry(childLink.destinationDialogueID);
                CalculatePositions(child, level + 1, childOffset);
                childOffset += GetSubtreeWidth(child) + HorizontalSpacing;
            }
        }

        // Calculate the width of the subtree rooted at a node
        private float GetSubtreeWidth(DialogueEntry node)
        {
            if (node == null) return 0;

            // If the node has no children, return its own width
            if (node.outgoingLinks.Count == 0) return canvasRectWidth;

            // Check if we've been to this subtree before so we don't infinite loop
            if (subtreeVisited.Contains(node))
            {
                if (subTreeWidths.ContainsKey(node))
                {
                    return subTreeWidths[node];
                }
                else
                {
                    return canvasRectWidth;
                }
            }

            subtreeVisited.Add(node);
            // Otherwise, return the sum of the widths of its children and the spacings between them
            float width = 0;
            foreach (Link childLink in node.outgoingLinks)
            {
                if (childLink.destinationConversationID != currentConversation.id)
                {
                    continue;
                }
                DialogueEntry child = currentConversation.GetDialogueEntry(childLink.destinationDialogueID);
                if (!subtreeVisited.Contains(child))
                {
                    width += GetSubtreeWidth(child) + HorizontalSpacing;
                }
            }
            width -= HorizontalSpacing; // Subtract the extra spacing at the end

            // Return the maximum of the node's own width and its children's width
            subTreeWidths[node] = Mathf.Max(width, canvasRectWidth);
            return subTreeWidths[node];
        }

        #endregion

        private void ArrangeLevel(List<DialogueEntry> nodes, float x, float y, float treeWidth, float treeHeight, bool vertically)
        {
            if (nodes == null || nodes.Count == 0) return;
            if (vertically)
            {
                float nodeCanvasWidth = treeWidth / nodes.Count;
                float nodeCanvasOffset = (nodeCanvasWidth - canvasRectWidth) / 2;
                for (int i = 0; i < nodes.Count; i++)
                {
                    float nodeX = x + (i * nodeCanvasWidth) + nodeCanvasOffset;
                    nodes[i].canvasRect = new Rect(nodeX, y, canvasRectWidth, canvasRectHeight);
                }
            }
            else
            {
                float nodeCanvasHeight = treeHeight / nodes.Count;
                float nodeCanvasOffset = (nodeCanvasHeight - canvasRectHeight) / 2;
                for (int i = 0; i < nodes.Count; i++)
                {
                    float nodeY = y + (i * nodeCanvasHeight) + nodeCanvasOffset;
                    nodes[i].canvasRect = new Rect(x, nodeY, canvasRectWidth, canvasRectHeight);
                }
            }
        }

        private void ArrangeOrphans(bool vertically)
        {
            if (vertically)
            {
                float y = AutoStartY;
                foreach (var orphan in orphans)
                {
                    var skip = multinodeSelection.nodes.Count > 1 && !multinodeSelection.nodes.Contains(orphan.entry);
                    if (skip) continue;
                    orphan.entry.canvasRect.x = AutoStartX;
                    orphan.entry.canvasRect.y = y;
                    y += orphan.entry.canvasRect.height + AutoHeightBetweenNodes;
                }
            }
            else
            {
                float x = AutoStartX;
                foreach (var orphan in orphans)
                {
                    var skip = multinodeSelection.nodes.Count > 1 && !multinodeSelection.nodes.Contains(orphan.entry);
                    if (skip) continue;
                    orphan.entry.canvasRect.x = x;
                    x += orphan.entry.canvasRect.width + AutoWidthBetweenNodes;
                    orphan.entry.canvasRect.y = AutoStartY;
                }
            }
        }

    }

}