/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Input
{
    using UnityEngine;
    using UnityEngine.EventSystems;
    
    /// <summary>
    /// This Component should be set on the Canvas of the UI for that Event System.
    /// </summary>
    public class EventSystemIdentifier : MonoBehaviour
    {
        [Tooltip("The ID for the EventSystem.")]
        [SerializeField] protected int m_ID;
        [Tooltip("The Event System to link with this ID.")]
        [SerializeField] protected EventSystem m_EventSystem;

        public int ID => m_ID;
        public EventSystem EventSystem => m_EventSystem;
    }
}