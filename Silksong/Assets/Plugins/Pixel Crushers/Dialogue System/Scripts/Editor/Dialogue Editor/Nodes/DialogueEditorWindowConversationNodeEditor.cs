// Copyright (c) Pixel Crushers. All rights reserved.

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Graphs;

namespace PixelCrushers.DialogueSystem.DialogueEditor
{

    /// <summary>
    /// This part of the Dialogue Editor window handles the main code for 
    /// the conversation node editor.
    /// </summary>
    public partial class DialogueEditorWindow
    {

        private bool nodeEditorDeleteCurrentConversation = false;

        private HashSet<int> orphanIDs = new HashSet<int>();
        private int orphanIDsConversationID = -1;

        [SerializeField]
        private bool showNodeIDs = false;

        [SerializeField]
        private bool showTitlesInsteadOfText = false;

        [SerializeField]
        private bool showAllActorNames = false;

        [SerializeField]
        private bool showOtherActorNames = true;

        [SerializeField]
        private bool showActorPortraits = false;

        [SerializeField]
        private bool showDescriptions = false;

        [SerializeField]
        private bool showParticipantNames = true;

        [SerializeField]
        private bool showEndNodeMarkers = true;

        [SerializeField]
        private bool showFullTextOnHover = true;

        [SerializeField]
        private bool addNewNodesToRight = false;

        [SerializeField]
        private bool autoArrangeOnCreate = false;

        private Dictionary<int, Sprite> actorPortraitCache = null;

        private Dictionary<int, Color> actorCustomColorCache = null;
        private Dictionary<int, bool> actorHasCustomColorCache = null;
        private Dictionary<int, bool> actorIsPlayerCache = null;

        [SerializeField]
        private bool zoomLocked;

        private bool isMakingLink = false;
        private DialogueEntry linkSourceEntry = null;
        private DialogueEntry linkTargetEntry = null;

        private Link selectedLink = null;
        private Link newSelectedLink = null;

        private ConversationState currentConversationState = null;
        private DialogueEntry currentRuntimeEntry = null;

        private const float kZoomMin = 0.1f;
        private const float kZoomMax = 2.0f;

        [SerializeField]
        private float _zoom = 1.0f;
        private Vector2 _zoomCoordsOrigin = Vector2.zero;
        private Rect scaledPosition;
        private Rect _zoomArea;

        private DialogueEntry currentHoveredEntry = null;
        private GUIContent currentHoverGUIContent = null;
        private Rect currentHoverRect;

        private static Color OutgoingLinkColor = Color.yellow;
        private static Color IncomingLinkColor = new Color(0.6f, 0.3f, 0.1f);

        private List<DialogueEntry> nodeClipboard = null;
        private Vector2 contextMenuPosition;

        private bool showQuickDialogueTextEntry = false;
        private Rect quickDialogueTextEntryRect;

        private Texture2D _sequenceIcon = null;
        private Texture2D sequenceIcon
        {
            get
            {
                if (_sequenceIcon == null) _sequenceIcon = EditorGUIUtility.Load("Dialogue System/Sequence.png") as Texture2D;
                return _sequenceIcon;
            }
        }
        private Texture2D _conditionsIcon = null;
        private Texture2D conditionsIcon
        {
            get
            {
                if (_conditionsIcon == null) _conditionsIcon = EditorGUIUtility.Load("Dialogue System/Conditions.png") as Texture2D;
                return _conditionsIcon;
            }
        }
        private Texture2D _scriptIcon = null;
        private Texture2D scriptIcon
        {
            get
            {
                if (_scriptIcon == null) _scriptIcon = EditorGUIUtility.Load("Dialogue System/Script.png") as Texture2D;
                return _scriptIcon;
            }
        }
        private Texture2D _eventIcon = null;
        private Texture2D eventIcon
        {
            get
            {
                if (_eventIcon == null) _eventIcon = EditorGUIUtility.Load("Dialogue System/Event.png") as Texture2D;
                return _eventIcon;
            }
        }
        private Texture2D _resizeIcon = null;
        private Texture2D resizeIcon
        {
            get
            {
                if (_resizeIcon == null) _resizeIcon = EditorGUIUtility.Load("Dialogue System/Resize.png") as Texture2D;
                return _resizeIcon;
            }
        }
        private Texture2D _boxTexture = null;
        private Texture2D boxTexture
        {
            get
            {
                if (_boxTexture == null)
                {
                    _boxTexture = new Texture2D(1, 1);
                    _boxTexture.SetPixel(0, 0, Color.white);
                }
                return _boxTexture;
            }
        }

        private Vector2 ConvertScreenCoordsToZoomCoords(Rect _zoomArea, Vector2 screenCoords)
        {
            return (screenCoords - _zoomArea.TopLeft()) / _zoom + _zoomCoordsOrigin;
        }

        public class MultinodeSelection
        {
            public List<DialogueEntry> nodes = new List<DialogueEntry>();
            public List<EntryGroup> groups = new List<EntryGroup>();

            public void Clear()
            {
                nodes.Clear();
                groups.Clear();
            }
        }

        private const float EntryGroupHeadingHeight = 20;
        private List<DialogueEntry> nodesInEntryGroup = null;
        private List<EntryGroup> subgroupsInEntryGroup = null;
        private bool isRenamingEntryGroup = false;
        private bool isResizingEntryGroup = false;

        private MultinodeSelection multinodeSelection = new MultinodeSelection();

        private bool dragged = false;

        private bool isLassoing = false;
        private Rect lassoRect = new Rect(0, 0, 0, 0);

        private Rect nodeEditorVisibleRect;

        private bool wasShiftDown = false;

        private EntryGroup selectedEntryGroup = null;


        private void DrawConversationSectionNodeStyle()
        {
            if (!(Application.isPlaying && DialogueManager.hasInstance)) currentRuntimeEntry = null;

            CheckDialogueTreeGUIStyles();
            if (nodeEditorDeleteCurrentConversation) DeleteCurrentConversationInNodeEditor();
            //--- Unnecessary: if (inspectorSelection == null) inspectorSelection = currentConversation;

            DrawNodeEditorTopControls();

            var topOffset = GetTopOffsetHeight();

            scaledPosition = new Rect(position.x, position.y, (1 / _zoom) * position.width, (1 / _zoom) * position.height);
            _zoomArea = new Rect(0, topOffset, position.width, position.height - topOffset);
            if (_zoom > 1)
            {
                _zoomArea = new Rect(0, topOffset, position.width + ((_zoom - 1) * position.width), position.height - topOffset + ((_zoom - 1) * (position.height - topOffset)));
            }
            nodeEditorVisibleRect = new Rect(canvasScrollPosition.x, canvasScrollPosition.y, position.width / _zoom, position.height / _zoom);
            EditorZoomArea.Begin(_zoom, _zoomArea);
            try
            {
                DrawCanvas();
                HandleEmptyCanvasEvents();
                HandleKeyEvents();
            }
            finally
            {
                EditorZoomArea.End();
            }

            DrawDatabaseName();
            if (showParticipantNames) DrawParticipantsOnCanvas();

            Handles.color = MajorGridLineColor;
            Handles.DrawLine(new Vector2(0, topOffset), new Vector2(position.width, topOffset));

            // Debugging: EditorGUI.LabelField(new Rect(10, 30, 500, 30), "pos=" + canvasScrollPosition);
            // Debugging: EditorGUI.LabelField(new Rect(10, 60, 500, 30), "size=" + position.width / _zoom);

            // Debugging context menu: GUI.Label(new Rect(8, position.height - 50, 500, 30), "menuPos=" + contextMenuPosition);
        }

        private float GetTopOffsetHeight()
        {
            return isSearchBarOpen ? 70f : 49f;
        }

        private void DrawCanvasContents()
        {
            if (currentConversation == null) return;
            newSelectedLink = null;
            DrawAllEntryGroups();
            DrawAllConnectors();
            DrawAllNodes();
            DrawLasso();
            CheckNewSelectedLink();
            if (showQuickDialogueTextEntry) DrawQuickDialogueTextEntry();
            else if (isRenamingEntryGroup) DrawEntryGroupRenameField();
            newSelectedLink = null;
        }

        private Conversation canvasParticipantsConversation = null;
        private int canvasActorID = -1;
        private int canvasConversantID = -1;
        private string canvasActorName = "unassigned";
        private string canvasConversantName = "unassigned";
        private bool isCanvasActorNameValid = false;
        private bool isCanvasConversantNameValid = false;

        private void DrawParticipantsOnCanvas()
        {
            if (currentConversation == null) return;
            if (currentConversation != null &&
                (currentConversation != canvasParticipantsConversation || (currentConversation.ActorID != canvasActorID || currentConversation.ConversantID != canvasConversantID)))
            {
                canvasParticipantsConversation = currentConversation;

                canvasActorID = currentConversation.ActorID;
                var actor = database.GetActor(canvasActorID);
                canvasActorName = (actor != null) ? actor.Name : "unassigned";
                isCanvasActorNameValid = (canvasActorID != -1) && (actor != null);

                canvasConversantID = currentConversation.ConversantID;
                var conversant = database.GetActor(canvasConversantID);
                canvasConversantName = (conversant != null) ? conversant.Name : "unassigned";
                isCanvasConversantNameValid = (canvasConversantID != -1) && (conversant != null);
            }
            try
            {
                if (!isCanvasActorNameValid) GUI.color = Color.red;
                EditorGUI.LabelField(new Rect(0, position.height - 50, position.width - 4, 50),
                    "Actor: " + canvasActorName,
                    conversationParticipantsStyle);
                if (!isCanvasActorNameValid && isCanvasConversantNameValid) GUI.color = Color.white;
                if (!isCanvasConversantNameValid) GUI.color = Color.red;
                EditorGUI.LabelField(new Rect(0, position.height - 26, position.width - 4, 26),
                    "Conversant: " + canvasConversantName,
                    conversationParticipantsStyle);
                if (!isCanvasConversantNameValid) GUI.color = Color.white;
            }
            catch (System.NullReferenceException)
            {
                // Hide errors with GUILayout.LabelFieldInternal.
            }
        }

