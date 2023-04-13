/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

//#define DEBUG_SYSTEM_SAVER

namespace Opsive.UltimateInventorySystem.SaveSystem
{
    using Opsive.Shared.Utility;
    using Opsive.UltimateInventorySystem.Core;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Text;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using EventHandler = Opsive.Shared.Events.EventHandler;

    /// <summary>
    /// The save system manager is used by saver components to load and save any serializable data.
    /// </summary>
    public class SaveSystemManager : MonoBehaviour
    {
        [Tooltip("Load the last save data automatically on start.")]
        [SerializeField] protected bool m_AutoLoadOnInitialize;
        [Tooltip("Load the last save data automatically on start.")]
        [SerializeField] protected bool m_AutoLoadOnSceneLoaded;
        [Tooltip("Save when a scene is unloaded.")]
        [SerializeField] protected bool m_AutoSaveOnSceneUnloaded;
        [Tooltip("Automatically save the game before the application quits.")]
        [SerializeField] protected bool m_AutoSaveOnApplicationQuit;
        [Header("Save Data")]
        [Tooltip("The save file name when saved on disk.")]
        [SerializeField] protected string m_SaveFileName = "SaveFile";
        [Tooltip("The save file extension name when saved on disk.")]
        [SerializeField] protected string m_SaveFileExtension = "save";
        [Tooltip("The maximum number of save files possible.")]
        [SerializeField] protected int m_MaxSaves = 5;
        [Header("Meta Data")]
        [Tooltip("The object which will create the meta data, in case it is custom.")]
        [SerializeField] protected SaveMetaDataCreator m_SaveMetaDataCreator;
        [Tooltip("The save meta data file extension name when saved on disk.")]
        [SerializeField] protected string m_SaveMetaDataFileExtension = "metadata";
        [Header("Other Settings")]
        [Tooltip("The Inventory System Manager Item Save used to save items that were created at runtime.")]
        [SerializeField] internal InventorySystemManagerItemSaver m_InventorySystemManagerItemSaver;
        [Tooltip("This will make a copy of the save file as Json, should only be used to debug save data.")]
        [SerializeField] protected bool m_DebugJsonCopy;

        protected SaveDataInfo m_LoadedSaveData;
        protected Dictionary<int, SaveDataInfo> m_Saves;
        protected List<SaverBase> m_Savers;
        protected SaveDataInfo[] m_SavesArray;
        protected float m_TimeLastSaved;

        #region Singleton and Static functions

#if UNITY_2019_3_OR_NEWER
        /// <summary>
        /// Reset the static variables for domain reloading.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void DomainReset()
        {
            s_Initialized = false;
            s_Instance = null;
        }
#endif

        private static SaveSystemManager s_Instance;
        public static SaveSystemManager Instance {
            get {
                if (IsNull) {

                    s_Instance = FindObjectOfType<SaveSystemManager>();

                    if (s_Instance == null) {
                        s_Instance = new GameObject("SaveSystemManager").AddComponent<SaveSystemManager>();
                    }

                    s_Instance.Initialize(false);
                }
                return s_Instance;
            }
        }
        private static bool s_Initialized;

        /// <summary>
        /// Returns true if the InventorySystemManger was not Initialized.
        /// </summary>
        public static bool IsNull => s_Instance == null || s_Initialized == false;

        public static int MaxSaves => Instance.m_MaxSaves;

        public static IReadOnlyList<SaverBase> Savers => Instance.m_Savers;

        public static IReadOnlyDictionary<int, SaveDataInfo> Saves => Instance.m_Saves;

        public static float TimeLastSaved => Instance.m_TimeLastSaved;

        public static InventorySystemManagerItemSaver InventorySystemManagerItemSaver
        {
            get
            {
                var inventorySystemManagerItemSaver = Instance.m_InventorySystemManagerItemSaver;
                if (inventorySystemManagerItemSaver == null) {
                    if (InventorySystemManager.IsNull == false) {
                        inventorySystemManagerItemSaver = InventorySystemManager.Instance
                            .GetComponent<InventorySystemManagerItemSaver>();
                    }

                    if (inventorySystemManagerItemSaver != null) {
                        Instance.m_InventorySystemManagerItemSaver = inventorySystemManagerItemSaver;
                    }
                }
                return inventorySystemManagerItemSaver;
            }
        }


