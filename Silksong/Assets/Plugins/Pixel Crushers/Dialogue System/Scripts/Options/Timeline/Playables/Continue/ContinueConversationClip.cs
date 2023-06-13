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
    public class ContinueConversationClip : PlayableAsset, ITimelineClipAsset
    {
        public ContinueConversationBehaviour template = new ContinueConversationBehaviour();

        public float m_duration = 1;
        public override double duration { get { return m_duration; } }

        public void SetDuration(float newDuration)
        {
            m_duration = newDuration;
        }

        public ClipCaps clipCaps
        {
            get { return ClipCaps.None; }
        }

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            return ScriptPlayable<ContinueConversationBehaviour>.Create(graph, template);
        }
    }
}
#endif
#endif
