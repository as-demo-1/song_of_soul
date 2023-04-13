/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.SaveSystem
{
    using Opsive.Shared.Utility;
    using UnityEngine;

    /// <summary>
    /// The base class for any saver component.
    /// It is used by the save manager to save and load components, and does so in order.
    /// </summary>
    public abstract class SaverBase : MonoBehaviour
    {
        //A higher priority (-1 > 1) will be saved first.
        public virtual int SavePriority => 0;
        //A higher priority (-1 > 1) will be loaded first.
        public virtual int LoadPriority => 0;

        [Tooltip("The key needs to be unique between objects with the same name and component. A prefix is added to the full key.")]
        [SerializeField] protected string m_Key;
        [Tooltip("Should this saver save data when the application quits.")]
        [SerializeField] protected bool m_SaveOnApplicationQuit;
        [Tooltip("Should this saver save data when its gameObject is destroyed.")]
        [SerializeField] protected bool m_SaveOnDestroy;
        [Tooltip("Should this saver load data when the game starts.")]
        [SerializeField] protected bool m_LoadOnStart;

        public virtual string FullKey => string.Format("{0}_{1}_{2}", new object[] { gameObject.name, this.GetType(), m_Key });
        public bool SaveOnApplicationQuit => m_SaveOnApplicationQuit;
        public bool SaveOnDestroy => m_SaveOnDestroy;
        public bool LoadOnStart => m_LoadOnStart;

        /// <summary>
        /// Serialize the save data.
        /// </summary>
        /// <returns>The serialization data.</returns>
        public abstract Serialization SerializeSaveData();

        /// <summary>
        /// Deserialize and load the save data.
        /// </summary>
        /// <param name="serializedSaveData">The serialized data.</param>
        public abstract void DeserializeAndLoadSaveData(Serialization serializedSaveData);

        /// <summary>
        /// 
        /// </summary>
        protected virtual void Awake()
        {

        }

        /// <summary>
        /// Starts by registering this component to the SaveSystemManager.
        /// </summary>
        protected virtual void Start()
        {
            SaveSystemManager.RegisterSaver(this);
            if (m_LoadOnStart) { Load(); }
        }

        /// <summary>
        /// Add this component to be saved by the save system manager.
        /// </summary>
        public virtual void Save()
        {
            var serializedSaveData = SerializeSaveData();
            SaveSystemManager.AddToSaveData(FullKey, serializedSaveData);
        }

        /// <summary>
        /// Load the save data from the save system manager for this component.
        /// </summary>
        public virtual void Load()
        {
            if (SaveSystemManager.TryGetSaveData(FullKey, out var serializedSaveData)) {
                if (serializedSaveData == null) {
                    return;
                }
                DeserializeAndLoadSaveData(serializedSaveData);
            }
        }

        /// <summary>
        /// Unregister this component from the save system manager.
        /// </summary>
        protected virtual void OnDestroy()
        {
            if (SaveSystemManager.IsNull) { return; }

            if (m_SaveOnDestroy) { Save(); }

            SaveSystemManager.UnregisterSaver(this);
        }
    }
}