        /// <summary>
        /// Called on Awake Initializes the Manager.
        /// </summary>
        protected virtual void Awake()
        {
            Initialize(false);
        }

        /// <summary>
        /// Initializes the Manager using the database if one is specified.
        /// </summary>
        public virtual void Initialize(bool force)
        {
            if (s_Initialized && !force) { return; }

            s_Instance = this;
            s_Initialized = true;
            m_TimeLastSaved = Time.time;
            SceneManager.sceneUnloaded -= SceneUnloadedWhileDisabled;

            if (m_InventorySystemManagerItemSaver == null) {
                m_InventorySystemManagerItemSaver = GetComponent<InventorySystemManagerItemSaver>();
            }

            if (m_SaveMetaDataCreator == null) {
                m_SaveMetaDataCreator = ScriptableObject.CreateInstance<BasicSaveMetaDataCreator>();
            }

            m_SavesArray = new SaveDataInfo[m_MaxSaves];
            m_Savers = new List<SaverBase>();
            m_Saves = new Dictionary<int, SaveDataInfo>();
            GetAllSaveMetaData(ref m_Saves);

            for (int i = 0; i < m_SavesArray.Length; i++) {
                if (m_Saves.ContainsKey(i)) {
                    m_SavesArray[i] = m_Saves[i];
                }
            }

            if (m_AutoLoadOnInitialize && m_Saves != null && m_Saves.ContainsKey(0)) {
                var saveData = ReadSaveDataFromDisk(0);
                m_Saves[0] = new SaveDataInfo(0, m_Saves[0].MetaData, saveData);
                m_LoadedSaveData = m_Saves[0];
            } else {
                m_LoadedSaveData = CreateEmptySaveDataInfo(0);
            }
        }

        protected virtual SaveDataInfo CreateEmptySaveDataInfo(int index)
        {
            return new SaveDataInfo(index, m_SaveMetaDataCreator.CreateEmpty(), new SaveData());
        }

        /// <summary>
        /// The object has been enabled.
        /// </summary>
        protected virtual void OnEnable()
        {
            SceneManager.sceneUnloaded += SceneUnloaded;
            SceneManager.sceneLoaded += SceneLoaded;
        }

        /// <summary>
        /// The object has been disabled.
        /// </summary>
        protected virtual void OnDisable()
        {
            SceneManager.sceneUnloaded += SceneUnloadedWhileDisabled;

            SceneManager.sceneUnloaded -= SceneUnloaded;
            SceneManager.sceneLoaded -= SceneLoaded;
        }

        /// <summary>
        /// Handle the scene being loaded.
        /// </summary>
        /// <param name="scene">The scene.</param>
        /// <param name="loadSceneMode">The loading mode.</param>
        protected virtual void SceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            if (m_AutoLoadOnSceneLoaded && m_Saves != null && m_Saves.ContainsKey(0)) {
                Load(0);
            }
        }

        /// <summary>
        /// Handle a scene being unloaded.
        /// </summary>
        /// <param name="scene">The scene being unloaded.</param>
        protected virtual void SceneUnloaded(Scene scene)
        {
            if (m_AutoSaveOnSceneUnloaded) {
                Save(0);
            }
        }

        #region Register Unregister Savers

        /// <summary>
        /// Register a saver component.
        /// </summary>
        /// <param name="saver">The saver to register.</param>
        public static void RegisterSaver(SaverBase saver)
        {
            Instance.RegisterSaverInternal(saver);
        }

        /// <summary>
        /// Unregister a saver.
        /// </summary>
        /// <param name="saver">The saver to unregister.</param>
        public static void UnregisterSaver(SaverBase saver)
        {
            Instance.UnregisterSaverInternal(saver);
        }

