using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.UI;

namespace Gamekit2D
{
    [Serializable]
    public class ScrollingTextClip : PlayableAsset, ITimelineClipAsset
    {
        public ScrollingTextBehaviour template = new ScrollingTextBehaviour ();

        public ClipCaps clipCaps
        {
            get { return ClipCaps.Looping | ClipCaps.Extrapolation; }
        }

        public override Playable CreatePlayable (PlayableGraph graph, GameObject owner)
        {
            return ScriptPlayable<ScrollingTextBehaviour>.Create (graph, template);
        }
    }
}