        private void UpdateRuntimeConversationsTab()
        {
            if (DialogueManager.hasInstance)
            {
                var newConversationState = DialogueManager.currentConversationState;
                if (newConversationState != currentConversationState)
                {
                    currentConversationState = newConversationState;
                    currentRuntimeEntry = (currentConversationState != null && currentConversationState.subtitle != null)
                        ? currentConversationState.subtitle.dialogueEntry
                            : null;
                    Repaint();
                }
            }
        }

        private void CheckNewSelectedLink()
        {
            if ((newSelectedLink != null) && (newSelectedLink != selectedLink))
            {
                selectedLink = newSelectedLink;
                inspectorSelection = selectedLink;
                isLassoing = false;
                multinodeSelection.Clear();
                Event.current.Use();
            }
            else if (currentEntry != null)
            {
                selectedLink = null;
            }
        }

        private void DrawAllEntryGroups()
        {
            var originalColor = GUI.color;
            for (int i = 0; i < currentConversation.entryGroups.Count; i++)
            {
                var group = currentConversation.entryGroups[i];
                if (!nodeEditorVisibleRect.Overlaps(group.rect)) continue; // Skip drawing if not in visible window.
                GUI.color = group.color;
                if (boxTexture != null) GUI.DrawTexture(group.rect, boxTexture);
                if (!isRenamingEntryGroup)
                {
                    GUI.color = new Color(group.color.r, group.color.g, group.color.b, 1);
                    GUI.Box(new Rect(group.rect.x, group.rect.y, group.rect.width, EntryGroupHeadingHeight), group.name, GUI.skin.button);
                }
                GUI.color = originalColor;
                var resizeRect = new Rect(group.rect.x + group.rect.width - 20, group.rect.y + group.rect.height - 20, 16, 16);
                if (resizeIcon != null) GUI.DrawTexture(resizeRect, resizeIcon);
            }
            GUI.color = originalColor;
        }

        private void DrawEntryGroupRenameField()
        {
            if (selectedEntryGroup == null) return;
            var group = selectedEntryGroup;
            selectedEntryGroup.name = EditorGUI.TextField(new Rect(group.rect.x, group.rect.y, group.rect.width, EntryGroupHeadingHeight), GUIContent.none, group.name);
        }

        public void DrawEntryGroupContents()
        {
            if (selectedEntryGroup == null) return;
            selectedEntryGroup.name = EditorGUILayout.TextField("Name", selectedEntryGroup.name);
#if UNITY_5 || UNITY_2017
            selectedEntryGroup.color = EditorGUILayout.ColorField(GUIContent.none, selectedEntryGroup.color, true, true, false, null);
#else
            selectedEntryGroup.color = EditorGUILayout.ColorField(GUIContent.none, selectedEntryGroup.color, true, true, false);
#endif
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            var delete = GUILayout.Button("Delete", GUILayout.Width(128));
            EditorGUILayout.EndHorizontal();
            if (delete) DeleteEntryGroup(selectedEntryGroup);
        }

        private void DrawAllConnectors()
        {
            for (int i = 0; i < currentConversation.dialogueEntries.Count; i++)
            {
                var entry = currentConversation.dialogueEntries[i];
                DrawEntryConnectors(entry);
            }
            if (isMakingLink) DrawNewLinkConnector();
        }

        private static Color SelectedNodeColor = new Color(0.5f, 0.75f, 1f, 1f);
        private static Color ConnectorColor = new Color(1, 0.5f, 0);

        private void DrawEntryConnectors(DialogueEntry entry)
        {
            if (entry == null) return;
            int numCrossConversationLinks = 0;
            for (int i = 0; i < entry.outgoingLinks.Count; i++)
            {
                var link = entry.outgoingLinks[i];
                if (link.destinationConversationID == currentConversation.id)
                {
                    // Fix any links whose originDialogueID doesn't match the origin entry's ID:
                    if (link.originDialogueID != entry.id) link.originDialogueID = entry.id;

                    // Only show connection if within same conversation:
                    DialogueEntry destination = currentConversation.dialogueEntries.Find(e => e.id == link.destinationDialogueID);
                    if (destination != null)
                    {
                        var startCenter = entry.canvasRect.center;
                        var endCenter = destination.canvasRect.center;
                        Vector3 start = new Vector3(startCenter.x, startCenter.y, 0);
                        Vector3 end = new Vector3(endCenter.x, endCenter.y, 0);
                        // Check if it's not in visible window and we can skip:
                        if (!(nodeEditorVisibleRect.Contains(startCenter) || nodeEditorVisibleRect.Contains(endCenter)))
                        {
                            var connectorRect = new Rect(Mathf.Min(startCenter.x, endCenter.x), Mathf.Min(startCenter.y, endCenter.y), Mathf.Abs(startCenter.x - endCenter.x), Mathf.Abs(startCenter.y - endCenter.y));
                            if (!nodeEditorVisibleRect.Overlaps(connectorRect)) continue; // Skip drawing if not in visible window.
                        }
                        Color connectorColor = (link == selectedLink) ? SelectedNodeColor : Color.white;
                        if (IsCurrentRuntimeEntry(entry))
                        {
                            // Show current runtime entry's links in color based on destination's Conditions:
                            connectorColor = IsValidRuntimeLink(link) ? Color.green : Color.red;
                        }
                        else if (entry == currentEntry)
                        {
                            // Show selected entry's links in special color:
                            connectorColor = OutgoingLinkColor;
                        }
                        else if (currentEntry != null && link.destinationDialogueID == currentEntry.id)
                        {
                            connectorColor = IncomingLinkColor;
                        }
                        DrawLink(start, end, connectorColor, link.priority != ConditionPriority.Normal);
                        HandleConnectorEvents(link, start, end);
                    }
                }
                else
                {
                    // Otherwise show special cross-conversation arrow link:
                    Vector3 start = new Vector3(entry.canvasRect.center.x, entry.canvasRect.center.y, 0);
                    Vector3 end = new Vector3(start.x, start.y + 26, 0);
                    DrawSpecialLink(start, end, ConnectorColor);
                    numCrossConversationLinks++;
                }
            }
            if (numCrossConversationLinks > 1)
            {
                // If more than one cross-conversation link, show the count:
                var originalColor = GUI.color;
                GUI.color = new Color(1, 0.6f, 0);
                EditorGUI.LabelField(new Rect(entry.canvasRect.center.x + 8, entry.canvasRect.center.y + 20, 50, 50), numCrossConversationLinks.ToString());
                GUI.color = originalColor;
            }
            if (showEndNodeMarkers && entry.outgoingLinks.Count == 0)
            {
                // If no links, show that it's an end node:
                Vector3 start = new Vector3(entry.canvasRect.center.x, entry.canvasRect.center.y, 0);
                Vector3 end = new Vector3(start.x, start.y + 26, 0);
                DrawSpecialLink(start, end, Color.red);
            }
        }

        private bool IsCurrentRuntimeEntry(DialogueEntry entry)
        {
            return (currentRuntimeEntry != null) && (entry.conversationID == currentRuntimeEntry.conversationID) && (entry.id == currentRuntimeEntry.id);
        }

        private bool IsValidRuntimeLink(Link link)
        {
            return (currentConversationState != null) &&
                (IsValidRuntimeResponse(link, currentConversationState.pcResponses) ||
                IsValidRuntimeResponse(link, currentConversationState.npcResponses));
        }

        private bool IsValidRuntimeResponse(Link link, Response[] responses)
        {
            for (int i = 0; i < responses.Length; i++)
            {
                var response = responses[i];
                if ((link.destinationConversationID == response.destinationEntry.conversationID) &&
                    (link.destinationDialogueID == response.destinationEntry.id))
                {
                    return true;
                }
            }
            return false;
        }

        private void DrawNewLinkConnector()
        {
            if (isMakingLink && (linkSourceEntry != null))
            {
                Vector3 start = new Vector3(linkSourceEntry.canvasRect.center.x, linkSourceEntry.canvasRect.center.y, 0);
                if ((linkTargetEntry != null) && Event.current.isMouse)
                {
                    if (!linkTargetEntry.canvasRect.Contains(Event.current.mousePosition))
                    {
                        linkTargetEntry = null;
                    }
                }
                Vector3 end = (linkTargetEntry != null)
                    ? new Vector3(linkTargetEntry.canvasRect.center.x, linkTargetEntry.canvasRect.center.y, 0)
                    : new Vector3(Event.current.mousePosition.x, Event.current.mousePosition.y, 0);
                DrawLink(start, end, Color.white, false);
            }
        }

        private void HandleNodeEditorScrollWheelEvents()
        {
            if (Event.current.type == EventType.ScrollWheel)
            {
                CloseQuickDialogueTextEntry();
                if (!zoomLocked)
                {
                    var prevZoom = _zoom;
                    var zoomChange = -Event.current.delta.y / 100f;
                    var newZoom = Mathf.Clamp(_zoom + zoomChange, kZoomMin, kZoomMax);
                    if (newZoom != _zoom)
                    {
                        _zoom = newZoom;
                        var prevWinWidth = (position.width * (position.width / (prevZoom * position.width)));
                        var winWidth = (position.width * (position.width / (_zoom * position.width)));
                        var widthChange = prevWinWidth - winWidth;

                        var prevWinHeight = (position.height * (position.height / (prevZoom * position.height)));
                        var winHeight = (position.height * (position.height / (_zoom * position.height)));
                        var heightChange = prevWinHeight - winHeight;

                        var xOfs = (Event.current.mousePosition.x / winWidth) * widthChange / _zoom;
                        var yOfs = ((Event.current.mousePosition.y - GetTopOffsetHeight()) / winHeight) * heightChange / _zoom;
                        canvasScrollPosition = new Vector2(
                            canvasScrollPosition.x + xOfs,
                            canvasScrollPosition.y + yOfs);
                    }
                }
                Event.current.Use();
            }
        }

        private void HandleConnectorEvents(Link link, Vector3 start, Vector3 end)
        {
            switch (Event.current.type)
            {
                case EventType.MouseUp:
                    bool isReallyLassoing = isLassoing && (Mathf.Abs(lassoRect.width) > 10f) && (Mathf.Abs(lassoRect.height) > 10f) && (selectedEntryGroup != null);
                    if (!isReallyLassoing && (Event.current.button == LeftMouseButton) &&
                        IsPointOnLineSegment(Event.current.mousePosition, start, end) &&
                        !IsMouseInQuickDialogueTextRect())
                    {
                        newSelectedLink = link;
                        currentEntry = null;
                        inspectorSelection = newSelectedLink;
                        if (IsMouseInQuickDialogueTextRect())
                        {
                            CloseQuickDialogueTextEntry();
                        }
                    }
                    break;
            }
        }