        #endregion

        #region Save & Load

        /// <summary>
        /// Add serialized data to the save data.
        /// </summary>
        /// <param name="fullKey">The serialized data key.</param>
        /// <param name="serializedSaveData">The serialized data.</param>
        public static void AddToSaveData(string fullKey, Serialization serializedSaveData)
        {
            Instance.AddToSaveDataInternal(fullKey, serializedSaveData);
        }

        /// <summary>
        /// Remove the serialized data from the save data.
        /// </summary>
        /// <param name="fullKey">The serialized data key.</param>
        public static void RemoveFromSaveData(string fullKey)
        {
            Instance.RemoveFromSaveDataInternal(fullKey);
        }

        /// <summary>
        /// Save the game at the file index.
        /// </summary>
        /// <param name="saveIndex">The save file index.</param>
        public static void Save(int saveIndex, bool saveToDisk = true)
        {
            Instance.SaveInternal(saveIndex, saveToDisk);
        }

        /// <summary>
        /// Load the game data from the save file index.
        /// </summary>
        /// <param name="saveIndex">The save file index.</param>
        public static void Load(int saveIndex)
        {
            Instance.LoadFromDisk(saveIndex);
        }

        /// <summary>
        /// Load the save data in the save index provided.
        /// </summary>
        /// <param name="saveIndex">The save index to load the save data into.</param>
        /// <param name="saveData">The save data to load.</param>
        public static void Load(int saveIndex, SaveData saveData)
        {
            Instance.LoadInternal(saveIndex, saveData);
        }

        #endregion

        #region Getters & Setters

        /// <summary>
        /// Try get the save data.
        /// </summary>
        /// <param name="fullKey">The serialized data key.</param>
        /// <param name="serializedData">The serialize data.</param>
        /// <returns>True if the save data exists.</returns>
        public static bool TryGetSaveData(string fullKey, out Serialization serializedData)
        {
            return Instance.TryGetSaverDataInternal(fullKey, out serializedData);
        }

        /// <summary>
        /// Get all the save data.
        /// </summary>
        /// <returns>The save datas.</returns>
        public static IReadOnlyList<SaveDataInfo> GetSaves()
        {
            return Instance.GetSavesInternal();
        }

        /// <summary>
        /// Get the current save data.
        /// </summary>
        /// <returns>The save data.</returns>
        public static SaveDataInfo GetCurrentSaveDataInfo()
        {
            return Instance.GetCurrentSaveDataInfoInternal();
        }

        /// <summary>
        /// Set the current save data.
        /// </summary>
        /// <param name="newSaveData">The new save data.</param>
        public static void SetCurrentSaveData(SaveData newSaveData)
        {
            Instance.SetCurrentSaveDataInternal(newSaveData);
        }

        #endregion

        #region Delete

        /// <summary>
        /// Delete a save from the save file index.
        /// </summary>
        /// <param name="saveIndex">The save file index.</param>
        public static void DeleteSave(int saveIndex)
        {
            Instance.DeleteSaveInternal(saveIndex);
        }

        #endregion

        #region Singleton

        /// <summary>
        /// Destroys the object instance on the network.
        /// </summary>
        /// <param name="obj">The object to destroy.</param>
        public static void Destroy(GameObject obj)
        {
            if (s_Instance == null) {
                Debug.LogError("Error: Unable to destroy object - the Inventory System Manager doesn't exist.");
                return;
            }
            s_Instance.DestroyInternal(obj);
        }

        /// <summary>
        /// Reset the initialized variable when the scene is no longer loaded.
        /// </summary>
        /// <param name="scene">The scene that was unloaded.</param>
        protected virtual void SceneUnloadedWhileDisabled(Scene scene)
        {
            s_Instance = null;
            s_Initialized = false;

            SceneManager.sceneUnloaded -= SceneUnloadedWhileDisabled;
        }

        /// <summary>
        /// Do something when object is destroyed.
        /// </summary>
        protected virtual void OnDestroy()
        {
            DestroyInternal(gameObject);
        }

