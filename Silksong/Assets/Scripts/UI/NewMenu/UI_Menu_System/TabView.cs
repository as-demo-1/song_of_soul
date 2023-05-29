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
	public CharmUIPanel charmUIPanel;


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
	public KeyCode Key1;
	[Header("绑定按键下一个")]
	public KeyCode Key2;


	int itemIndex = 0;
	void Start()//初始化一下
    {
	}
	private void Update()
	{
		if (panelIndex == 1)
		{
			if (isFirst)
			{
				if (Input.GetKeyDown(EnterKey))
				{
					isFirst = false;
				}
				if (Input.GetKeyDown(Key1))
				{
					tabIndex = tabIndex > 0 ? tabIndex - 1 : 0;
					SelectTab(tabIndex);
				}
				if (Input.GetKeyDown(Key2))
				{
					tabIndex = tabIndex < btnConut ? tabIndex + 1 : btnConut;
					SelectTab(tabIndex);
				}
			}
		}
		else if (panelIndex == 2)
		{
			if (isFirst)
			{
				if (Input.GetKeyDown(EnterKey))
				{
					isFirst = false;
					isSecond = true;
					SelectTab(0);
				}
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
						if (secondBtnImg != null)
						{
							secondBtnImg.SetActive(true);
						}
					}
				}

				if (Input.GetKeyDown(ExitKey))
				{
					isFirst = true;
					isSecond = false;
					tabButtons[tabIndex].ChangeBg(false);
					tabview.isFirst = true;

				}
				if (Input.GetKeyDown(Key1))
				{
					tabIndex = tabIndex > 0 ? tabIndex - 1 : 0;
					SelectTab(tabIndex);
				}
				if (Input.GetKeyDown(Key2))
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
					if (secondBtnImg != null)
					{
						secondBtnImg.SetActive(false);
					}
					if (charmUIPanel != null)
					{
						charmUIPanel.selectCharm = null;
					}
					EventSystem.current.SetSelectedGameObject(null);
				}

				if(uiEquipView != null)
				{
					if (Input.GetKeyDown(KeyCode.Q))
					{
						itemIndex--;
						itemIndex = itemIndex < 0 ? 0 : itemIndex;
						uiEquipView.RefreshUI(tabIndex, itemIndex);
					}
					if(Input.GetKeyDown(KeyCode.E))
					{
						itemIndex++;
						itemIndex = itemIndex > 4 ? 4: itemIndex;
						uiEquipView.RefreshUI(tabIndex, itemIndex);
					}
				}

				if (charmUIPanel != null)
				{
					charmUIPanel.RefreshUI();
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
