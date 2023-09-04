#if USE_TWINE
// Copyright (c) Pixel Crushers. All rights reserved.

using System;
using UnityEngine;

namespace PixelCrushers.DialogueSystem.Twine
{
    /// <summary>
    /// Twison JSON format for a Twine story.
    /// </summary>
    [Serializable]
    public class TwineStory
    {
        public TwinePassage[] passages;
        public string name;
        public string startnode;
        public string creator;
    }

    [Serializable]
    public class TwinePassage
    {
        public string text;
        public TwineLink[] links;
        public string name;
        public string pid;
        public TwinePosition position;
    }

    [Serializable]
    public class TwineLink
    {
        public string name;
        public string link;
        public string pid;
    }

    [Serializable]
    public class TwinePosition
    {
        public float x;
        public float y;

        public TwinePosition() { }
        public TwinePosition(float x, float y) { this.x = x; this.y = y; }
    }

}
#endif
