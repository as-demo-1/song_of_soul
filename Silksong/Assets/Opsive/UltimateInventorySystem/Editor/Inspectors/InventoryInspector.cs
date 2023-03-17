/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.Inspectors
{
    using Opsive.Shared.Editor.UIElements;
    using Opsive.UltimateInventorySystem.Core.InventoryCollections;
    using Opsive.UltimateInventorySystem.Editor.Styles;
    using Opsive.UltimateInventorySystem.Editor.VisualElements;
    using Opsive.UltimateInventorySystem.Storage;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>
    /// The Inventory inspector used to modify the content of the Item Collections in the editor.
    /// It also shows the real time content of the inventory at runtime.
    /// </summary>
    [CustomEditor(typeof(Inventory), true)]
    public class InventoryInspector : DatabaseInspectorBase
    {
        protected const string c_ItemCollectionData = "m_ItemCollectionData";
        protected const string c_ItemCollections = "m_ItemCollections";

        protected override List<string> ExcludedFields => new List<string>() { c_ItemCollectionData, c_ItemCollections };
        
        protected ReorderableList m_ItemCollectionsReorderableList;
        protected VisualElement m_SelectedContainer;

        protected Inventory m_Inventory;
        protected int m_SelectedItemCollectionIndex;
        protected ItemCollection m_SelectedItemCollection;

        /// <summary>
        /// Initialize the inspector when it is first selected.
        /// </summary>
        protected override void InitializeInspector()
        {
            m_Inventory = target as Inventory;

            if (!Application.isPlaying) { m_Inventory.Initialize(true); }

            base.InitializeInspector();
        }

        /// <summary>
        /// Create the Inspector.
        /// </summary>
        /// <param name="container">The parent container.</param>
        protected override void ShowFooterElements(VisualElement container)
        {
            m_SelectedContainer = new VisualElement();

            m_ItemCollectionsReorderableList = new ReorderableList(
                m_Inventory.ItemCollections,
                (parent, index) =>
                {
                    parent.Add(new ListElement());
                }, (parent, index) =>
                {
                    var listElement = parent.ElementAt(0) as ListElement;
                    listElement.SetElement(m_Inventory.ItemCollections[index], index);
                }, (parent) =>
                {
                    parent.Add(new Label("Item Collections"));
                },
                (index) =>
                {
                    var itemCollection = m_Inventory.ItemCollections[index];
                    Refresh(itemCollection, index);
                }, () =>
                {
                    var newItemCollection = new ItemCollection();
                    newItemCollection.SetName(string.Format("ItemCollection{0}", m_Inventory.ItemCollections.Count));
                    m_Inventory.AddItemCollection(newItemCollection);

                    Serialize();
                    m_ItemCollectionsReorderableList.Refresh(m_Inventory.ItemCollections);
                    m_ItemCollectionsReorderableList.SelectedIndex = m_Inventory.ItemCollections.Count - 1;
                }, (index) =>
                {
                    var list = m_ItemCollectionsReorderableList.ItemsSource;
                    if (index < 0 || index >= list.Count) { return; }

                    m_Inventory.ItemCollections.RemoveAt(index);

                    Serialize();
                    m_ItemCollectionsReorderableList.Refresh(m_Inventory.ItemCollections);
                    Refresh();
                }, (i1, i2) =>
                {

                });

            container.Add(m_ItemCollectionsReorderableList);

            container.Add(m_SelectedContainer);
        }

        /// <summary>
        /// Refresh When the database has changed.
        /// </summary>
        protected void Refresh()
        {
            Refresh(null, -1);
        }

        /// <summary>
        /// Refresh to show the content of the selected item collection.
        /// </summary>
        /// <param name="itemCollection">The new item collection.</param>
        protected void Refresh(ItemCollection itemCollection, int index)
        {
            m_SelectedContainer.Clear();

            m_SelectedItemCollection = itemCollection;
            m_SelectedItemCollectionIndex = index;

            if (itemCollection == null) {
                m_SelectedContainer.Add(new Label("No Item Collection selected."));
                return;
            }

            var database = m_DatabaseField.value as InventorySystemDatabase;

            if (database == null) {
                m_SelectedContainer.Add(new Label("The database is null."));
                return;
            }

            database.Initialize(false);

            var itemCollectionField = new ItemCollectionField(itemCollection, database);
            itemCollectionField.OnCollectionContentChange += () =>
            {
                m_SelectedItemCollection = itemCollectionField.ItemCollection;
                m_Inventory.ItemCollections[m_SelectedItemCollectionIndex] = m_SelectedItemCollection;
                m_ItemCollectionsReorderableList.Refresh(m_Inventory.ItemCollections);

                Serialize();
            };
            itemCollectionField.OnItemCollectionTypeChange += (newValue) =>
            {
                m_SelectedItemCollection = newValue;
                m_Inventory.ItemCollections[m_SelectedItemCollectionIndex] = m_SelectedItemCollection;
                m_ItemCollectionsReorderableList.Refresh(m_Inventory.ItemCollections);

                Serialize();
                Refresh(m_SelectedItemCollection, m_SelectedItemCollectionIndex);
            };

            m_SelectedContainer.Add(itemCollectionField);
        }

        /// <summary>
        /// Serialize and flags the object as dirty.
        /// </summary>
        protected void Serialize()
        {
            m_Inventory.Serialize();
            Shared.Editor.Utility.EditorUtility.SetDirty(m_Inventory);
        }

        /// <summary>
        /// And element of the list.
        /// </summary>
        class ListElement : VisualElement
        {
            protected Label m_Name;
            protected Label m_Type;

            /// <summary>
            /// Constructor.
            /// </summary>
            public ListElement()
            {
                m_Name = new Label();
                m_Type = new Label();

                Add(m_Name);
                Add(m_Type);

                AddToClassList("horizontal-layout");
                AddToClassList(CommonStyles.s_AlignChildrenCenter);

                style.justifyContent = Justify.SpaceBetween;
            }

            /// <summary>
            /// Set the element.
            /// </summary>
            /// <param name="itemCollection">The itemCollection.</param>
            /// <param name="index">The index.</param>
            public void SetElement(ItemCollection itemCollection, int index)
            {
                m_Type.text = itemCollection.GetType().Name;

                if (itemCollection.Name == null) {
                    m_Name.text = "No Name";
                } else {
                    m_Name.text = itemCollection.Name;
                }

                if (index == 0) { m_Name.text = "(Main) " + m_Name.text; }
            }
        }
    }
}
