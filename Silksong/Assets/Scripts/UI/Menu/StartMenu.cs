using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    [SerializeField] AudioClip SelectSoundEffect;
    [SerializeField] AudioClip ClickSoundEffect;

    private AudioSource SEAudioSource;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        ScreenList = new List<CanvasGroup>() { InvitationScreen, MainScreen, SaveScreen, ConfigScreen, ExitConfirmScreen };
        SetActiveAllScreen();
        OpenScreen(InvitationScreen); // 关闭其他所有界面，打开Invitation界面

    }

    // Update is called once per frame
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

    #region 被Button调用的

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
        Debug.Log("on select");
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
        Debug.Log("on click");
        if (SelectSoundEffect != null)
        {
            SEAudioSource.clip = ClickSoundEffect;
            SEAudioSource.Play();
        }

    }

    #endregion


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

}