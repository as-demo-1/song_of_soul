/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.Managers
{
    using Opsive.Shared.Editor.UIElements;
    using Opsive.UltimateInventorySystem.Editor.Styles;
    using Opsive.UltimateInventorySystem.Editor.VisualElements;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>
    /// The List Panel provides a container for a ReorderableList with searching and sorting.
    /// </summary>
    public class ListPanel<T> : VisualElement where T : class
    {
        private IListPanelProvider<T> m_Provider;

        private UnicodeTextField m_AddField;
        private Button m_AddButton;
        private SearchableList<T> m_SearchableList;

        private T m_PreviousSelectObject;

        public SearchableList<T> SearchableList => m_SearchableList;

        /// <summary>
        /// ListPanel constructor.
        /// </summary>
        /// <param name="panelProvider">A reference to the object that is providing the panel content.</param>
        public ListPanel(IListPanelProvider<T> panelProvider)
        {
            name = "leftPanel";
            m_Provider = panelProvider;

            var addListItemContainer = new VisualElement();
            addListItemContainer.AddToClassList(CommonStyles.s_AddListItemContainer);

            m_AddField = new UnicodeTextField();
            m_AddField.RegisterCallback<ChangeEvent<string>>(evt =>
            {
                var valid = !string.IsNullOrEmpty(evt.newValue) && m_Provider.IsObjectNameValidAndUnique(evt.newValue);
                m_AddButton.SetEnabled(valid);
            });
            m_AddField.AddToClassList("flex-grow");
            addListItemContainer.Add(m_AddField);

            m_AddButton = new Button();
            m_AddButton.text = "Add";
            m_AddButton.SetEnabled(false);
            m_AddButton.clickable.clicked += () =>
            {
                var obj = m_Provider.OnAdd(m_AddField.text);
                // Categories are an immutable list. The list must be refreshed with the correct entries.
                m_SearchableList.Refresh(m_Provider.GetList());
                m_AddField.SetValueWithoutNotify(string.Empty);
                m_AddButton.SetEnabled(false);
                m_SearchableList.SelectObject(obj);
            };
            addListItemContainer.Add(m_AddButton);
            Add(addListItemContainer);

            m_SearchableList = new SearchableList<T>(m_Provider.GetList(), (VisualElement parent, int index) =>
            {
                m_Provider.MakeListItem(parent, index);
            }, (VisualElement parent, int index) =>
            {
                m_Provider.BindListItem(parent, index);
            }, null,
            (int index) =>
            {
                m_Provider.OnSelected(index);
                m_PreviousSelectObject = m_SearchableList.SelectedObject;
            }, null,
            (int index) =>
            {
                m_Provider.OnRemove(index);
                // Categories are an immutable list. The list must be refreshed with the correct entries.
                m_SearchableList.Refresh(m_Provider.GetList());
            }, null,
            m_Provider.GetSortOptions(),
            m_Provider.GetSearchFilter);

            Add(m_SearchableList);
        }

        /// <summary>
        /// Refresh the ListPanel contents.
        /// </summary>
        /// <param name="selected">The selected index.</param>
        public void Refresh(int selected)
        {
            m_AddField.SetValueWithoutNotify(string.Empty);
            m_AddButton.SetEnabled(false);
            m_SearchableList.Refresh(m_Provider.GetList());
            m_SearchableList.SelectedIndex = selected;
        }

        /// <summary>
        /// Refresh the ListPanel contents.
        /// </summary>
        public void Refresh()
        {
            m_AddField.SetValueWithoutNotify(string.Empty);
            m_AddButton.SetEnabled(false);
            m_SearchableList.Refresh(m_Provider.GetList());
            m_SearchableList.SelectObject(m_PreviousSelectObject);
        }
    }
}