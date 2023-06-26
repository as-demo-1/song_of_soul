/// ---------------------------------------------
/// Ultimate Inventory System.
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Crafting
{
    using Opsive.Shared.Utility;
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Utility;
    using System;
    using UnityEngine;

    /// <summary>
    /// Crafting Recipe category, used to organize recipes in a sensible way.
    /// </summary>
    public class CraftingCategory : ObjectCategoryBase<CraftingCategory, CraftingRecipe>
    {
        [Tooltip("All recipes of this category should share the same type.")]
        [SerializeField] protected Type m_RecipeType;
        [Tooltip("AssemblyQualifiedName type is required to serialize a Type.")]
        [SerializeField] protected string m_RecipeTypeAssemblyQualifiedName;

        public Type RecipeType {
            get => m_RecipeType;
            internal set {
                if (m_RecipeType == value) { return; }

                m_RecipeType = value;
                m_RecipeTypeAssemblyQualifiedName = m_RecipeType.AssemblyQualifiedName;
                m_Dirty = true;
            }
        }

        /// <summary>
        /// Create a Category.
        /// </summary>
        /// <param name="name">The category name.</param>
        /// <param name="isAbstract">If abstract it cannot have direct recipes.</param>
        /// <param name="recipeType">The recipe type for the recipes within the crafting category.</param>
        /// <param name="manager">The inventory manager.</param>
        /// <returns>The crafting recipe category.</returns>
        internal static CraftingCategory Create(string name, bool isAbstract = false, Type recipeType = null, IInventorySystemManager manager = null)
        {
            //construct
            var craftingCategory = CreateInstance<CraftingCategory>();

            craftingCategory.Dirty = true;

            craftingCategory.m_Manager = manager;
            craftingCategory.ID = RandomID.Generate();
            craftingCategory.name = name;
            craftingCategory.m_IsAbstract = isAbstract;
            craftingCategory.m_RecipeType = (recipeType == null || recipeType.Name == "NULL") ? typeof(CraftingRecipe) : recipeType;
            craftingCategory.m_RecipeTypeAssemblyQualifiedName = craftingCategory.m_RecipeType.AssemblyQualifiedName;

#if UNITY_EDITOR
            UnityEngine.Random.InitState(name.GetHashCode());
            craftingCategory.m_Color = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value, 1);
#endif

            if (craftingCategory.Initialize(true) == false) {
                return null;
            }

            return craftingCategory;
        }

        /// <summary>
        /// Initializes the category and registers it.
        /// </summary>
        /// <returns>True if initialized correctly.</returns>
        public override bool Initialize(bool force)
        {
            if (m_Initialized && !force) { return true; }
            base.Initialize(force);

            // It is required to check .Equals as the interface could point to a unity Object.
            if (InterfaceUtility.IsNotNull(m_Manager)) {
                var success = m_Manager.Register?.CraftingCategoryRegister?.Register(this);
                if (success.HasValue) { return success.Value; }
            }

#if UNITY_EDITOR
            if (!Application.isPlaying) { return true; }
#endif

            if (InventorySystemManager.IsNull) { return false; }
            m_Manager = InventorySystemManager.Manager;
            return m_Manager.Register.CraftingCategoryRegister.Register(this);
        }

        /// <summary>
        /// Deserializes the recipe type.
        /// </summary>
        internal override void Deserialize()
        {
            base.Deserialize();

            m_RecipeType = TypeUtility.GetType(m_RecipeTypeAssemblyQualifiedName);
        }

        /// <summary>
        /// Serializes the recipe type.
        /// </summary>
        public override void Serialize()
        {
            base.Serialize();

            m_RecipeTypeAssemblyQualifiedName = m_RecipeType?.AssemblyQualifiedName;
        }

        /// <summary>
        /// Add an CraftingRecipe to the category, should only be used by the craftingRecipes when it is created.
        /// </summary>
        /// <param name="craftingRecipe">The new recipe.</param>
        /// <returns>Returns false if the recipe could not be added.</returns>
        protected override bool AddElementConditions(CraftingRecipe craftingRecipe)
        {
            var valid = base.AddElementConditions(craftingRecipe);
            if (valid == false) { return false; }

            if (RecipeType == null || RecipeType.ToString() == "NULL") {
                Debug.LogWarning("The crafting category recipe type should not be null.");
                return false;
            }

            if (RecipeType != craftingRecipe.GetType() && !craftingRecipe.GetType().IsSubclassOf(RecipeType)) {
                Debug.LogWarning($"The crafting category recipe type {RecipeType} does not match the recipe type {craftingRecipe.GetType()}.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// String name.
        /// </summary>
        /// <returns>The name.</returns>
        public override string ToString()
        {
            return name;
        }
    }
}
