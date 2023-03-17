/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.Inspectors
{
    using Opsive.Shared.Editor.UIElements;
    using Opsive.UltimateInventorySystem.Storage;
    using System;
    using System.Collections.Generic;
    using Opsive.UltimateInventorySystem.Editor.VisualElements;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>
    /// base class for Reorderable list for Inventory Objects.
    /// </summary>
    /// <typeparam name="T">The Inventory object type.</typeparam>
    public class InventoryObjectReorderableList<T> : VisualElement where T : ScriptableObject
    {
        Func<T[]> m_GetObject;
        private Action<T[]> m_SetObjects;

        protected List<T> m_List;
        protected ReorderableList m_ReorderableList;
        
        protected InventoryObjectSearchableListWindow<T> m_SearchableListWindow;

        public InventoryObjectSearchableListWindow<T> SearchableListWindow
        {
            get { return m_SearchableListWindow; }
            set => m_SearchableListWindow = value;
        }

        /// <summary>
        /// The constructor.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="getObject">A function to get the object.</param>
        /// <param name="setObjects">A function to set the object.</param>
        /// <param name="createListElement">A function to create the list element.</param>
        public InventoryObjectReorderableList(string title, Func<T[]> getObject, Action<T[]> setObjects, Func<InventoryObjectListElement<T>> createListElement)
        {
            m_GetObject = getObject;
            m_SetObjects = setObjects;

            var array = m_GetObject.Invoke();
            m_List = array == null ? new List<T>() : new List<T>(array);

            m_ReorderableList = new ReorderableList(
                m_List,
                (parent, index) =>
                {
                    var listElement = createListElement.Invoke();
                    listElement.Index = index;
                    listElement.OnValueChanged += OnValueChanged;
                    parent.Add(listElement);
                }, (parent, index) =>
                {
                    var listElement = parent.ElementAt(0) as InventoryObjectListElement<T>;
                    listElement.Index = index;
                    listElement.Refresh(m_ReorderableList.ItemsSource[index] as T);
                }, (parent) =>
                {
                    parent.Add(new Label(title));
                }, (index) => { return 25f; },
                (index) =>
                {
                    //nothing
                }, Add,Remove,
                (i1, i2) =>
                {
                    var element1 = m_ReorderableList.ListItems[i1].ItemContents.ElementAt(0) as InventoryObjectListElement<T>;
                    element1.Index = i1;
                    var element2 = m_ReorderableList.ListItems[i2].ItemContents.ElementAt(0) as InventoryObjectListElement<T>;
                    element2.Index = i2;
                    m_SetObjects.Invoke(m_List.ToArray());
                });
            Add(m_ReorderableList);
        }

        /// <summary>
        /// Add an the element.
        /// </summary>
        protected virtual void Add()
        {
            if (SearchableListWindow == null) {
                AddObjectToList(default);
            } else {
                var buttonWorldBound = m_ReorderableList.AddButton.worldBound;
                m_SearchableListWindow.OpenPopUpWindow(buttonWorldBound.position,buttonWorldBound.size);
            }
        }

        /// <summary>
        /// Add an object to the list.
        /// </summary>
        /// <param name="obj">The object to add.</param>
        protected virtual void AddObjectToList(T obj)
        {
            m_List.Add(obj);
            m_SetObjects.Invoke(m_List.ToArray());
            m_ReorderableList.Refresh(m_List);
        }

        /// <summary>
        /// Remove the element at the index.
        /// </summary>
        /// <param name="index">The index to remove at.</param>
        protected virtual void Remove(int index)
        {
            if (index < 0 || index >= m_List.Count) {
                return;
            }

            m_List.RemoveAt(index);

            m_SetObjects.Invoke(m_List.ToArray());
            m_ReorderableList.Refresh(m_List);
        }

        /// <summary>
        /// Serialize and update the visuals.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="value">The value.</param>
        private void OnValueChanged(int index, T value)
        {
            m_List[index] = value;
            m_SetObjects.Invoke(m_List.ToArray());
            m_ReorderableList.Refresh(m_List);
        }

        public void Refresh()
        {
            var array = m_GetObject.Invoke();
            m_List = array == null ? new List<T>() : new List<T>(array);
            
            m_ReorderableList.Refresh(m_List);
        }
    }

    public abstract class InventoryObjectListElement<T> : VisualElement
    {
        private InventorySystemDatabase m_Database;
        public event Action<int, T> OnValueChanged;
        public int Index { get; set; }
        public InventorySystemDatabase Database { get; set; }

        /// <summary>
        /// The list element.
        /// </summary>
        public InventoryObjectListElement(InventorySystemDatabase database)
        {
            m_Database = database;
        }

        /// <summary>
        /// Update the visuals.
        /// </summary>
        /// <param name="value">The new value.</param>
        public abstract void Refresh(T value);

        public void HandleValueChanged(T value)
        {
            OnValueChanged?.Invoke(Index, value);
        }
    }
}