        private bool IsPointOnLineSegment(Vector2 point, Vector3 start, Vector3 end)
        {
            const float tolerance = 10f;
            float minX = Mathf.Min(start.x, end.x);
            float minY = Mathf.Min(start.y, end.y);
            float width = Mathf.Abs(start.x - end.x);
            float height = Mathf.Abs(start.y - end.y);
            float midX = minX + (width / 2);
            if ((width <= tolerance) && (Mathf.Abs(point.x - midX) <= tolerance) && (minY <= point.y) && (point.y <= minY + height))
            {
                return true; // Special case: vertical line.
            }
            else if ((minX < point.x && point.x < (minX + width)) && (Mathf.Abs(start.y - end.y) <= 2) && (Mathf.Abs(point.y - end.y) <= 2))
            {
                return true; // Special case: horizontal line.
            }
            Rect boundingRect = new Rect(minX, minY, width, height);
            if (boundingRect.Contains(point))
            {
                float slope = (end.y - start.y) / (end.x - start.x);
                float yIntercept = -(slope * start.x) + start.y;
                float distanceFromLine = Mathf.Abs(point.y - (slope * point.x + yIntercept));
                return (distanceFromLine <= tolerance);
            }
            return false;
        }

        public void DrawSelectedLinkContents()
        {
            if (selectedLink == null) return;
            selectedLink.priority = (ConditionPriority)EditorGUILayout.Popup("Priority", (int)selectedLink.priority, priorityStrings);
        }

        private void DrawAllNodes()
        {
            CheckOrphanIDs();
            for (int i = 0; i < currentConversation.dialogueEntries.Count; i++)
            {
                DrawEntryNode(currentConversation.dialogueEntries[i]);
            }
            var prevHoveredEntry = currentHoveredEntry;
            currentHoveredEntry = null;
            for (int i = currentConversation.dialogueEntries.Count - 1; i >= 0; i--)
            {
                HandleNodeEvents(currentConversation.dialogueEntries[i]);
            }
            if (currentHoveredEntry == null)
            {
                if (prevHoveredEntry != null)
                {
                    currentHoverGUIContent = null;
                }
            }
            else if (currentHoveredEntry != prevHoveredEntry)
            {
                var text = currentHoveredEntry.currentDialogueText;
                if (string.IsNullOrEmpty(text)) text = currentHoveredEntry.currentMenuText;
                if (string.IsNullOrEmpty(text)) text = currentHoveredEntry.Title;
                var linkText = GetLinkSummaryHoverText();
                if (!string.IsNullOrEmpty(linkText))
                {
                    text += linkText;
                }
                if (Application.isPlaying && DialogueManager.instance != null && DialogueManager.instance.includeSimStatus)
                {
                    text += "\nSimStatus=" + DialogueLua.GetSimStatus(currentHoveredEntry);
                }
                currentHoverGUIContent = string.IsNullOrEmpty(text) ? null : new GUIContent(text);
                if (currentHoverGUIContent != null)
                {
                    var guiStyle = GUI.skin.textField;
                    var size = guiStyle.CalcSize(new GUIContent(currentHoverGUIContent));
                    var canvasRect = currentHoveredEntry.canvasRect;
                    currentHoverRect = new Rect(canvasRect.x + (canvasRect.width / 2) - (size.x / 2), canvasRect.y + canvasRect.height, size.x + 1, size.y);
                }
            }
            if (Event.current.type == EventType.Repaint && currentHoverGUIContent != null)
            {
                if (!(showQuickDialogueTextEntry && currentHoveredEntry == currentEntry))
                {
                    GUI.Label(currentHoverRect, currentHoverGUIContent, GUI.skin.textField);
                }
            }
        }

        private string GetLinkSummaryHoverText()
        {
            if (currentHoveredEntry == null || currentHoveredEntry.outgoingLinks == null || currentHoveredEntry.outgoingLinks.Count == 0) return string.Empty;
            var s = string.Empty;
            for (int i = (int)ConditionPriority.High; i >= 0; i--)
            {
                var priority = (ConditionPriority)i;
                for (int j = 0; j < currentHoveredEntry.outgoingLinks.Count; j++)
                {
                    var link = currentHoveredEntry.outgoingLinks[j];
                    if (link.priority == priority)
                    {
                        if (!string.IsNullOrEmpty(s)) s += ", ";
                        if (link.destinationConversationID == currentHoveredEntry.conversationID)
                        {
                            s += "[" + link.destinationDialogueID + "]";
                        }
                        else
                        {
                            s += "[" + link.destinationConversationID + ":" + link.destinationDialogueID + "]";
                        }
                    }
                }
            }
            if (!string.IsNullOrEmpty(s)) s = "\n" + s;
            return s;
        }

        #region Node Styles

        private Dictionary<Styles.Color, GUIStyle> m_nodeStyles = new Dictionary<Styles.Color, GUIStyle>();
        private Dictionary<Styles.Color, GUIStyle> m_nodeStylesSelected = new Dictionary<Styles.Color, GUIStyle>();
        private GUIStyle m_customNodeStyle = null;

        private GUIStyle GetNodeStyle(Styles.Color color, bool isSelected)
        {
            return isSelected ? GetNodeStyle(m_nodeStylesSelected, color, isSelected) : GetNodeStyle(m_nodeStyles, color, isSelected);
        }

        private GUIStyle GetNodeStyle(Dictionary<Styles.Color, GUIStyle> dict, Styles.Color color, bool isSelected)
        {
            GUIStyle nodeStyle;
            if (!dict.TryGetValue(color, out nodeStyle))
            {
                nodeStyle = new GUIStyle(Styles.GetNodeStyle("node", color, isSelected));
                nodeStyle.alignment = TextAnchor.MiddleCenter;
                nodeStyle.contentOffset = new Vector2(0, -16);
                dict.Add(color, nodeStyle);
            }
            return nodeStyle;
        }

        private void GetNodeStyle(DialogueEntry entry, out GUIStyle nodeStyle, out bool isSelected, out bool isCustomColor, out Color customColor)
        {
            isCustomColor = false;
            customColor = Color.white;
            isSelected = multinodeSelection.nodes.Contains(entry);
            if (IsCurrentRuntimeEntry(entry))
            {
                // Current runtime entry is green:
                nodeStyle = GetNodeStyle(Styles.Color.Green, isSelected);
            }
            else if (entry.id == 0)
            {
                // START node is orange:
                nodeStyle = GetNodeStyle(Styles.Color.Orange, isSelected);
            }
            else if (orphanIDs.Contains(entry.id) && (entry.id != 0))
            {
                // Orphaned nodes are red:
                nodeStyle = GetNodeStyle(Styles.Color.Red, isSelected);
            }
            else
            {
                // Check if actor has custom color:
                var actorID = entry.ActorID;
                if (actorHasCustomColorCache == null) actorHasCustomColorCache = new Dictionary<int, bool>();
                if (actorIsPlayerCache == null) actorIsPlayerCache = new Dictionary<int, bool>();
                if (actorCustomColorCache == null) actorCustomColorCache = new Dictionary<int, Color>();
                if (!actorHasCustomColorCache.ContainsKey(actorID) ||
                    !actorCustomColorCache.ContainsKey(actorID) ||
                    (actorCustomColorCache.ContainsKey(actorID) && (actorCustomColorCache[actorID].a == 0)))
                {
                    var actor = database.GetActor(actorID);
                    actorIsPlayerCache[actorID] = (actor != null) ? actor.IsPlayer : false;
                    if (actor != null && actor.FieldExists(NodeColorFieldTitle))
                    {
                        var colorFieldValue = actor.LookupValue(NodeColorFieldTitle);
                        if (!string.IsNullOrEmpty(colorFieldValue))
                        {
                            var actorColor = EditorTools.NodeColorStringToColor(colorFieldValue);
                            if (actorColor.r == 0 && actorColor.g == 0 && actorColor.b == 0)
                            {
                                actorColor = actor.IsPlayer ? Color.blue : Color.gray;
                            }
                            actorColor.a = 1;
                            actorCustomColorCache.Add(actorID, actorColor);
                            actorHasCustomColorCache[actorID] = true;
                        }
                        else
                        {
                            actorHasCustomColorCache[actorID] = false;
                        }
                    }
                    else
                    {
                        actorHasCustomColorCache[actorID] = false;
                    }
                }
                if (actorHasCustomColorCache[actorID])
                {
                    // Use actor's custom color:
                    isCustomColor = true;
                    customColor = actorCustomColorCache[actorID];
                    if (customColor.a == 0)
                    {
                        actorHasCustomColorCache.Remove(actorID);
                        actorCustomColorCache.Remove(actorID);
                        customColor = Color.gray;
                    }
                    if (m_customNodeStyle == null || m_customNodeStyle.normal.background == null)
                    {
#if UNITY_2019_3_OR_NEWER
                        m_customNodeStyle = new GUIStyle(GUI.skin.button);
#else
                        m_customNodeStyle = new GUIStyle(GUI.skin.box);
#endif
                        m_customNodeStyle.contentOffset = new Vector2(0, -4);
                        var nodeTexture = EditorGUIUtility.Load("Dialogue System/EditorNode.png") as Texture2D;
                        m_customNodeStyle.normal.background = nodeTexture ?? Texture2D.whiteTexture;
                        m_customNodeStyle.normal.textColor = EditorGUIUtility.isProSkin ? new Color(0.9f, 0.9f, 0.9f) : Color.black;
                        m_customNodeStyle.wordWrap = false;
                        m_customNodeStyle.alignment = TextAnchor.MiddleCenter;
                    }
                    nodeStyle = m_customNodeStyle;
                }
                else
                {
                    // Use default colors (blue for player, gray for NPCs):
                    nodeStyle = actorIsPlayerCache[actorID]
                        ? nodeStyle = GetNodeStyle(Styles.Color.Blue, isSelected)
                    : nodeStyle = GetNodeStyle(Styles.Color.Gray, isSelected);
                }
            }
        }

        #endregion

