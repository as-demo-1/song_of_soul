// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using System.Collections;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// This component combines Application.LoadLevel[Async] with the saved-game data
    /// features of PersistentDataManager. To use it, add it to your Dialogue Manager
    /// object and pass the saved-game data to LevelManager.LoadGame().
    /// </summary>
    [AddComponentMenu("")] // Use wrapper.
    public class LevelManager : MonoBehaviour
    {

        /// <summary>
        /// The default starting level to use if none is recorded in the saved-game data.
        /// </summary>
        [Tooltip("Level to use if none is recorded in saved-game data.")]
        public string defaultStartingLevel;

        [Tooltip("Load asynchronously to prevent freeze while loading.")]
        public bool useAsyncLoad = true;

        /// <summary>
        /// Indicates whether a level is currently loading. Only useful in Unity Pro, which
        /// uses Application.LoadLevelAsync().
        /// </summary>
        /// <value><c>true</c> if loading; otherwise, <c>false</c>.</value>
        public bool isLoading { get; private set; }

        /// @cond FOR_V1_COMPATIBILITY
        public bool IsLoading { get { return isLoading; } set { isLoading = value; } }
        /// @endcond

        protected virtual void Awake()
        {
            isLoading = false;
        }

        protected virtual void OnEnable()
        {
            PersistentDataManager.RegisterPersistentData(gameObject);
        }

        protected virtual void OnDisable()
        {
            PersistentDataManager.UnregisterPersistentData(gameObject);
        }

        /// <summary>
        /// Loads the game recorded in the provided saveData.
        /// </summary>
        /// <param name="saveData">Save data.</param>
        public void LoadGame(string saveData)
        {
            StartCoroutine(LoadLevelFromSaveData(saveData));
        }

        /// <summary>
        /// Restarts the game at the default starting level and resets the
        /// Dialogue System to its initial database state.
        /// </summary>
        public void RestartGame()
        {
            StartCoroutine(LoadLevelFromSaveData(null));
        }

        private IEnumerator LoadLevelFromSaveData(string saveData)
        {
            if (DialogueDebug.logInfo) Debug.Log("Dialogue System: LevelManager: Starting LoadLevelFromSaveData coroutine");
            string levelName = defaultStartingLevel;
            if (string.IsNullOrEmpty(saveData))
            {
                // If no saveData, reset the database.
                if (DialogueDebug.logInfo) Debug.Log("Dialogue System: LevelManager: Save data is empty, so just resetting database");
                DialogueManager.ResetDatabase(DatabaseResetOptions.RevertToDefault);
            }
            else
            {
                // Put saveData in Lua so we can get Variable["SavedLevelName"]:
                if (DialogueDebug.logInfo) Debug.Log("Dialogue System: LevelManager: Applying save data to get value of 'SavedLevelName' variable");
                Lua.Run(saveData, DialogueDebug.logInfo);
                levelName = DialogueLua.GetVariable("SavedLevelName").asString;
                if (string.IsNullOrEmpty(levelName) || string.Equals(levelName, "nil"))
                {
                    levelName = defaultStartingLevel;
                    if (DialogueDebug.logInfo) Debug.Log("Dialogue System: LevelManager: 'SavedLevelName' isn't defined. Using default level " + levelName);
                }
                else
                {
                    if (DialogueDebug.logInfo) Debug.Log("Dialogue System: LevelManager: SavedLevelName = " + levelName);
                }
            }

            // Load the level:
            PersistentDataManager.LevelWillBeUnloaded();

            if (CanLoadAsync())
            {
                AsyncOperation async = Tools.LoadLevelAsync(levelName);
                isLoading = true;
                while (!async.isDone)
                {
                    yield return null;
                }
                isLoading = false;
            }
            else
            {
                Tools.LoadLevel(levelName);
            }

            // Wait two frames for objects in the level to finish their Start() methods:
            if (DialogueDebug.logInfo) Debug.Log("Dialogue System: LevelManager finished loading level " + levelName + ". Waiting 2 frames for scene objects to start.");
            yield return null;
            yield return null;

            // Then apply saveData to the objects:
            if (!string.IsNullOrEmpty(saveData))
            {
                if (DialogueDebug.logInfo) Debug.Log("Dialogue System: LevelManager waited 2 frames. Appling save data: " + saveData);
                PersistentDataManager.ApplySaveData(saveData);
            }

            // Update quest tracker HUD:
            DialogueManager.SendUpdateTracker();
        }

        /// <summary>
        /// Loads a level. Use to change levels while keeping data synced. This method
        /// also calls PersistentDataManager.Record() before changing levels and
        /// PersistentDataManager.Apply() after changing levels. After loading the level,
        /// it waits two frames to allow GameObjects to finish their initialization first.
        /// </summary>
        /// <param name="levelName">Level name.</param>
        public void LoadLevel(string levelName)
        {
            StartCoroutine(LoadLevelCoroutine(levelName, -1));
        }

        /// <summary>
        /// Loads a level. Use to change levels while keeping data synced. This method
        /// also calls PersistentDataManager.Record() before changing levels and
        /// PersistentDataManager.Apply() after changing levels. After loading the level,
        /// it waits two frames to allow GameObjects to finish their initialization first.
        /// </summary>
        /// <param name="levelIndex">Scene index in build settings.</param>
        public void LoadLevel(int levelIndex)
        {
            StartCoroutine(LoadLevelCoroutine(null, levelIndex));
        }

        private IEnumerator LoadLevelCoroutine(string levelName, int levelIndex)
        {
            PersistentDataManager.Record();

            // Load the level:
            PersistentDataManager.LevelWillBeUnloaded();
            if (CanLoadAsync())
            {
                AsyncOperation async = !string.IsNullOrEmpty(levelName) ? Tools.LoadLevelAsync(levelName) : Tools.LoadLevelAsync(levelIndex);
                isLoading = true;
                while (!async.isDone)
                {
                    yield return null;
                }
                isLoading = false;
            }
            else
            {
                if (!string.IsNullOrEmpty(levelName)) Tools.LoadLevel(levelName); else Tools.LoadLevel(levelIndex);
            }

            // Wait two frames for objects in the level to finish their Start() methods:
            yield return null;
            yield return null;

            // Apply position data, but don't apply player's position:
            var player = GameObject.FindGameObjectWithTag("Player");
            var persistentPos = (player != null) ? player.GetComponent<PersistentPositionData>() : null;
            var originalValue = false;
            if (persistentPos != null)
            {
                originalValue = persistentPos.restoreCurrentLevelPosition;
                persistentPos.restoreCurrentLevelPosition = false;
            }

            PersistentDataManager.Apply();
            if (persistentPos != null)
            {
                persistentPos.restoreCurrentLevelPosition = originalValue;
            }

            // Update quest tracker HUD:
            DialogueManager.SendUpdateTracker();
        }

        private bool CanLoadAsync()
        {
            return useAsyncLoad;
        }

        /// <summary>
        /// Records the current level in Lua.
        /// </summary>
        public virtual void OnRecordPersistentData()
        {
            DialogueLua.SetVariable("SavedLevelName", Tools.loadedLevelName);
        }

    }

}