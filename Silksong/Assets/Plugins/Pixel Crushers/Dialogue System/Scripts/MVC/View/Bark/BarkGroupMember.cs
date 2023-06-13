// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// A member of a bark group. Barks are mutually exclusive within a bark group.
    /// When one member barks, the other members hide their active barks.
    /// </summary>
    [AddComponentMenu("")] // Use wrapper.
    public class BarkGroupMember : MonoBehaviour
    {

        /// <summary>
        /// Member of this group. Can be a Lua expression.
        /// </summary>
        [Tooltip("Member of this group. Can be a Lua expression.")]
        public string groupId;

        /// <summary>
        /// Evaluate Group Id before every bark. Useful if Id is a Lua expression that can change value.
        /// </summary>
        [Tooltip("Evaluate Group Id before every bark. Useful if Id is a Lua expression that can change value.")]
        public bool evaluateIdEveryBark = false;

        /// <summary>
        /// When another group member forces this member's bark to hide, delay this many seconds before hiding.
        /// </summary>
        [Tooltip("When another group member forces this member's bark to hide, delay this many seconds before hiding.")]
        public float forcedHideDelay = 0;

        /// <summary>
        /// If another group member is barking, wait in a queue to bark.
        /// </summary>
        [Tooltip("If another group member is barking, wait in a queue to bark instead of cancelling the other member's bark.")]
        public bool queueBarks = false;

        public float minDelayBetweenQueuedBarks = 0;
        public float maxDelayBetweenQueuedBarks = 1;

        [Tooltip("Hide bark when conversations start.")]
        public bool hideBarkOnConversationStart = false;

        private string m_currentIdValue = string.Empty;
        public string currentIdValue { get { return m_currentIdValue; } }

        private IBarkUI m_barkUI = null;
        private bool m_applicationIsQuitting = false;

        private IBarkUI barkUI
        {
            get
            {
                if (m_barkUI == null) m_barkUI = GetComponentInChildren(typeof(IBarkUI)) as IBarkUI;
                return m_barkUI;
            }
        }

        protected virtual void Awake()
        {
            m_currentIdValue = groupId;
        }

        protected virtual void Start()
        {
            if (!string.IsNullOrEmpty(groupId)) BarkGroupManager.instance.AddToGroup(groupId, this);
        }

        private void OnApplicationQuit()
        {
            m_applicationIsQuitting = true;
        }

        private void OnEnable()
        {
            if (hideBarkOnConversationStart) DialogueManager.instance.conversationStarted += OnConversationStarted;
        }

        private void OnDisable()
        {
            if (m_applicationIsQuitting || BarkGroupManager.instance == null) return;
            BarkGroupManager.instance.RemoveFromGroup(m_currentIdValue, this);
            if (hideBarkOnConversationStart) DialogueManager.instance.conversationStarted -= OnConversationStarted;
        }

        private void OnConversationStarted(Transform actor)
        {
            CancelBark();
        }

        public void GroupBark(string conversation, Transform listener, BarkHistory barkHistory, float delayTime = -1)
        {
            BarkGroupManager.instance.GroupBark(conversation, this, listener, barkHistory, delayTime);
        }

        public void GroupBarkString(string barkText, Transform listener, string sequence, float delayTime = -1)
        {
            BarkGroupManager.instance.GroupBarkString(barkText, this, listener, sequence, delayTime);
        }

        private void OnBarkStart(Transform listener)
        {
            if (string.IsNullOrEmpty(m_currentIdValue) || evaluateIdEveryBark)
            {
                UpdateMembership();
            }
            BarkGroupManager.instance.MutexBark(m_currentIdValue, this);
        }

        public void UpdateMembership()
        {
            var newIdValue = Lua.Run("return " + groupId, DialogueDebug.logInfo, false).asString;
            if (string.Equals(newIdValue, "nil")) newIdValue = groupId;
            if (newIdValue != m_currentIdValue)
            {
                BarkGroupManager.instance.RemoveFromGroup(m_currentIdValue, this);
                BarkGroupManager.instance.AddToGroup(newIdValue, this);
                m_currentIdValue = newIdValue;
            }
        }

        public void CancelBark()
        {
            if (barkUI == null || !barkUI.isPlaying) return;
            CancelInvoke("HideBarkNow");
            Invoke("HideBarkNow", forcedHideDelay);
        }

        private void HideBarkNow()
        {
            if (barkUI == null)
            {
                if (DialogueDebug.logWarnings) Debug.LogWarning("Dialogue System: Didn't find a bark UI on " + name, this);
            }
            else if (barkUI.isPlaying)
            {
                if (DialogueDebug.logInfo) Debug.Log("Dialogue System: Hiding bark on " + name, this);
                barkUI.Hide();
            }
        }

    }
}