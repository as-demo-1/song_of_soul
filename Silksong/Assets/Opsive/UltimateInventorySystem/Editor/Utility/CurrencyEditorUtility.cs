/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.Utility
{
    using Opsive.UltimateInventorySystem.Editor.Managers;
    using Opsive.UltimateInventorySystem.Editor.VisualElements;
    using Opsive.UltimateInventorySystem.Exchange;
    using Opsive.UltimateInventorySystem.Storage;
    using System.Collections.Generic;
    using System.Globalization;
    using UnityEditor;
    using UnityEngine;
    using Object = UnityEngine.Object;

    /// <summary>
    /// Static class that has useful function for currency.
    /// </summary>
    public static class CurrencyEditorUtility
    {
        /// <summary>
        /// Add an itemDefinition to the database.
        /// </summary>
        /// <param name="newCurrencyName">The itemDefinition name.</param>
        /// <param name="itemCategory">The itemCategory.</param>
        /// <param name="database">The database.</param>
        /// <param name="assetDirectory">The asset directory.</param>
        public static Currency AddCurrency(string newCurrencyName,
            InventorySystemDatabase database,
            string assetDirectory)
        {
            Undo.RegisterCompleteObjectUndo(database, "Add Currency");
            // Create the ScriptableObject representing the category and add the category to the database.
            var currency = Currency.Create(newCurrencyName);
            if (currency == null) {
                Debug.LogError("Error: The Currency cannot be created.");
                return null;
            }

            Undo.RegisterCreatedObjectUndo(currency, "Add Currency");

            database.AddCurrency(currency);

            AssetDatabaseUtility.CreateAsset(currency,
                $"{assetDirectory}\\Currencies\\{((Object)currency).name}",
                new string[] { database.name });

            Shared.Editor.Utility.EditorUtility.SetDirty(database);
            return currency;
        }

        /// <summary>
        /// Removes the Currency from any of its connections with the database
        /// </summary>
        /// <param name="currency">The currency to remove.</param>
        /// <param name="database">The database to remove from.</param>
        public static void RemoveCurrency(Currency currency, InventorySystemDatabase database)
        {
            if (currency == null) { return; }
            var allCurrencies = database.Currencies;
            for (int i = 0; i < allCurrencies.Length; i++) {
                var otherCurrency = allCurrencies[i];
                if (otherCurrency.Parent == currency) {
                    var rootExchangeRate = otherCurrency.GetRootExchangeRate();
                    SetParent(otherCurrency, rootExchangeRate.Currency);
                    SetExchangeRateToParent(otherCurrency, rootExchangeRate.ExchangeRate);

                    SetCurrencyDirty(otherCurrency, true);
                    SetCurrencyDirty(rootExchangeRate.Currency, true);
                }

                if (otherCurrency.Children.Contains(currency)) {
                    otherCurrency.Children.Remove(currency);

                    SetCurrencyDirty(otherCurrency, true);
                }

                if (otherCurrency.FractionCurrency == currency) {
                    otherCurrency.SetFractionCurrency(null);

                    SetCurrencyDirty(otherCurrency, true);
                }

                if (otherCurrency.OverflowCurrency == currency) {
                    otherCurrency.SetOverflowCurrency(null);

                    SetCurrencyDirty(otherCurrency, true);
                }

            }

            database.RemoveCurrency(currency);

            AssetDatabaseUtility.DeleteAsset(currency);
            Shared.Editor.Utility.EditorUtility.SetDirty(database);
        }

        /// <summary>
        /// Add an itemDefinition to the database.
        /// </summary>
        /// <param name="originalCurrency">The itemDefinition name.</param>
        /// <param name="itemCategory">The itemCategory.</param>
        /// <param name="database">The database.</param>
        /// <param name="assetDirectory">The asset directory.</param>
        public static Currency DuplicateCurrency(Currency originalCurrency,
            InventorySystemDatabase database,
            string assetDirectory)
        {
            Undo.RegisterCompleteObjectUndo(database, "Add Currency");

            var name = AssetDatabaseUtility.FindValidName(originalCurrency.name, database.Currencies);

            // Create the ScriptableObject representing the category and add the category to the database.
            var currency = Currency.Create(name, originalCurrency.Parent, originalCurrency.ExchangeRateToParent);
            if (currency == null) {
                Debug.LogError("Error: The Currency cannot be created.");
                return null;
            }

            currency.SetIcon(originalCurrency.Icon);
            currency.SetFractionCurrencyWithoutNotify(originalCurrency.FractionCurrency);
            currency.SetOverflowCurrencyWithoutNotify(originalCurrency.OverflowCurrency);
            currency.SetMaxAmount(originalCurrency.MaxAmount);

            Undo.RegisterCreatedObjectUndo(currency, "Add Currency");

            database.AddCurrency(currency);

            AssetDatabaseUtility.CreateAsset(currency,
                $"{assetDirectory}\\Currencies\\{((Object)currency).name}",
                new string[] { database.name });

            Shared.Editor.Utility.EditorUtility.SetDirty(database);

            SetCurrencyDirty(currency, true);

            if (currency.FractionCurrency != null) {
                SetCurrencyDirty(currency.FractionCurrency, true);
            }
            if (currency.OverflowCurrency != null) {
                SetCurrencyDirty(currency.OverflowCurrency, true);
            }
            if (currency.Parent != null) {
                SetCurrencyDirty(currency.Parent, true);
            }

            return currency;
        }

        /// <summary>
        /// Set ItemDefinition Dirty & sets the children array if necessary
        /// </summary>
        /// <param name="currency">The ItemDefinition to dirty.</param>
        /// <param name="force">Force the dirtying.</param>
        public static void SetCurrencyDirty(Currency currency, bool force)
        {
            if (currency.Dirty == false && !force) { return; }
            currency.Serialize();

            Shared.Editor.Utility.EditorUtility.SetDirty(currency);
            currency.Dirty = false;
        }

        /// <summary>
        /// Set the icon of the currency.
        /// </summary>
        /// <param name="currency">The currency.</param>
        /// <param name="icon">The sprite.</param>
        public static void SetIcon(Currency currency, Sprite icon)
        {
            Undo.RegisterCompleteObjectUndo(currency, "Set Icon");
            currency.SetIcon(icon);
            SetCurrencyDirty(currency, true);
        }

        /// <summary>
        /// Set the value for the protected currency BaseCurrency field
        /// </summary>
        /// <param name="currency">The currency.</param>
        /// <param name="newParentCurrency">The new parent.</param>
        public static void SetParent(Currency currency, Currency newParentCurrency)
        {
            var previousParent = currency.Parent;

            Undo.RegisterCompleteObjectUndo(currency, "Set Parent Currency");
            if (previousParent != null) { Undo.RegisterCompleteObjectUndo(previousParent, "Set Parent Currency"); }
            if (newParentCurrency != null) { Undo.RegisterCompleteObjectUndo(newParentCurrency, "Set Parent Currency"); }

            currency.SetParent(newParentCurrency);

            SetCurrencyDirty(currency, true);
            if (previousParent != null) { SetCurrencyDirty(previousParent, true); }
            if (newParentCurrency != null) { SetCurrencyDirty(newParentCurrency, true); }
        }

        /// <summary>
        /// Set the Fraction Currency.
        /// </summary>
        /// <param name="currency">The selected object.</param>
        /// <param name="newFractionCurrency">The currency.</param>
        public static void SetFractionCurrency(Currency currency, Currency newFractionCurrency)
        {
            var previousFractionCurrency = currency.FractionCurrency;

            Undo.RegisterCompleteObjectUndo(currency, "Set Fraction Currency");
            if (previousFractionCurrency != null) { Undo.RegisterCompleteObjectUndo(previousFractionCurrency, "Set Fraction Currency"); }
            if (newFractionCurrency != null) { Undo.RegisterCompleteObjectUndo(newFractionCurrency, "Set Fraction Currency"); }

            currency.SetFractionCurrency(newFractionCurrency);

            SetCurrencyDirty(currency, false);
            if (previousFractionCurrency != null) { SetCurrencyDirty(previousFractionCurrency, false); }
            if (newFractionCurrency != null) { SetCurrencyDirty(newFractionCurrency, false); }
        }

        /// <summary>
        /// Set the Overflow Currency.
        /// </summary>
        /// <param name="currency">The selected object.</param>
        /// <param name="newOverflowCurrency">The currency.</param>
        public static void SetOverflowCurrency(Currency currency, Currency newOverflowCurrency)
        {
            var previousOverflowCurrency = currency.OverflowCurrency;

            Undo.RegisterCompleteObjectUndo(currency, "Set Overflow Currency");
            if (previousOverflowCurrency != null) { Undo.RegisterCompleteObjectUndo(previousOverflowCurrency, "Set Overflow Currency"); }
            if (newOverflowCurrency != null) { Undo.RegisterCompleteObjectUndo(newOverflowCurrency, "Set Overflow Currency"); }

            currency.SetOverflowCurrency(newOverflowCurrency);

            SetCurrencyDirty(currency, false);
            if (previousOverflowCurrency != null) { SetCurrencyDirty(previousOverflowCurrency, false); }
            if (newOverflowCurrency != null) { SetCurrencyDirty(newOverflowCurrency, false); }
        }

        /// <summary>
        /// Set the value for the protected currency ExchangeRateToBase field
        /// </summary>
        /// <param name="currency">The currency.</param>
        /// <param name="newExchangeRateToBase">The new exchange to base value.</param>
        public static void SetExchangeRateToParent(Currency currency, double newExchangeRateToBase)
        {
            Undo.RegisterCompleteObjectUndo(currency, "Set Exchange Rate To Base");
            currency.SetParentExchangeRate(newExchangeRateToBase);
            SetCurrencyDirty(currency, false);
        }

        /// <summary>
        /// Set the value for the protected currency ExchangeRateToBase field
        /// </summary>
        /// <param name="currency">The currency.</param>
        /// <param name="newMaxAmount">The new max amount.</param>
        public static void SetMaxAmount(Currency currency, int newMaxAmount)
        {
            Undo.RegisterCompleteObjectUndo(currency, "Set Exchange Rate To Base");
            currency.SetMaxAmount(newMaxAmount);
            SetCurrencyDirty(currency, false);
        }

        /// <summary>
        /// Returns a list of options for sorting.
        /// </summary>
        /// <returns>The list of sort options.</returns>
        public static IList<SortOption> SortOptions()
        {
            return new SortOption[]
            {
                new SortOption("A-Z", list => (list as List<Currency>).Sort(
                    (x, y) =>
                    {
                        if (x == null) { return -1;}
                        if (y == null) { return 1; }
                        return ((Object) x)?.name.CompareTo(((Object) y)?.name ?? "") ?? 1;
                    })),
                new SortOption("Z-A", list => (list as List<Currency>).Sort(
                    (x, y) =>
                    {
                        if (x == null) { return -1;}
                        if (y == null) { return 1; }
                        return ((Object) y)?.name.CompareTo(((Object) x)?.name ?? "") ?? 1;
                    })),
            };
        }

        /// <summary>
        /// Search filter for the ItemDefinition list.
        /// </summary>
        /// <param name="list">The list to search.</param>
        /// <param name="searchValue">The search string.</param>
        /// <returns>A new filtered list.</returns>
        public static IList<Currency> SearchFilter(IList<Currency> list, string searchValue)
        {
            var compareInfo = CultureInfo.CurrentCulture.CompareInfo;

            return ManagerUtility.SearchFilter(list, searchValue, null);
        }
    }
}