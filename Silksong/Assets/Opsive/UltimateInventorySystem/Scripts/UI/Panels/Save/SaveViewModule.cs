/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Panels.Save
{
    using Opsive.UltimateInventorySystem.SaveSystem;
    using Opsive.UltimateInventorySystem.UI.CompoundElements;
    using Opsive.UltimateInventorySystem.UI.Views;
    using System;
    using UnityEngine;
    using Text = Opsive.Shared.UI.Text;

    /// <summary>
    /// A box used to display a save.
    /// </summary>
    public class SaveViewModule : ViewModule<SaveDataInfo>
    {
        [Tooltip("The file number format after the save file name.")]
        [SerializeField] protected string m_FileNumberFormat = "{0:00}";
        [Tooltip("The file number text.")]
        [SerializeField] protected Text m_FileNumberText;
        [Tooltip("The save content text.")]
        [SerializeField] protected Text m_SaveContentText;

        protected SaveDataInfo m_SaveDataInfo;
        protected int m_Index;

        public SaveDataInfo SaveDataInfo => m_SaveDataInfo;
        public int Index => m_Index;

        /// <summary>
        /// The box index.
        /// </summary>
        /// <param name="index">The index.</param>
        public void SetIndex(int index)
        {
            m_Index = index;
        }

        /// <summary>
        /// The the box value.
        /// </summary>
        /// <param name="info">The itemInfo.</param>
        public override void SetValue(SaveDataInfo info)
        {
            m_SaveDataInfo = info;
            m_FileNumberText.text = string.Format(m_FileNumberFormat, m_Index);

            if (m_SaveDataInfo.MetaData == null || m_SaveDataInfo.MetaData.IsEmpty) {
                m_SaveContentText.text = "Empty";
            } else {
                if (m_SaveDataInfo.MetaData is BasicSaveMetaData basicSaveMetaData) {
                    m_SaveContentText.text = new DateTime(basicSaveMetaData.DateTimeTicks).ToString();
                } else {
                    m_SaveContentText.text = "Save File "+m_SaveDataInfo.Index;
                }
            }
        }

        /// <summary>
        /// Clear the box.
        /// </summary>
        public override void Clear()
        {
            m_FileNumberText.text = string.Format(m_FileNumberFormat, m_Index);
            m_SaveContentText.text = "Empty";
        }
    }
}