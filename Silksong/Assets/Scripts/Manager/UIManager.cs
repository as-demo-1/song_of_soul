using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FairyGUI;

public class UIManager : MonoSingleton<UIManager>
{
    private GameObject UIMenu_PlayerStatus;
    public GameObject Prefabs_UIMenu_PlayerStatus;

    public bool isShowGameUI = false;

    public override void Init()
    {
        DontDestroyOnLoad(gameObject);
        Application.targetFrameRate = 60;
        UIPackage.AddPackage("FairyGUI/Common");
        UIPackage.AddPackage("FairyGUI/MainMenu");
        UIPackage.AddPackage("FairyGUI/OnSureMenu");
        UIPackage.AddPackage("FairyGUI/PlayerStatusMenu");
    }

    public void ShowGameUI()
    {
        if (!isShowGameUI)
        {
            isShowGameUI = true;
            if (Prefabs_UIMenu_PlayerStatus == null)
            {
                Prefabs_UIMenu_PlayerStatus = Resources.Load<GameObject>("Prefabs/UIMenu_PlayerStatus");
            }
            GameObject UIMenu_PlayerStatus = Instantiate(Prefabs_UIMenu_PlayerStatus);
            DontDestroyOnLoad(UIMenu_PlayerStatus);
        }
    }
}
