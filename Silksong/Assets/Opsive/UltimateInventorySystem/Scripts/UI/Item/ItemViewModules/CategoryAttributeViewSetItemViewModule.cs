/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Item.ItemViewModules
{
    using Opsive.Shared.Game;
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using Opsive.UltimateInventorySystem.UI.DataContainers;
    using Opsive.UltimateInventorySystem.UI.Item.AttributeViewModules;
    using Opsive.UltimateInventorySystem.UI.Views;
    using UnityEngine;

    /// <summary>
    /// Item View component that shows the item name.
    /// </summary>
    public class CategoryAttributeViewSetItemViewModule : ItemViewModule
    {
        [Tooltip("The categories attribute UIs.")]
        [SerializeField] internal CategoryAttributeViewSet m_CategoryAttributeViewSet;
        [Tooltip("The attribute UIs parent.")]
        [SerializeField] protected RectTransform m_AttributeUIParent;

        /// <summary>
        /// Initialize the category attribute view set item view module.
        /// </summary>
        /// <param name="view">The view for this module.</param>
        public override void Initialize(View view)
        {
            base.Initialize(view);

            if (m_CategoryAttributeViewSet == null) {
                return;
            }

            var categoriesAttributeUIs = m_CategoryAttributeViewSet.CategoriesAttributeBoxArray;

            for (int i = 0; i < categoriesAttributeUIs.Length; i++) {

                var categoryAttributeUIs = categoriesAttributeUIs[i];

                if (categoryAttributeUIs.Category == null) {
                    Debug.LogError($"ERROR: The item description has a category attribute view set '{m_CategoryAttributeViewSet.name}' with null categories and or categories from another database at index {i}.", gameObject);
                }
            }
        }

        /// <summary>
        /// Set the value.
        /// </summary>
        /// <param name="info">The item info.</param>
        public override void SetValue(ItemInfo info)
        {
            if (info.Item == null) {
                Clear();
                return;
            }

            UpdateAttributeUIs();
        }

        /// <summary>
        /// Clear the value.
        /// </summary>
        public override void Clear()
        {
            RemoveAttributeUIs();
        }

        /// <summary>
        /// Update the attribute ui.
        /// </summary>
        protected virtual void UpdateAttributeUIs()
        {
            RemoveAttributeUIs();
            if (m_CategoryAttributeViewSet == null) { return; }

            var categoriesAttributeUIs = m_CategoryAttributeViewSet.CategoriesAttributeBoxArray;

            for (int i = 0; i < categoriesAttributeUIs.Length; i++) {

                var categoryAttributeUIs = categoriesAttributeUIs[i];
                if (categoryAttributeUIs.Category.InherentlyContains(ItemInfo.Item) == false) { continue; }

                for (int j = 0; j < categoryAttributeUIs.AttributeViews.Length; j++) {

                    var attributeNameObject = categoryAttributeUIs.AttributeViews[j];
                    if (!ItemInfo.Item.TryGetAttribute(attributeNameObject.Name, out var attribute)) {
                        continue;
                    }

                    var attributeUIGameObject = ObjectPool.Instantiate(attributeNameObject.Object, m_AttributeUIParent, false);
                    attributeUIGameObject.transform.localScale = Vector3.one;

                    var attributeUI = attributeUIGameObject.GetComponent<AttributeView>();

                    attributeUI.SetValue((attribute, ItemInfo));
                }
            }
        }

        /// <summary>
        /// Remove the attribute ui.
        /// </summary>
        protected void RemoveAttributeUIs()
        {
            for (int i = m_AttributeUIParent.childCount - 1; i >= 0; i--) {
                var childObject = m_AttributeUIParent.GetChild(i).gameObject;
                if (ObjectPool.IsPooledObject(childObject)) {
                    ObjectPool.Destroy(childObject);
                } else {
                    Destroy(childObject);
                }
            }
        }
    }
}