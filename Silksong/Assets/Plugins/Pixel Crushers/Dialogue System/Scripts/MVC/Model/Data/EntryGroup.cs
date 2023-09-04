// Copyright (c) Pixel Crushers. All rights reserved.

using System;
using UnityEngine;

namespace PixelCrushers.DialogueSystem
{

    [Serializable]
    public class EntryGroup
    {
        public string name;
        public Rect rect;
        public Color color;

        public EntryGroup() { }
        public EntryGroup(string name, Rect rect) { this.name = name; this.rect = rect; this.color = new Color(1, 1, 1, 0.5f); }
        public EntryGroup(EntryGroup source) { this.name = source.name; this.rect = source.rect; this.color = source.color; }
    }
}