/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.VisualElements
{
    using Opsive.UltimateInventorySystem.Editor.Styles;
    using System;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UIElements;

    public enum IconOption
    {
        Cog,
        QuestionMark,
        MagnifyingGlass
    }

    public static class IconOptionUtility
    {
        public static Texture2D GetIcon(IconOption icon)
        {
            switch (icon) {
                case IconOption.Cog:
                    return Shared.Editor.Utility.EditorUtility.LoadAsset<Texture2D>(
                        EditorGUIUtility.isProSkin
                            ? "8b097a15afc724641b5dfbf342ce4940"
                            : "01d35754fb059ea45b7bd731f4ede1cb");
                case IconOption.QuestionMark:
                    return /*Shared.Editor.Utility.EditorUtility.LoadAsset<Texture2D>(
                        EditorGUIUtility.isProSkin
                            ? "6012202242263741038"
                            : "6012202242263741038");*/
                        EditorGUIUtility.FindTexture("_Help@2x");
                case IconOption.MagnifyingGlass:
                    return Shared.Editor.Utility.EditorUtility.LoadAsset<Texture2D>(
                        EditorGUIUtility.isProSkin
                            ? "a2cd301cd71673749b65d1ce06332afa"
                            : "e26581d8c0102fb4e8812ee8c6d1b5fe");
                //EditorGUIUtility.FindTexture("UnityEditor.SceneHierarchyWindow@2x");
                default:
                    throw new ArgumentOutOfRangeException(nameof(icon), icon, null);
            }
        }
    }

    /// <summary>
    /// Unity button with an icon of choice.
    /// </summary>
    public class IconOptionButton : Button
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public IconOptionButton(IconOption icon)
        {
            RemoveFromClassList(ussClassName);
            AddToClassList(InventoryManagerStyles.SubMenuIconOption);
            style.backgroundImage = new StyleBackground(IconOptionUtility.GetIcon(icon));
        }
    }

    /// <summary>
    /// Unity button with an icon of choice.
    /// </summary>
    public class IconOptionImage : Image
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public IconOptionImage(IconOption icon)
        {
            RemoveFromClassList(ussClassName);
            AddToClassList(InventoryManagerStyles.SubMenuIconOption);
            style.backgroundImage = new StyleBackground(IconOptionUtility.GetIcon(icon));
        }
    }
}