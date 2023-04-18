/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Panels.Save
{
    using Opsive.UltimateInventorySystem.SaveSystem;
    using Opsive.UltimateInventorySystem.UI.Views;

    /// <summary>
    /// The box for a save data.
    /// </summary>
    public class SaveView : View<SaveDataInfo>
    {
        protected int m_Index;

        /// <summary>
        /// Set the index of the box.
        /// </summary>
        /// <param name="index">The index.</param>
        public virtual void SetIndex(int index)
        {
            for (int i = 0; i < m_Modules.Count; i++) {
                if (m_Modules[i] is SaveViewModule tcomponent) {
                    tcomponent.SetIndex(index);
                }
            }
            m_Index = index;
        }
    }
}