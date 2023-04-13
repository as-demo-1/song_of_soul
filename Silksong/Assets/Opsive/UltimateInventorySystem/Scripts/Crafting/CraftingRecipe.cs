/// ---------------------------------------------
/// Ultimate Inventory System.
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Crafting
{
    using Opsive.Shared.Utility;
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using Opsive.UltimateInventorySystem.Utility;
    using UnityEngine;

    /// <summary>
    /// Crafting recipe, used as data by a crafting processor to know the category, ingredients and outputs 
    /// </summary>
    public class CraftingRecipe : ScriptableObject, ICategoryElement<CraftingCategory, CraftingRecipe>, IObjectWithID, IDirtyable
    {
        [Tooltip("The ID of the category.")]
        [SerializeField] [HideInInspector] protected uint m_ID;
        [Tooltip("Crafting recipe category, used to organize recipes.")]
        [SerializeField] [HideInInspector] protected CraftingCategory m_Category;
        [Tooltip("Recipe Ingredients Serialized data.")]
        [SerializeField] [HideInInspector] protected Serialization m_IngredientsData;
        [Tooltip("Recipe Default Output Serialized data.")]
        [SerializeField] [HideInInspector] protected Serialization m_DefaultOutputData;

        [Tooltip("Recipe ingredients.")]
        [System.NonSerialized] [HideInInspector] protected CraftingIngredients m_Ingredients;
        [Tooltip("Recipe output, result of crafting this recipe.")]
        [System.NonSerialized] [HideInInspector] protected CraftingOutput m_DefaultOutput;

#if UNITY_EDITOR
        [Tooltip("Used in editor to add an icon next to the recipe name.")]
        [SerializeField] [HideInInspector] internal Sprite m_EditorIcon;
#endif

        [System.NonSerialized] [HideInInspector] protected bool m_Initialized = false;

        [HideInInspector] protected bool m_Dirty;
        internal bool Dirty {
            get => m_Dirty;
            set => m_Dirty = value;
        }
        bool IDirtyable.Dirty {
            get => Dirty;
            set => Dirty = value;
        }

        public uint ID {
            get => m_ID;
            internal set {
                if (m_ID == value) { return; }
                m_ID = value;
                m_Dirty = true;
            }
        }

        uint IObjectWithID.ID {
            get => ID;
            set {
                if (m_ID == value) { return; }
                m_ID = value;
                m_Dirty = true;
            }
        }

        public CraftingCategory Category => m_Category;
        public virtual CraftingIngredients Ingredients {
            get => m_Ingredients;
            internal set {
                if (m_Ingredients == value) { return; }
                m_Ingredients = value;
                m_Dirty = true;
            }
        }

        public virtual CraftingOutput DefaultOutput {
            get => m_DefaultOutput;
            internal set {
                if (m_DefaultOutput == value) { return; }
                m_DefaultOutput = value;
                m_Dirty = true;
            }
        }

        public bool IsInitialized => m_Initialized;
        public virtual ItemAmount? MainItemAmountOutput => m_DefaultOutput?.MainItemAmount;

        /// <summary>
        /// Creates a new recipe and copies as much data as possible from the otherRecipe. 
        /// </summary>
        /// <param name="name">The recipe name.</param>
        /// <param name="category">The crafting category.</param>
        /// <param name="otherRecipe">The other recipe.</param>
        /// <returns>The crafting recipe.</returns>
        internal static CraftingRecipe CreateFrom(string name, CraftingCategory category, CraftingRecipe otherRecipe)
        {
            var createdRecipe = Create(name, category);
            ReflectionUtility.ObjectCopy(otherRecipe, createdRecipe);
            createdRecipe.ID = RandomID.Generate();
            createdRecipe.name = name;
            createdRecipe.m_Category = category;

            createdRecipe.Initialize(true);

            return createdRecipe;
        }

        /// <summary>
        /// Creates an Crafting Recipe using a category.
        /// </summary>
        /// <param name="name">The recipe name.</param>
        /// <param name="category">The recipe category.</param>
        /// <returns>The crafting recipe.</returns>
        internal static CraftingRecipe Create(string name, CraftingCategory category)
        {
            if (category == null || category.IsAbstract) {
                Debug.LogWarning("Cannot create Recipe with null or abstract Category");
                return null;
            }
            // Initialization.
            var createdRecipe = CreateInstance(category.RecipeType) as CraftingRecipe;
            createdRecipe.ID = RandomID.Generate();
            createdRecipe.name = name;
            createdRecipe.m_Category = category;

            if (createdRecipe.m_Category != null) {
                createdRecipe.m_Category.AddElement(createdRecipe);
            }

            createdRecipe.OnCreate();

            // Register.
            var result = createdRecipe.Initialize(true);

            if (result == false) {
                return null;
            }

            return createdRecipe;
        }

        /// <summary>
        /// Called on a newly created crafting recipe is created.
        /// </summary>
        protected virtual void OnCreate()
        {
            //Nothing here but it is useful for overriding.
        }

        /// <summary>
        /// Initializes the crafting recipe.
        /// </summary>
        /// <returns>True if initialized correctly.</returns>
        public virtual bool Initialize(bool force)
        {
            if (m_Initialized && !force) { return true; }

            if (m_Category == null) {
                return false;
            }

            if (m_Category.RecipeType == null || m_Category.RecipeType.Name == "NULL") {
                Debug.LogWarning($"'{this}' has a category recipe type '{m_Category.RecipeType}' (null).");
                return false;
            }

            if (m_Category.RecipeType != GetType() && !GetType().IsSubclassOf(m_Category.RecipeType)) {
                Debug.LogWarning($"'{this}' has a category recipe type '{m_Category.RecipeType}' that is not a subclass of this recipe type {GetType()}.");
                return false;
            }

            Deserialize();

            m_Category.AddElement(this);

            // It is required to check .Equals as the interface could point to a unity Object.
            if (InterfaceUtility.IsNotNull(m_Category.Manager)) {
                var success = m_Category.Manager.Register?.CraftingRecipeRegister.Register(this);
                if (success.HasValue) {
                    m_Initialized = success.Value;
                    return success.Value;
                }
            }

#if UNITY_EDITOR
            if (!Application.isPlaying) {
                m_Initialized = true;
                return true;
            }
#endif

            m_Initialized = InventorySystemManager.CraftingRecipeRegister.Register(this);
            return m_Initialized;
        }

        /// <summary>
        /// Deserialize all the properties of the ItemDefinition.
        /// </summary>
        internal void Deserialize()
        {
            DeserializeIngredients();
            DeserializeOutputs();
        }

        /// <summary>
        /// Initialize the output data.
        /// </summary>
        protected void DeserializeOutputs()
        {
            if (m_DefaultOutputData != null && m_DefaultOutputData.Values != null && m_DefaultOutputData.Values.Length > 0) {
                var obj = m_DefaultOutputData.DeserializeFields(MemberVisibility.Public);
                m_DefaultOutput = obj != null ? obj as CraftingOutput : new CraftingOutput();
            } else {
                m_DefaultOutput = new CraftingOutput();
            }

            DeserializeDefaultOutputInternal();
        }

        /// <summary>
        /// This ensures that the crafting recipe is of the exact CraftingOutputType and not a extension.
        /// </summary>
        protected virtual void DeserializeDefaultOutputInternal()
        {
            if (m_DefaultOutput.GetType() == typeof(CraftingOutput)) { return; }

            var previousOutput = m_Ingredients;
            m_DefaultOutput = new CraftingOutput();
            ReflectionUtility.ObjectCopy(previousOutput, m_DefaultOutput);
        }

        /// <summary>
        /// Initialize the ingredient data.
        /// </summary>
        protected void DeserializeIngredients()
        {
            if (m_IngredientsData != null && m_IngredientsData.Values != null && m_IngredientsData.Values.Length > 0) {
                var obj = m_IngredientsData.DeserializeFields(MemberVisibility.Public);
                m_Ingredients = obj != null ? obj as CraftingIngredients : new CraftingIngredients();
            } else {
                m_Ingredients = new CraftingIngredients();
            }

            DeserializeIngredientsInternal();
        }

        /// <summary>
        /// This ensures that the crafting recipe is of the exact CraftingIngredientType and not a extension.
        /// </summary>
        protected virtual void DeserializeIngredientsInternal()
        {
            if (m_Ingredients.GetType() == typeof(CraftingIngredients)) { return; }

            var previousIngredients = m_Ingredients;
            m_Ingredients = new CraftingIngredients();
            ReflectionUtility.ObjectCopy(previousIngredients, m_Ingredients);
        }

        /// <summary>
        /// Serialize the recipe
        /// </summary>
        public virtual void Serialize()
        {
            m_IngredientsData = Serialization.Serialize(m_Ingredients);
            m_DefaultOutputData = Serialization.Serialize(m_DefaultOutput);
        }

        /// <summary>
        /// Set the Category.
        /// </summary>
        /// <param name="newCategory">The new Category.</param>
        /// <returns>Returns a success or fail.</returns>
        public void SetCategoryWithoutNotify(CraftingCategory newCategory)
        {
            m_Category = newCategory;
        }

        /// <summary>
        /// Set the Category.
        /// </summary>
        /// <param name="newCategory">The new Category.</param>
        /// <returns>Returns a success or fail.</returns>
        public virtual bool SetCategory(CraftingCategory newCategory)
        {
            if (newCategory == null) {
                Debug.LogWarning("Cannot set null Category.");
                return false;
            }
            if (newCategory.IsAbstract) {
                Debug.LogWarning("Category is abstract, it cannot have elements added.");
                return false;
            }

            if (m_Category != null) {
                m_Category.RemoveElement(this);
            }

            m_Category = newCategory;
            m_Dirty = true;

            Initialize(true);

            return true;
        }

        /// <summary>
        /// Set a new crafting ingredients.
        /// </summary>
        /// <param name="newIngredients">Ingredients.</param>
        public virtual void SetIngredients(CraftingIngredients newIngredients)
        {
            if (newIngredients == null) { return; }
            m_Ingredients = newIngredients;
            m_Dirty = true;
        }

        /// <summary>
        /// Set a new output.
        /// </summary>
        /// <param name="newOutput">Output.</param>
        public virtual void SetOutput(CraftingOutput newOutput)
        {
            if (newOutput == null) { return; }
            m_DefaultOutput = newOutput;
            m_Dirty = true;
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