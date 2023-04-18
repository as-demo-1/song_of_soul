namespace Opsive.UltimateInventorySystem.Editor.Inspectors
{
    using Opsive.Shared.Editor.Inspectors.Utility;
    using Opsive.Shared.Editor.UIElements;
    using Opsive.UltimateInventorySystem.Core.InventoryCollections;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Opsive.UltimateInventorySystem.Storage;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UIElements;

    [CustomEditor(typeof(ItemRestrictionSetObject), true)]
    public class ItemRestrictionSetObjectInspector : DatabaseInspectorBase
    {
        protected override List<string> ExcludedFields => new List<string>() {"m_Restriction"};

        private ItemRestrictionSetObject m_ItemRestrictionSetObject;
        protected override void ShowFooterElements(VisualElement container)
        {
            m_ItemRestrictionSetObject = target as ItemRestrictionSetObject;

            if (m_ItemRestrictionSetObject.ItemRestrictionSet == null) {
                m_ItemRestrictionSetObject.ItemRestrictionSet = new ItemRestrictionSet();
            }
            
            m_ItemRestrictionSetObject.ItemRestrictionSet.Deserialize();
            var itemRestrictionReorderableList = new ItemRestrictionReorderableList(m_ItemRestrictionSetObject.ItemRestrictionSet.RestrictionList, m_Database);
            itemRestrictionReorderableList.OnValueChangedE += x =>
            {
                m_ItemRestrictionSetObject.ItemRestrictionSet.RestrictionList = x;
                m_ItemRestrictionSetObject.ItemRestrictionSet.Serialize();
                Shared.Editor.Utility.EditorUtility.SetDirty(m_ItemRestrictionSetObject);
            };
            container.Add(itemRestrictionReorderableList);
        }
    }

    public class ItemRestrictionReorderableList : VisualElement
    {
        public event Action<List<IItemRestriction>> OnValueChangedE;

        protected List<IItemRestriction> m_List;
        protected ReorderableList m_ReorderableList;

        protected VisualElement m_SelectionContainer;
        protected RestrictionField m_RestrictionField;
        protected InventorySystemDatabase m_Database;

        public ItemRestrictionReorderableList(List<IItemRestriction> list, InventorySystemDatabase database)
        {
            m_List = list;
            m_Database = database;
            m_ReorderableList = new ReorderableList(
                m_List,
                (parent, index) =>
                {
                    var listElement = new ListElement();
                    listElement.Index = index;
                    //listElement.OnValueChanged += OnElementValueChanged;
                    parent.Add(listElement);
                }, (parent, index) =>
                {
                    if (index < 0 || index >= m_ReorderableList.ItemsSource.Count) { return; }

                    var listElement = parent.ElementAt(0) as ListElement;
                    listElement.Index = index;
                    listElement.Refresh((IItemRestriction) m_ReorderableList.ItemsSource[index]);
                }, (parent) => { parent.Add(new Label("Restrictions")); }, (index) => { return 20; },
                (index) => { Refresh(); }, () =>
                {
                    InspectorUtility.AddObjectType(typeof(IItemRestriction),
                        (type) =>
                        {
                            var attributes = type.GetCustomAttributes(typeof(HideFromItemRestrictionSetAttribute),true);

                            if (attributes != null && attributes.Length >= 1) {
                                return false;
                            }

                            return true;
                        },
                        (evt) =>
                        {
                            var IItemRestriction = Activator.CreateInstance(evt as Type) as IItemRestriction;

                            m_List.Add(IItemRestriction);

                            m_ReorderableList.SelectedIndex = m_List.Count - 1;
                            InvokeValueChange(true);
                        });
                }, (index) =>
                {
                    if (index < 0 || index >= m_List.Count) { return; }

                    m_List.RemoveAt(index);

                    InvokeValueChange(true);
                }, (i1, i2) =>
                {
                    var element1 = m_ReorderableList.ListItems[i1].ItemContents.ElementAt(0) as ListElement;
                    element1.Index = i1;
                    var element2 = m_ReorderableList.ListItems[i2].ItemContents.ElementAt(0) as ListElement;
                    element2.Index = i2;

                    InvokeValueChange(true);
                });
            Add(m_ReorderableList);

            m_SelectionContainer = new VisualElement();
            Add(m_SelectionContainer);

            m_RestrictionField = new RestrictionField(m_Database);
            m_RestrictionField.OnValueChanged += (newstat) =>
            {
                OnElementValueChanged(m_ReorderableList.SelectedIndex, newstat);
            };
        }

        /// <summary>
        /// Refresh the inspector.
        /// </summary>
        protected void Refresh()
        {
            var selectedIndex = m_ReorderableList.SelectedIndex;
            if (selectedIndex < 0 || selectedIndex >= m_List.Count) {
                m_SelectionContainer.Clear();
                return;
            }

            m_RestrictionField.Refresh(m_List[selectedIndex]);
            if (m_SelectionContainer.childCount == 0) { m_SelectionContainer.Add(m_RestrictionField); }

        }

        /// <summary>
        /// Refresh the inspector.
        /// </summary>
        public void Refresh(List<IItemRestriction> statModifiers)
        {
            m_List.Clear();
            m_List.AddRange(statModifiers);
            m_ReorderableList.Refresh(m_List);

            Refresh();
        }

        /// <summary>
        /// Serialise when a value changes.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="value">The value.</param>
        private void OnElementValueChanged(int index, IItemRestriction value)
        {
            m_List[index] = value;
            InvokeValueChange(false);
        }

        /// <summary>
        /// Serialise when a value changes.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="value">The value.</param>
        private void InvokeValueChange(bool refresh)
        {
            OnValueChangedE?.Invoke(m_List);
            m_ReorderableList.Refresh(m_List);
            if (refresh) { Refresh(); }

        }

        /// <summary>
        /// The visual element for the object in the list.
        /// </summary>
        public class ListElement : VisualElement
        {
            //public event Action<int, IItemRestriction> OnValueChanged;
            public int Index { get; set; }

            protected Label m_Label;

            /// <summary>
            /// The constructor.
            /// </summary>
            /// <param name="database">The inventory database.</param>
            public ListElement()
            {
                m_Label = new Label();
                m_Label.text = "Null";
                Add(m_Label);
            }

            /// <summary>
            /// Refresh the component.
            /// </summary>
            /// <param name="value">The new category Item Viewes.</param>
            public void Refresh(IItemRestriction value)
            {
                m_Label.text = value == null ? "NULL" : value.ToString();
            }
        }
        
        public class RestrictionField : VisualElement
        {
            public event Action<IItemRestriction> OnValueChanged;

            protected IItemRestriction m_ItemRestriction;

            protected VisualElement m_Container;
            public InventorySystemDatabase InventorySystemDatabase;

            /// <summary>
            /// The constructor.
            /// </summary>
            /// <param name="database">The inventory database.</param>
            public RestrictionField(InventorySystemDatabase database)
            {
                InventorySystemDatabase = database;
                m_Container = new VisualElement();
                Add(m_Container);
            }

            /// <summary>
            /// Refresh the component.
            /// </summary>
            /// <param name="value">The new category Item Viewes.</param>
            public void Refresh(IItemRestriction value)
            {
                if (value == m_ItemRestriction) { return; }

                m_ItemRestriction = value;

                m_Container.Clear();
                if (m_ItemRestriction == null) { return; }

                var label = m_ItemRestriction.GetType().Name;

                FieldInspectorView.AddField(
                    null,
                    null, null,
                    -1, m_ItemRestriction.GetType(),
                    label, string.Empty, true,
                    m_ItemRestriction,
                    m_Container,
                    (object obj) =>
                    {
                        m_ItemRestriction = obj as IItemRestriction;
                        OnValueChanged?.Invoke(m_ItemRestriction);
                    }, null, false, null, null, InventorySystemDatabase);
            }
        }
    }

    
}