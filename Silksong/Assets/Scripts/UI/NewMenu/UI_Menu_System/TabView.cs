using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class TabView : MonoBehaviour 
{
	public TabView tabview;
	public GameObject secondBtnImg;

	public TabBtn[] tabButtons;//按钮
    public GameObject[] tabPages;//页数   一般相等
	public Transform[] transforms;
    public int index = -1;

	public UIEquipView uiEquipView;


	public bool isFirst;
	public bool isSecond;
	public bool isThird;

	public int panelIndex;
	public int tabIndex = 0;
	public int btnConut;

	[Header("进入下一级按键")]
	public KeyCode EnterKey;
	[Header("返回上一级按键")]
	public KeyCode ExitKey;

	[Header("绑定按键上一个")]
	public KeyCode PreviousKey;
	[Header("绑定按键下一个")]
	public KeyCode NextKey;


	int itemIndex = 0;
	void Start()//初始化一下
    {
	}
	private void Update()
	{
		if (panelIndex == 1)
		{
			if (Input.GetKeyDown(PreviousKey))
			{
				tabIndex = tabIndex > 0 ? tabIndex - 1 : 0;
				SelectTab(tabIndex);
			}
			if (Input.GetKeyDown(NextKey))
			{
				tabIndex = tabIndex < btnConut ? tabIndex + 1 : btnConut;
				SelectTab(tabIndex);
			}
		}
		else if (panelIndex == 2)
		{
			if (isFirst)
			{
				isFirst = false;
				isSecond = true;
				SelectTab(tabIndex);
			}
			else if (isSecond)
			{
				if (Input.GetKeyDown(EnterKey))
				{
					//物品界面下级选中
					if (transforms[tabIndex] != null)
					{
						isSecond = false;
						isThird = true;
						transforms[tabIndex].GetComponentInChildren<Selectable>().Select();
					}
				}
				if (Input.GetKeyDown(PreviousKey))
				{
					tabIndex = tabIndex > 0 ? tabIndex - 1 : 0;
					SelectTab(tabIndex);
				}
				if (Input.GetKeyDown(NextKey))
				{
					tabIndex = tabIndex < btnConut ? tabIndex + 1 : btnConut;
					SelectTab(tabIndex);
				}
			}
			else if (isThird)
			{
				if (Input.GetKeyDown(ExitKey))
				{
					isSecond = true;
					isThird = false;
					if (uiEquipView != null)
					{
						uiEquipView.selectedEquip.Exict();
						uiEquipView.selectedEquip = null;
						EventSystem.current.SetSelectedGameObject(null);
					}
				}
			}
		}
	}

	public void SelectTab(int index)
    {
        for (int i = 0; i < tabButtons.Length; i++)
        {
            if (i < tabPages.Length)
            {
                tabButtons[i].ChangeBg(i == index);
                tabPages[i].SetActive(i == index);
            }
        }
        this.index = index;
    }
}
