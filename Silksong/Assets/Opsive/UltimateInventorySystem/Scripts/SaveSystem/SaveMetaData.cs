/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.SaveSystem
{
    using System;
    using UnityEngine;

    /// <summary>
    /// A save meta data bas class.
    /// </summary>
    [Serializable]
    public abstract class SaveMetaData
    {
        [Tooltip("Is the save data empty.")]
        [SerializeField] protected bool m_IsEmpty;
        
        public bool IsEmpty { get => m_IsEmpty; set => m_IsEmpty = value; }

        /// <summary>
        /// Bas constructor.
        /// </summary>
        public SaveMetaData()
        {
            m_IsEmpty = true;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="saveSystemManager">The save system manager.</param>
        /// <param name="saveDataInfo">The save data info.</param>
        public SaveMetaData(SaveSystemManager saveSystemManager, SaveDataInfo saveDataInfo)
        {
            m_IsEmpty = false;
        }
    }
}