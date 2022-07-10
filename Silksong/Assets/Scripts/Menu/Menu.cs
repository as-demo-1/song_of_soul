using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{

    //主菜单
    public Button[] mainButtons;

    //二级菜单面板
    public GameObject[] SecondPanel;

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
    private int childButtonIndex = 0;

    //是否为一级菜单
    private bool childPanelExpanded = true;

    //是否为二级菜单
    private bool isSecond = false;

    //是否为三级菜单
    private bool isThird = false;

    void Start()
    {
        //主菜单事件
        for (int i = 0; i < mainButtons.Length; i++)
        {
            mainButtons[i].onClick.AddListener(OnMainButtonClick);
        }

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
                EnterThrid();
                isSecond = false;
                isThird = true;
            }
        }

        //切换，按下数字'2'
        if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha2))
        {

            if (childPanelExpanded)//是一级菜单
            {
                mainButtonIndex++;
                if (mainButtonIndex > mainButtons.Length - 1)
                {
                    mainButtonIndex = 0;
                }
                mainButtons[mainButtonIndex].Select();
                DisplayChildPanel(mainButtonIndex);

            }
            else if (isSecond)//是二级菜单
            {
                SecondButton();
                /*childButtonIndex++;
                childButtonIndex = childButtonIndex > childButtonGroup[mainButtonIndex].Count - 1 ? 0 : childButtonIndex;
                childButtonGroup[mainButtonIndex][childButtonIndex].Select();
                if (mainButtonIndex == 1)
                {
                    DisplayChild2Panel(childButtonIndex);
                }*/
            }
            else
            {
                ThirdButton();
            }

        }

        //返回，按下数字'3'
        if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha3))//getReal3dInputs.ResetButtonDown
        {
            if (isSecond)
            {
                childButtonIndex = 0;
                mainButtons[mainButtonIndex].Select();
                if(mainButtonIndex == 4)
                {
                    achievement.ScrollTarget.verticalNormalizedPosition = 1;
                    achievement.index = 0;
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
            //CloseChildPanel();
        }
    }

    //主菜单点击事件函数
    public void OnMainButtonClick()
    {
        childButtonIndex = 0;
        EnterSecond();
    }

    //显示某个子菜单面板
    void DisplayChildPanel(int index)
    {
        for (int i = 0; i < SecondPanel.Length; i++)
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
    void CloseChildPanel()
    {
        childPanelExpanded = false;
        for (int i = 0; i < SecondPanel.Length; i++)
        {
            if (SecondPanel[i].activeSelf)
                SecondPanel[i].SetActive(false);
        }
    }


    public void EnterSecond()
    {
        switch (mainButtonIndex)
        {
            case 0:
                map.btns[childButtonIndex].Select();
                break;
            case 1:
                equip.btns[childButtonIndex].Select();
                break;
            case 2:
                talisman.btns[childButtonIndex].Select();
                break;
            case 3:
                picture.btns[childButtonIndex].Select();
                break;
            case 4:
                achievement.btns[childButtonIndex].Select();
                break;
            case 5:
                options.btns[childButtonIndex].Select();
                break;
        }
    }
    public void EnterThrid()
    {
        switch (mainButtonIndex)
        {
            case 1:
                {
                    switch (childButtonIndex)
                    {
                        case 0:
                            equip.fragment.btns[0].Select();
                            break;
                    }
                }
                break;
            case 3:
                {
                    switch (childButtonIndex)
                    {
                        case 0:
                            picture.minsterPicture.btns[0].Select();
                            break;
                    }
                    
                }
                break;
        }
    }
    public void SecondButton()
    {
        switch (mainButtonIndex)
        {
            case 0:
                {
                    childButtonIndex++;
                    childButtonIndex = childButtonIndex > map.btns.Count - 1 ? 0 : childButtonIndex;
                    map.btns[childButtonIndex].Select();
                }
                break;
            case 1:
                {
                    childButtonIndex++;
                    childButtonIndex = childButtonIndex > equip.btns.Count - 1 ? 0 : childButtonIndex;
                    equip.btns[childButtonIndex].Select();
                    equip.ChildPanel(childButtonIndex);
                }
                break;
            case 2:
                {
                    childButtonIndex++;
                    childButtonIndex = childButtonIndex > talisman.btns.Count - 1 ? 0 : childButtonIndex;
                    talisman.btns[childButtonIndex].Select();
                }
                break;
            case 3:
                {
                    childButtonIndex++;
                    childButtonIndex = childButtonIndex > picture.btns.Count - 1 ? 0 : childButtonIndex;
                    picture.btns[childButtonIndex].Select();
                }
                break;
            case 4:
                {
                    childButtonIndex++;
                    childButtonIndex = childButtonIndex > achievement.btns.Count - 1 ? 0 : childButtonIndex;
                    achievement.btns[childButtonIndex].Select();
                    achievement.MoveDown();
                    
                }
                break;
            case 5:
                {
                    childButtonIndex++;
                    childButtonIndex = childButtonIndex > options.btns.Count - 1 ? 0 : childButtonIndex;
                    options.btns[childButtonIndex].Select();
                }
                break;
        }
    }

    public void ThirdButton()
    {
        switch (mainButtonIndex)
        {

            case 1:
                {
                    equip.ThirdButton(childButtonIndex);
                    
                }
                break;

            case 3:
                {
                    picture.ThirdButton(childButtonIndex);
                }
                break;
        }
    }

    public void ReSecond()
    {
        switch (mainButtonIndex)
        {
            case 0:
                {
                    map.btns[childButtonIndex].Select();
                }
                break;
            case 1:
                {
                    equip.btns[childButtonIndex].Select();
                    equip.secondIndex = 0;
                }
                break;
            case 2:
                {
                    talisman.btns[childButtonIndex].Select();
                }
                break;
            case 3:
                {
                    picture.btns[childButtonIndex].Select();
                    picture.secondIndex = 0;
                    picture.minsterPicture.ScrollTarget.verticalNormalizedPosition = 1;
                }
                break;
            case 4:
                {
                    achievement.btns[childButtonIndex].Select();
                }
                break;
            case 5:
                {
                    options.btns[childButtonIndex].Select();
                }
                break;
        }
    }

}