        private void DrawEntryNode(DialogueEntry entry)
        {
            if (!nodeEditorVisibleRect.Overlaps(entry.canvasRect)) return; // Skip drawing if not in visible window.

            GUIStyle nodeStyle;
            bool isSelected;
            bool isCustomColor;
            Color customColor;
            GetNodeStyle(entry, out nodeStyle, out isSelected, out isCustomColor, out customColor);

            if (isCustomColor && customColor.a == 0) customColor = Color.gray; // Safeguard.

            string nodeLabel = GetDialogueEntryNodeText(entry);
            if (showQuickDialogueTextEntry && entry == currentEntry) nodeLabel = string.Empty;

            var guicolor_backup = GUI.backgroundColor;

            var boxRect = new Rect(entry.canvasRect.x, entry.canvasRect.y, entry.canvasRect.width + 4, entry.canvasRect.height + 4);

            if (isCustomColor)
            {
                if (isSelected)
                {
                    var bigRect = new Rect(boxRect.x + 2, boxRect.y, boxRect.width - 4, boxRect.height - 2);
                    GUI.Box(bigRect, string.Empty, Styles.GetNodeStyle("node", Styles.Color.Blue, true));
                }
                GUI.backgroundColor = customColor;
            }

            GUI.Box(boxRect, nodeLabel, nodeStyle);

            if (isCustomColor)
            {
                GUI.backgroundColor = guicolor_backup;
            }

            if (_zoom > 0.5f)
            {
                // Draw icons for Sequence, Conditions, Script, & Event:
                if (DoesDialogueEntryHaveSequence(entry))
                {
                    GUI.Label(new Rect((boxRect.x + boxRect.width) - 44, (boxRect.y + boxRect.height) - 15, 16, 16), new GUIContent(sequenceIcon, entry.Sequence));
                }

                if (!string.IsNullOrEmpty(entry.conditionsString))
                {
                    GUI.Label(new Rect((boxRect.x + boxRect.width) - 30, (boxRect.y + boxRect.height) - 15, 16, 16), new GUIContent(conditionsIcon, entry.conditionsString));
                }

                if (!string.IsNullOrEmpty(entry.userScript))
                {
                    GUI.Label(new Rect((boxRect.x + boxRect.width) - 16, (boxRect.y + boxRect.height) - 15, 16, 16), new GUIContent(scriptIcon, entry.userScript));
                }
                if (eventIcon != null && DoesDialogueEntryHaveEvent(entry))
                {
                    GUI.DrawTexture(new Rect((boxRect.x + boxRect.width) - 58, (boxRect.y + boxRect.height) - 15, 16, 16), eventIcon);
                }
            }

            if (showActorPortraits)
            {
                var portrait = GetActorPortrait(entry.ActorID);
                if (portrait != null)
                {
                    GUIDrawSprite(new Rect(boxRect.x - 30, boxRect.y, 30, 30), portrait);
                }
            }

            if (showDescriptions)
            {
                var descriptionGUIContent = GetDialogueEntryNodeDescription(entry);
                if (descriptionGUIContent != null)
                {
                    var height = EditorStyles.textField.CalcHeight(descriptionGUIContent, boxRect.width);
                    GUI.Label(new Rect(boxRect.x, boxRect.y + boxRect.height, boxRect.width, height), descriptionGUIContent, EditorStyles.textField);
                }
            }

            if (customDrawDialogueEntryNode != null)
            {
                customDrawDialogueEntryNode(database, entry, boxRect);
            }
        }

        public void GUIDrawSprite(Rect rect, Sprite sprite)
        {
            Rect spriteRect = sprite.rect;
            Texture2D tex = sprite.texture;
            GUI.DrawTextureWithTexCoords(rect, tex, new Rect(spriteRect.x / tex.width, spriteRect.y / tex.height, spriteRect.width / tex.width, spriteRect.height / tex.height));
        }

        private void DrawQuickDialogueTextEntry()
        {
            if (currentEntry == null) return;
            GUI.SetNextControlName("QuickDialogueText");
            EditorGUI.BeginChangeCheck();
            currentEntry.DialogueText = GUI.TextArea(GetQuickDialogueTextRect(), currentEntry.DialogueText);
            if (EditorGUI.EndChangeCheck())
            {
                dialogueEntryNodeText[currentEntry.id] = BuildDialogueEntryNodeText(currentEntry);
            }
            GUI.FocusControl("QuickDialogueText");
        }

        private void OpenQuickDialogueTextEntry()
        {
            if (!showQuickDialogueTextEntry)
            {
                if (currentEntry == null) return;
                if (multinodeSelection.nodes.Count > 1) return;
                showQuickDialogueTextEntry = true;
                var rect = currentEntry.canvasRect;
                quickDialogueTextEntryRect = new Rect(rect.x, rect.y, rect.width + DialogueEntry.CanvasRectWidth, rect.height + DialogueEntry.CanvasRectHeight);
            }
        }

        private void CloseQuickDialogueTextEntry()
        {
            if (showQuickDialogueTextEntry)
            {
                showQuickDialogueTextEntry = false;
            }
        }

        private bool IsMouseInQuickDialogueTextRect()
        {
            return showQuickDialogueTextEntry && GetQuickDialogueTextRect().Contains(Event.current.mousePosition + canvasScrollPosition);
        }

        private Rect GetQuickDialogueTextRect()
        {
            return quickDialogueTextEntryRect;
        }

        private Sprite GetActorPortrait(int actorID)
        {
            if (actorPortraitCache == null) actorPortraitCache = new Dictionary<int, Sprite>();
            if (!actorPortraitCache.ContainsKey(actorID))
            {
                var actor = database.GetActor(actorID);
                actorPortraitCache.Add(actorID, (actor != null) ? actor.GetPortraitSprite(1) : null);
            }
            return actorPortraitCache[actorID];
        }

        private Color GetNodeColor(DialogueEntry entry)
        {
            if (entry == null) return EditorTools.NodeColor_Gray;
            var actorID = entry.ActorID;
            if (actorCustomColorCache == null) actorCustomColorCache = new Dictionary<int, Color>();
            if (!actorCustomColorCache.ContainsKey(actorID))
            {
                var nodeColor = database.IsPlayerID(actorID) ? EditorTools.NodeColor_Blue : EditorTools.NodeColor_Gray;
                var actor = database.GetActor(actorID);
                if (actor != null && actor.FieldExists(NodeColorFieldTitle))
                {
                    nodeColor = EditorTools.NodeColorStringToColor(actor.LookupValue(NodeColorFieldTitle));
                }
                actorCustomColorCache.Add(actorID, nodeColor);
            }
            return actorCustomColorCache[actorID];
        }

        private void ClearActorInfoCaches()
        {
            actorPortraitCache = null;
            actorCustomColorCache = null;
            actorHasCustomColorCache = null;
            actorIsPlayerCache = null;
        }

        private void CheckOrphanIDs()
        {
            if (currentConversation.id != orphanIDsConversationID)
            {
                orphanIDsConversationID = currentConversation.id;
                orphanIDs.Clear();
                for (int i = 0; i < currentConversation.dialogueEntries.Count; i++)
                {
                    orphanIDs.Add(currentConversation.dialogueEntries[i].id);
                }
                for (int i = 0; i < currentConversation.dialogueEntries.Count; i++)
                {
                    var entry = currentConversation.dialogueEntries[i];
                    for (int j = 0; j < entry.outgoingLinks.Count; j++)
                    {
                        var link = entry.outgoingLinks[j];
                        if (link.originConversationID == link.destinationConversationID)
                        {
                            orphanIDs.Remove(link.destinationDialogueID);
                        }
                    }
                }
            }
        }

        private void ResetOrphanIDs()
        {
            orphanIDsConversationID = -1;
        }

        private bool IsDragCanvasEvent()
        {
            return (Event.current.button == MiddleMouseButton) ||
                ((Event.current.button == LeftMouseButton) && Event.current.alt);
        }

        private bool IsRightMouseButtonEvent()
        {
            // Single button Mac mouse ctrl+click is the same as right click:
            return (Event.current.button == RightMouseButton) ||
                ((Event.current.button == LeftMouseButton) && Event.current.control && !Event.current.alt);
        }

        private void HandleKeyEvents()
        {
            if (Event.current.isKey && Event.current.type == EventType.KeyDown)
            {
                if (Event.current.keyCode == KeyCode.Home && !showQuickDialogueTextEntry)
                {
                    // Home (top of canvas):
                    GotoCanvasHomePosition();
                    Event.current.Use();
                }
                if (Event.current.keyCode == KeyCode.PageUp)
                {
                    // PageUp canvas:
                    PageUpCanvas();
                    Event.current.Use();
                }
                if (Event.current.keyCode == KeyCode.PageDown)
                {
                    // PageDown canvas:
                    PageDownCanvas();
                    Event.current.Use();
                }
                else if (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter)
                {
                    if (showQuickDialogueTextEntry)
                    {
                        CloseQuickDialogueTextEntry();
                    }
                    else if (string.Equals(GUI.GetNameOfFocusedControl(), "SearchTextField"))
                    {
                        // Find next search result if press enter in search text field:
                        SearchDialogueTree(1);
                    }
                }
                else if (Event.current.keyCode == KeyCode.Delete || Event.current.keyCode == KeyCode.Backspace)
                {
                    // Delete/backspace key:
                    if (showQuickDialogueTextEntry)
                    {
                        // Do nothing; using quick Dialogue Text entry.
                    }
                    else if (string.Equals(GUI.GetNameOfFocusedControl(), "SearchTextField"))
                    {
                        // Do nothing; is editing search bar text.
                    }
                    else if (selectedLink != null)
                    {
                        Event.current.Use();
                        DeleteLinkCallback(selectedLink);
                    }
                    else if (currentEntry != null)
                    {
                        Event.current.Use();
                        if (multinodeSelection.nodes.Count > 1)
                        {
                            DeleteMultipleEntriesCallback(currentEntry);
                        }
                        else if (currentEntry != startEntry)
                        {
                            DeleteEntryCallback(currentEntry);
                        }
                    }
                }
                else if (Event.current.keyCode == KeyCode.D && (Event.current.command || Event.current.control) && Event.current.alt)
                {
                    // Ctrl/Cmd+Alt+D (Duplicate) key:
                    if (currentEntry != null && !showQuickDialogueTextEntry)
                    {
                        Event.current.Use();
                        DuplicateMultipleEntries();
                    }
                }
                else if (Event.current.keyCode == KeyCode.N && (Event.current.command || Event.current.control) && Event.current.alt)
                {
                    wasShiftDown = Event.current.shift;
                    // Ctrl/Cmd+Alt+N (New) key:
                    if (currentEntry != null && !showQuickDialogueTextEntry)
                    {
                        AddChildCallback(currentEntry);
                    }
                    else
                    {
                        AddChildCallback(null);
                    }
                    Event.current.Use();
                }
                else if (Event.current.keyCode == KeyCode.C && (Event.current.command || Event.current.control) && Event.current.alt)
                {
                    // Ctrl/Cmd+Alt+C (Copy) key:
                    if (showQuickDialogueTextEntry)
                    {
                        // Do nothing; in quick dialogue text entry.
                    }
                    else if (multinodeSelection.nodes.Count > 1)
                    {
                        CopyMultipleEntriesCallback(null);
                    }
                    else if (currentEntry != null)
                    {
                        CopyEntryCallback(currentEntry);
                    }
                    Event.current.Use();
                }
                else if (Event.current.keyCode == KeyCode.V && (Event.current.command || Event.current.control) && Event.current.alt)
                {
                    // Ctrl/Cmd+Alt+V (Paste) key:
                    if (showQuickDialogueTextEntry)
                    {
                        // Do nothing; in quick dialogue text entry.
                    }
                    if (!IsNodeClipboardEmpty())
                    {
                        PasteMultipleEntriesCallback(currentEntry);
                    }
                    Event.current.Use();
                }
                else if (Event.current.keyCode == KeyCode.Comma && (Event.current.command || Event.current.control) && Event.current.shift)
                {
                    // Ctrl/Cmd+Shift+< key: (previous conversation)
                    OpenPreviousConversation();
                    Event.current.Use();
                }
                else if (Event.current.keyCode == KeyCode.Period && (Event.current.command || Event.current.control) && Event.current.shift)
                {
                    // Ctrl/Cmd+Shift+> key: (next conversation)
                    OpenNextConversation();
                    Event.current.Use();
                }
            }
        }

