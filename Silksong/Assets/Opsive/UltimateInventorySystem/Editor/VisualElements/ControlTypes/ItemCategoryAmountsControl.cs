/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.VisualElements.ControlTypes
{
    using Opsive.Shared.Editor.UIElements.Controls;
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using Opsive.UltimateInventorySystem.Storage;
    using System;
    using System.Reflection;
    using Opsive.Shared.Editor.Utility;
    using UnityEngine.UIElements;

    /// <summary>
    /// Implements TypeControlBase for the ObjectAmounts ControlType.
    /// </summary>
    [ControlType(typeof(ItemCategory))]
    public class ItemCategoryControl : ControlWithInventoryDatabase
    {
        /// <summary>
        /// Create the ObjectAmountsView.
        /// </summary>
        /// <param name="value">The default value.</param>
        /// <param name="onChangeEvent">The on change event function.</param>
        /// <param name="field"></param>
        /// <returns>The ObjectAmountsView.</returns>
        public override VisualElement CreateCustomControlVisualElement(object value, Func<object, bool> onChangeEvent,
            FieldInfo field)
        {
            ItemCategoryField objectField = null;
            objectField = new ItemCategoryField(null, m_Database, (newValue) =>
            {
                var result = onChangeEvent?.Invoke(newValue);
                if(result.HasValue && result.Value == false){ return; }

                if (objectField.Value != newValue) {
                    objectField.Refresh(newValue);
                }
            }, null);

            objectField.Refresh(value as ItemCategory);
            return objectField;
        }
    }
    
    /// <summary>
    /// Implements TypeControlBase for the ObjectAmounts ControlType.
    /// </summary>
    [ControlType(typeof(DynamicItemCategory))]
    public class DynamicItemCategoryControl : ControlWithInventoryDatabase
    {
        /// <summary>
        /// Create the ObjectAmountsView.
        /// </summary>
        /// <param name="value">The default value.</param>
        /// <param name="onChangeEvent">The on change event function.</param>
        /// <param name="field"></param>
        /// <returns>The ObjectAmountsView.</returns>
        public override VisualElement CreateCustomControlVisualElement(object value, Func<object, bool> onChangeEvent,
            FieldInfo field)
        {
            ItemCategoryField objectField = null;
            objectField = new ItemCategoryField(null, m_Database, (newValue) =>
            {
                var result = onChangeEvent?.Invoke(new DynamicItemCategory(newValue));
                if(result.HasValue && result.Value == false){ return; }

                if (objectField.Value != newValue) {
                    objectField.Refresh(newValue);
                }
            }, null);

            ItemCategory itemCategory;
            if (value is DynamicItemCategory dynamicItemCategory) {
                itemCategory = dynamicItemCategory.Value;
            } else {
                itemCategory = value as ItemCategory;
            }
            objectField.Refresh(itemCategory);
            return objectField;
        }
    }
    
    /// <summary>
    /// Implements TypeControlBase for the ObjectAmounts ControlType.
    /// </summary>
    [ControlType(typeof(DynamicItemCategoryArray))]
    public class DynamicItemCategoryArrayControl : ControlWithInventoryDatabase
    {
        public override bool UseLabel => false;
        
        /// <summary>
        /// Create the ObjectAmountsView.
        /// </summary>
        /// <param name="value">The default value.</param>
        /// <param name="onChangeEvent">The on change event function.</param>
        /// <param name="field"></param>
        /// <returns>The ObjectAmountsView.</returns>
        public override VisualElement CreateCustomControlVisualElement(object value, Func<object, bool> onChangeEvent,
            FieldInfo field)
        {
            DynamicItemCategoryArray dynamicItemCategoryArray;
            if (value is DynamicItemCategoryArray valueDynamicItemCategoryArray) {
                dynamicItemCategoryArray = valueDynamicItemCategoryArray;
            } else {
                dynamicItemCategoryArray = new DynamicItemCategoryArray(value as ItemCategory[]);
            }
            
            ItemCategoryReorderableList objectField = null;
            objectField = new ItemCategoryReorderableList(EditorUtility.SplitCamelCase(field.Name), m_Database,
                () =>
                {
                    return dynamicItemCategoryArray.Value;
                },
                (newValue) =>
                {
                    var result = onChangeEvent?.Invoke(new DynamicItemCategoryArray(newValue));
                    if (result.HasValue && result.Value == false) {
                        return;
                    }
                });

            objectField.Refresh();
            return objectField;
        }
    }
    
    /// <summary>
    /// Implements TypeControlBase for the ObjectAmounts ControlType.
    /// </summary>
    [ControlType(typeof(ItemCategoryAmount))]
    public class ItemCategoryAmountControl : ControlWithInventoryDatabase
    {
        /// <summary>
        /// Create the ObjectAmountsView.
        /// </summary>
        /// <param name="value">The default value.</param>
        /// <param name="onChangeEvent">The on change event function.</param>
        /// <param name="field"></param>
        /// <returns>The ObjectAmountsView.</returns>
        public override VisualElement CreateCustomControlVisualElement(object value, Func<object, bool> onChangeEvent,
            FieldInfo field)
        {
            return new ItemCategoryAmountView(value as IObjectAmount<ItemCategory>, m_Database, onChangeEvent);
        }
    }
    
    /// <summary>
    /// Implements TypeControlBase for the ObjectAmounts ControlType.
    /// </summary>
    [ControlType(typeof(ItemCategoryAmounts))]
    public class ItemCategoryAmountsControl : ControlWithInventoryDatabase
    {
        /// <summary>
        /// Create the ObjectAmountsView.
        /// </summary>
        /// <param name="value">The default value.</param>
        /// <param name="onChangeEvent">The on change event function.</param>
        /// <param name="field"></param>
        /// <returns>The ObjectAmountsView.</returns>
        public override VisualElement CreateCustomControlVisualElement(object value, Func<object, bool> onChangeEvent,
            FieldInfo field)
        {
            return new ItemCategoryAmountsView(value as ItemCategoryAmounts, m_Database, onChangeEvent);
        }
    }

    /// <summary>
    /// Object Amounts view from ObjectAmountsBasView
    /// </summary>
    public class ItemCategoryAmountsView : InventoryObjectAmountsView<ItemCategoryAmounts, ItemCategoryAmount, ItemCategory>
    {
        /// <summary>
        /// Requires base constructor.
        /// </summary>
        /// <param name="objectAmounts">The object amounts.</param>
        /// <param name="database">The inventory system database.</param>
        /// <param name="onChangeEvent">The onChangeEvent function.</param>
        public ItemCategoryAmountsView(ItemCategoryAmounts objectAmounts, InventorySystemDatabase database, Func<object, bool> onChangeEvent) :
            base("Item Category Amounts", objectAmounts, database, onChangeEvent)
        {
            m_SearchableListWindow = new ItemCategorySearchableListWindow(database, AddObjectAmount, null, false);
        }

        /// <summary>
        /// Create the ObjectAmountView.
        /// </summary>
        /// <returns>The ObjectAmountView.</returns>
        protected override InventoryObjectAmountView<ItemCategory> CreateObjectAmountView()
        {
            return new ItemCategoryAmountView(m_Database);
        }

        /// <summary>
        /// Create a default ObjectAmount.
        /// </summary>
        /// <returns>The default ObjectAmount.</returns>
        protected override ItemCategoryAmount CreateObjectAmount(ItemCategory itemCategory)
        {
            return new ItemCategoryAmount(1, itemCategory);
        }

        /// <summary>
        /// Create a default ObjectAmounts
        /// </summary>
        /// <returns>The default ObjectAmounts.</returns>
        protected override ItemCategoryAmounts CreateObjectAmounts()
        {
            return new ItemCategoryAmounts();
        }
    }

    /// <summary>
    /// ObjectAmounts View from ObjectAmountBaseView
    /// </summary>
    public class ItemCategoryAmountView : InventoryObjectAmountView<ItemCategory>
    {
        protected ItemCategoryField m_ItemCategoryField;

        protected override ItemCategory ObjectFieldValue {
            get => m_ItemCategoryField?.Value;
            set => m_ItemCategoryField.Refresh(value);
        }

        /// <summary>
        /// The item category amount view.
        /// </summary>
        /// <param name="database">The database.</param>
        public ItemCategoryAmountView(InventorySystemDatabase database) : base(database)
        {
            m_ItemCategoryField = new ItemCategoryField("",
                m_Database,
                new (string, Action<ItemCategory>)[]
            {
                ("Set ItemCategory", (x) =>InvokeOnValueChanged(CreateNewObjectAmount(x,m_IntegerField.value)))
            }, (x) => true);
            Add(m_ItemCategoryField);
            m_ItemCategoryField.Refresh();
        }
        
        /// <summary>
        /// The item definition amount view.
        /// </summary>
        /// <param name="database">The database.</param>
        public ItemCategoryAmountView(IObjectAmount<ItemCategory> defaultValue, InventorySystemDatabase database, Func<object, bool> onChangeEvent) : this(database)
        {
            Refresh(defaultValue);
            OnValueChanged += (newValue) =>
            {
                var value = newValue == null ? new ItemCategoryAmount(1, null) : (ItemCategoryAmount)newValue;
                var result = onChangeEvent?.Invoke(value);
                if(result.HasValue && result.Value == false){ return; }
                Refresh(value);
            };
        }
        

        /// <summary>
        /// Refresh the object Icon.
        /// </summary>
        public override void RefreshInternal()
        {
            m_ItemCategoryField.Refresh(m_ObjectAmount.Object);
        }

        /// <summary>
        /// Create an objectAmount.
        /// </summary>
        /// <param name="obj">The new Object.</param>
        /// <param name="amount">The new Amount.</param>
        /// <returns>The ObjectAmount.</returns>
        public override IObjectAmount<ItemCategory> CreateNewObjectAmount(ItemCategory obj, int amount)
        {
            return new ItemCategoryAmount(obj, amount);
        }

        /// <summary>
        /// Set if the Amount View is interactable
        /// </summary>
        /// <param name="interactable">true if interactable.</param>
        public override void SetInteractable(bool interactable)
        {
            m_Interactable = interactable;

            m_IntegerField.SetEnabled(m_Interactable);
            m_ItemCategoryField.SetInteractable(m_Interactable);
        }

        /// <summary>
        /// Show the Type in the name
        /// </summary>
        /// <param name="showType">Show or hide the type.</param>
        public override void SetShowType(bool showType)
        {
            m_ShowType = showType;
            m_ItemCategoryField.ViewName.SetShowType(showType);
        }
    }
}