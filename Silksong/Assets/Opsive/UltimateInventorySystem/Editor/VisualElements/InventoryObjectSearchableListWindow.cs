namespace Opsive.UltimateInventorySystem.Editor.VisualElements
{
    using System;
    using System.Collections.Generic;
    using Opsive.Shared.Editor.UIElements.Managers;
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Editor.Styles;
    using Opsive.UltimateInventorySystem.Editor.Utility;
    using Opsive.UltimateInventorySystem.Editor.VisualElements.ViewNames;
    using Opsive.UltimateInventorySystem.Storage;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UIElements;
    using PopupWindow = Opsive.UltimateInventorySystem.Editor.Managers.PopupWindow;

    public abstract class InventoryObjectSearchableListWindow<T> where T : class
    {
        public event Action OnClose;
        public event Action OnActionComplete;

        protected PopupWindow m_PopupWindow;
        protected VisualElement m_PopupWindowContent;
        protected SearchableList<T> m_SearchableList;

        protected InventorySystemDatabase m_InventorySystemDatabase;
        protected IList<(string, Action<T>)> m_Actions;
        protected T m_SelectedValue;
        protected Func<T, bool> m_PrefilterCondition;

        public bool IncludeNullOption { get; set; } = true;
        public bool ByPassActionConfirmationIfUnique { get; set; } = true;
        public bool CloseOnActionComplete { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="inventorySystemDatabase">The inventory system database.</param>
        /// <param name="actions">The actions that can be performed on the selected object.</param>
        /// <param name="preFilterCondition">The pre filter conditions can filter the fields objects.</param>
        /// <param name="closeOnActionComplete">Should the popup close when the action is complete.</param>
        public InventoryObjectSearchableListWindow(
            InventorySystemDatabase inventorySystemDatabase,
            IList<(string, Action<T>)> actions,
            Func<T, bool> preFilterCondition,
            bool closeOnActionComplete)
        {
            m_InventorySystemDatabase = inventorySystemDatabase;
            m_Actions = actions;
            m_PrefilterCondition = preFilterCondition;
            CloseOnActionComplete = closeOnActionComplete;

            BuildPopupContent();
            Refresh();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="inventorySystemDatabase">The inventory system database.</param>
        /// <param name="action">The actions that can be performed on the selected object.</param>
        /// <param name="preFilterCondition">The pre filter conditions can filter the fields objects.</param>
        public InventoryObjectSearchableListWindow(
            InventorySystemDatabase inventorySystemDatabase,
            Action<T> action,
            Func<T, bool> preFilterCondition,
            bool closeOnActionComplete) : this(inventorySystemDatabase, new[] { ("Select", action) }, preFilterCondition, closeOnActionComplete)
        {
        }

        public void OpenPopUpWindow(Vector2 buttonPosition, Vector2 buttonSize)
        {
            Refresh();

            var size = new Vector2(250, 300);
            var position = EditorWindow.focusedWindow.position.position + buttonPosition
                                                                        + new Vector2(buttonSize.x - size.x, 0);
            m_PopupWindow = PopupWindow.OpenWindow(
                new Rect(position, size),
                size, m_PopupWindowContent);
            m_PopupWindow.rootVisualElement.styleSheets.Add(Shared.Editor.Utility.EditorUtility.LoadAsset<StyleSheet>("e70f56fae2d84394b861a2013cb384d0"));
            m_PopupWindow.rootVisualElement.styleSheets.Add(CommonStyles.StyleSheet);
            m_PopupWindow.rootVisualElement.styleSheets.Add(InventoryManagerStyles.StyleSheet);
            m_PopupWindow.rootVisualElement.styleSheets.Add(ControlTypeStyles.StyleSheet);
            m_SearchableList.FocusSearchField();
        }

        /// <summary>
        /// Build the pop up window content.
        /// </summary>
        protected virtual void BuildPopupContent()
        {
            var genericMenu = new GenericMenu();

            for (int i = 0; i < m_Actions.Count; i++) {
                var localI = i;
                genericMenu.AddItem(new GUIContent(m_Actions[localI].Item1), false, () => InvokeAction(localI));
            }

            m_PopupWindowContent = new VisualElement();
            m_PopupWindowContent.name = "PopupSearchableList";
            m_SearchableList = new SearchableList<T>(
                GetSource(),
                MakeItem,
                BindItem, null,
                (index) =>
                {
                    if (index == -1) { return; }

                    m_SelectedValue = m_SearchableList.SelectedObject;
                    if (ByPassActionConfirmationIfUnique && m_Actions.Count == 1) {
                        InvokeAction(0);
                    } else {
                        genericMenu.ShowAsContext();
                    }
                    
                    m_SearchableList.SelectedIndex = -1;
                },
                null, null, null,
                GetSortOptions(),
                FilterOptions);
            m_SearchableList.selectOnRefresh = false;

            m_PopupWindowContent.Add(m_SearchableList);
        }

        protected virtual void InvokeAction(int index)
        {
            m_Actions[index].Item2?.Invoke(m_SelectedValue);
            m_SearchableList.SelectedIndex = -1;
            OnActionComplete?.Invoke();
            if (CloseOnActionComplete) {
                ClosePopup();
            }
        }

        /// <summary>
        /// Set the preFilter.
        /// </summary>
        /// <param name="preFilterCondition">The prefilter condition.</param>
        public virtual void SetPreFilter(Func<T, bool> preFilterCondition)
        {
            m_PrefilterCondition = preFilterCondition;
        }

        /// <summary>
        /// Filter options for the search list.
        /// </summary>
        /// <param name="list">The list source.</param>
        /// <param name="searchValue">The search value.</param>
        /// <returns>The filtered list.</returns>
        protected abstract IList<T> FilterOptions(IList<T> list, string searchValue);

        /// <summary>
        /// The sort options.
        /// </summary>
        /// <returns>The sort options.</returns>
        protected abstract IList<SortOption> GetSortOptions();

        /// <summary>
        /// Bind the list item.
        /// </summary>
        /// <param name="parent">The parent visual element.</param>
        /// <param name="index">The index.</param>
        protected abstract void BindItem(VisualElement parent, int index);

        /// <summary>
        /// Make the list item.
        /// </summary>
        /// <param name="parent">The parent visual element.</param>
        /// <param name="index">The index.</param>
        protected abstract void MakeItem(VisualElement parent, int index);

        /// <summary>
        /// Return the source of the list.
        /// </summary>
        /// <returns>The list source.</returns>
        protected IList<T> GetSource()
        {
            var source = GetSourceInternal();
            if (source == null) {
                return null;
            }
            var filteredList = new List<T>();
            for (int i = 0; i < source.Count; i++) {
                if (m_PrefilterCondition?.Invoke(source[i]) ?? true) {
                    filteredList.Add(source[i]);
                }
            }
            //Add a null option to remove object.
            if (IncludeNullOption) { filteredList.Add(null); }
            return filteredList;
        }

        /// <summary>
        /// Return the source of the list.
        /// </summary>
        /// <returns>The list source.</returns>
        protected abstract IList<T> GetSourceInternal();

        /// <summary>
        /// Refresh the ObjectField.
        /// </summary>
        public void Refresh()
        {
            m_SearchableList.Refresh(GetSource());
        }

        /// <summary>
        /// Close the pop up window.
        /// </summary>
        protected void ClosePopup()
        {
            m_PopupWindow.Close();
            OnClose?.Invoke();
        }
    }
}