        #endregion
        #endregion

        /// <summary>
        /// Register the saver components
        /// </summary>
        /// <param name="saver">The saver.</param>
        protected virtual void RegisterSaverInternal(SaverBase saver)
        {
            for (int i = 0; i < m_Savers.Count; i++) {
                if (m_Savers[i].FullKey != saver.FullKey) { continue; }

                if (m_Savers[i] != saver) {
                    Debug.LogWarningFormat("Saver won't be registered because one with the same key is already registered");
                }

                return;
            }
            m_Savers.Add(saver);
        }

        /// <summary>
        /// Unregister saver components
        /// </summary>
        /// <param name="saver">The saver.</param>
        protected virtual void UnregisterSaverInternal(SaverBase saver)
        {
            m_Savers.Remove(saver);
        }

        /// <summary>
        /// Get the current save data.
        /// </summary>
        /// <returns>The save data.</returns>
        public virtual SaveDataInfo GetCurrentSaveDataInfoInternal()
        {
            return m_LoadedSaveData;
        }

        /// <summary>
        /// Set the current save data.
        /// </summary>
        /// <param name="newSaveData">The new save data.</param>
        public virtual void SetCurrentSaveDataInternal(SaveData newSaveData)
        {
            m_LoadedSaveData = new SaveDataInfo(m_LoadedSaveData.Index, m_LoadedSaveData.MetaData, newSaveData);
        }

        /// <summary>
        /// Add save data.
        /// </summary>
        /// <param name="fullKey">The full key.</param>
        /// <param name="serializedSaveData">The serialized data.</param>
        protected virtual void AddToSaveDataInternal(string fullKey, Serialization serializedSaveData)
        {
            m_LoadedSaveData.Data[fullKey] = serializedSaveData;
        }

        /// <summary>
        /// Add save data.
        /// </summary>
        /// <param name="fullKey">The full key.</param>
        /// <param name="serializedSaveData">The serialized data.</param>
        protected virtual void RemoveFromSaveDataInternal(string fullKey)
        {
            m_LoadedSaveData.Data.Remove(fullKey);
        }

        /// <summary>
        /// Save the data to the index provided.
        /// </summary>
        /// <param name="saveIndex">The save index.</param>
        /// <param name="saveToDisk">Should the save be written to disk.</param>
        protected virtual void SaveInternal(int saveIndex, bool saveToDisk)
        {
            EventHandler.ExecuteEvent<int>(EventNames.c_WillStartSaving_Index,saveIndex);

            SaveAllSavers();

            if (saveToDisk) {
                SaveToDiskInternal(saveIndex);
            }
            m_TimeLastSaved = Time.time;
            
            EventHandler.ExecuteEvent<int>(EventNames.c_SavingComplete_Index,saveIndex);
        }

        /// <summary>
        /// Save all the saver components.
        /// </summary>
        public virtual void SaveAllSavers()
        {
            OrderSaversByPriority(true);
            for (int i = 0; i < m_Savers.Count; i++) {
                m_Savers[i].Save();
            }
        }

        /// <summary>
        /// Try get the save data.
        /// </summary>
        /// <param name="fullKey">The full key.</param>
        /// <param name="serializedData">The serialized data.</param>
        /// <returns>True if the save data exists.</returns>
        protected virtual bool TryGetSaverDataInternal(string fullKey, out Serialization serializedData)
        {
            if (m_LoadedSaveData.Data == null) { Initialize(false); }
            return m_LoadedSaveData.Data.TryGetValue(fullKey, out serializedData);
        }

        /// <summary>
        /// Load the saved data from the save index provided.
        /// </summary>
        /// <param name="saveIndex">The save index.</param>
        protected virtual void LoadFromDisk(int saveIndex)
        {
#if DEBUG_SYSTEM_SAVER
            Debug.Log("Load file at index "+saveIndex);
#endif
            if (m_Saves == null || !m_Saves.ContainsKey(saveIndex)) {
                Debug.LogError($"Cannot load save at index {saveIndex}.");
                return;
            }
            
            var saveData = ReadSaveDataFromDisk(saveIndex);
            if (saveData == null) {
                Debug.LogError($"Cannot load save at index {saveIndex}.");
                return;
            }

            LoadInternal(saveIndex, saveData);
        }

