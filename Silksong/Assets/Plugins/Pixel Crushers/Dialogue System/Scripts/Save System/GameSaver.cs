// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// Allows you to save and load games to the SaveSystem or PlayerPrefs without writing any scripts. 
    /// To use this component:
    /// 
    /// -# Add this component to a game object.
    /// -# Set the playerPrefs key (i.e., saved game slot) where the game will be saved and loaded.
    /// To make multiple slots, you can add more GamerSaver components to change the playerPrefsKey 
    /// field.
    /// -# To save a game, send the message "SaveGame" to this component (or call it from a script).
    /// For example, you can use the SendMessageOnConversation component to send the message.
    /// -# To load a game, send the message "LoadGame" to this component (or call it from a script).
    /// -# To restart the game, send the message "RestartGame" (or call it from a script).
    /// -# To record the state of the game without saving, for example when changing levels, send 
    /// the "Record" message.
    /// -# To apply the saved state of the game, for example when returning to a level, send the 
    /// "Apply" message.
    /// -# You can also configure this component to save or load when the player "uses" the game 
    /// object. To do this:
    /// 	- Set Function On Use to Save or Load. This tells the component whether to save or load 
    /// when it receives an OnUse message.
    /// 	- Send the message "OnUse" to this component or game object. For example, the sample 
    /// Selector component sends OnUse when the player hits spacebar over the object.
    /// -# If you tick Apply Game State When Loading Levels, then this component will automatically
    /// apply the recorded game state as soon as the level is loaded. You're responsible for 
    /// issuing the "Record" message before loading the level. You can use PersistentDataManager to
    /// do this.
    /// -# If the scene contains a LevelManager, it will load using the LevelManager
    /// instead of just applying the saved-game data.
    /// </summary>
    [AddComponentMenu("")] // Use wrapper.
    public class GameSaver : MonoBehaviour
    {

        public enum FunctionOnUse { None, Save, Load, Restart };

        /// <summary>
        /// The root of the PlayerPrefs key where the game will be saved and loaded.
        /// </summary>
        public string playerPrefsKey = "savedgame";

        /// <summary>
        /// The default slot. This number is appended to playerPrefsKey to make the complete 
        /// PlayerPrefs key.
        /// </summary>
        public int slot = 0;

        /// <summary>
        /// The function (save or load) to perform when receiving the "OnUse" message.
        /// </summary>
        public FunctionOnUse functionOnUse = FunctionOnUse.None;

        /// <summary>
        /// If <c>true</c>, saved-game data will include all item fields. If <c>false</c>,
        /// it will only include quests' State and Track fields.
        /// </summary>
        public bool includeAllItemData = false;

        /// <summary>
        /// If <c>true</c>, saved-game data will include all location fields. If <c>false</c>,
        /// it will not include any location fields.
        /// </summary>
        public bool includeLocationData = false;

        /// <summary>
        /// If <c>true</c>, saved-game data will include the offered/spoken status of
        /// all dialogue entries.
        /// </summary>
        public bool includeSimStatus = false;

        /// <summary>
        /// The starting level to use when restarting the game.
        /// </summary>
        public string startingLevel = string.Empty;

        /// <summary>
        /// If <c>true</c>, this game object isn't destroyed when you load a new level.
        /// </summary>
        public bool dontDestroyOnLoad = false;

        public void Awake()
        {
            if (dontDestroyOnLoad)
            {
                this.transform.parent = null;
                DontDestroyOnLoad(this.gameObject);
            }
            PersistentDataManager.includeAllItemData = includeAllItemData;
            PersistentDataManager.includeLocationData = includeLocationData;
            PersistentDataManager.includeSimStatus = includeSimStatus;
        }

        /// <summary>
        /// Upon receiving an "OnUse" message, this method saves, loads, or does nothing, based on 
        /// the value of functionOnUse.
        /// </summary>
        public void OnUse()
        {
            switch (functionOnUse)
            {
                case FunctionOnUse.Save:
                    SaveGame(); break;
                case FunctionOnUse.Load:
                    LoadGame(); break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Saves the game under the PlayerPrefs key, adding the slot number to support multiple 
        /// save game slots.
        /// </summary>
        /// <param name='slot'>
        /// The slot to use.
        /// </param>
        public void SaveGame(int slot)
        {
            if (SaveSystem.instance != null)
            {
                SaveSystem.SaveToSlot(slot);
            }
            else
            {
                if (string.IsNullOrEmpty(playerPrefsKey))
                {
                    if (DialogueDebug.logWarnings) Debug.LogWarning(string.Format("{0}: PlayerPrefs Key isn't set. Not saving.", new System.Object[] { DialogueDebug.Prefix }));
                    return;
                }
                if (DialogueDebug.logInfo) Debug.Log(string.Format("{0}: Saving game in slot {1}.", new System.Object[] { DialogueDebug.Prefix, slot }));
                string key = playerPrefsKey + slot.ToString();
                PlayerPrefs.SetString(key, PersistentDataManager.GetSaveData());
            }
        }

        /// <summary>
        /// Saves the game in default slot 0.
        /// </summary>
        public void SaveGame()
        {
            SaveGame(slot);
        }

        /// <summary>
        /// Loads the game from the data saved under the PlayerPrefs key.
        /// </summary>
        public void LoadGame(int slot)
        {
            if (SaveSystem.instance != null)
            {
                SaveSystem.LoadFromSlot(slot);
            }
            else
            {
                if (string.IsNullOrEmpty(playerPrefsKey))
                {
                    if (DialogueDebug.logWarnings) Debug.LogWarning(string.Format("{0}: PlayerPrefs Key isn't set. Not loading.", new System.Object[] { DialogueDebug.Prefix }));
                    return;
                }
                string key = playerPrefsKey + slot.ToString();
                if (!PlayerPrefs.HasKey(key))
                {
                    if (DialogueDebug.logWarnings) Debug.LogWarning(string.Format("{0}: No saved game in PlayerPrefs key '{1}'. Not loading.", new System.Object[] { DialogueDebug.Prefix, key }));
                    return;
                }
                if (DialogueDebug.logInfo) Debug.Log(string.Format("{0}: Loading save data from slot {1} and applying it.", new System.Object[] { DialogueDebug.Prefix, slot }));

                // Load using the LevelManager if available; otherwise just apply saved-game data:
                string saveData = PlayerPrefs.GetString(key);
                LevelManager levelManager = FindLevelManager();
                if (levelManager != null)
                {
                    levelManager.LoadGame(saveData);
                }
                else
                {
                    PersistentDataManager.ApplySaveData(saveData, DatabaseResetOptions.KeepAllLoaded);
                    DialogueManager.SendUpdateTracker(); // Update quest tracker HUD.
                }
            }
        }

        /// <summary>
        /// Loads the game from default slot 0.
        /// </summary>
        public void LoadGame()
        {
            LoadGame(slot);
        }

        /// <summary>
        /// Saves the game using a string parameter for the slot number.
        /// </summary>
        /// <param name='slotString'>
        /// Slot string to convert to an int.
        /// </param>
        public void SaveGame(string slotString)
        {
            SaveGame(StringToSlot(slotString));
        }

        /// <summary>
        /// Loads the game using a string parameter for the slot number.
        /// </summary>
        /// <param name='slotString'>
        /// Slot string to convert to an int.
        /// </param>
        public void LoadGame(string slotString)
        {
            LoadGame(StringToSlot(slotString));
        }

        /// <summary>
        /// Restarts the game.
        /// </summary>
        public void RestartGame()
        {
            LevelManager levelManager = FindLevelManager();
            if (SaveSystem.instance != null)
            {
                var startingSceneName = (levelManager != null && !string.IsNullOrEmpty(levelManager.defaultStartingLevel)) ? levelManager.defaultStartingLevel : startingLevel;
                SaveSystem.RestartGame(startingSceneName);
            }
            else
            {
                if (levelManager != null)
                {
                    levelManager.RestartGame();
                }
                else
                {
                    DialogueManager.ResetDatabase(DatabaseResetOptions.RevertToDefault);
                    if (string.IsNullOrEmpty(startingLevel))
                    {
                        Tools.LoadLevel(0);
                    }
                    else
                    {
                        Tools.LoadLevel(startingLevel);
                    }
                    // Update quest tracker HUD:
                    DialogueManager.SendUpdateTracker();
                }
            }
        }

        private LevelManager FindLevelManager()
        {
            var levelManager = GetComponentInChildren<LevelManager>();
            if (levelManager == null) levelManager = FindObjectOfType<LevelManager>();
            return levelManager;
        }

        private int StringToSlot(string slotString)
        {
            int slot = 0;
            int.TryParse(slotString, out slot);
            return slot;
        }

        /// <summary>
        /// Records the state of the game.
        /// </summary>
        public void Record()
        {
            PersistentDataManager.Record();
        }

        /// <summary>
        /// Applies the recorded state of the game to the current scene.
        /// </summary>
        public void Apply()
        {
            PersistentDataManager.Apply();
        }

    }

}
