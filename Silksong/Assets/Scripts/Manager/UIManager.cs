using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FairyGUI;

public class UIManager : MonoSingleton<UIManager>
{
    // todo 后续优化为字典或栈
    private GameObject UIMenu_PlayerStatus;
    private GameObject UIMenu_System;
    public GameObject Prefabs_UIMenu_PlayerStatus;
    public GameObject Prefabs_UIMenu_System;

    // todo 后续将多个bool优化为枚举状态
    public bool isShowGameUI = false;
    public bool isShowSystemUI = false;

    public override void Init()
    {
        DontDestroyOnLoad(gameObject);
        Application.targetFrameRate = 60;
        UIPackage.AddPackage("FairyGUI/Common");
        UIPackage.AddPackage("FairyGUI/MainMenu");
        UIPackage.AddPackage("FairyGUI/OnSureMenu");
        UIPackage.AddPackage("FairyGUI/PlayerStatusMenu");
        UIPackage.AddPackage("FairyGUI/SystemMenu");
    }

    public void ShowGameUI()
    {
        if (!isShowGameUI)
        {
            isShowGameUI = true;
            if (UIMenu_PlayerStatus != null)
            {
                UIMenu_PlayerStatus.SetActive(true);
            }
            else
            {
                if (Prefabs_UIMenu_PlayerStatus == null)
                {
                    Prefabs_UIMenu_PlayerStatus = Resources.Load<GameObject>("Prefabs/UIMenu_PlayerStatus");
                }
                UIMenu_PlayerStatus = Instantiate(Prefabs_UIMenu_PlayerStatus);
                DontDestroyOnLoad(UIMenu_PlayerStatus);
            }
        }
    }
    
    public void HideGameUI()
    {
        if (isShowGameUI)
        {
            isShowGameUI = false;
            if (UIMenu_PlayerStatus != null)
            {
                UIMenu_PlayerStatus.SetActive(false);
            }
        }
    }

    public void ShowSystemUI()
    {
        if (!isShowSystemUI)
        {
            isShowSystemUI = true;
            if (UIMenu_System != null)
            {
                UIMenu_System.SetActive(true);
            }
            else
            {
                if (Prefabs_UIMenu_System == null)
                {
                    Prefabs_UIMenu_System = Resources.Load<GameObject>("Prefabs/UIMenu_System");
                }
                UIMenu_System = Instantiate(Prefabs_UIMenu_System);
                DontDestroyOnLoad(UIMenu_System);
            }
        }
    }

    public void HideSystemUI()
    {
        if (isShowSystemUI)
        {
            isShowSystemUI = false;
            if (UIMenu_System != null)
            {
                UIMenu_System.SetActive(false);
            }
        }
    }

    public void Update()
    {
        if (PlayerInput.Instance)
        {
            // 显示/隐藏地图 todo 直接把MapController里代码挪过来了，后续可考虑优化成事件
            // show quick map
            if (PlayerInput.Instance.quickMap.Down) {
                ShowSystemUI();
                HideGameUI();
                PlayerAnimatorParamsMapping.SetControl(false);
            }

            // hide quick map
            if (PlayerInput.Instance.quickMap.Up) {
                HideSystemUI();
                ShowGameUI();
                PlayerAnimatorParamsMapping.SetControl(true);
            }
        }
    }
}
