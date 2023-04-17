/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.Styles
{
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>
    /// Specifies the names for the inventory manager style sheet.
    /// </summary>
    public static class InventoryManagerStyles
    {
        public static StyleSheet StyleSheet => Shared.Editor.Utility.EditorUtility.LoadAsset<StyleSheet>("dde60a90d31dc9d44be74c339427d85f");

        public static Texture2D UncategorizedIcon => Shared.Editor.Utility.EditorUtility.LoadAsset<Texture2D>("cae5ceed97260c84aa54154a126e1c78");
        public static Texture2D MissingCurrencyIcon => Shared.Editor.Utility.EditorUtility.LoadAsset<Texture2D>("70014ee6687184847a67dc36978f3001");
        
        public static string ManagerContentContainer => "manager-content-container";
        
        public static string LinkCursor => "link-cursor";

        public static string ColoredBox => "colored-box";
        public static string ColoredBox_Error => "colored-box__error";
        public static string ColoredBox_Preview => "colored-box__preview";

        public static string AttributeView_Margin => "attribute-view__margin";
        public static string AttributeViewNameAndValue_Value => "attribute-view-name-and-value__value";
        public static string AttributeViewName => "attribute-view-name";
        public static string AttributeViewName_Label => "attribute-view-name__label";
        public static string AttributeViewName_LabelSmall => "attribute-view-name__label-small";

        public static string AttributeValueField => "attribute-value-field";
        public static string AttributeOverrideValueField => "attribute-override-value-field";

        public static string AttributeBinding => "attribute-binding";
        public static string AttributeBindingObject => "attribute-binding__object";
        public static string AttributeBindingPopup => "attribute-binding__popup";

        public static string InventoryObjectField => "inventory-object-field";

        public static string WarningPopupWindow => "warning-popup-window";
        public static string WarningPopupWindow_Message => "warning-popup-window__message";
        public static string WarningPopupWindow_ButtonContainer => "warning-popup-window__button-container";

        public static string CurrencyFamilyItemView => "currency-family-item-view";
        public static string CurrencyFamilyItemView_LeftSide => "currency-family-item-view__left-side";
        public static string CurrencyFamilyItemView_RightSide => "currency-family-item-view__right-side";

        public static string CraftingRecipeView => "crafting-recipe-view";

        public static string SubMenu => "sub-menu";
        public static string SubMenuTitle => "sub-menu-title";
        public static string SubMenuButton => "sub-menu-button";
        public static string SubMenuTop => "sub-menu-top";
        public static string SubMenuIconOptions => "sub-menu-icon-options";
        public static string SubMenuIconOption => "sub-menu-icon-option";
        public static string SubMenuIconDescription => "sub-menu-description";
        public static string CreateDeleteSelectContainer => "create-delete-select-container";
    }
}
