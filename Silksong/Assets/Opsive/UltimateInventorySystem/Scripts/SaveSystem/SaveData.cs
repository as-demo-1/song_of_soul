/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.SaveSystem
{
    using Opsive.Shared.Utility;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Save Data contains all the information that needs to be saved or loaded.
    /// </summary>
    [System.Serializable]
    public class SaveData
    {
        [Tooltip("The save data keys. The keys index matches the serialized data index.")]
        [SerializeField] protected List<string> m_SaveDataKeys;
        [Tooltip("The serialized save data.")]
        [SerializeField] protected List<Serialization> m_SerializedSaveData;

        public int Count => m_SaveDataKeys.Count;

        public IReadOnlyList<string> SaveDataKeys => m_SaveDataKeys;
        public IReadOnlyList<Serialization> SerializedSaveData => m_SerializedSaveData;

        /// <summary>
        /// Constructor for the save data.
        /// </summary>
        public SaveData()
        {
            if (m_SaveDataKeys == null) { m_SaveDataKeys = new List<string>(); }

            if (m_SerializedSaveData == null) { m_SerializedSaveData = new List<Serialization>(); }
        }

        /// <summary>
        /// Create a saveData by copying another.
        /// </summary>
        /// <param name="otherSaveData">The other save data to copy.</param>
        public SaveData(SaveData otherSaveData) : this()
        {
            m_SaveDataKeys.Clear();
            m_SaveDataKeys.AddRange(otherSaveData.m_SaveDataKeys);

            m_SerializedSaveData.Clear();
            m_SerializedSaveData.AddRange(otherSaveData.m_SerializedSaveData);
        }

        /// <summary>
        /// Try to get the serialization value for the given key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>True if the key exists.</returns>
        public bool TryGetValue(string key, out Serialization value)
        {
            for (int i = 0; i < m_SaveDataKeys.Count; i++) {
                if (key == m_SaveDataKeys[i]) {
                    value = m_SerializedSaveData[i];
                    return true;
                }
            }

            value = null;
            return false;
        }

        /// <summary>
        /// Add a serialization value for the provided key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public void Set(string key, Serialization value)
        {
            for (int i = 0; i < m_SaveDataKeys.Count; i++) {
                if (key == m_SaveDataKeys[i]) {
                    m_SerializedSaveData[i] = value;
                    return;
                }
            }

            m_SaveDataKeys.Add(key);
            m_SerializedSaveData.Add(value);
        }

        /// <summary>
        /// Add a serialization value for the provided key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public void Remove(string key)
        {
            var indexofKey = m_SaveDataKeys.IndexOf(key);
            if (indexofKey == -1) { return; }

            m_SaveDataKeys.RemoveAt(indexofKey);
            m_SerializedSaveData.RemoveAt(indexofKey);
        }

        /// <summary>
        /// Get and set the value given the key.
        /// </summary>
        /// <param name="key">The key.</param>
        public Serialization this[string key] {
            get {
                if (TryGetValue(key, out var value)) {
                    return value;
                }
                Debug.LogWarning($"The save key '{key}' does not exist in the save data.");
                return null;
            }
            set {
                Set(key, value);
            }
        }
    }
}
