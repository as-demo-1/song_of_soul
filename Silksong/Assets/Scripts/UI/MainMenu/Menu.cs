using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    //主菜单
    public Button[] mainButtons;
    //二级菜单面板
    public List<GameObject> SecondPanel;
    //地图
    public Map map;
    //装备
    public Equip equip;
    //护符
    public Talisman talisman;
    //图鉴
    public Picture picture;
    //成就
    public Achievement achievement;
    //选项
    public Options options;

    //当前选中主菜单索引
    private int mainButtonIndex = 0;
    //当前选中子菜单索引
    private int SecondButtonIndex = 0;
    //
    private int ThirdButtonIndex = 0;

    //是否为一级菜单
    private bool childPanelExpanded = true;

    //是否为二级菜单
    private bool isSecond = false;

    //是否为三级菜单
    private bool isThird = false;

    public Dictionary<int, List<Button>> dicMenu;

    Dictionary<int, List<Button>> dicMenuEquip;

    Dictionary<int, List<Button>> dicMenuPic;

    public List<List<Button>> menuSecondBnt;

    void Start()
    {
        ShowChildPanel();

        dicMenu = new Dictionary<int, List<Button>>();
        dicMenu.Add(0, map.btns);
        dicMenu.Add(1, equip.btns);
        dicMenu.Add(2, talisman.btns);
        dicMenu.Add(3, picture.btns);
        dicMenu.Add(4, achievement.btns);
        dicMenu.Add(5, options.btns);

        
        achievement.Init();
        picture.monsterPicture.Init();

        dicMenuEquip = new Dictionary<int, List<Button>>();
        dicMenuEquip.Add(0, equip.fragment.btns);
        dicMenuPic = new Dictionary<int, List<Button>>();
        dicMenuPic.Add(0, picture.monsterPicture.btns);
        DisplayChildPanel(mainButtonIndex);

        if (mainButtons.Length > 0)
        {
            mainButtons[mainButtonIndex].Select();
        }
        
    }


    void Update()
    {
        //点击，按下数字'1'
        if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (childPanelExpanded)//是一级菜单
            {
                EnterSecond();
                childPanelExpanded = false;
                isSecond = true;
            }
            else//是二级菜单
            {
                if (mainButtonIndex == 1 || mainButtonIndex == 3)
                {
                    EnterThrid();
                    isSecond = false;
                    isThird = true;
                }
            }
        }

        //切换，按下数字'2'
        if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha2))
        {

            if (childPanelExpanded)//是一级菜单
            {
                FirstButton();
            }
            else if (isSecond)//是二级菜单
            {
                SecondButton();
            }
            else
            {
                ThirdButton();
            }

        }

        //返回，按下数字'3'返回上级面板
        if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha3))
        {
            if (isSecond)
            {
                SecondButtonIndex = 0;
                mainButtons[mainButtonIndex].Select();
                if(mainButtonIndex == 4)
                {
                    achievement.MoveDown(SecondButtonIndex);
                }
                isSecond = false;
                childPanelExpanded = true;
            }
            else if (isThird)
            {
                ReSecond();
                isSecond = true;
                isThird = false;
            }
        }
    }


    //显示某个子菜单面板
    void DisplayChildPanel(int index)
    {
        for (int i = 0; i < SecondPanel.Count; i++)
        {
            if (i == index)
            {
                if (!SecondPanel[i].activeSelf)
                {
                    SecondPanel[i].SetActive(true);
                }
            }
            else
            {
                if (SecondPanel[i].activeSelf)
                    SecondPanel[i].SetActive(false);
            }
        }
    }

    //显示关闭所有菜单面板
    void ShowChildPanel()
    {
        for (int i = 0; i < SecondPanel.Count; i++)
        {
            if (!SecondPanel[i].activeSelf)
                SecondPanel[i].SetActive(true);
        }
    }

    //进入二级菜单
    public void EnterSecond()
    {
        dicMenu[mainButtonIndex][0].Select();
    }
    //进入三级菜单
    public void EnterThrid()
    {
        if(mainButtonIndex == 1)
        {
            dicMenuEquip[SecondButtonIndex][0].Select();
        }
        else if(mainButtonIndex == 3)
        {
            dicMenuPic[SecondButtonIndex][0].Select();
        }
    }

    //一级菜单按钮切换
    public void FirstButton()
    {
        mainButtonIndex++;
        if (mainButtonIndex > mainButtons.Length - 1)
        {
            mainButtonIndex = 0;
        }
        
        mainButtons[mainButtonIndex].Select();
        DisplayChildPanel(mainButtonIndex);
    }

    //二级菜单按钮切换
    public void SecondButton()
    {
        SecondButtonIndex++;
        SecondButtonIndex = SecondButtonIndex > dicMenu[mainButtonIndex].Count - 1 ? 0 : SecondButtonIndex;
        dicMenu[mainButtonIndex][SecondButtonIndex].Select();
        if(mainButtonIndex ==4)
        {
            achievement.MoveDown(SecondButtonIndex);
        }
    }

    //三级菜单按钮切换
    public void ThirdButton()
    {
        ThirdButtonIndex++;
        
        if (mainButtonIndex == 1)
        {
            ThirdButtonIndex = ThirdButtonIndex > dicMenuEquip[SecondButtonIndex].Count - 1 ? 0 : ThirdButtonIndex;
            dicMenuEquip[SecondButtonIndex][ThirdButtonIndex].Select();
        }
        else
        {
            ThirdButtonIndex = ThirdButtonIndex > dicMenuPic[SecondButtonIndex].Count - 1 ? 0 : ThirdButtonIndex;
            dicMenuPic[SecondButtonIndex][ThirdButtonIndex].Select();
            picture.monsterPicture.MoveDown(ThirdButtonIndex);
        }
    }

    public void ReSecond()
    {
        dicMenu[mainButtonIndex][SecondButtonIndex].Select();
        ThirdButtonIndex = 0;
        if (mainButtonIndex == 3)
        {
            picture.monsterPicture.MoveDown(ThirdButtonIndex);
        }
    }

}