        /// <summary>
        /// Load the save data in the save index provided.
        /// </summary>
        /// <param name="saveIndex">The save index to load the save data into.</param>
        /// <param name="saveData">The save data to load.</param>
        protected virtual void LoadInternal(int saveIndex, SaveData saveData)
        {
            if (m_Saves.ContainsKey(saveIndex) == false) {
                m_Saves[saveIndex] = new SaveDataInfo(CreateEmptySaveDataInfo(saveIndex), saveData);
            } else {
                m_Saves[saveIndex] = new SaveDataInfo(m_Saves[saveIndex], saveData);
            }
            
            EventHandler.ExecuteEvent(EventNames.c_WillStartLoadingSave_Index, saveIndex);

            //Make a deep copy to not modify the original save data.
            m_LoadedSaveData = SaveDataInfo.DeepCopy(m_Saves[saveIndex]);

            LoadAllSavers();

            EventHandler.ExecuteEvent(EventNames.c_LoadingSaveComplete_Index, saveIndex);
        }

        /// <summary>
        /// Load all the saver components.
        /// </summary>
        public virtual void LoadAllSavers()
        {
            OrderSaversByPriority(false);
            for (int i = 0; i < m_Savers.Count; i++) {
                m_Savers[i].Load();
            }
        }

        /// <summary>
        /// Delete the save from the save index.
        /// </summary>
        /// <param name="saveIndex">The save index.</param>
        protected virtual void DeleteSaveInternal(int saveIndex)
        {
            DeleteFromDiskInternal(saveIndex);
        }

        /// <summary>
        /// Return the save folder path.
        /// </summary>
        [ContextMenu("PrintSaveFolderPath")]
        public void PrintSaveFolderPath()
        {
            Debug.Log(GetSaveFolderPath());
        }

        /// <summary>
        /// Return the save folder path.
        /// </summary>
        /// <returns>The save folder path.</returns>
        protected virtual string GetSaveFolderPath()
        {
            return Application.persistentDataPath;
        }
        
        /// <summary>
        /// Get the save file path.
        /// </summary>
        /// <param name="saveIndex">The save index.</param>
        /// <returns>The save file path.</returns>
        protected virtual string GetSaveMetaDataFilePath(int saveIndex)
        {
            return string.Format("{0}/{1}_{2:000}.{3}",
                GetSaveFolderPath(), m_SaveFileName, saveIndex, m_SaveMetaDataFileExtension);
        }

        /// <summary>
        /// Get the save file path.
        /// </summary>
        /// <param name="saveIndex">The save index.</param>
        /// <returns>The save file path.</returns>
        protected virtual string GetSaveDataFilePath(int saveIndex)
        {
            return string.Format("{0}/{1}_{2:000}.{3}",
                GetSaveFolderPath(), m_SaveFileName, saveIndex, m_SaveFileExtension);
        }

