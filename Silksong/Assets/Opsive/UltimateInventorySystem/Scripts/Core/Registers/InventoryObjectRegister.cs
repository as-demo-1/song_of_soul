/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Core.Registers
{
    using Opsive.Shared.Utility;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// The base base for inventory object registers.
    /// </summary>
    /// <typeparam name="T">The object type.</typeparam>
    public abstract class InventoryObjectRegister<T> : InventoryObjectIDOnlyRegister<T>
        where T : class, IObjectWithIDReadOnly
    {
        protected Dictionary<string, T> m_DictionaryByName;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="register">The inventory system register.</param>
        protected InventoryObjectRegister(InventorySystemRegister register) : base(register)
        {
            m_DictionaryByName = new Dictionary<string, T>();
        }

        /// <summary>
        /// Try get value from its name.
        /// </summary>
        /// <param name="name">The object name.</param>
        /// <param name="value">The object.</param>
        /// <returns>True if the value exists.</returns>
        public virtual bool TryGetValue(string name, out T value)
        {
            return m_DictionaryByName.TryGetValue(name, out value);
        }

        /// <summary>
        /// Register Conditions.
        /// </summary>
        /// <param name="obj">The object to register.</param>
        /// <returns>True if the object can be registered.</returns>
        public override bool RegisterConditions(T obj)
        {
            if (base.RegisterConditions(obj) == false) { return false; }

            if (!m_DictionaryByName.TryGetValue(obj.name, out var registered)) { return true; }

            if (registered == null || ReferenceEquals(registered, obj)) { return true; }

            Debug.LogError($"Cannot register {typeof(T)} because it has a duplicate name {obj.name}.");
            return false;

        }

        /// <summary>
        /// Add the object to the dictionaries.
        /// </summary>
        /// <param name="obj">The object.</param>
        protected override void AddInternal(T obj)
        {
            base.AddInternal(obj);
            m_DictionaryByName[obj.name] = obj;
        }

        /// <summary>
        /// Remove the object to the dictionaries.
        /// </summary>
        /// <param name="obj">The object.</param>
        protected override void RemoveInternal(T obj)
        {
            base.RemoveInternal(obj);
            m_DictionaryByName.Remove(obj.name);
        }
    }

    /// <summary>
    /// Generic class for InventoryObject Register.
    /// </summary>
    /// <typeparam name="T">The inventory object type.</typeparam>
    public abstract class InventoryObjectIDOnlyRegister<T>
        where T : class, IObjectWithIDReadOnly
    {
        protected InventorySystemRegister m_Register;

        protected Dictionary<uint, T> m_DictionaryByID;

        public int Count => m_DictionaryByID.Count;
        public T First {
            get {
                if (Count == 0) { return null; }

                var enumarator = m_DictionaryByID.GetEnumerator();
                enumarator.MoveNext();
                var value = enumarator.Current.Value;
                enumarator.Dispose();
                return value;
            }
        }

        /// <summary>
        /// Initialize the dictionaries in the constructor.
        /// </summary>
        protected InventoryObjectIDOnlyRegister(InventorySystemRegister register)
        {
            m_Register = register;
            m_DictionaryByID = new Dictionary<uint, T>();
        }

        /// <summary>
        /// Try get value from its name.
        /// </summary>
        /// <param name="id">The object id.</param>
        /// <param name="value">The object.</param>
        /// <returns>True if the value exists.</returns>
        public virtual bool TryGetValue(uint id, out T value)
        {
            return m_DictionaryByID.TryGetValue(id, out value);
        }

        /// <summary>
        /// Returns all objects.
        /// </summary>
        /// <returns>all the objects.</returns>
        public IReadOnlyCollection<T> GetAll()
        {
            return m_DictionaryByID.Values;
        }

        /// <summary>
        /// Returns true if the itemCategory is registered.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns>True if it is registered.</returns>
        public virtual bool IsRegistered(uint id)
        {
            return m_DictionaryByID.ContainsKey(id);
        }

        /// <summary>
        /// Check if the object ID is empty.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>true if the id is not set.</returns>
        public bool IsIDEmpty(T obj)
        {
            return RandomID.IsIDEmpty(obj.ID);
        }

        /// <summary>
        ///  returns true if the itemCategory is registered
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="showWarningIfNotRegistered">Show a warning if the object is from the wrong database.</param>
        /// <returns>True if the object is registered.</returns>
        public virtual bool IsRegistered(T obj, bool showWarningIfNotRegistered = true)
        {
            if (obj == null) { return false; }

            if (IsIDEmpty(obj)) {
                return false;
            }

            if (m_DictionaryByID.TryGetValue(obj.ID, out var registeredObject)) {
                if (ReferenceEquals(registeredObject, obj)) {
                    return true;
                }

                if (showWarningIfNotRegistered) {
                    Debug.LogWarning($"A different {typeof(T)} {registeredObject} is registered with the same ID as {obj}, you may be referencing objects from the wrong database.");
                }

                return false;
            }

            return false;
        }

        /// <summary>
        /// Assigns a new ID to the object.
        /// </summary>
        /// <param name="obj">The object.</param>
        internal bool AssignNewID(T obj)
        {
            if (!(obj is IObjectWithID objWithID)) {
                Debug.LogWarning($"Duplicate ID '{obj.ID}' found, a new ID cannot be auto generated for: {obj}.");
                return false;
            }

            var newID = RandomID.Generate();
            while (IDIsAvailable(newID) == false) { newID = RandomID.Generate(); }

            objWithID.ID = newID;
            return true;
        }

        /// <summary>
        /// Resets the id of the object.
        /// </summary>
        /// <param name="obj">The object.</param>
        internal void ResetID(T obj)
        {
            if (!(obj is IObjectWithID objWithID)) {
                Debug.LogWarning("Cannot change the ID of: " + obj);
                return;
            }

            objWithID.ID = RandomID.Empty;
        }

        /// <summary>
        /// Check if the Id Is available.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns>True if the id is available.</returns>
        public bool IDIsAvailable(uint id)
        {
            if (RandomID.IsIDEmpty(id)) { return false; }

            if (!m_DictionaryByID.TryGetValue(id, out var registered)) { return true; }

            if (registered == null || registered.Equals(null)) {
                m_DictionaryByID.Remove(id);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Register Conditions.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>True if the object can be registered.</returns>
        public virtual bool RegisterConditions(T obj)
        {
            if (obj == null) {
                Debug.LogError($"Cannot register {typeof(T)} because it is null.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Registers the Object.
        /// </summary>
        /// <param name="obj">The object to register.</param>
        /// <returns>True if the object was registered.</returns>
        public virtual bool Register(T obj)
        {
            return RegisterInternal(obj);
        }

        /// <summary>
        /// Registers the Object.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>True if the object was registered.</returns>
        protected virtual bool RegisterInternal(T obj)
        {
            if (RegisterConditions(obj) == false) { return false; }

            if (IsIDEmpty(obj)) { AssignNewID(obj); }

            var addToDictionaries = false;

            if (m_DictionaryByID.TryGetValue(obj.ID, out var registered)) {

                if (registered == null || registered.Equals(null)) {
                    m_DictionaryByID.Remove(obj.ID);
                    addToDictionaries = true;
                } else if (ReferenceEquals(registered, obj) == false) {
                    addToDictionaries = AssignNewID(obj);
                }
            } else {
                addToDictionaries = true;
            }

            if (addToDictionaries) {
                AddInternal(obj);
            }

            return true;
        }

        /// <summary>
        /// Add object to Dictionary.
        /// </summary>
        /// <param name="obj">The object.</param>
        protected virtual void AddInternal(T obj)
        {
            m_DictionaryByID.Add(obj.ID, obj);
        }

        /// <summary>
        /// Unregister the itemCategory.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns>True if the object was unregistered.</returns>
        public virtual bool Unregister(uint id)
        {
            if (RandomID.IsIDEmpty(id)) {
                return false;
            }

            if (m_DictionaryByID.TryGetValue(id, out var registeredObj)) {
                RemoveInternal(registeredObj);
                ResetID(registeredObj);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Remove internal removes the object form the dictionaries.
        /// </summary>
        /// <param name="obj">The object.</param>
        protected virtual void RemoveInternal(T obj)
        {
            m_DictionaryByID.Remove(obj.ID);
        }

        /// <summary>
        /// Unregister the itemCategory.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>True if the object was unregistered.</returns>
        public virtual bool Unregister(T obj)
        {
            return Unregister(obj.ID);
        }

        /// <summary>
        /// Delete an itemCategory.
        /// </summary>
        /// <param name="itemCategoryID">The id.</param>
        public virtual void Delete(uint itemCategoryID)
        {
            m_DictionaryByID.TryGetValue(itemCategoryID, out var obj);
            Delete(obj);
        }

        /// <summary>
        /// Delete an itemCategory.
        /// </summary>
        /// <param name="obj">The object.</param>
        public virtual void Delete(T obj)
        {
            Unregister(obj);
        }
    }
}