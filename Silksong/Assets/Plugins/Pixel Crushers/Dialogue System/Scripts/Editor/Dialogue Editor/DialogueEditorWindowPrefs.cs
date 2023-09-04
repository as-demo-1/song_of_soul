// Copyright (c) Pixel Crushers. All rights reserved.

using System;
using UnityEngine;
using UnityEditor;

namespace PixelCrushers.DialogueSystem.DialogueEditor
{

    [Serializable]
    public class DialogueEditorPrefs
    {
        public bool preferTitlesForLinksTo = false;
        public bool showNodeIDs = false;
        public bool showTitlesInsteadOfText = false;
        public bool showAllActorNames = false;
        public bool showOtherActorNames = true;
        public bool showActorPortraits = false;
        public bool showDescriptions = false;
        public bool showParticipantNames = true;
        public bool showEndNodeMarkers = true;
        public bool showFullTextOnHover = true;
        public bool showLinkOrderOnConnectors = true;
        public bool addNewNodesToRight = false;
        public bool autoArrangeOnCreate = false;
        public float snapToGridAmount = 0;
    }

}