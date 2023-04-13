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
    [ControlType(typeof(ItemDefinition))]
    public class ItemDefinitionControl : ControlWithInventoryDatabase
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
            ItemDefinitionField definitionField = null;
            definitionField = new ItemDefinitionField(null, m_Database, (newValue) =>
            {
                var result = onChangeEvent?.Invoke(newValue);
                if(result.HasValue && result.Value == false){ return; }

                if (definitionField.Value != newValue) {
                    definitionField.Refresh(newValue);
                }
            }, null);

            definitionField.Refresh(value as ItemDefinition);
            return definitionField;
        }
    }
    
    /// <summary>
    /// Implements TypeControlBase for the ObjectAmounts ControlType.
    /// </summary>
    [ControlType(typeof(DynamicItemDefinition))]
    public class DynamicItemDefinitionControl : ControlWithInventoryDatabase
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
            ItemDefinitionField objectField = null;
            objectField = new ItemDefinitionField(null, m_Database, (newValue) =>
            {
                var result = onChangeEvent?.Invoke(new DynamicItemDefinition(newValue));
                if(result.HasValue && result.Value == false){ return; }

                if (objectField.Value != newValue) {
                    objectField.Refresh(newValue);
                }
            }, null);

            ItemDefinition itemDefinition;
            if (value is DynamicItemDefinition dynamicItemDefinition) {
                itemDefinition = dynamicItemDefinition.Value;
            } else {
                itemDefinition = value as ItemDefinition;
            }
            objectField.Refresh(itemDefinition);
            return objectField;
        }
    }
    
    /// <summary>
    /// Implements TypeControlBase for the ObjectAmounts ControlType.
    /// </summary>
    [ControlType(typeof(DynamicItemDefinitionArray))]
    public class DynamicItemDefinitionArrayControl : ControlWithInventoryDatabase
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
            DynamicItemDefinitionArray dynamicItemDefinitionArray;
            if (value is DynamicItemDefinitionArray valueDynamicItemDefinitionArray) {
                dynamicItemDefinitionArray = valueDynamicItemDefinitionArray;
            } else {
                dynamicItemDefinitionArray = new DynamicItemDefinitionArray(value as ItemDefinition[]);
            }
            
            ItemDefinitionReorderableList objectField = null;
            objectField = new ItemDefinitionReorderableList(EditorUtility.SplitCamelCase(field.Name), m_Database,
                () =>
                {
                    return dynamicItemDefinitionArray.Value;
                },
                (newValue) =>
                {
                    var result = onChangeEvent?.Invoke(new DynamicItemDefinitionArray(newValue));
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
    [ControlType(typeof(ItemDefinitionAmount))]
    public class ItemDefinitionAmountControl : ControlWithInventoryDatabase
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
            return new ItemDefinitionAmountView(value as IObjectAmount<ItemDefinition>, m_Database, onChangeEvent);
        }
    }
    
    /// <summary>
    /// Implements TypeControlBase for the ObjectAmounts ControlType.
    /// </summary>
    [ControlType(typeof(ItemDefinitionAmounts))]
    public class ItemDefinitionAmountsControl : ControlWithInventoryDatabase
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
            return new ItemDefinitionAmountsView(value as ItemDefinitionAmounts, m_Database, onChangeEvent);
        }
    }

    /// <summary>
    /// Object Amounts view from ObjectAmountsBasView
    /// </summary>
    public class ItemDefinitionAmountsView : InventoryObjectAmountsView<ItemDefinitionAmounts, ItemDefinitionAmount, ItemDefinition>
    {
        /// <summary>
        /// Requires base constructor.
        /// </summary>
        /// <param name="objectAmounts">The object amounts.</param>
        /// <param name="database">The inventory system database.</param>
        /// <param name="onChangeEvent">The onChangeEvent function.</param>
        public ItemDefinitionAmountsView(ItemDefinitionAmounts objectAmounts, InventorySystemDatabase database, Func<object, bool> onChangeEvent) :
            base("Item Definition Amounts", objectAmounts, database, onChangeEvent)
        {
            m_SearchableListWindow = new ItemDefinitionSearchableListWindow(database, AddObjectAmount, null, false);
        }

        /// <summary>
        /// Create the ObjectAmountView.
        /// </summary>
        /// <returns>The ObjectAmountView.</returns>
        protected override InventoryObjectAmountView<ItemDefinition> CreateObjectAmountView()
        {
            return new ItemDefinitionAmountView(m_Database);
        }

        /// <summary>
        /// Create a default ObjectAmount.
        /// </summary>
        /// <returns>The default ObjectAmount.</returns>
        protected override ItemDefinitionAmount CreateObjectAmount(ItemDefinition itemDefinition)
        {
            return new ItemDefinitionAmount(1, itemDefinition);
        }

        /// <summary>
        /// Create a default ObjectAmounts
        /// </summary>
        /// <returns>The default ObjectAmounts.</returns>
        protected override ItemDefinitionAmounts CreateObjectAmounts()
        {
            return new ItemDefinitionAmounts();
        }
    }

    /// <summary>
    /// ObjectAmounts View from ObjectAmountBaseView
    /// </summary>
    public class ItemDefinitionAmountView : InventoryObjectAmountView<ItemDefinition>
    {
        protected ItemDefinitionField m_ItemDefinitionField;
        public ItemDefinitionField ItemDefinitionField => m_ItemDefinitionField;

        protected override ItemDefinition ObjectFieldValue {
            get => m_ItemDefinitionField?.Value;
            set => m_ItemDefinitionField.Refresh(value);
        }

        /// <summary>
        /// The item definition amount view.
        /// </summary>
        /// <param name="database">The database.</param>
        public ItemDefinitionAmountView(InventorySystemDatabase database) : base(database)
        {
            m_ItemDefinitionField = new ItemDefinitionField("", m_Database, new (string, Action<ItemDefinition>)[]
            {
                ("Set Item Definition", (x) => InvokeOnValueChanged(CreateNewObjectAmount(x,m_IntegerField.value)))
            }, (x) => true);
            Add(m_ItemDefinitionField);
            m_ItemDefinitionField.Refresh();
        }
        
        /// <summary>
        /// The item definition amount view.
        /// </summary>
        /// <param name="database">The database.</param>
        public ItemDefinitionAmountView(IObjectAmount<ItemDefinition> defaultValue, InventorySystemDatabase database, Func<object, bool> onChangeEvent) : this(database)
        {
            Refresh(defaultValue);
            OnValueChanged += (newValue) =>
            {
                var value = newValue == null ? new ItemDefinitionAmount(1, null) : (ItemDefinitionAmount)newValue;
                var result = onChangeEvent?.Invoke(value);
                if(result.HasValue && result.Value == false){ return; }
                Refresh(value);
            };
        }

        /// <summary>
        /// Refresh the object icon.
        /// </summary>
        public override void RefreshInternal()
        {
            m_ItemDefinitionField.Refresh(m_ObjectAmount.Object);
        }

        /// <summary>
        /// Create an objectAmount.
        /// </summary>
        /// <param name="obj">The new Object.</param>
        /// <param name="amount">The new Amount.</param>
        /// <returns>The ObjectAmount.</returns>
        public override IObjectAmount<ItemDefinition> CreateNewObjectAmount(ItemDefinition obj, int amount)
        {
            return new ItemDefinitionAmount(obj, amount);
        }

        /// <summary>
        /// Set if the Amount View is interactable
        /// </summary>
        /// <param name="interactable">true if interactable.</param>
        public override void SetInteractable(bool interactable)
        {
            m_Interactable = interactable;

            m_IntegerField.SetEnabled(m_Interactable);
            m_ItemDefinitionField.SetInteractable(m_Interactable);
        }

        /// <summary>
        /// Show the Type in the name
        /// </summary>
        /// <param name="showType">show or hide the type.</param>
        public override void SetShowType(bool showType)
        {
            m_ShowType = showType;
            m_ItemDefinitionField.ViewName.SetShowType(showType);
        }
    }
}