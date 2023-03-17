/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Core
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// This register is used to set and get any type of object by a user defined id.
    /// The objects are registered by ID within a Type.
    /// Many objects can have the same ID as long as their Types are different. 
    /// </summary>
    public class GlobalRegister
    {
        protected Dictionary<Type, Dictionary<uint, object>> m_GlobalObjectDictionary
            = new Dictionary<Type, Dictionary<uint, object>>();

        /// <summary>
        /// Set the object to register.
        /// </summary>
        /// <param name="obj">The object to register.</param>
        /// <param name="id">The Id for that object.</param>
        /// <typeparam name="T">The type of the object.</typeparam>
        public void Set<T>(T obj, uint id)
        {
            var typeDictionary = m_GlobalObjectDictionary;
            var type = typeof(T);

            if (typeDictionary.ContainsKey(type)) {
                var objDictionary = typeDictionary[type];
                if (objDictionary.TryGetValue(id, out var value)) {

                    if (value != null) {
                        Debug.LogWarning("[Toolbox] Global component of type <" + typeof(T).Name + "> ID \""
                                         + id + "\" already exist!");
                        return;
                    }
                }

                objDictionary.Add(id, obj);
                typeDictionary[type] = objDictionary;
                return;
            }

            var newDictionary = new Dictionary<uint, object>();
            newDictionary.Add(id, obj);
            typeDictionary.Add(type, newDictionary);
        }

        /// <summary>
        /// Get the object for the ID and Type.
        /// </summary>
        /// <param name="id">The ID of the object.</param>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <returns>The object.</returns>
        public T Get<T>(uint id)
        {
            var typeDictionary = m_GlobalObjectDictionary;
            var type = typeof(T);

            if (typeDictionary.ContainsKey(type) == false) {
                Debug.LogWarning("[Toolbox] Global component of type <" + typeof(T).Name + "> ID \""
                                 + id + "\" doesn't exist! Typo?");
                return default;
            }

            var objDictionary = typeDictionary[type];
            if (objDictionary.ContainsKey(id) == false) {
                Debug.LogWarning("[Toolbox] Global component of type <" + typeof(T).Name + "> ID \""
                                 + id + "\" doesn't exist! Typo?");
                return default;
            }

            return (T)objDictionary[id];
        }

        /// <summary>
        /// Remove the object with the type and ID from the register.
        /// </summary>
        /// <param name="id">The id of the object to remove.</param>
        /// <typeparam name="T">The type of the object to remove.</typeparam>
        public void Remove<T>(uint id)
        {
            var typeDictionary = m_GlobalObjectDictionary;

            var type = typeof(T);

            if (typeDictionary.ContainsKey(type) == false) {
                return;
            }

            var objDictionary = typeDictionary[type];
            if (objDictionary.ContainsKey(id) == false) {
                return;
            }

            objDictionary.Remove(id);
        }
    }
}