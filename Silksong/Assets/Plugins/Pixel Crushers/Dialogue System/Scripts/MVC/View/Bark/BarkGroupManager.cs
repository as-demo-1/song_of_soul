// Copyright (c) Pixel Crushers. All rights reserved.

using System.Collections.Generic;
using UnityEngine;

namespace PixelCrushers.DialogueSystem
{

    public enum BarkGroupQueueLimitMode { NoLimit, StopAtLimit, DropOldestAtLimit }

    /// <summary>
    /// Manages bark groups specified by BarkGroupMember.
    /// Adds itself to the Dialogue Manager.
    /// </summary>
    [AddComponentMenu("")] // Added automatically at runtime.
    public class BarkGroupManager : MonoBehaviour
    {

        public BarkGroupQueueLimitMode queueLimitMode = BarkGroupQueueLimitMode.NoLimit;

        [Tooltip("Only used if mode is Stop At Limit or Drop Oldest At Limit")]
        public int queueLimit = 256;

        private static bool s_applicationIsQuitting = false;
        private static BarkGroupManager s_instance = null;
        public static BarkGroupManager instance
        {
            get
            {
                if (s_applicationIsQuitting) return null;
                if (s_instance == null)
                {
                    s_instance = DialogueManager.instance.GetComponent<BarkGroupManager>();
                    if (s_instance == null)
                    {
                        s_instance = DialogueManager.instance.gameObject.AddComponent<BarkGroupManager>();
                    }
                }
                return s_instance;
            }
        }

#if UNITY_2019_3_OR_NEWER && UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void InitStaticVariables()
        {
            s_applicationIsQuitting = false;
            s_instance = null;
        }
#endif

        public Dictionary<string, HashSet<BarkGroupMember>> groups = new Dictionary<string, HashSet<BarkGroupMember>>();

        private  class BarkRequest
        {
            public BarkGroupMember member;
            public Transform listener;
            public string conversation;
            public BarkHistory barkHistory;
            public string barkText;
            public string sequence;
            public AbstractBarkUI barkUI;
            public float delayTime;
            public bool isPlaying = false;

            public BarkRequest(string conversation, BarkGroupMember member, Transform listener, BarkHistory barkHistory, float delayTime = -1)
            {
                this.member = member;
                this.listener = listener;
                this.conversation = conversation;
                this.barkHistory = barkHistory;
                this.barkText = null;
                this.sequence = null;
                this.barkUI = GetBarkUI(member);
                this.delayTime = GetDelayTime(member, delayTime);
            }

            public BarkRequest(string barkText, BarkGroupMember member, Transform listener, string sequence, float delayTime = -1)
            {
                this.member = member;
                this.listener = listener;
                this.conversation = null;
                this.barkHistory = null;
                this.barkText = barkText;
                this.sequence = sequence;
                this.barkUI = GetBarkUI(member);
                this.delayTime = GetDelayTime(member, delayTime);
            }

            private AbstractBarkUI GetBarkUI(BarkGroupMember member)
            {
                if (member == null) return null;
                var dialogueActor = member.GetComponentInChildren<DialogueActor>();
                if (dialogueActor != null && dialogueActor.barkUISettings.barkUI != null) return dialogueActor.barkUISettings.barkUI;
                return member.GetComponentInChildren<AbstractBarkUI>();
            }

            private float GetDelayTime(BarkGroupMember member, float delayTime)
            {
                if (delayTime >= 0) return delayTime;
                return (member == null) ? 0 : Random.Range(member.minDelayBetweenQueuedBarks, member.maxDelayBetweenQueuedBarks);
            }
        }

        private Dictionary<string, Queue<BarkRequest>> queues = new Dictionary<string, Queue<BarkRequest>>();

        private void OnApplicationQuit()
        {
            s_applicationIsQuitting = true;
        }

        /// <summary>
        /// Registers a bark group member.
        /// </summary>
        public void AddToGroup(string groupId, BarkGroupMember member)
        {
            if (string.IsNullOrEmpty(groupId) || member == null) return;
            if (!groups.ContainsKey(groupId)) groups.Add(groupId, new HashSet<BarkGroupMember>());
            groups[groupId].Add(member);
        }

        /// <summary>
        /// Unregisters a bark group member.
        /// </summary>
        public void RemoveFromGroup(string groupId, BarkGroupMember member)
        {
            if (string.IsNullOrEmpty(groupId) || member == null) return;
            if (!groups.ContainsKey(groupId)) return;
            if (!groups[groupId].Contains(member)) return;
            groups[groupId].Remove(member);
            if (groups[groupId].Count == 0) groups.Remove(groupId);
        }