        private void HandleNodeEvents(DialogueEntry entry)
        {
            if (showFullTextOnHover && entry.canvasRect.Contains(Event.current.mousePosition))
            {
                currentHoveredEntry = entry;
            }
            switch (Event.current.type)
            {
                case EventType.MouseDown:
                    if (!showQuickDialogueTextEntry) GUI.FocusControl("SearchClearButton"); // Deselect search bar text input field.
                    if (showQuickDialogueTextEntry && GetQuickDialogueTextRect().Contains(Event.current.mousePosition))
                    {
                        // Do nothing; clicked in quick dialogue text entry.
                    }
                    else if (entry.canvasRect.Contains(Event.current.mousePosition))
                    {
                        if (IsRightMouseButtonEvent())
                        {
                            currentEntry = entry;
                            ShowNodeContextMenu(entry);
                            Event.current.Use();
                            CloseQuickDialogueTextEntry();
                        }
                        else if (Event.current.button == LeftMouseButton)
                        {
                            CloseQuickDialogueTextEntry();
                            newSelectedLink = null;
                            if (isMakingLink)
                            {
                                FinishMakingLink();
                            }
                            else
                            {
                                nodeToDrag = entry;
                                dragged = false;
                                if (!IsShiftDown() && ((multinodeSelection.nodes.Count <= 1) || !multinodeSelection.nodes.Contains(entry)))
                                {
                                    if (IsAltDown()) // Alt+Click makes link.
                                    {
                                        linkSourceEntry = currentEntry;
                                        linkTargetEntry = entry;
                                        FinishMakingLink();
                                    }
                                    SetCurrentEntry(entry);
                                    if (Event.current.clickCount == 2 && !IsAltDown())
                                    {
                                        OpenQuickDialogueTextEntry();
                                    }
                                }
                            }
                            Event.current.Use();
                        }
                    }
                    break;
                case EventType.MouseUp:
                    if (hasStartedSnapToGrid)
                    {
                        FinishSnapToGrid();
                        hasStartedSnapToGrid = false;
                    }
                    if (Event.current.button == LeftMouseButton)
                    {
                        if (!isMakingLink && entry.canvasRect.Contains(Event.current.mousePosition))
                        {
                            newSelectedLink = null;
                            if (isLassoing)
                            {
                                FinishLasso();
                                CloseQuickDialogueTextEntry();
                            }
                            else if (IsShiftDown())
                            {
                                if (multinodeSelection.nodes.Contains(entry))
                                {
                                    RemoveEntryFromSelection(entry);
                                }
                                else
                                {
                                    AddEntryToSelection(entry);
                                }
                                CloseQuickDialogueTextEntry();
                            }
                            else if (showQuickDialogueTextEntry && GetQuickDialogueTextRect().Contains(Event.current.mousePosition))
                            {
                                return;
                            }
                            else
                            {
                                if (!(dragged && (multinodeSelection.nodes.Count > 1)))
                                {
                                    SetCurrentEntry(entry);
                                }
                            }
                            nodeToDrag = null;
                            dragged = false;
                            Event.current.Use();
                        }
                    }
                    break;
                case EventType.MouseDrag:
                    if ((entry == nodeToDrag))
                    {
                        dragged = true;
                        DragMultiSelection();
                        Event.current.Use();
                    }
                    break;
            }
            if (isMakingLink && Event.current.isMouse)
            {
                if (entry.canvasRect.Contains(Event.current.mousePosition))
                {
                    linkTargetEntry = entry;
                }
            }
        }

        private void DragMultiSelection()
        {
            var snapToGrid = snapToGridAmount >= MinorGridLineWidth;
            for (int i = 0; i < multinodeSelection.nodes.Count; i++)
            {
                var dragEntry = multinodeSelection.nodes[i];

                if (snapToGrid && dragEntry == nodeToDrag)
                {
                    dragEntry.canvasRect.x = ((int)((Event.current.mousePosition.x - dragEntry.canvasRect.width / 2) / snapToGridAmount) * snapToGridAmount);
                    dragEntry.canvasRect.y = ((int)((Event.current.mousePosition.y - dragEntry.canvasRect.height / 2) / snapToGridAmount) * snapToGridAmount);
                }
                else if (snapToGrid && !hasStartedSnapToGrid)
                {
                    dragEntry.canvasRect.x = (((int)dragEntry.canvasRect.x) / snapToGridAmount) * snapToGridAmount;
                    dragEntry.canvasRect.y = (((int)dragEntry.canvasRect.y) / snapToGridAmount) * snapToGridAmount;
                }
                else
                {
                    dragEntry.canvasRect.x += Event.current.delta.x;
                    dragEntry.canvasRect.y += Event.current.delta.y;
                }
                dragEntry.canvasRect.x = Mathf.Max(1f, dragEntry.canvasRect.x);
                dragEntry.canvasRect.y = Mathf.Max(1f, dragEntry.canvasRect.y);
            }
            for (int i = 0; i < multinodeSelection.groups.Count; i++)
            {
                var group = multinodeSelection.groups[i];

                if (snapToGrid)
                {
                    group.rect.x = (((int)group.rect.x) / snapToGridAmount) * snapToGridAmount;
                    group.rect.y = (((int)group.rect.y) / snapToGridAmount) * snapToGridAmount;
                }
                else
                {
                    group.rect.x += Event.current.delta.x;
                    group.rect.y += Event.current.delta.y;
                }
                group.rect.x = Mathf.Max(1f, group.rect.x);
                group.rect.y = Mathf.Max(1f, group.rect.y);
            }
            hasStartedSnapToGrid = true;
            SetDatabaseDirty("Drag");
        }

        private void FinishSnapToGrid()
        {
            var snapToGrid = snapToGridAmount >= MinorGridLineWidth;
            if (!snapToGrid) return;
            for (int i = 0; i < multinodeSelection.nodes.Count; i++)
            {
                var entry = multinodeSelection.nodes[i];
                var canvasRect = entry.canvasRect;
                entry.canvasRect.x = ((int)(canvasRect.x / snapToGridAmount) * snapToGridAmount);
                entry.canvasRect.y = ((int)(canvasRect.y / snapToGridAmount) * snapToGridAmount);
            }
            for (int i = 0; i < multinodeSelection.groups.Count; i++)
            {
                var group = multinodeSelection.groups[i];
                var canvasRect = group.rect;
                group.rect.x = ((int)(canvasRect.x / snapToGridAmount) * snapToGridAmount);
                group.rect.y = ((int)(canvasRect.y / snapToGridAmount) * snapToGridAmount);
            }
            hasStartedSnapToGrid = false;
            SetDatabaseDirty("Drag End");
        }

        private bool IsModifierDown(EventModifiers modifier)
        {
            return (Event.current.modifiers & modifier) == modifier;
        }

        private bool IsShiftDown()
        {
            return IsModifierDown(EventModifiers.Shift);
        }

        private bool IsControlDown()
        {
            return IsModifierDown(EventModifiers.Control);
        }

        private bool IsAltDown()
        {
            return IsModifierDown(EventModifiers.Alt);
        }

        private void SetCurrentEntry(DialogueEntry entry)
        {
            if (entry != null && currentConversation != null && entry.conversationID != currentConversation.id)
            {
                var conversation = database.GetConversation(entry.conversationID);
                OpenConversation(conversation);
                SetConversationDropdownIndex(GetCurrentConversationIndex());
                InitializeDialogueTree();
                inspectorSelection = entry;
            }
            newSelectedLink = null;
            if (entry != currentEntry) ResetLuaWizards();
            currentEntry = entry;
            multinodeSelection.nodes.Clear();
            multinodeSelection.nodes.Add(entry);
            UpdateEntrySelection();
        }

        private void CenterOnCurrentEntry()
        {
            if (currentEntry == null) return;
            var rect = currentEntry.canvasRect;
            var x = rect.x + (rect.width / 2) - (position.width / 2);
            var y = rect.y + (rect.height / 2) - (position.height / 2);
            x = Mathf.Clamp(x, 0, CanvasSize - position.width);
            y = Mathf.Clamp(y, 0, CanvasSize - position.height);
            canvasScrollPosition = new Vector2(x, y);
        }

