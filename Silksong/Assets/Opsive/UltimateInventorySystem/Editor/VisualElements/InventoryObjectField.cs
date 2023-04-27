/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.VisualElements
{
    using Opsive.Shared.Editor.UIElements.Managers;
    using Opsive.UltimateInventorySystem.Editor.Styles;
    using Opsive.UltimateInventorySystem.Editor.VisualElements.ViewNames;
    using Opsive.UltimateInventorySystem.Storage;
    using System;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UIElements;
    using PopupWindow = Opsive.UltimateInventorySystem.Editor.Managers.PopupWindow;

    /// <summary>
    /// Inventory Object Field used for a customized object picker.
    /// </summary>
    /// <typeparam name="T">The Object type.</typeparam>
    public abstract class InventoryObjectField<T> : VisualElement where T : class
    {
        public event Action OnClose;

        protected Label m_Label;
        protected ViewName<T> m_ViewName;
        protected Button m_Button;
        
        protected InventoryObjectSearchableListWindow<T> m_SearchableListWindow;

        protected InventorySystemDatabase m_InventorySystemDatabase;

        public T Value => m_ViewName?.Object;
        public ViewName<T> ViewName => m_ViewName;
        public bool IncludeNullOption { get => m_SearchableListWindow.IncludeNullOption; set=> m_SearchableListWindow.IncludeNullOption = value; }
        public InventoryObjectSearchableListWindow<T> SearchableListWindow => m_SearchableListWindow;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="label">The label.</param>
        /// <param name="inventorySystemDatabase">The inventory system database.</param>
        /// <param name="actions">The actions that can be performed on the selected object.</param>
        /// <param name="preFilterCondition">The pre filter conditions can filter the fields objects.</param>
        public InventoryObjectField(string label,
            InventorySystemDatabase inventorySystemDatabase,
            IList<(string, Action<T>)> actions,
            Func<T, bool> preFilterCondition)
        {
            m_InventorySystemDatabase = inventorySystemDatabase;

            m_ViewName = MakeFieldViewName();

            if (string.IsNullOrEmpty(label) == false) {
                m_Label = new Label(label);
                m_Label.style.alignSelf = Align.Center;
                Add(m_Label);

                var box = new VisualElement();
                box.AddToClassList("flex-grow");
                box.AddToClassList(ManagerStyles.BoxBackground);
                box.Add(m_ViewName);
                Add(box);
            } else {
                Add(m_ViewName);
            }

            m_Button = new Button();
            m_Button.text = "Change";
            m_Button.clickable.clicked += ChangeButtonClick;
            Add(m_Button);

            AddToClassList(InventoryManagerStyles.InventoryObjectField);
            m_SearchableListWindow = CreateSearchableListWindow(actions, preFilterCondition);
            if (m_SearchableListWindow != null) {
                m_SearchableListWindow.OnClose += () =>OnClose?.Invoke();
            }

            Refresh();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="label">The label.</param>
        /// <param name="inventorySystemDatabase">The inventory system database.</param>
        /// <param name="mainManagerWindow">The main manager window used to position the popup.</param>
        /// <param name="action">The actions that can be performed on the selected object.</param>
        /// <param name="preFilterCondition">The pre filter conditions can filter the fields objects.</param>
        public InventoryObjectField(string label,
            InventorySystemDatabase inventorySystemDatabase,
            Action<T> action,
            Func<T, bool> preFilterCondition) : this(label, inventorySystemDatabase, new[] { ("Select", action) }, preFilterCondition)
        {
        }

        /// <summary>
        /// Create the SearchableListWindow.
        /// </summary>
        /// <returns></returns>
        public abstract InventoryObjectSearchableListWindow<T> CreateSearchableListWindow(IList<(string, Action<T>)> actions, Func<T, bool> preFilterCondition);

        /// <summary>
        /// Make the field view name.
        /// </summary>
        /// <returns>The new field view name.</returns>
        protected abstract ViewName<T> MakeFieldViewName();

        /// <summary>
        /// Open the pop up window.
        /// </summary>
        private void ChangeButtonClick()
        {
            m_SearchableListWindow.OpenPopUpWindow(
                m_Button.worldBound.position,
                m_Button.worldBound.size);
        }

        /// <summary>
        /// Set the preFilter.
        /// </summary>
        /// <param name="preFilterCondition">The prefilter condition.</param>
        public virtual void SetPreFilter(Func<T, bool> preFilterCondition)
        {
            m_SearchableListWindow.SetPreFilter(preFilterCondition);
        }

        /// <summary>
        /// Set if interactable.
        /// </summary>
        /// <param name="interactable">interactable.</param>
        public virtual void SetInteractable(bool interactable)
        {
            m_Button.SetEnabled(interactable);
            m_Button.visible = interactable;
        }

        /// <summary>
        /// Refresh the ObjectField.
        /// </summary>
        /// <param name="obj">The new object to view.</param>
        public void Refresh(T obj)
        {
            if (obj == null || obj.Equals(null)) { obj = null; }

            m_ViewName.Refresh(obj);
            
            // DO NOT Refresh(); because it causes an infinite loop with the searchable list window.
        }

        /// <summary>
        /// Refresh the ObjectField.
        /// </summary>
        public void Refresh()
        {
            m_ViewName.Refresh(Value);
            m_SearchableListWindow.Refresh();
        }
    }
}