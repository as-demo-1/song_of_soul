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
    /// The save data info is a struct with information about the save data.
    /// </summary>
    [Serializable]
    public struct SaveDataInfo
    {
        [SerializeField] private int m_Index;
        [SerializeField] private SaveMetaData m_MetaData;
        [SerializeField] private SaveData m_Data;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="index">The save file index.</param>
        /// <param name="metaData">The save meta data.</param>
        /// <param name="data">The save data.</param>
        public SaveDataInfo(int index, SaveMetaData metaData, SaveData data)
        {
            m_Index = index;
            m_MetaData = metaData;
            m_Data = data;
        }
        
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="other">The save data to copy.</param>
        /// <param name="data">The save data to override.</param>
        public SaveDataInfo(SaveDataInfo other, SaveData data)
        {
            m_Index = other.Index;
            m_MetaData = other.MetaData;
            m_Data = data;
        }
        
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="other">The save data to copy.</param>
        /// <param name="metaData">The save meta data to override.</param>
        public SaveDataInfo(SaveDataInfo other, SaveMetaData metaData)
        {
            m_Index = other.Index;
            m_MetaData = metaData;
            m_Data = other.Data;
        }
        
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="other">The save data to copy.</param>
        /// <param name="index">The save file index to override.</param>
        public SaveDataInfo(SaveDataInfo other, int index)
        {
            m_Index = index;
            m_MetaData = other.MetaData;
            m_Data = other.Data;
        }

        public SaveData Data => m_Data;

        public SaveMetaData MetaData => m_MetaData;

        public int Index => m_Index;
        public static SaveDataInfo None => new SaveDataInfo();

        /// <summary>
        /// Make a deep copy of the save data.
        /// </summary>
        /// <param name="saveDataInfo">The save data info.</param>
        /// <returns>The new save data info.</returns>
        public static SaveDataInfo DeepCopy(SaveDataInfo saveDataInfo)
        {
            return new SaveDataInfo(saveDataInfo.Index, saveDataInfo.MetaData, new SaveData(saveDataInfo.Data));
        }
    }
}