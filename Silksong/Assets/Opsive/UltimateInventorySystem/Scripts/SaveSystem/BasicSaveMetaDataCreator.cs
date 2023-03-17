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
    /// A basic save meta data creator.
    /// </summary>
    [CreateAssetMenu(fileName = "BasicSaveMetaDataCreator", menuName = "Opsive/Save System/Save Meta Data Creator.")]
    public class BasicSaveMetaDataCreator : SaveMetaDataCreator
    {
        /// <summary>
        /// Create the save meta data.
        /// </summary>
        /// <param name="saveSystemManager">The save system manager.</param>
        /// <param name="saveDataInfo">The save data info linked to that meta data.</param>
        /// <returns>The save meta data.</returns>
        public override SaveMetaData CreateMetaData(SaveSystemManager saveSystemManager, SaveDataInfo saveDataInfo)
        {
            return new BasicSaveMetaData(saveSystemManager, saveDataInfo);
        }

        /// <summary>
        /// Create an empty save meta data.
        /// </summary>
        /// <returns></returns>
        public override SaveMetaData CreateEmpty()
        {
            return new BasicSaveMetaData();
        }
    }
    
    /// <summary>
    /// The save meta data which can be serialized.
    /// </summary>
    [Serializable]
    public class BasicSaveMetaData : SaveMetaData
    {
        [Tooltip("The date and time in ticks.")]
        [SerializeField] protected long m_DateTimeTicks;

        public long DateTimeTicks => m_DateTimeTicks;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public BasicSaveMetaData() : base()
        {
            m_DateTimeTicks = new DateTime().Ticks;
        }

        /// <summary>
        /// Overloaded constructor.
        /// </summary>
        /// <param name="saveSystemManager">The save system manager.</param>
        /// <param name="saveDataInfo">The save data info linked to that meta data.</param>
        public BasicSaveMetaData(SaveSystemManager saveSystemManager, SaveDataInfo saveDataInfo) : base(saveSystemManager, saveDataInfo)
        {
            m_DateTimeTicks = DateTime.Now.Ticks;
        }

        /// <summary>
        /// Set the date of the save data.
        /// </summary>
        /// <param name="newDateTime">The new date.</param>
        public void SetDateTime(DateTime newDateTime)
        {
            m_DateTimeTicks = newDateTime.Ticks;
        }
    }
}