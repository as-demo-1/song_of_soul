/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.Managers
{
    using Opsive.Shared.Editor.UIElements;
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using Opsive.UltimateInventorySystem.Crafting;
    using Opsive.UltimateInventorySystem.Editor.Styles;
    using Opsive.UltimateInventorySystem.Editor.VisualElements.ControlTypes;
    using Opsive.UltimateInventorySystem.Editor.VisualElements.ViewNames;
    using Opsive.UltimateInventorySystem.Exchange;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UIElements;
    using Object = UnityEngine.Object;

    /// <summary>
    /// Contains static functions used by multiple managers
    /// </summary>
    public static class ManagerUtility
    {
        /// <summary>
        /// Create a viewName for an object of unknown type.
        /// </summary>
        /// <param name="obj">The object to create a ViewName for.</param>
        /// <returns>The viewName.</returns>
        public static ViewName CreateViewNameFor(object obj)
        {

            if (obj is ItemCategory) { return new ItemCategoryViewName(); }
            if (obj is ItemDefinition) { return new ItemDefinitionViewName(); }
            if (obj is CraftingCategory) { return new CraftingCategoryViewName(); }
            if (obj is CraftingRecipe) { return new CraftingRecipeViewName(); }
            if (obj is Currency) { return new CurrencyViewName(); }

            if (obj is ItemCategoryAmount) {
                var amountViewName = new ItemCategoryAmountView(InventoryMainWindow.Instance.Database);
                amountViewName.SetInteractable(false);
                return amountViewName;
            }

            if (obj is ItemDefinitionAmount) {
                var amountViewName = new ItemDefinitionAmountView(InventoryMainWindow.Instance.Database);
                amountViewName.SetInteractable(false);
                return amountViewName;
            }

            if (obj is CurrencyAmount) {
                var amountViewName = new CurrencyAmountView(InventoryMainWindow.Instance.Database);
                amountViewName.SetInteractable(false);
                return amountViewName;
            }

            if (obj is ItemAmount) {
                var amountViewName = new ItemAmountView(InventoryMainWindow.Instance.Database);
                amountViewName.SetInteractable(false);
                return amountViewName;
            }

            return new ViewName<object>();
        }

        /// <summary>
        /// Creates a object preview visual element.
        /// </summary>
        /// <param name="objectPreview">The visual element.</returns>
        /// <param name="obj">The object.</param>
        public static void ObjectPreview(VisualElement objectPreview, Object obj)
        {
            if (objectPreview == null) { return; }

            objectPreview.AddToClassList(CommonStyles.s_ObjectPreview);
            if (obj == null) { return; }

            // GetAssetPreview is async therefore we need to refresh the view until it gets loaded.
            IVisualElementScheduledItem scheduleAction = null;
            scheduleAction = objectPreview.schedule.Execute(() =>
            {
                var needsRefresh = false;
                var texture = new StyleBackground(AssetPreview.GetAssetPreview(obj));
                if (texture == null) {
                    needsRefresh = true;
                    texture = new StyleBackground(AssetPreview.GetMiniThumbnail(obj));
                }

                objectPreview.style.backgroundImage = texture != null ? texture : objectPreview.style.backgroundImage;
                if (!needsRefresh) { scheduleAction?.Pause(); }
            });
            scheduleAction.Every(100);
        }

        /// <summary>
        /// Creates a object preview visual element.
        /// </summary>
        /// <param name="unityObject">The unity object containing the object.</param>
        /// <param name="target">The target object.</param>
        /// <param name="obj">The object.</param>
        /// <returns>The visual element.</returns>
        public static VisualElement ObjectFieldWithPreview(UnityEngine.Object unityObject,
            object target, Object obj)
        {
            var container = new VisualElement();
            var preview = new VisualElement();
            ObjectPreview(preview, obj);
            preview.style.width = 18;
            preview.style.height = 18;
            preview.style.marginRight = 5;
            container.Add(preview);

            FieldInspectorView.AddField(
                unityObject,
                target, null, -1, typeof(UnityEngine.Object),
                string.Empty, string.Empty, true,
                obj, container,
                (object o) => { });

            container.AddToClassList(InventoryManagerStyles.AttributeViewNameAndValue_Value);
            return container;
        }

        /// <summary>
        /// Search filter for the ItemCategory list
        /// </summary>
        /// <param name="list">The list to search.</param>
        /// <param name="searchValue">The search string.</param>
        /// <param name="searchOptions">The search options.</param>
        /// <returns>A new filtered list.</returns>
        public static IList<T> SearchFilter<T>(IList<T> list, string searchValue,
            (string prefix, Func<string, T, bool>)[] searchOptions)
        where T : ScriptableObject
        {
            var searchWords = searchValue.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
            var compareInfo = CultureInfo.CurrentCulture.CompareInfo;
            var newList = new List<T>();
            for (int i = 0; i < list.Count; ++i) {
                var itemCategory = list[i];
                if (itemCategory == null) { continue; }

                var addElement = false;
                var additiveTags = false;

                // Match search tags additively.
                for (int j = 0; j < searchWords.Length; j++) {

                    var searchWord = searchWords[j];
                    addElement = false;

                    // Search by name.
                    if (searchWord.Contains(":") == false) {
                        // Case insensitive Contains(string).
                        if (compareInfo.IndexOf(itemCategory.name, searchWord, CompareOptions.IgnoreCase) >= 0) {
                            addElement = true;
                            if (additiveTags) { break; }
                        }
                        if (addElement == false) { break; } else { continue; }
                    }

                    //search options
                    for (int k = 0; k < searchOptions.Length; k++) {
                        var searchPrefix = searchOptions[k].prefix;
                        var searchFunction = searchOptions[k].Item2;
                        if (searchWord.StartsWith(searchPrefix)) {
                            var optionSearchWord = searchWord.Remove(0, 2);
                            var optionAddElement = false;

                            optionAddElement = searchFunction(optionSearchWord, itemCategory);

                            if (optionAddElement) {
                                addElement = true;
                                if (additiveTags) { break; }
                            }

                            if (addElement == false) { break; } else { continue; }
                        }
                    }
                }

                if (addElement) {
                    newList.Add(list[i]);
                }
            }

            return newList;
        }
    }
}
