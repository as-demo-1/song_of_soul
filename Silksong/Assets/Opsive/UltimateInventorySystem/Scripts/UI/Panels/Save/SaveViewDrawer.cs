/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Panels.Save
{
    using Opsive.UltimateInventorySystem.SaveSystem;
    using Opsive.UltimateInventorySystem.UI.Views;
    using UnityEngine;

    /// <summary>
    /// A box drawer used to draw save boxes.
    /// </summary>
    public class SaveViewDrawer : ViewDrawer<SaveDataInfo>
    {
        [Tooltip("The save box prefab.")]
        [SerializeField] protected GameObject m_SaveBoxPrefab;

        /// <summary>
        /// Get the box prefab for a save data.
        /// </summary>
        /// <param name="saveDataInfo">The save data.</param>
        /// <returns>The box prefab.</returns>
        public override GameObject GetViewPrefabFor(SaveDataInfo saveDataInfo)
        {
            return m_SaveBoxPrefab;
        }

        /// <summary>
        /// An event called before the box is drawn.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="view">The box Ui.</param>
        /// <param name="element">The element.</param>
        protected override void BeforeDrawingBox(int index, View<SaveDataInfo> view, SaveDataInfo element)
        {
            base.BeforeDrawingBox(index, view, element);
            if (view is SaveView saveBoxUI) { saveBoxUI.SetIndex(index); }
        }
    }
}