        private void AddEntryToSelection(DialogueEntry entry)
        {
            newSelectedLink = null;
            currentEntry = entry;
            multinodeSelection.nodes.Add(entry);
            UpdateEntrySelection();
        }

        private void RemoveEntryFromSelection(DialogueEntry entry)
        {
            newSelectedLink = null;
            multinodeSelection.nodes.Remove(entry);
            if (multinodeSelection.nodes.Count == 0)
            {
                currentEntry = null;
            }
            else
            {
                currentEntry = multinodeSelection.nodes[multinodeSelection.nodes.Count - 1];
            }
            UpdateEntrySelection();
        }

        private void UpdateEntrySelection()
        {
            ResetDialogueTreeCurrentEntryParticipants();
            if (multinodeSelection.nodes.Count == 0)
            {
                inspectorSelection = currentConversation;
                selectedLink = null;
            }
            else if (multinodeSelection.nodes.Count == 1)
            {
                inspectorSelection = currentEntry;
            }
            else
            {
                inspectorSelection = multinodeSelection;
            }
        }

        private void HandleEmptyCanvasEvents() // Also handles entry group events.
        {
            wantsMouseMove = true;
            Event e = Event.current;
            switch (Event.current.type)
            {
                case EventType.MouseDown:
                    GUI.FocusControl("SearchClearButton"); // Deselect search bar text input field.
                    isDraggingCanvas = IsDragCanvasEvent();
                    if (isDraggingCanvas) nodeToDrag = null;
                    CheckClickOnEntryGroup();
                    if (isMakingLink)
                    {
                        if ((Event.current.button == LeftMouseButton) || (Event.current.button == RightMouseButton))
                        {
                            FinishMakingLink();
                        }
                    }
                    else if (IsRightMouseButtonEvent())
                    {
                        if (selectedLink != null)
                        {
                            ShowLinkContextMenu();
                        }
                        else
                        {
                            if (currentConversation != null)
                            {
                                ShowEmptyCanvasContextMenu();
                            }
                        }
                        CloseQuickDialogueTextEntry();
                    }
                    else if (Event.current.button == LeftMouseButton)
                    {
                        nodeToDrag = null;
                        if (selectedEntryGroup == null)
                        {
                            isLassoing = true;
                            lassoRect = new Rect(Event.current.mousePosition.x + canvasScrollPosition.x, Event.current.mousePosition.y + canvasScrollPosition.y, 1, 1);
                        }
                    }
                    break;
                case EventType.MouseUp:
                    isDraggingCanvas = false;
                    if (!IsMouseOnSelectedEntryGroupHeading())
                    {
                        ClearSelectedEntryGroup();
                    }
                    if (isLassoing)
                    {
                        FinishLasso();
                        newSelectedLink = null;
                    }
                    else if (newSelectedLink == null && Event.current.button != MiddleMouseButton && selectedEntryGroup == null &&
                        ((currentEntry == null) || !IsMouseInQuickDialogueTextRect()))
                    {
                        InspectConversationProperties();
                    }
                    else if (selectedEntryGroup != null && !IsMouseOnSelectedEntryGroupHeading())
                    {
                        ClearSelectedEntryGroup();
                    }
                    break;
                case EventType.MouseDrag:
                    if (isDraggingCanvas)
                    {
                        canvasScrollPosition -= Event.current.delta;
                        canvasScrollPosition.x = Mathf.Clamp(canvasScrollPosition.x, 0, Mathf.Infinity);
                        canvasScrollPosition.y = Mathf.Clamp(canvasScrollPosition.y, 0, Mathf.Infinity);
                    }
                    else if (selectedEntryGroup != null)
                    {
                        if (isResizingEntryGroup)
                        {
                            ResizeSelectedEntryGroup();
                        }
                        else
                        {
                            DragSelectedEntryGroup();
                        }
                    }
                    else if (isLassoing)
                    {
                        lassoRect.width += Event.current.delta.x;
                        lassoRect.height += Event.current.delta.y;
                    }
                    break;
            }
            if (Event.current.isMouse) e.Use();
        }

        private void CheckClickOnEntryGroup()
        {
            if (currentConversation == null) return;
            var clickPos = Event.current.mousePosition + canvasScrollPosition;
            for (int i = 0; i < currentConversation.entryGroups.Count; i++)
            {
                var group = currentConversation.entryGroups[i];
                var groupHeadingRect = new Rect(group.rect.x, group.rect.y, group.rect.width, EntryGroupHeadingHeight);
                var resizeRect = new Rect(group.rect.x + group.rect.width - 20, group.rect.y + group.rect.height - 20, 20, 20);
                if (groupHeadingRect.Contains(clickPos) || resizeRect.Contains(clickPos))
                {
                    if (selectedEntryGroup != group || nodesInEntryGroup == null || subgroupsInEntryGroup == null)
                    {
                        selectedEntryGroup = group;
                        inspectorSelection = group;
                        nodesInEntryGroup = new List<DialogueEntry>();
                        isResizingEntryGroup = resizeRect.Contains(clickPos);
                        foreach (var entry in currentConversation.dialogueEntries)
                        {
                            if (group.rect.Contains(entry.canvasRect.TopLeft()) && 
                                group.rect.Contains(entry.canvasRect.BottomRight()))
                            {
                                nodesInEntryGroup.Add(entry);
                            }
                        }
                        subgroupsInEntryGroup = new List<EntryGroup>();
                        foreach (var otherGroup in currentConversation.entryGroups)
                        {
                            if (otherGroup == group) continue;
                            if (group.rect.Contains(otherGroup.rect.TopLeft()) &&
                                group.rect.Contains(otherGroup.rect.BottomRight()))
                            {
                                subgroupsInEntryGroup.Add(otherGroup);
                            }
                        }
                        return;
                    }
                }
            }
            ClearSelectedEntryGroup();
        }

        private bool IsMouseOnSelectedEntryGroupHeading()
        {
            if (currentConversation == null || selectedEntryGroup == null) return false;
            var clickPos = Event.current.mousePosition + canvasScrollPosition;
            var group = selectedEntryGroup;
            var groupHeadingRect = new Rect(group.rect.x, group.rect.y, group.rect.width, EntryGroupHeadingHeight);
            return groupHeadingRect.Contains(clickPos);
        }

        private void ClearSelectedEntryGroup()
        {
            selectedEntryGroup = null;
            nodesInEntryGroup = null;
            subgroupsInEntryGroup = null;
            isRenamingEntryGroup = false;
            isResizingEntryGroup = false;
        }

        private void DragSelectedEntryGroup()
        {
            if (selectedEntryGroup == null) return;
            selectedEntryGroup.rect.x += Event.current.delta.x;
            selectedEntryGroup.rect.y += Event.current.delta.y;
            foreach (var entry in nodesInEntryGroup)
            {
                entry.canvasRect.x += Event.current.delta.x;
                entry.canvasRect.y += Event.current.delta.y;
            }
            foreach (var subgroup in subgroupsInEntryGroup)
            {
                subgroup.rect.x += Event.current.delta.x;
                subgroup.rect.y += Event.current.delta.y;
            }
        }

        private void ResizeSelectedEntryGroup()
        {
            if (selectedEntryGroup == null) return;
            selectedEntryGroup.rect.width += Event.current.delta.x;
            selectedEntryGroup.rect.height += Event.current.delta.y;
        }

        private void InspectConversationProperties()
        {
            currentEntry = null;
            selectedLink = null;
            multinodeSelection.nodes.Clear();
            inspectorSelection = currentConversation;
        }

        private void DrawLasso()
        {
            if (isLassoing)
            {
                EditorGUI.DrawRect(lassoRect, new Color(1, 1, 1, 0.3f));
            }
        }

        private void FinishLasso()
        {
            if (currentConversation == null) return;
            isLassoing = false;
            lassoRect = new Rect(Mathf.Min(lassoRect.x, lassoRect.x + lassoRect.width),
                                 Mathf.Min(lassoRect.y, lassoRect.y + lassoRect.height),
                                 Mathf.Abs(lassoRect.width),
                                 Mathf.Abs(lassoRect.height));
            currentEntry = null;
            if (IsControlDown())
            {
                CreateEntryGroup(lassoRect);
            }
            else
            {
                if (!IsShiftDown()) multinodeSelection.Clear();
                for (int i = 0; i < currentConversation.dialogueEntries.Count; i++)
                {
                    var entry = currentConversation.dialogueEntries[i];
                    if (lassoRect.Overlaps(entry.canvasRect))
                    {
                        currentEntry = entry;
                        if (!multinodeSelection.nodes.Contains(entry)) multinodeSelection.nodes.Add(entry);
                    }
                }
                for (int i = 0; i < currentConversation.entryGroups.Count; i++)
                {
                    var group = currentConversation.entryGroups[i];
                    if (lassoRect.Contains(group.rect.TopLeft()) && lassoRect.Contains(group.rect.BottomRight()))
                    {
                        if (!multinodeSelection.groups.Contains(group)) multinodeSelection.groups.Add(group);
                    }
                }
            }
            UpdateEntrySelection();
        }

        private void CreateEntryGroup(Rect rect)
        {
            var entryGroup = new EntryGroup("Group", rect);
            currentConversation.entryGroups.Add(entryGroup);
            SetDatabaseDirty("Group");
        }

        private void RenameEntryGroup(object o)
        {
            if (currentConversation == null || selectedEntryGroup == null) return;
            isRenamingEntryGroup = true;
        }

        private void DeleteEntryGroup(object o)
        {
            if (currentConversation == null || selectedEntryGroup == null) return;
            currentConversation.entryGroups.Remove(selectedEntryGroup);
            ClearSelectedEntryGroup();
            SetDatabaseDirty("Delete Group");
        }