        /// <summary>
        /// Hides all bark members' bark UIs and clears any queued barks.
        /// </summary>
        public void CancelAllBarks()
        {
            foreach (var queue in queues.Values)
            {
                queue.Clear();
            }
            foreach (var group in groups.Values)
            {
                foreach (var member in group)
                {
                    if (member != null) member.CancelBark();
                }
            }
        }

        /// <summary>
        /// Hides other members' barks if they're playing.
        /// Pass null to hide all members' barks.
        /// </summary>
        public void MutexBark(string groupId, BarkGroupMember member)
        {
            if (string.IsNullOrEmpty(groupId)) return;
            if (!groups.ContainsKey(groupId)) return;
            foreach (var other in groups[groupId])
            {
                if (other == member) continue;
                other.CancelBark();
            }
        }

        /// <summary>
        /// Barks with group awareness.
        /// </summary>
        /// <param name="conversation">Conversation to bark from.</param>
        /// <param name="member">Barker.</param>
        /// <param name="listener">Bark target.</param>
        /// <param name="barkHistory">Bark history.</param>
        /// <param name="delayTime">Omit/zero to use the member's random delay settings; if nonzero, use this delay time.</param>
        public void GroupBark(string conversation, BarkGroupMember member, Transform listener, BarkHistory barkHistory, float delayTime = 0)
        {
            if (member == null || !member.queueBarks)
            {
                DialogueManager.Bark(conversation, (member != null) ? member.transform : null, listener, barkHistory);
            }
            else
            {
                Enqueue(new BarkRequest(conversation, member, listener, barkHistory, delayTime));
            }
        }

        /// <summary>
        /// Barks with group awareness.
        /// </summary>
        /// <param name="barkText">Text to bark.</param>
        /// <param name="member">Barker.</param>
        /// <param name="listener">Bark target.</param>
        /// <param name="sequence">Optional sequence to play during the bark.</param>
        /// <param name="delayTime">Omit/zero to use the member's random delay settings; if nonzero, use this delay time.</param>
        public void GroupBarkString(string barkText, BarkGroupMember member, Transform listener, string sequence, float delayTime = 0)
        {
            if (member == null || !member.queueBarks)
            {
                DialogueManager.BarkString(barkText, (member != null) ? member.transform : null, listener, sequence);
            }
            else
            {
                Enqueue(new BarkRequest(barkText, member, listener, sequence, delayTime));
            }
        }

        private void Enqueue(BarkRequest barkRequest)
        {
            var member = barkRequest.member;
            if (member.evaluateIdEveryBark) member.UpdateMembership();
            var groupId = member.currentIdValue;
            if (!queues.ContainsKey(groupId)) queues.Add(groupId, new Queue<BarkRequest>());
            var queue = queues[groupId];
            if (queueLimitMode != BarkGroupQueueLimitMode.NoLimit && queue.Count > queueLimit)
            {
                switch (queueLimitMode)
                {
                    case BarkGroupQueueLimitMode.StopAtLimit:
                        return;
                    case BarkGroupQueueLimitMode.DropOldestAtLimit:
                        queue.Dequeue();
                        break;
                }
            }
            queue.Enqueue(barkRequest);
            if (queue.Count == 1) barkRequest.delayTime = 0; // Play immediately.
        }

        private void Update()
        {
            // Check each group's queue:
            var enumerator = queues.GetEnumerator(); // Enumerates manually to avoid garbage.
            while (enumerator.MoveNext())
            {
                var queue = enumerator.Current.Value;
                if (queue.Count == 0) continue;
                var barkRequest = queue.Peek();
                if (!barkRequest.isPlaying)
                {
                    // If request at front of queue isn't playing yet, update time left:
                    barkRequest.delayTime -= Time.deltaTime;
                    if (barkRequest.delayTime <= 0)
                    {
                        // If it's now time, play the bark:
                        if (barkRequest.member == null || barkRequest.barkUI == null || (string.IsNullOrEmpty(barkRequest.conversation) && string.IsNullOrEmpty(barkRequest.barkText)))
                        {
                            queue.Dequeue();
                        }
                        else if (!string.IsNullOrEmpty(barkRequest.conversation))
                        {
                            DialogueManager.Bark(barkRequest.conversation, barkRequest.member.transform, barkRequest.listener, barkRequest.barkHistory);
                        }
                        else
                        {
                            DialogueManager.BarkString(barkRequest.barkText, barkRequest.member.transform, barkRequest.listener, barkRequest.sequence);
                        }
                        barkRequest.isPlaying = true;
                        barkRequest.delayTime = 0.5f; // Wait a little for bark to set up before checking if it's done playing.
                    }
                }
                else
                {
                    barkRequest.delayTime -= Time.deltaTime;
                    if (barkRequest.delayTime <= 0 && !barkRequest.barkUI.isPlaying)
                    {

                        // If request at front of queue is playing but bark UI is done playing, dequeue:
                        queue.Dequeue();
                    }
                }
            }
        }
    }
}
