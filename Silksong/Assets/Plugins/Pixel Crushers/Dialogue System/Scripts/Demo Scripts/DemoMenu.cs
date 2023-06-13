using UnityEngine;
using UnityEngine.Events;
using PixelCrushers.DialogueSystem.UnityGUI;

namespace PixelCrushers.DialogueSystem.Demo
{

    /// <summary>
    /// This script provides a rudimentary main menu for the Dialogue System's Demo.
    /// </summary>
    [AddComponentMenu("")] // Use wrapper.
    public class DemoMenu : MonoBehaviour
    {

        [TextArea]
        public string startMessage = "Press Escape for Menu";
        public KeyCode menuKey = KeyCode.Escape;
        public GUISkin guiSkin;
        public bool closeWhenQuestLogOpen = true;
        public bool lockCursorDuringPlay = false;

        public UnityEvent onOpen = new UnityEvent();
        public UnityEvent onClose = new UnityEvent();

        private QuestLogWindow questLogWindow = null;
        private bool isMenuOpen = false;
        private Rect windowRect = new Rect(0, 0, 500, 500);
        private ScaledRect scaledRect = ScaledRect.FromOrigin(ScaledRectAlignment.MiddleCenter, ScaledValue.FromPixelValue(300), ScaledValue.FromPixelValue(320));

        void Start()
        {
            if (questLogWindow == null) questLogWindow = FindObjectOfType<QuestLogWindow>();
            if (!string.IsNullOrEmpty(startMessage)) DialogueManager.ShowAlert(startMessage);
        }

        private void OnDestroy()
        {
            if (isMenuOpen) Time.timeScale = 1;
        }

        void Update()
        {
            if (InputDeviceManager.IsKeyDown(menuKey) && !DialogueManager.isConversationActive && !IsQuestLogOpen())
            {
                SetMenuStatus(!isMenuOpen);
            }
            if (lockCursorDuringPlay)
            {
                CursorControl.SetCursorActive(DialogueManager.isConversationActive || isMenuOpen || IsQuestLogOpen());
            }
        }

        void OnGUI()
        {
            if (isMenuOpen && !IsQuestLogOpen())
            {
                if (guiSkin != null)
                {
                    GUI.skin = guiSkin;
                }
                windowRect = GUI.Window(0, windowRect, WindowFunction, "Menu");
            }
        }

        private void WindowFunction(int windowID)
        {
            if (GUI.Button(new Rect(10, 60, windowRect.width - 20, 48), "Quest Log"))
            {
                if (closeWhenQuestLogOpen) SetMenuStatus(false);
                OpenQuestLog();
            }
            if (GUI.Button(new Rect(10, 110, windowRect.width - 20, 48), "Save Game"))
            {
                SetMenuStatus(false);
                SaveGame();
            }
            if (GUI.Button(new Rect(10, 160, windowRect.width - 20, 48), "Load Game"))
            {
                SetMenuStatus(false);
                LoadGame();
            }
            if (GUI.Button(new Rect(10, 210, windowRect.width - 20, 48), "Clear Saved Game"))
            {
                SetMenuStatus(false);
                ClearSavedGame();
            }
            if (GUI.Button(new Rect(10, 260, windowRect.width - 20, 48), "Close Menu"))
            {
                SetMenuStatus(false);
            }
        }

        public void Open()
        {
            SetMenuStatus(true);
        }

        public void Close()
        {
            SetMenuStatus(false);
        }

        private void SetMenuStatus(bool open)
        {
            isMenuOpen = open;
            if (open) windowRect = scaledRect.GetPixelRect();
            Time.timeScale = open ? 0 : 1;
            if (open) onOpen.Invoke(); else onClose.Invoke();
        }

        private bool IsQuestLogOpen()
        {
            return (questLogWindow != null) && questLogWindow.isOpen;
        }

        private void OpenQuestLog()
        {
            if ((questLogWindow != null) && !IsQuestLogOpen())
            {
                questLogWindow.Open();
            }
        }

        private void SaveGame()
        {
            var saveSystem = FindObjectOfType<SaveSystem>();
            if (saveSystem != null)
            {
                SaveSystem.SaveToSlot(1);
            }
            else
            {
                string saveData = PersistentDataManager.GetSaveData();
                PlayerPrefs.SetString("SavedGame", saveData);
                Debug.Log("Save Game Data: " + saveData);
            }
            DialogueManager.ShowAlert("Game saved.");
        }

        private void LoadGame()
        {
            PersistentDataManager.LevelWillBeUnloaded();
            var saveSystem = FindObjectOfType<SaveSystem>();
            if (saveSystem != null)
            {
                if (SaveSystem.HasSavedGameInSlot(1))
                {
                    SaveSystem.LoadFromSlot(1);
                    DialogueManager.ShowAlert("Game loaded.");
                }
                else
                {
                    DialogueManager.ShowAlert("Save a game first.");
                }
            }
            else
            {
                if (PlayerPrefs.HasKey("SavedGame"))
                {
                    string saveData = PlayerPrefs.GetString("SavedGame");
                    Debug.Log("Load Game Data: " + saveData);
                    LevelManager levelManager = FindObjectOfType<LevelManager>();
                    if (levelManager != null)
                    {
                        levelManager.LoadGame(saveData);
                    }
                    else
                    {
                        PersistentDataManager.ApplySaveData(saveData);
                        DialogueManager.SendUpdateTracker();
                    }
                    DialogueManager.ShowAlert("Game loaded.");
                }
                else
                {
                    DialogueManager.ShowAlert("Save a game first.");
                }
            }
        }


        private void ClearSavedGame()
        {
            var saveSystem = FindObjectOfType<SaveSystem>();
            if (saveSystem != null)
            {
                if (SaveSystem.HasSavedGameInSlot(1))
                {
                    SaveSystem.DeleteSavedGameInSlot(1);
                }
            }
            else if (PlayerPrefs.HasKey("SavedGame"))
            {
                PlayerPrefs.DeleteKey("SavedGame");
                Debug.Log("Cleared saved game data");
            }
            DialogueManager.ShowAlert("Saved Game Cleared");
        }

    }

}
