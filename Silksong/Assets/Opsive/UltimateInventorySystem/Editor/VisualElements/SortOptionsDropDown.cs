/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.VisualElements
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor.UIElements; // 2022-
    using UnityEngine.UIElements; // 2022+

    /// <summary>
    /// Sort Options drop down lets you define a field for sorting lists.
    /// </summary>
    public class SortOptionsDropDown : PopupField<SortOption>
    {
        private IList<SortOption> m_Options;
        private SortOption m_CurrentOption;

        public SortOption CurrentOption => m_CurrentOption;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="options">The sort options.</param>
        public SortOptionsDropDown(IList<SortOption> options)
            : this((string)null, options, 0)
        {
            m_Options = options;

            SetOption(options[0]);
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="label">The field label.</param>
        /// <param name="choices">The sort options.</param>
        /// <param name="defaultIndex">The starting index.</param>
        public SortOptionsDropDown(
            string label,
            IList<SortOption> choices,
            int defaultIndex)
            : base(label, new List<SortOption>(choices), defaultIndex, null, null)
        {
            formatListItemCallback = ListItemCallback;
            formatSelectedValueCallback = SelectedValueCallback;
        }

        /// <summary>
        /// list item callback.
        /// </summary>
        /// <param name="option">The sort option.</param>
        /// <returns>The option name.</returns>
        private string ListItemCallback(SortOption option)
        {
            return option.Name;
        }

        /// <summary>
        /// The selected value callback.
        /// </summary>
        /// <param name="option">The sort option selected.</param>
        /// <returns>The option name.</returns>
        private string SelectedValueCallback(SortOption option)
        {
            SetOption(option);
            return option.Name;
        }

        /// <summary>
        /// Set Option.
        /// </summary>
        /// <param name="option">The option to set.</param>
        private void SetOption(SortOption option)
        {
            m_CurrentOption = option;
        }
    }

    /// <summary>
    /// Sort Option used in a sort option drop down.
    /// </summary>
    public class SortOption
    {
        private string m_Name;
        private Action<IList> m_Sort;

        public string Name => m_Name;
        public Action<IList> Sort => m_Sort;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">The option name.</param>
        /// <param name="sort">The action returning a sorted list.</param>
        public SortOption(string name, Action<IList> sort)
        {
            m_Name = name;
            m_Sort = sort;
        }
    }
}