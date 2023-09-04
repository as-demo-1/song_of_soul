#if USE_TIMELINE
#if UNITY_2017_1_OR_NEWER
// Copyright (c) Pixel Crushers. All rights reserved.

using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace PixelCrushers.DialogueSystem
{

    [Serializable]
    public class BarkClip : PlayableAsset, ITimelineClipAsset
    {
        public BarkBehaviour template = new BarkBehaviour();
        public ExposedReference<Transform> listener;

        public ClipCaps clipCaps
        {
            get { return ClipCaps.None; }
        }

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<BarkBehaviour>.Create(graph, template);
            BarkBehaviour clone = playable.GetBehaviour();
            clone.listener = listener.Resolve(graph.GetResolver());
            return playable;
        }
    }
}
#endif
#endif