        /// <summary>
        /// Save to disk.
        /// </summary>
        /// <param name="saveIndex">The save index.</param>
        protected virtual void SaveToDiskInternal(int saveIndex)
        {
#if DEBUG_SYSTEM_SAVER
            Debug.Log("Save file at index "+saveIndex);
#endif
            var saveFilePath = GetSaveDataFilePath(saveIndex);
            var saveMetaDataFilePath = GetSaveMetaDataFilePath(saveIndex);

#if DEBUG_SYSTEM_SAVER
            Debug.Log($"Save file at index {saveIndex} with path {saveFilePath}" +
                      $"\nThere are {m_SaveData.Count} Savers");
#endif
            BinaryFormatter bf = new BinaryFormatter();
            
            //Save MetaData.
            m_LoadedSaveData =
                new SaveDataInfo(m_LoadedSaveData, m_SaveMetaDataCreator.CreateMetaData(this, m_LoadedSaveData));
            FileStream metaDataFile = File.Create(saveMetaDataFilePath);
            var metaDataJson = JsonUtility.ToJson(m_LoadedSaveData.MetaData);
            bf.Serialize(metaDataFile, metaDataJson);
            metaDataFile.Close();

            //Save SaveData
            FileStream dataFile = File.Create(saveFilePath);
            var dataJson = JsonUtility.ToJson(m_LoadedSaveData.Data);
            bf.Serialize(dataFile, dataJson);
            dataFile.Close();

            m_Saves[saveIndex] = SaveDataInfo.DeepCopy(m_LoadedSaveData);

            if (!m_DebugJsonCopy) { return; }
            CreateDebugSaveDataFile(m_LoadedSaveData);
        }
        
        /// <summary>
        /// Create a Text Save File.
        /// </summary>
        /// <param name="filePath">The File Path.</param>
        /// <param name="saveMetaData">The string to save.</param>
        private void CreateDebugSaveMetaDataFile(string filePath, SaveMetaData saveMetaData)
        {
            // Delete the file if it exists.
            if (File.Exists(filePath)) { File.Delete(filePath); }

            var standardSaveDataJson = JsonUtility.ToJson(saveMetaData, true);

            //Create the file.
            using (FileStream fs = File.Create(filePath)) {
                //Write the Entire save file
                WriteToFile(fs, standardSaveDataJson);
            }
        }

        /// <summary>
        /// Create a Text Save File.
        /// </summary>
        /// <param name="filePath">The File Path.</param>
        /// <param name="saveData">The string to save.</param>
        private void CreateDebugSaveDataFile(SaveDataInfo saveDataInfo)
        {
            var metaDataFilePath = GetSaveMetaDataFilePath(saveDataInfo.Index) + ".json";
            Debug.Log("You are making a Debug Json Copy of the save file at the path: " + metaDataFilePath);
            
            // Delete the file if it exists.
            if (File.Exists(metaDataFilePath)) { File.Delete(metaDataFilePath); }

            var saveMetaDataJson = JsonUtility.ToJson(saveDataInfo.MetaData, true);

            //Create the file.
            using (FileStream fs = File.Create(metaDataFilePath)) {
                //Write the Entire save file
                WriteToFile(fs, saveMetaDataJson);
            }
            
            //Make the same Save Data Debug file.
            var dataFilePath = GetSaveDataFilePath(saveDataInfo.Index) + ".json";
            var saveData = saveDataInfo.Data;
            
            // Delete the file if it exists.
            if (File.Exists(dataFilePath)) { File.Delete(dataFilePath); }

            var standardSaveDataJson = JsonUtility.ToJson(saveData, true);

            //Create the file.
            using (FileStream fs = File.Create(dataFilePath)) {
                //Write the Entire save file
                WriteToFile(fs, "{\n\t\"StandardSaveData\": " + standardSaveDataJson + ",\n");

                WriteToFile(fs, "\t\"ReadableSaveData\": [\n");

                for (int i = 0; i < saveData.Count; i++) {

                    WriteToFile(fs, "{\n\t\t\"SaverKey\": \"" + saveData.SaveDataKeys[i] + "\",\n");

                    var jsonSaverData =
                        JsonUtility.ToJson(saveData.SerializedSaveData[i].DeserializeFields(MemberVisibility.All), true);
                    WriteToFile(fs, "\t\t\"SaverData\": " + jsonSaverData + "\n},\n");
                }

                WriteToFile(fs, "\t]\n}");
            }
        }

        /// <summary>
        /// Write to a file.
        /// </summary>
        /// <param name="fs">The file stream.</param>
        /// <param name="value">The string to write.</param>
        private static void WriteToFile(FileStream fs, string value)
        {
            var info = new UTF8Encoding(true).GetBytes(value);
            fs.Write(info, 0, info.Length);
        }

