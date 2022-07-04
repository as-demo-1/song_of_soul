using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class StartMenu : MonoBehaviour
{
    public static StartMenu Instance { get; private set; }
    

    [Header("Screens")]
    [SerializeField] CanvasGroup InvitationScreen;
    [SerializeField] CanvasGroup MainScreen;
    [SerializeField] CanvasGroup SaveScreen;
    [SerializeField] CanvasGroup ConfigScreen;
    [SerializeField] CanvasGroup ExitConfirmScreen;

    /// <summary>
    /// 储存所有界面的List
    /// </summary>
    List<CanvasGroup> ScreenList;

    [SerializeField] CanvasGroup CurrentScreen;

    [Header("Press Any Key To Continue")]
    [SerializeField] CanvasGroup PressAnyKey;

    [Tooltip("Adjust the blink speed of \"Press Any Key To Continue\"")]
    [Range(0.0f, 10.0f)]
    [SerializeField] float BreathSpeed = 1f;


    [Header("Sound Effects")]
    [SerializeField] AudioMixer audioMixer;
    [SerializeField] AudioClip SelectSoundEffect;
    [SerializeField] AudioClip ClickSoundEffect;

    private AudioSource SEAudioSource;


    [Header("Config Resolution")]
    [SerializeField] Dropdown resolutionDropdown;
    Resolution[] availableResolutions;
    private Dictionary<string, string> KeyConfigDictionary = new Dictionary<string, string>()
    {
        { "左", "A" },
        { "右", "D" },
        { "下", "S" },
        { "跳跃", "Space" },
        { "冲刺", "Left Shift" },
        { "攻击", "J" },
        { "技能", "L" },
        { "碎月", "Q" },
    };

    [Header("Config KeyBinding")]
    [SerializeField] GameObject KeyBindingPanel;
    [SerializeField] GameObject KeyBindingPanelContainer;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        ScreenList = new List<CanvasGroup>() { InvitationScreen, MainScreen, SaveScreen, ConfigScreen, ExitConfirmScreen };
        SetActiveAllScreen();
        OpenScreen(InvitationScreen); // 关闭其他所有界面，打开Invitation界面

        KeyBindingPanel.SetActive(false);

        UpdateAvailableResolutions();
    }

    void Update()
    {
        // 如果现在处于Invitation界面，就让“按任意键继续”的提示持续闪烁
        if (CurrentScreen == InvitationScreen)
        {
            if (BreathSpeed == 0) // 如果闪烁速度（BreathSpeed）为0，则常亮不闪烁
            {
                PressAnyKey.alpha = 1;
            }
            else
            {
                PressAnyKey.alpha = Mathf.Clamp01((Mathf.Sin(Time.time * BreathSpeed) + 1) * 0.52f);
            }
            if (Input.anyKey)
            {
                // 如果现在处于Invitation界面，而且玩家敲了任意按键，就进入主界面
                OpenScreen(MainScreen);
            }
        }

        if (CurrentScreen == ExitConfirmScreen)
        {
            if (Input.GetKeyDown(KeyCode.Escape) /*|| ( some random controller key)*/)
            {
                DisableScreen(ExitConfirmScreen, true);
            }
        }
        if (CurrentScreen != InvitationScreen && CurrentScreen != MainScreen)
        {
            if (Input.GetKeyDown(KeyCode.Escape) /*|| ( some random controller key)*/)
            {
                ReturnMainScreen();
            }
        }
    }

    #region 被主界面按钮调用的Function

    /// <summary>
    /// 开始新游戏，被按钮的OnClick调用
    /// </summary>
    public void NewGame()
    {

    }

    /// <summary>
    /// 打开存档界面，关闭其他窗口
    /// </summary>
    public void LoadGame()
    {
        OpenScreen(SaveScreen);
        //EnableScreen(SaveScreen, true);
    }

    /// <summary>
    /// 打开选项界面，关闭其他窗口
    /// </summary>
    public void OpenConfigScreen()
    {
        OpenScreen(ConfigScreen);
        //EnableScreen(ConfigScreen, true);
    }

    /// <summary>
    /// 回到主界面，关闭其他窗口
    /// </summary>
    public void ReturnMainScreen()
    {
        OpenScreen(MainScreen);
        //DisableScreen(SaveScreen, true);
    }

    /// <summary>
    /// 打开退出确认的窗口，不关闭其他窗口
    /// </summary>
    public void OpenExitConfirmScreen()
    {
        //OpenScreen(ExitConfirmScreen);
        EnableScreen(ExitConfirmScreen, true);
    }
    /// <summary>
    /// 关闭退出确认的窗口
    /// </summary>
    public void CloseExitConfirmScreen()
    {
        DisableScreen(ExitConfirmScreen, true);

        CurrentScreen = MainScreen;
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }

    #endregion

    #region 音效

    /// <summary>
    /// 玩家选择按钮时播放音效
    /// </summary>
    public void PlaySelectSoundEffect()
    {
        //Debug.Log("on select");
        if (SelectSoundEffect != null)
        {
            SEAudioSource.clip = SelectSoundEffect;
            SEAudioSource.Play();
        }

    }

    /// <summary>
    /// 玩家点击按钮时播放音效
    /// </summary>
    public void PlayClickSoundEffect()
    {
        //Debug.Log("on click");
        if (SelectSoundEffect != null)
        {
            SEAudioSource.clip = ClickSoundEffect;
            SEAudioSource.Play();
        }

    }

    /// <summary>
    /// 用于调整整体的音量，被Slider调用
    /// </summary>
    /// <param name="volume">从Slider接受的数值，最大值为0，最小值为-80（为了方便Audio Mixer）</param>
    public void SetMasterVolume(float volume)
    {
        //audioMixer.SetFloat("MasterVolume", volume);
    }

    #endregion

    #region 设置

    private void UpdateAvailableResolutions()
    {
        availableResolutions = Screen.resolutions; // 获取可用的分辨率

        List<string> options = new List<string>();
        int currentResolutionIndex = 0;
        for (int i = 0; i < availableResolutions.Length; i++)
        {
            string option = availableResolutions[i].width + " * " + availableResolutions[i].height;
            options.Add(option);

            if (Screen.currentResolution.width == availableResolutions[i].width && Screen.currentResolution.height == availableResolutions[i].height)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.ClearOptions();
        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();

        SetFullScreen(true);
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = availableResolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }

    public void SetFullScreen(bool fullscreen)
    {
        Screen.fullScreen = fullscreen;
    }

    /// <summary>
    /// 开关自定义键位的界面
    /// </summary>
    /// <param name="toggle">Toggle组件传入的参数</param>
    public void SetKeyBinding(bool toggle)
    {
        if (toggle)
        {
            UpdateKeyConfig();
        }

        KeyBindingPanel.SetActive(toggle);
    }

    /// <summary>
    /// 根据键位字典，重新绘制键位表
    /// </summary>
    private void UpdateKeyConfig()
    {
        // make sure the prefab is active, but Key Item is inactive
        KeyBindingPanelContainer.transform.Find("Key Item").gameObject.SetActive(true);
        GameObject prefab = KeyBindingPanelContainer.transform.Find("Key Item").gameObject;
        KeyBindingPanelContainer.transform.Find("Key Item").gameObject.SetActive(false);

        // destory all children
        foreach (Transform child in KeyBindingPanelContainer.transform)
        {
            if (child.name == "Key Item")
            {
                continue;
            }
            Destroy(child.gameObject);
        }

        foreach (string key in KeyConfigDictionary.Keys)
        {
            GameObject keyItem = Instantiate(prefab, KeyBindingPanelContainer.transform);
            keyItem.SetActive(true);
            keyItem.transform.Find("Text").GetComponent<Text>().text = key;
            keyItem.transform.Find("Button").GetComponentInChildren<Text>().text = KeyConfigDictionary[key];
        }
    }


    #endregion

    #region Helper Functions

    private void OpenScreen(CanvasGroup canvasGroup, bool withAnimation = false)
    {
        foreach (CanvasGroup screen in ScreenList)
        {
            if (screen == canvasGroup)
            {
                EnableScreen(screen, withAnimation);
            }
            else if (screen.alpha == 1 && screen.interactable == true && screen.blocksRaycasts == true)
            {
                DisableScreen(screen, withAnimation);
            }
        }
        CurrentScreen = canvasGroup;
    }
    private void EnableScreen(CanvasGroup canvasGroup, bool withAnimation)
    {
        if (withAnimation)
        {
            // transition animation to be added
        }
        //canvasGroup.gameObject.SetActive(true);
        canvasGroup.alpha = 1;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        CurrentScreen = canvasGroup;
    }
    private void DisableScreen(CanvasGroup canvasGroup, bool withAnimation)
    {
        if (withAnimation)
        {
            // transition animation to be added
        }
        //canvasGroup.gameObject.SetActive(false);
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }
    private void SetActiveAllScreen()
    {
        foreach (CanvasGroup screen in ScreenList)
        {
            screen.gameObject.SetActive(true);
        }
    }

    #endregion
}