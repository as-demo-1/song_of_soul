/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.Styles
{
    using Opsive.Shared.Editor.Utility;
    using UnityEngine.UIElements;

    /// <summary>
    /// A static class that compiles the common styles.
    /// </summary>
    public static class CommonStyles
    {
        public static StyleSheet StyleSheet =>
            EditorUtility.LoadAsset<StyleSheet>("4ff86fbeece30534ca88317c5f5702b9");

        public static readonly string s_VerticalLayout = "vertical-layout";
        public static readonly string s_HorizontalAlignCenter = "horizontal-align-center";
        public static readonly string s_AlignChildrenCenter = "align-children-center";

        public static readonly string s_AddListItemContainer = "add-list-item-container";

        public static readonly string s_FlexGrow = "flex-grow";
        public static readonly string s_FlexWrap = "flex-wrap";

        public static readonly string s_ReverseToggle = "reverse-toggle";

        public static readonly string s_SearchList = "search-list";
        public static readonly string s_SearchList_SearchSortContainer = "search-list__search-sort-container";
        public static readonly string s_SearchList_FilterPresetContainer = "search-list__filter-preset-container";

        public static readonly string s_ObjectPreview = "object-preview";
        public static readonly string s_ObjectPreviewSmall = "object-preview-small";

        public static readonly string s_ShrinkZero = "shrink-zero";
    }
}
