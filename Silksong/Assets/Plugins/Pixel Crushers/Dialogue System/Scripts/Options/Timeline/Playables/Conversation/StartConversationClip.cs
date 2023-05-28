#if USE_TIMELINE
#if UNITY_2017_1_OR_NEWER
// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using System;

namespace PixelCrushers.DialogueSystem
{

    [Serializable]
    public class StartConversationClip : PlayableAsset, ITimelineClipAsset
    {
        public StartConversationBehaviour template = new StartConversationBehaviour();
        public ExposedReference<Transform> conversant;

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
            var playable = ScriptPlayable<StartConversationBehaviour>.Create(graph, template);
            StartConversationBehaviour clone = playable.GetBehaviour();
            clone.conversant = conversant.Resolve(graph.GetResolver());
            return playable;
        }
    }
}
#endif
#endif
