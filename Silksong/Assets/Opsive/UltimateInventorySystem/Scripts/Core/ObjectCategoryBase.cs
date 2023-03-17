/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Core
{
    using Opsive.Shared.Utility;
    using Opsive.UltimateInventorySystem.Utility;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// The base class of category objects.
    /// </summary>
    public abstract class ObjectCategoryBase : ScriptableObject, IObjectWithID
    {
        [Tooltip("The ID of the category.")]
        [SerializeField] protected uint m_ID;
        [Tooltip("The serialization data for the parents of this category, used to inherit any properties of the parents.")]
        [SerializeField] protected Serialization m_ParentsData;
        [Tooltip("The serialization data for the direct children of this category, used to easily find the descendants without going through the entire database.")]
        [SerializeField] protected Serialization m_ChildrenData;
        [Tooltip("The serialization data for all the elements that use this category, used to change the elements in case the case the category is modified, without going through the entire database.")]
        [SerializeField] protected Serialization m_ElementsData;
        [Tooltip("An abstract category cannot have a direct connection to any CraftingRecipes, used to create a solid structure and pass down required attributes.")]
        [SerializeField] protected bool m_IsAbstract;
#if UNITY_EDITOR
        [Tooltip("Used in editor to differentiate categories visually.")]
        [SerializeField] internal Color m_Color;
        [Tooltip("Used in editor to add an icon next to the category name.")]
        [SerializeField] internal Sprite m_EditorIcon;
#endif

        [System.NonSerialized] protected bool m_Initialized = false;

        [System.NonSerialized] protected IInventorySystemManager m_Manager;

        public IInventorySystemManager Manager => m_Manager;

        protected bool m_Dirty;
        internal virtual bool Dirty {
            get => m_Dirty;
            set => m_Dirty = value;
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

        public bool IsInitialized => m_Initialized;

        public bool IsAbstract {
            get => m_IsAbstract;
            internal set {
                if (m_IsAbstract == value) { return; }
                m_IsAbstract = value;
                m_Dirty = true;
            }
        }

        /// <summary>
        /// Serialize the Category.
        /// </summary>
        public abstract void Serialize();

        /// <summary>
        /// Returns all object related to this category, meaning connected (parent & children) categories and craftingRecipes.
        /// </summary>
        /// <returns>An array of the family objects.</returns>
        public abstract UnityEngine.Object[] GetAllFamilyConnectedObjects();

        /// <summary>
        /// Returns all object that are children to this category, meaning children categories and craftingRecipes.
        /// </summary>
        /// <param name="includeThis">Include this category in the children.</param>
        /// <returns>An array of children Objects.</returns>
        public abstract UnityEngine.Object[] GetAllChildrenConnectedObjects(bool includeThis);
    }

    /// <summary>
    /// Generic class for category objects.
    /// </summary>
    /// <typeparam name="Tc">The category type.</typeparam>
    /// <typeparam name="Te">The element type.</typeparam>
    public class ObjectCategoryBase<Tc, Te> : ObjectCategoryBase//, ISerializationCallbackReceiver
        where Tc : ObjectCategoryBase<Tc, Te>
        where Te : ScriptableObject, ICategoryElement<Tc, Te>
    {

        protected ResizableArray<Tc> m_Parents;
        protected ResizableArray<Tc> m_Children;
        protected ResizableArray<Te> m_Elements;

        public IReadOnlyList<Tc> ParentsReadOnly => m_Parents;
        public IReadOnlyList<Tc> ChildrenReadOnly => m_Children;
        public IReadOnlyList<Te> ElementsReadOnly => m_Elements;

        internal ResizableArray<Tc> Parents => m_Parents;
        internal ResizableArray<Tc> Children => m_Children;
        internal ResizableArray<Te> Elements => m_Elements;

        /// <summary>
        /// Initializes the category and registers it.
        /// </summary>
        /// <returns>Returns false if not initialized correctly.</returns>
        public virtual bool Initialize(bool force)
        {
            if (m_Initialized && !force) { return true; }

            Deserialize();

            m_Initialized = true;

            return m_Initialized;
        }

        /*/// <summary>
        /// Unity Serialize Callback.
        /// </summary>
        public void OnBeforeSerialize()
        {
            Serialize();
        }

        /// <summary>
        /// Unity Deserialize Callback.
        /// </summary>
        public void OnAfterDeserialize()
        {
            Deserialize();
        }*/

        /// <summary>
        /// Serialize the Category.
        /// </summary>
        public override void Serialize()
        {
            SerializeParents();
            SerializeChildren();
            SerializeElements();
        }

        /// <summary>
        /// Serializes the Parents ArrayStruct to Serialization data.
        /// </summary>
        public virtual void SerializeParents()
        {
            m_Parents.Truncate();
            m_ParentsData = Serialization.Serialize(m_Parents);
        }

        /// <summary>
        /// Serializes the Children ArrayStruct to Serialization data.
        /// </summary>
        public virtual void SerializeChildren()
        {
            m_Children.Truncate();
            m_ChildrenData = Serialization.Serialize(m_Children);
        }

        /// <summary>
        /// Serializes the Elements ArrayStruct to Serialization data.
        /// </summary>
        public virtual void SerializeElements()
        {
            m_Elements.Truncate();
            m_ElementsData = Serialization.Serialize(m_Elements);
        }

        /// <summary>
        /// Deserialize the Category.
        /// </summary>
        internal virtual void Deserialize()
        {
            DeserializeParents();
            DeserializeChildren();
            DeserializeElements();
        }

        /// <summary>
        /// Deserializes the Parents ArrayStruct to Serialization data.
        /// </summary>
        protected virtual void DeserializeParents()
        {
            if (m_ParentsData != null && m_ParentsData.Values != null && m_ParentsData.Values.Length > 0) {
                var obj = m_ParentsData.DeserializeFields(MemberVisibility.Public);
                m_Parents = obj != null ? (ResizableArray<Tc>)obj : new ResizableArray<Tc>();
            } else {
                m_Parents = new ResizableArray<Tc>();
            }
        }

        /// <summary>
        /// Deserializes the Children ArrayStruct to Serialization data.
        /// </summary>
        protected virtual void DeserializeChildren()
        {
            if (m_ChildrenData != null && m_ChildrenData.Values != null && m_ChildrenData.Values.Length > 0) {
                var obj = m_ChildrenData.DeserializeFields(MemberVisibility.Public);
                m_Children = obj != null ? (ResizableArray<Tc>)obj : new ResizableArray<Tc>();
            } else {
                m_Children = new ResizableArray<Tc>();
            }
        }

        /// <summary>
        /// Deserializes the Elements ArrayStruct to Serialization data.
        /// </summary>
        protected virtual void DeserializeElements()
        {
            if (m_ElementsData != null && m_ElementsData.Values != null && m_ElementsData.Values.Length > 0) {
                var obj = m_ElementsData.DeserializeFields(MemberVisibility.Public);
                m_Elements = obj != null ? (ResizableArray<Te>)obj : new ResizableArray<Te>();
            } else {
                m_Elements = new ResizableArray<Te>();
            }
        }

        /// <summary>
        /// Add a multiple parents to the Item category.
        /// </summary>
        /// <param name="parents">The new additional parents.</param>
        public virtual void AddParents(IReadOnlyList<Tc> parents)
        {
            for (var i = 0; i < parents.Count; i++) {
                var parent = parents[i];
                AddParent(parent);
            }
        }

        /// <summary>
        /// Checks if the otherCategory can be set as a parent.
        /// </summary>
        /// <param name="otherCategory">The otherCategory.</param>
        /// <returns>True if the otherCategory could be set as a parent.</returns>
        public virtual bool AddParentCondition(Tc otherCategory)
        {
            if (otherCategory == null) {
                return false;
            }
            if (AllParentsContains(otherCategory, true)) {
                return false;
            }
            if (otherCategory.AllParentsContains((Tc)this, true)) {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Adds a parent to the category.
        /// </summary>
        /// <param name="newParent">The new parent.</param>
        /// <returns>Returns true if the parent was successfully added.</returns>
        public virtual bool AddParent(Tc newParent)
        {
            if (AddParentCondition(newParent) == false) { return false; }
            if (newParent == null) {
                return false;
            }

            m_Parents.Add(newParent);
            newParent.m_Children.Add(this);
            newParent.Dirty = true;
            m_Dirty = true;

            return true;
        }

        /// <summary>
        /// Remove a category parent.
        /// </summary>
        /// <param name="parent">The parent that needs to be removed.</param>
        /// <returns>True if the parent was successfully removed.</returns>
        public virtual bool RemoveParent(Tc parent)
        {
            if (m_Parents.Contains(parent) == false) {
                return false;
            }

            m_Parents.Remove(parent);
            m_Dirty = true;

            parent.m_Children.Remove(this);
            parent.Dirty = true;

            return true;
        }

        /// <summary>
        /// Add multiple Element to the category, should only be used by the Element when it is created.
        /// </summary>
        /// <param name="elements">The new additional elements.</param>
        public virtual void AddElements(IReadOnlyList<Te> elements)
        {
            for (var i = 0; i < elements.Count; i++) {
                AddElement(elements[i]);
            }
        }

        /// <summary>
        /// Add an Element to the category, should only be used by the element when it is created.
        /// </summary>
        /// <param name="element">The new Element.</param>
        /// <returns>Returns false if the element could not be added.</returns>
        public virtual bool AddElement(Te element)
        {
            if (AddElementConditions(element) == false) { return false; }

            if (m_Elements == null) {
                Debug.LogError($"The category: '{this}' was not initialized");
                return false;
            }

            for (int i = 0; i < m_Elements.Count; i++) {
                if (m_Elements[i] == element) {
                    return true;
                }
            }

            m_Elements.Add(element);
            m_Dirty = true;

            if (element is IDirtyable dirtyable) { dirtyable.Dirty = true; }

            return true;
        }

        /// <summary>
        /// Add an Element to the category, should only be used by the element when it is created.
        /// </summary>
        /// <param name="element">The new Element.</param>
        /// <returns>Returns false if the element could not be added.</returns>
        protected virtual bool AddElementConditions(Te element)
        {
            if (element == null) {
                return false;
            }

            if (m_IsAbstract) {
                return false;
            }

            //The element must be part be part of the Category to be added as an Element of the category.
            if (element.Category != this) {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Remove an Element from the category.
        /// </summary>
        /// <param name="element">The Element that needs to be removed.</param>
        /// <returns>Returns false if the element could not be removed.</returns>
        public virtual bool RemoveElement(Te element)
        {
            if (element.Category != this) {
                return false;
            }

            m_Elements?.Remove(element);
            m_Dirty = true;

            if (element is IDirtyable dirtyable) { dirtyable.Dirty = true; }

            return true;
        }

        /// <summary>
        /// Get the quantity of the Elements that are part of children categories.
        /// </summary>
        /// <param name="includeThis">True if the elements of this category should be included.</param>
        /// <returns>The count.</returns>
        public int GetAllChildrenElementCount(bool includeThis = true)
        {
            var count = 0;

            var itemCats = GenericObjectPool.Get<Tc[]>();
            var allChildrenCount = GetAllChildren(ref itemCats, includeThis);
            for (int i = 0; i < allChildrenCount; ++i) {
                count += itemCats[i].m_Elements.Count;
            }
            GenericObjectPool.Return(itemCats);

            return count;
        }

        /// <summary>
        /// Get all the Elements that are part of children categories.
        /// </summary>
        /// <param name="elements">Reference to an array of Elements, Can be resized up.</param>
        /// <param name="includeThis">True if the elements of this category should be included.</param>
        /// <returns>A count of the assigned elements of the array.</returns>
        public int GetAllChildrenElements(ref Te[] elements, bool includeThis = true)
        {
            var pooledChildren = GenericObjectPool.Get<Tc[]>();
            var allChildrenCount = GetAllChildren(ref pooledChildren, includeThis);
            var count = 0;
            for (int i = 0; i < allChildrenCount; ++i) {
                var childrenElements = pooledChildren[i]?.m_Elements;
                if (childrenElements == null) {
                    Debug.LogError($"The category {this} has a null or uninitialized child {pooledChildren[i]}. You may be referencing an object from another database by mistake.");
                    continue;
                }
                for (int j = 0; j < childrenElements.Count; j++) {

                    if (elements.Contains(childrenElements[j], 0, count)) { continue; }

                    TypeUtility.ResizeIfNecessary(ref elements, count);

                    elements[count] = childrenElements[j];
                    count++;
                }
            }
            GenericObjectPool.Return(pooledChildren);

            return count;
        }

        /// <summary>
        /// Get the quantity of the Elements that are part of the family of this category.
        /// </summary>
        /// <returns>The count.</returns>
        public int GetAllFamilyElementCount()
        {
            var count = 0;

            var itemCats = GenericObjectPool.Get<Tc[]>();
            var familyCount = GetAllFamily(ref itemCats);
            for (int i = 0; i < familyCount; ++i) {
                count += itemCats[i].m_Elements.Count;
            }
            GenericObjectPool.Return(itemCats);

            return count;
        }

        /// <summary>
        /// Get Elements that are part of the family of this category.
        /// </summary>
        /// <param name="elements">An array of Elements. Can be resized up.</param>
        /// <returns>A count of the assigned elements of the array.</returns>
        public int GetAllFamilyElements(ref Te[] elements)
        {
            var itemCats = GenericObjectPool.Get<Tc[]>();
            var familyCount = GetAllFamily(ref itemCats);
            var count = 0;
            for (int i = 0; i < familyCount; ++i) {
                var itemDefs = itemCats[i].m_Elements;
                for (int j = 0; j < itemDefs.Count; j++) {

                    if (elements.Contains(itemDefs[j], 0, count)) { continue; }

                    TypeUtility.ResizeIfNecessary(ref elements, count);

                    elements[count] = itemDefs[j];
                    count++;
                }
            }
            GenericObjectPool.Return(itemCats);

            return count;
        }

        /// <summary>
        /// Returns true if the category has a child that contains the element provided.
        /// </summary>
        /// <param name="element">The Element.</param>
        /// <param name="includeThis">If false it will not check for direct inheritance.</param>
        /// <returns>True if the element is inherited part of the category.</returns>
        public bool InherentlyContains(Te element, bool includeThis = true)
        {
            if (IsInitialized == false) {
                Debug.LogError($"The object \"{this}\" with ID {m_ID} is not part of the active database. " +
                               $"Please run the 'Replace Database Objects' script by right-clicking on the folder with the affected prefabs, scriptable objects, or scenes.");
                return false;
            }

            if (element == null) { return false; }

            if (includeThis && DirectlyContains(element)) {
                return true;
            }

            var childrenCount = m_Children.Count;
            var childrenArray = m_Children.Array;
            for (int i = 0; i < childrenCount; i++) {
                if (childrenArray[i].InherentlyContains(element, true)) {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns true if the category has a child that contains the category provided.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="includeThis">If false it will not check for direct inheritance.</param>
        /// <returns>True if the element is inherited part of the category.</returns>
        public bool InherentlyContains(Tc category, bool includeThis = true)
        {
            if (category == null) { return false; }

            if (includeThis && this == category) {
                return true;
            }

            var childrenCount = m_Children.Count;
            var childrenArray = m_Children.Array;
            for (int i = 0; i < childrenCount; i++) {
                if (childrenArray[i].InherentlyContains(category, true)) {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns true if the category directly contains the element provided.
        /// </summary>
        /// <param name="element">The Element.</param>
        /// <returns>True if the category directly contains the Element.</returns>
        public bool DirectlyContains(Te element)
        {
            if (element == null) { return false; }
            return m_Elements?.Contains(element) ?? false;
        }

        /// <summary>
        /// Get the quantity of all generations of children of this category.
        /// </summary>
        /// <param name="includeThis">If true will include this category.</param>
        /// <returns>The count.</returns>
        public int GetAllChildrenCount(bool includeThis = false)
        {
            var count = 0;
            if (includeThis) { count++; }

            var childrenCount = m_Children.Count;
            var childrenArray = m_Children.Array;
            for (int i = 0; i < childrenCount; i++) {
                count += 1 + childrenArray[i].GetAllChildrenCount();
            }

            return count;
        }

        /// <summary>
        /// Get all the children of this category.
        /// </summary>
        /// <param name="children">An array of Category, can be resized up.</param>
        /// <param name="includeThis">If trues this category will be included in the result.</param>
        /// <returns>A count of the assigned elements of the array.</returns>
        public int GetAllChildren(ref Tc[] children, bool includeThis)
        {
            return IterationHelper.GetAllRecursive(ref children, includeThis, (Tc)this, x => x.m_Children, true);
        }

        /// <summary>
        /// Returns all the children of the category as well as the difference of generation.
        /// For convenience it is sorted by generation.
        /// </summary>
        /// <param name="includeThis">Include this as a child.</param>
        /// <param name="level">The generation level in case you do not start from 0.</param>
        /// <returns>List of children and their level.</returns>
        public List<(Tc, int)> GetAllChildrenWithLevel(bool includeThis = false, int level = 0)
        {
            var childrenWithLevel = GetAllChildrenWithLevelInternal(includeThis, level);
            childrenWithLevel.Sort((x1, x2) => x1.Item2.CompareTo(x2.Item2));
            return childrenWithLevel;
        }

        /// <summary>
        /// Returns all the children of the category as well as the difference of generation.
        /// </summary>
        /// <param name="includeThis">Include this as a child.</param>
        /// <param name="level">The generation level in case you do not start from 0.</param>
        /// <returns>List of children and their level.</returns>
        protected List<(Tc, int)> GetAllChildrenWithLevelInternal(bool includeThis = false, int level = 0)
        {
            var allChildren = new List<(Tc, int)>();
            if (includeThis) {
                allChildren.Add(((Tc)this, level));
            }

            var childrenCount = m_Children.Count;
            var childrenArray = m_Children.Array;
            for (int i = 0; i < childrenCount; i++) {
                allChildren.Add((childrenArray[i], level + 1));
                allChildren.AddRange(childrenArray[i].GetAllChildrenWithLevelInternal(false, level + 1));
            }

            return allChildren;
        }

        /// <summary>
        /// Get quantity of all generations of parents.
        /// </summary>
        /// <param name="includeThis">Include this category.</param>
        /// <returns>The parent count.</returns>
        public int GetAllParentsCount(bool includeThis = false)
        {
            var count = 0;
            if (includeThis) { count++; }

            var parentCount = m_Parents.Count;
            var parentArray = m_Parents.Array;

            for (int i = 0; i < parentCount; i++) {
                count += 1 + parentArray[i].GetAllParentsCount();
            }

            return count;
        }

        /// <summary>
        /// Get all the parents of this category.
        /// </summary>
        /// <param name="parents">An array if Category, can be resized up.</param>
        /// <param name="includeThis">If trues this category will be included in the result.</param>
        /// <returns>A count of the assigned elements of the array.</returns>
        public int GetAllParents(ref Tc[] parents, bool includeThis = false)
        {
            return IterationHelper.GetAllRecursive(ref parents, includeThis, (Tc)this, x => x.m_Parents, true);
        }

        /// <summary>
        /// Get the parent at the index of all generations of parents.
        /// </summary>
        /// <param name="category">The category to find.</param>
        /// <param name="includeThis">Include this category.</param>
        /// <returns>The parent.</returns>
        public bool AllParentsContains(Tc category, bool includeThis = false)
        {
            if (includeThis) {
                if (category == this) { return true; }
            }

            var parentCount = m_Parents.Count;
            var parentArray = m_Parents.Array;

            for (int i = 0; i < parentCount; i++) {
                if (parentArray[i] == null) {
                    Debug.LogWarning($"A parent category of {this} is null. This should never be the case.");
                    continue;
                }
                if (category == parentArray[i]) { return true; }

                var matchFound = parentArray[i].AllParentsContains(category);
                if (matchFound) { return true; }
            }

            return false;
        }

        /// <summary>
        /// Get all generations of parent with the difference of generation.
        /// The list is sorted by generation.
        /// </summary>
        /// <param name="includeThis">Include this category as a parent.</param>
        /// <param name="level">The generation level in case you do not start from 0.</param>
        /// <returns>The list of categories and their levels.</returns>
        public List<(Tc, int)> GetAllParentsWithLevel(bool includeThis = false, int level = 0)
        {
            var parentsWithLevel = GetAllParentsWithLevelInternal(includeThis, level);
            parentsWithLevel.Sort((x1, x2) => x1.Item2.CompareTo(x2.Item2));
            return parentsWithLevel;
        }

        /// <summary>
        /// Get all generations of parent with the difference of generation.
        /// </summary>
        /// <param name="includeThis">Include this category as a parent.</param>
        /// <param name="level">The generation level in case you do not start from 0.</param>
        /// <returns>The list of categories and their levels.</returns>
        protected List<(Tc, int)> GetAllParentsWithLevelInternal(bool includeThis = false, int level = 0)
        {
            var allParents = new List<(Tc, int)>();
            if (includeThis) {
                allParents.Add(((Tc)this, level));
            }

            var parentCount = m_Parents.Count;
            var parentArray = m_Parents.Array;
            for (int i = 0; i < parentCount; i++) {
                allParents.Add((parentArray[i], level + 1));
                allParents.AddRange(parentArray[i].GetAllParentsWithLevelInternal(false, level + 1));
            }
            return allParents;
        }

        /// <summary>
        /// Get the number of roots of the category, Meaning its oldest parent.
        /// </summary>
        /// <returns>Root count.</returns>
        public int GetRootsCount()
        {
            var count = 0;
            var parentCount = m_Parents.Count;
            var parentArray = m_Parents.Array;
            if (parentCount == 0) {
                return 1;
            }

            for (int i = 0; i < parentCount; i++) {
                count += parentArray[i].GetRootsCount();
            }
            return count;
        }

        /// <summary>
        /// Get the roots of this category.
        /// </summary>
        /// <param name="roots">Reference to an array of ItemCategories, Can be resized up.</param>
        /// <returns>A count of the assigned elements of the array.</returns>
        public int GetRoots(ref Tc[] roots)
        {
            return IterationHelper.GetLeafRecursiveDFS<Tc>(ref roots, (Tc)this, x => x.m_Parents);
        }

        /// <summary>
        /// Get the entire family count of this category, all parents and children included.
        /// </summary>
        /// <returns>The family count.</returns>
        public int GetFullFamilyCount()
        {
            var count = 0;

            var itemCats = GenericObjectPool.Get<Tc[]>();
            var rootsCount = GetRoots(ref itemCats);
            for (int i = 0; i < rootsCount; i++) {
                count += itemCats[i].GetAllChildrenCount(true);
            }
            GenericObjectPool.Return(itemCats);

            return count;
        }

        /// <summary>
        /// Get the categories of the entire family of this category, all parents and children included.
        /// </summary>
        /// /// <param name="fullFamily">Reference to a Category array, Can be resized up.</param>
        /// <returns>A count of the assigned elements of the array.</returns>
        public int GetAllFamily(ref Tc[] fullFamily)
        {
            var itemCats = GenericObjectPool.Get<Tc[]>();
            var rootsCount = GetRoots(ref itemCats);
            var count = 0;
            for (int i = 0; i < rootsCount; i++) {
                count = IterationHelper.GetAllRecursiveDFS(ref fullFamily, count, itemCats[i], x => x.m_Children);
            }
            GenericObjectPool.Return(itemCats);

            return count;
        }

        /// <summary>
        /// Returns all object related to this category, meaning connected (parent & children) categories and craftingRecipes.
        /// </summary>
        /// <returns>An array of the family objects.</returns>
        public override UnityEngine.Object[] GetAllFamilyConnectedObjects()
        {
            var pooledCategoryFamily = GenericObjectPool.Get<Tc[]>();
            var categoryFamilyCount = GetAllFamily(ref pooledCategoryFamily);

            var pooledElementFamily = GenericObjectPool.Get<Te[]>();
            var elementFamilyCount = GetAllFamilyElements(ref pooledElementFamily);

            var size = categoryFamilyCount + elementFamilyCount;
            var connectedObjects = new List<UnityEngine.Object>();

            for (int i = 0; i < categoryFamilyCount; i++) {
                if (pooledCategoryFamily[i] == null) {
                    Debug.LogWarning($"A family member of {this} is null");
                    continue;
                }
                connectedObjects.Add(pooledCategoryFamily[i]);
            }
            for (int i = 0; i < elementFamilyCount; i++) {
                if (pooledElementFamily[i] == null) {
                    Debug.LogWarning($"A family member of {this} is null");
                    continue;
                }
                connectedObjects.Add(pooledElementFamily[i]);
            }

            GenericObjectPool.Return(pooledCategoryFamily);
            GenericObjectPool.Return(pooledElementFamily);

            return connectedObjects.ToArray();
        }

        /// <summary>
        /// Returns all object that are children to this category, meaning children categories and elements.
        /// </summary>
        /// <param name="includeThis">Include this category in the children.</param>
        /// <returns>An array of children Objects.</returns>
        public override UnityEngine.Object[] GetAllChildrenConnectedObjects(bool includeThis)
        {
            var pooledAllChildren = GenericObjectPool.Get<Tc[]>();
            var categoryChildrenCount = GetAllChildren(ref pooledAllChildren, includeThis);

            var pooledElementChildren = GenericObjectPool.Get<Te[]>();
            var recipeChildrenCount = GetAllChildrenElements(ref pooledElementChildren, includeThis);

            var size = categoryChildrenCount + recipeChildrenCount;
            var connectedObjects = new UnityEngine.Object[size];

            for (int i = 0; i < categoryChildrenCount; i++) {
                connectedObjects[i] = pooledAllChildren[i];
            }
            for (int i = categoryChildrenCount; i < size; i++) {
                connectedObjects[i] = pooledElementChildren[i - categoryChildrenCount];
            }

            GenericObjectPool.Return(pooledAllChildren);
            GenericObjectPool.Return(pooledElementChildren);

            return connectedObjects;
        }

    }
}