        /// <summary>
        /// Get the save data from the disk.
        /// </summary>
        /// <param name="saves">The saves dictionary.</param>
        protected virtual void GetAllSaveMetaData(ref Dictionary<int, SaveDataInfo> saves)
        {
            for (int i = 0; i < m_MaxSaves; i++) {
                var saveFilePath = GetSaveMetaDataFilePath(i);
                if (!File.Exists(saveFilePath)) { continue; }

                var saveMetaData = m_SaveMetaDataCreator.CreateEmpty();

                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Open(saveFilePath, FileMode.Open);
                JsonUtility.FromJsonOverwrite((string)bf.Deserialize(file), saveMetaData);
                file.Close();
                
                saves.Add(i, new SaveDataInfo(i, saveMetaData, null));
            }
        }

        protected virtual SaveData ReadSaveDataFromDisk(int index)
        {
            var saveFilePath = GetSaveDataFilePath(index);
            if (!File.Exists(saveFilePath)) { return null; }

            var saveData = new SaveData();

            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(saveFilePath, FileMode.Open);
            JsonUtility.FromJsonOverwrite((string)bf.Deserialize(file), saveData);
            file.Close();

            return saveData;
        }

        /// <summary>
        /// Delete the save from disk.
        /// </summary>
        /// <param name="saveIndex">The save index.</param>
        protected virtual void DeleteFromDiskInternal(int saveIndex)
        {
#if DEBUG_SYSTEM_SAVER
            Debug.Log("Delete file at index "+saveIndex);
#endif
            
            EventHandler.ExecuteEvent(EventNames.c_WillDeleteSave_Index,saveIndex);
            
            //Remove save file.
            var saveFilePath = GetSaveDataFilePath(saveIndex);
            if (!File.Exists(saveFilePath)) { return; }

            File.Delete(saveFilePath);
            
            //Remove MetaData
            var saveMetaDataFilePath = GetSaveMetaDataFilePath(saveIndex);
            if (!File.Exists(saveMetaDataFilePath)) { return; }

            File.Delete(saveMetaDataFilePath);
            
            m_Saves.Remove(saveIndex);
            
            EventHandler.ExecuteEvent(EventNames.c_DeleteSaveComplete_Index,saveIndex);
        }

        /// <summary>
        /// Get the saves data.
        /// </summary>
        /// <returns>Returns all the saves.</returns>
        protected virtual IReadOnlyList<SaveDataInfo> GetSavesInternal()
        {
            for (int i = 0; i < m_SavesArray.Length; i++) {
                if (m_Saves.ContainsKey(i)) {
                    m_SavesArray[i] = m_Saves[i];
                } else { m_SavesArray[i] = CreateEmptySaveDataInfo(i); }
            }

            return m_SavesArray;
        }

        /// <summary>
        /// Order saver components by priority.
        /// </summary>
        protected void OrderSaversByPriority(bool savePriority)
        {
            if (savePriority) {
                m_Savers.Sort((x, y) => x.SavePriority == y.SavePriority ? 0 : x.SavePriority > y.SavePriority ? 1 : -1);
            } else {
                m_Savers.Sort((x, y) => x.LoadPriority == y.LoadPriority ? 0 : x.LoadPriority > y.LoadPriority ? 1 : -1);
            }
        }

        /// <summary>
        /// Do something when object is destroyed.
        /// </summary>
        /// <param name="obj">The object being destroyed.</param>
        protected virtual void DestroyInternal(GameObject obj)
        {
            s_Initialized = false;
        }

        /// <summary>
        /// The game has ended. Determine if the game should be auto saved.
        /// </summary>
        protected virtual void OnApplicationQuit()
        {
            if (!m_AutoSaveOnApplicationQuit) { return; }

            OrderSaversByPriority(true);
            for (int i = 0; i < m_Savers.Count; i++) {
                if (m_Savers[i].SaveOnApplicationQuit) {
                    m_Savers[i].Save();
                }
            }

            SaveToDiskInternal(0);
        }
    }
}