        private void ShowEmptyCanvasContextMenu()
        {
            EditorZoomArea.End();
            wasShiftDown = Event.current.shift;

            GenericMenu contextMenu = new GenericMenu();
            contextMenu.AddItem(new GUIContent("Create Node"), false, AddChildCallback, null);
            contextMenu.AddItem(new GUIContent("Arrange Nodes..."), false, ArrangeNodesCallback, null);
            contextMenu.AddItem(new GUIContent("Snap All Nodes to Grid"), false, SnapAllNodesToGrid);
            if (IsNodeClipboardEmpty())
            {
                contextMenu.AddDisabledItem(new GUIContent("Paste Nodes"));
            }
            else
            {
                contextMenu.AddItem(new GUIContent("Paste Nodes"), false, PasteMultipleEntriesCallback, null);
            }
            contextMenu.AddItem(new GUIContent("Duplicate Conversation"), false, CopyConversationCallback, null);
            contextMenu.AddItem(new GUIContent("Delete Conversation"), false, DeleteConversationCallback, null);

            if (selectedEntryGroup != null)
            {
                contextMenu.AddSeparator(string.Empty);
                contextMenu.AddItem(new GUIContent("Group/Rename"), false, RenameEntryGroup, null);
                contextMenu.AddItem(new GUIContent("Group/Delete"), false, DeleteEntryGroup, null);
            }

            AddCanvasContextMenuGotoItems(contextMenu);

            contextMenu.ShowAsContext();
            contextMenuPosition = Event.current.mousePosition;

            EditorZoomArea.Begin(_zoom, _zoomArea);
        }

        private void ShowLinkContextMenu()
        {
            EditorZoomArea.End();

            GenericMenu contextMenu = new GenericMenu();
            contextMenu.AddItem(new GUIContent("Delete Link"), false, DeleteLinkCallback, selectedLink);
            contextMenu.AddItem(new GUIContent("Arrange Nodes..."), false, ArrangeNodesCallback, null);

            AddCanvasContextMenuGotoItems(contextMenu);

            contextMenu.ShowAsContext();
            contextMenuPosition = Event.current.mousePosition;

            EditorZoomArea.Begin(_zoom, _zoomArea);
        }

        private void ShowNodeContextMenu(DialogueEntry entry)
        {
            EditorZoomArea.End();
            wasShiftDown = Event.current.shift;

            GenericMenu contextMenu = new GenericMenu();
            contextMenu.AddItem(new GUIContent("Create Child Node"), false, AddChildCallback, entry);
            contextMenu.AddItem(new GUIContent("Make Link"), false, MakeLinkCallback, entry);
            if ((multinodeSelection.nodes.Count > 1) && (multinodeSelection.nodes.Contains(entry)))
            {
                contextMenu.AddItem(new GUIContent("Copy"), false, CopyMultipleEntriesCallback, entry);
                if (IsNodeClipboardEmpty())
                {
                    contextMenu.AddDisabledItem(new GUIContent("Paste"));
                }
                else
                {
                    contextMenu.AddItem(new GUIContent("Paste"), false, PasteMultipleEntriesCallback, entry);
                }
                contextMenu.AddItem(new GUIContent("Delete"), false, DeleteMultipleEntriesCallback, entry);
            }
            else if (entry == startEntry)
            {
                contextMenu.AddDisabledItem(new GUIContent("Copy"));
                if (IsNodeClipboardEmpty())
                {
                    contextMenu.AddDisabledItem(new GUIContent("Paste"));
                }
                else
                {
                    contextMenu.AddItem(new GUIContent("Paste"), false, PasteEntryCallback, entry);
                }
                contextMenu.AddDisabledItem(new GUIContent("Delete"));
            }
            else
            {
                contextMenu.AddItem(new GUIContent("Copy"), false, CopyEntryCallback, entry);
                if (IsNodeClipboardEmpty())
                {
                    contextMenu.AddDisabledItem(new GUIContent("Paste"));
                }
                else
                {
                    contextMenu.AddItem(new GUIContent("Paste"), false, PasteEntryCallback, entry);
                }
                contextMenu.AddItem(new GUIContent("Delete"), false, DeleteEntryCallback, entry);
            }
            contextMenu.AddItem(new GUIContent("Arrange Nodes..."), false, ArrangeNodesCallback, entry);
            contextMenu.AddItem(new GUIContent("Snap All Nodes to Grid"), false, SnapAllNodesToGrid);

            AddCanvasContextMenuGotoItems(contextMenu);

            contextMenu.ShowAsContext();
            contextMenuPosition = Event.current.mousePosition;

            EditorZoomArea.Begin(_zoom, _zoomArea);
        }

        private void AddCanvasContextMenuGotoItems(GenericMenu contextMenu)
        {
            contextMenu.AddSeparator(string.Empty);
            contextMenu.AddItem(new GUIContent("Home Position"), false, GotoCanvasHomePosition);
            if (currentConversation != null)
            {
                contextMenu.AddItem(new GUIContent("Center on START"), false, GotoStartNodePosition);
                if (IsConversationActive())
                {
                    contextMenu.AddItem(new GUIContent("Center on Current Entry"), false, GotoCurrentRuntimeEntry);
                }
            }
            else
            {
                contextMenu.AddDisabledItem(new GUIContent("Center on START"));
            }
        }

        private void AddChildCallback(object o)
        {
            DialogueEntry parentEntry = o as DialogueEntry;
            if (parentEntry == null) parentEntry = startEntry;
            LinkToNewEntry(parentEntry, wasShiftDown);
            wasShiftDown = false;
            InitializeDialogueTree();
            if (addNewNodesToRight)
            {
                currentEntry.canvasRect.x = parentEntry.canvasRect.x + parentEntry.canvasRect.width + AutoWidthBetweenNodes;
                currentEntry.canvasRect.y = parentEntry.canvasRect.y;
            }
            else
            {
                currentEntry.canvasRect.x = parentEntry.canvasRect.x;
                currentEntry.canvasRect.y = parentEntry.canvasRect.y + parentEntry.canvasRect.height + AutoHeightBetweenNodes;
            }
            SetCurrentEntry(currentEntry);
            inspectorSelection = currentEntry;
            ResetDialogueEntryText();
            if (autoArrangeOnCreate) AutoArrangeNodes(!addNewNodesToRight);
            Repaint();
        }

        private void MakeLinkCallback(object o)
        {
            linkSourceEntry = o as DialogueEntry;
            isMakingLink = (linkSourceEntry != null);
        }

        private bool LinkExists(DialogueEntry origin, DialogueEntry destination)
        {
            Link link = origin.outgoingLinks.Find(x => ((x.destinationConversationID == destination.conversationID) && (x.destinationDialogueID == destination.id)));
            return (link != null);
        }

        private void FinishMakingLink()
        {
            if ((linkSourceEntry != null) && (linkTargetEntry != null) &&
                (linkSourceEntry != linkTargetEntry) &&
                !LinkExists(linkSourceEntry, linkTargetEntry))
            {
                Link link = new Link();
                link.originConversationID = currentConversation.id;
                link.originDialogueID = linkSourceEntry.id;
                link.destinationConversationID = currentConversation.id;
                link.destinationDialogueID = linkTargetEntry.id;
                linkSourceEntry.outgoingLinks.Add(link);
                InitializeDialogueTree();
                ResetDialogueEntryText();
                Repaint();
            }
            isMakingLink = false;
            linkSourceEntry = null;
            linkTargetEntry = null;
            SetDatabaseDirty("Make Link");
        }

        private void DeleteEntryCallback(object o)
        {
            DialogueEntry entryToDelete = o as DialogueEntry;
            if (entryToDelete == null) return;
            if (!confirmDelete || EditorUtility.DisplayDialog("Delete selected entry?", "You cannot undo this action.", "Delete", "Cancel"))
            {
                foreach (var origin in currentConversation.dialogueEntries)
                {
                    DeleteNodeLinkToDialogueID(origin, entryToDelete.conversationID, entryToDelete.id);
                }
                DialogueEntry entry = currentConversation.dialogueEntries.Find(x => x.id == entryToDelete.id);
                currentConversation.dialogueEntries.Remove(entry);
                InitializeDialogueTree();
                ResetDialogueEntryText();
                Repaint();
                SetDatabaseDirty("Delete Dialogue Entry");
            }
        }

        private void DeleteMultipleEntriesCallback(object o)
        {
            if (!confirmDelete || EditorUtility.DisplayDialog("Delete selected entries?", "You cannot undo this action.", "Delete", "Cancel"))
            {
                foreach (DialogueEntry entryToDelete in multinodeSelection.nodes)
                {
                    if (entryToDelete != startEntry)
                    {
                        foreach (var origin in currentConversation.dialogueEntries)
                        {
                            DeleteNodeLinkToDialogueID(origin, entryToDelete.conversationID, entryToDelete.id);
                        }
                        DialogueEntry entry = currentConversation.dialogueEntries.Find(x => x.id == entryToDelete.id);
                        currentConversation.dialogueEntries.Remove(entry);
                    }
                }
                InitializeDialogueTree();
                ResetDialogueEntryText();
                Repaint();
                SetDatabaseDirty("Delete Dialogue Entries");
            }
        }

        private void DeleteLinkCallback(object o)
        {
            Link linkToDelete = o as Link;
            if (linkToDelete == null) return;
            if (!confirmDelete || EditorUtility.DisplayDialog("Delete selected link?", "You cannot undo this action.", "Delete", "Cancel"))
            {
                DialogueEntry origin = currentConversation.GetDialogueEntry(linkToDelete.originDialogueID);
                DeleteNodeLinkToDialogueID(origin, linkToDelete.destinationConversationID, linkToDelete.destinationDialogueID);
                InitializeDialogueTree();
                ResetDialogueEntryText();
                Repaint();
                SetDatabaseDirty("Delete Link");
            }
        }

        private void DeleteNodeLinkToDialogueID(DialogueEntry origin, int destinationConversationID, int destinationDialogueID)
        {
            if (origin == null) return;
            Link link = origin.outgoingLinks.Find(x => (x.destinationConversationID == destinationConversationID) && (x.destinationDialogueID == destinationDialogueID));
            if (link == null) return;
            origin.outgoingLinks.Remove(link);
            SetDatabaseDirty("Delete Node Link");
        }

        private void ArrangeNodesCallback(object o)
        {
            ConfirmAndAutoArrangeNodes();
        }

        private void DeleteConversationCallback(object o)
        {
            if (currentConversation == null) return;
            if (EditorUtility.DisplayDialog(string.Format("Delete '{0}'?", currentConversation.Title),
                "Are you sure you want to delete this conversation?\nYou cannot undo this operation!", "Delete", "Cancel"))
            {
                nodeEditorDeleteCurrentConversation = true;
            }
        }

        private void DeleteCurrentConversationInNodeEditor()
        {
            nodeEditorDeleteCurrentConversation = false;
            if (currentConversation != null) database.conversations.Remove(database.conversations.Find(c => c.id == currentConversation.id));
            ResetConversationSection();
            ActivateNodeEditorMode();
            inspectorSelection = database;
            SetDatabaseDirty("Delete Conversation");
        }

        private void CopyConversationCallback(object o)
        {
            if (currentConversation == null) return;
            if (EditorUtility.DisplayDialog(string.Format("Copy '{0}'?", currentConversation.Title), "Make a copy of this conversation?", "Copy", "Cancel"))
            {
                CopyConversation();
            }
        }

        private void CopyConversation()
        {
            int oldID = currentConversation.id;
            int newID = GetAvailableConversationID();
            var copyTitle = GetAvailableCopyTitle();
            var copy = new Conversation(currentConversation);
            copy.id = newID;
            copy.Title = copyTitle;
            foreach (var entry in copy.dialogueEntries)
            {
                entry.conversationID = newID;
                foreach (var link in entry.outgoingLinks)
                {
                    if (link.originConversationID == oldID) link.originConversationID = newID;
                    if (link.destinationConversationID == oldID) link.destinationConversationID = newID;
                }
            }
            database.conversations.Add(copy);
            SetCurrentConversation(copy);
            if (showNodeEditor) ActivateNodeEditorMode();
            SetDatabaseDirty("Copy Conversation");
        }

        private int GetAvailableConversationID()
        {
            int highestID = database.baseID - 1;
            database.conversations.ForEach(a => highestID = Mathf.Max(highestID, a.id));
            return Mathf.Max(1, highestID + 1);
        }

        private string GetAvailableCopyTitle()
        {
            string copyTitle = currentConversation.Title + " Copy";
            int count = 0;
            while ((database.GetConversation(copyTitle) != null) && (count < 1000))
            {
                count++;
                copyTitle = currentConversation.Title + " Copy " + count;
            }
            return copyTitle;
        }

        private bool IsNodeClipboardEmpty()
        {
            return nodeClipboard == null || nodeClipboard.Count == 0;
        }

        private void CopyEntryCallback(object o)
        {
            nodeClipboard = new List<DialogueEntry>();
            nodeClipboard.Add(DuplicateEntryForClipboard(currentEntry));
            RemoveOutgoingLinksFromClipboard();
        }

        private void CopyMultipleEntriesCallback(object o)
        {
            nodeClipboard = new List<DialogueEntry>();
            foreach (var entry in multinodeSelection.nodes)
            {
                nodeClipboard.Add(DuplicateEntryForClipboard(entry));
            }
            RemoveOutgoingLinksFromClipboard();
        }

        private void RemoveOutgoingLinksFromClipboard()
        {
            if (nodeClipboard == null) return;
            var clipboardIDs = new List<int>();
            foreach (var node in nodeClipboard)
            {
                clipboardIDs.Add(node.id);
            }
            foreach (var node in nodeClipboard)
            {
                node.outgoingLinks.RemoveAll(x => !clipboardIDs.Contains(x.destinationDialogueID));
            }
        }

        private void PasteEntryCallback(object o)
        {
            PasteClipboardNodes(o as DialogueEntry);
        }

        private void PasteMultipleEntriesCallback(object o)
        {
            PasteClipboardNodes(o as DialogueEntry);
        }

        private DialogueEntry DuplicateEntryForClipboard(DialogueEntry entry)
        {
            if (entry == null || currentConversation == null) return null;
            var newEntry = new DialogueEntry(entry);
            ApplyDialogueEntryTemplate(newEntry.fields);
            return newEntry;
        }

        private void PasteClipboardNodes(DialogueEntry originEntry)
        {
            if (nodeClipboard == null || nodeClipboard.Count == 0) return;

            // Position:
            var xMin = nodeClipboard[0].canvasRect.xMin;
            var yMin = nodeClipboard[0].canvasRect.yMin;
            var xMax = nodeClipboard[0].canvasRect.xMax;
            foreach (var node in nodeClipboard)
            {
                xMin = Mathf.Min(xMin, node.canvasRect.xMin);
                yMin = Mathf.Min(yMin, node.canvasRect.yMin);
                xMax = Mathf.Max(xMax, node.canvasRect.xMax);
            }

            var topOffset = GetTopOffsetHeight() / _zoom;
            yMin += topOffset; // Account for menu stuff at top of window.

            var width = xMax - xMin;
            var xDelta = Mathf.Max(0, (contextMenuPosition.x / _zoom) - (width / 2)) - xMin;
            var yDelta = (contextMenuPosition.y / _zoom) - yMin;
            xDelta += canvasScrollPosition.x;
            yDelta += canvasScrollPosition.y;

            // Copy nodes to new entries:
            var newEntries = new List<DialogueEntry>();
            foreach (var node in nodeClipboard)
            {
                var newEntry = new DialogueEntry(node);
                newEntries.Add(newEntry);
                newEntry.conversationID = currentConversationID;
                currentConversation.dialogueEntries.Add(newEntry);
            }

            // Fix up new entries:
            foreach (var newEntry in newEntries)
            {
                // Assign a new ID:
                var oldID = newEntry.id;
                var newID = GetNextDialogueEntryID();
                newEntry.id = newID;

                // Make sure all new entries point to the new ID:
                foreach (var tempEntry in newEntries)
                {
                    foreach (var link in tempEntry.outgoingLinks)
                    {
                        link.originConversationID = currentConversationID;
                        link.destinationConversationID = currentConversationID;
                        if (link.originDialogueID == oldID) link.originDialogueID = newID;
                        if (link.destinationDialogueID == oldID) link.destinationDialogueID = newID;
                    }
                }

                // Handle scene event:
                DuplicateEntrySceneEventIfPresent(newEntry);

                // Position and select:
                newEntry.canvasRect.x = newEntry.canvasRect.x + xDelta;
                newEntry.canvasRect.y = newEntry.canvasRect.y + yDelta;
                currentEntry = newEntry;
                inspectorSelection = currentEntry;
            }

            // Select and repaint:
            multinodeSelection.nodes = new List<DialogueEntry>(newEntries);
            InitializeDialogueTree();
            ResetDialogueEntryText();
            Repaint();
            SetDatabaseDirty("Paste Nodes");
        }

        private void DuplicateMultipleEntries()
        {
            foreach (var entry in multinodeSelection.nodes)
            {
                DuplicateEntry(entry);
            }
        }

        private void DuplicateEntry(DialogueEntry entry)
        {
            if (entry == null || currentConversation == null) return;
            DialogueEntry newEntry = new DialogueEntry(entry);
            newEntry.id = GetNextDialogueEntryID();
            foreach (var link in newEntry.outgoingLinks)
            {
                link.originDialogueID = newEntry.id;
            }
            currentConversation.dialogueEntries.Add(newEntry);
            newEntry.canvasRect.x = entry.canvasRect.x + 10f;
            newEntry.canvasRect.y = entry.canvasRect.y + entry.canvasRect.height + AutoHeightBetweenNodes;
            ApplyDialogueEntryTemplate(newEntry.fields);
            DuplicateEntrySceneEventIfPresent(newEntry);
            currentEntry = newEntry;
            inspectorSelection = currentEntry;
            InitializeDialogueTree();
            ResetDialogueEntryText();
            Repaint();
            SetDatabaseDirty("Duplicate Dialogue Entry");
        }

        private void DuplicateEntrySceneEventIfPresent(DialogueEntry entry)
        {
            var sceneEventGuidField = Field.Lookup(entry.fields, DialogueEntry.SceneEventGuidFieldName);

            // If no scene event, we can end here:
            if (sceneEventGuidField == null) return;
            if (string.IsNullOrEmpty(sceneEventGuidField.value)) return;

            var sceneEventIndex = DialogueSystemSceneEvents.GetDialogueEntrySceneEventIndex(sceneEventGuidField.value);
            if (sceneEventIndex == -1)
            {
                // If wrong scene, remove scene event:
                entry.fields.Remove(sceneEventGuidField);
            }
            else
            {
                // For now, just add empty event block:
                //var sourceEvent = DialogueSystemSceneEvents.GetDialogueEntrySceneEvent(sceneEventGuidField.value);
                string newGuid;
                DialogueSystemSceneEvents.AddNewDialogueEntrySceneEvent(out newGuid);
                //var destEvent = DialogueSystemSceneEvents.GetDialogueEntrySceneEvent(newGuid);
                sceneEventGuidField.value = newGuid;

                ////--- TO DO: Copy event (Need to resolve duplication issues.)
                //var numEvents = sourceEvent.onExecute.GetPersistentEventCount();
                //for (int i = 0; i < numEvents; i++)
                //{
                //    try
                //    {
                //        var target = sourceEvent.onExecute.GetPersistentTarget(i);
                //        var methodName = sourceEvent.onExecute.GetPersistentMethodName(i);
                //        // etc.
                //    }
                //    catch (System.Exception e)
                //    {
                //        Debug.LogException(e);
                //    }
                //}
            }
        }

        private void SplitPipesIntoEntries(object data)
        {
            currentConversation.SplitPipesIntoEntries(true, trimWhitespaceAroundPipes);
            InitializeDialogueTree();
            ResetDialogueEntryText();
            Repaint();
            SetDatabaseDirty("Split Pipes");
        }

        private void SnapAllNodesToGrid()
        {
            if (snapToGridAmount < MinorGridLineWidth) return;
            for (int i = 0; i < currentConversation.dialogueEntries.Count; i++)
            {
                var entry = currentConversation.dialogueEntries[i];
                var canvasRect = entry.canvasRect;
                entry.canvasRect.x = ((int)(canvasRect.x / snapToGridAmount) * snapToGridAmount);
                entry.canvasRect.y = ((int)(canvasRect.y / snapToGridAmount) * snapToGridAmount);
            }
            SetDatabaseDirty("Snap All To Grid");
        }

        private void MoveToEntry(DialogueEntry entry)
        {
            var x = entry.canvasRect.x + (entry.canvasRect.width / 2) - ((position.width / _zoom) / 2);
            var y = entry.canvasRect.y - (entry.canvasRect.height / _zoom);
            canvasScrollPosition = new Vector2(Mathf.Max(0, x), Mathf.Max(0, y));
            Repaint();
        }

    }

}