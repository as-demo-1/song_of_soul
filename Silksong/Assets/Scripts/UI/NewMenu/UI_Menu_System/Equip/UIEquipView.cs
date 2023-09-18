using UnityEngine;
using UnityEngine.UI;
using Opsive.UltimateInventorySystem.Core.InventoryCollections;
using Opsive.UltimateInventorySystem.Core;
using System.Collections.Generic;
using Opsive.UltimateInventorySystem.Exchange;
using Opsive.UltimateInventorySystem.Core.DataStructures;
using Opsive.UltimateInventorySystem.UI.CompoundElements;
using UnityEngine.EventSystems;
using System;

public class UIEquipView : MonoBehaviour
{

	private Inventory inventory;
	public Text HpCountText;
	public Text MpCountText;
	public Image HpImg;
	public Image[] MpImgs;
	public HPSlot[] HpSlots;
	public HPSlot[] MpSlots;
	public EquipSlot[] equipSlots;
	public Dictionary<int, List<ItemInfo>> dic;
	public List<ItemCategory> itemCategory;

	public EquipSlot selectedEquip;
	public Image selectedImage;

	int switchIndex = 0;
	public Text[] switchTexts;

	public event Action OnUnavailableNavigationRight;
	public event Action OnUnavailableNavigationLeft;
	public event Action OnUnavailableNavigationUp;
	public event Action OnUnavailableNavigationDown;

	void Start()
    {
		// var currencyOwner = InventorySystemManager.GetInventoryIdentifier(1).CurrencyOwner;
		// CurrencyCollection ownerCurrencyCollection = currencyOwner.CurrencyAmount;
		//
		// var a = ownerCurrencyCollection.GetCurrencyAmountAt(0);
		// Debug.Log(a.Amount);
		// var b = ownerCurrencyCollection.GetCurrencyAmountAt(1);
		// var c = currencyOwner.CurrencyAmount.GetCurrencyAmountCount();
		// var Hp = InventorySystemManager.GetCurrency("Hp");
		// var Mp = InventorySystemManager.GetCurrency("Mp");
		//HpUpdate(ownerCurrencyCollection.GetAmountOf(Hp));
		//MpUpdate(ownerCurrencyCollection.GetAmountOf(Mp));
		//HpUpdate(GameManager.Instance.currencyOwner.CurrencyAmount.GetCurrencyAmountAt(0).Amount);
		//MpUpdate(GameManager.Instance.currencyOwner.CurrencyAmount.GetCurrencyAmountAt(1).Amount);

		

		Init();	
	}

	private void OnDestroy()
	{
	}

	private void OnEnable()
	{
		HpUpdate(GameManager.Instance.currencyOwner.CurrencyAmount.GetCurrencyAmountAt(0).Amount);
		MpUpdate(GameManager.Instance.currencyOwner.CurrencyAmount.GetCurrencyAmountAt(1).Amount);
		
		// TODO: 更新玩家最大血量和能量，应当绑定碎片拾取事件
		PlayerController.Instance.playerCharacter.refreshMaxHp();
		PlayerController.Instance.playerCharacter.refreshMaxMana();
	}

	/// <summary>
	/// 用的默认数据库，会有一些问题
	/// </summary>
	public void Init()
	{
		inventory = GameManager.Instance.inventory;

		dic = new Dictionary<int, List<ItemInfo>>()
		{
			{ 0, new List<ItemInfo>()},
			{ 1, new List<ItemInfo>()},
			{ 2, new List<ItemInfo>()},
			{ 3, new List<ItemInfo>()},
		};
		for(int i = 0;i<4;i++)
		{		
			ItemInfo[] ccc = new ItemInfo[] { };
			inventory.GetItemCollection("Main").GetItemInfosWithCategory(itemCategory[i], ref ccc, false);
			for(int j = 0;j<ccc.Length;j++)
			{
				if (ccc[j].Item!=null)
				{
					dic[i].Add(ccc[j]);
				}
			}
		}
		
		for (int i  = 0;i<4;i++)
		{
			int count;
			count = dic[i].Count < 20 ? dic[i].Count : 20;
			for (int j = 0;j< 20; j++)
			{
				equipSlots[i * 20 + j].tabIndex = i;
				equipSlots[i * 20 + j].slotIndex = i*20+j;
				equipSlots[i * 20 + j].GetComponent<ActionButton>().OnMoveE += (eventData, moved) => OnMove(eventData, moved);
				if (j < count)
				{
					equipSlots[i * 20 + j].Init(dic[i][j].Item, this, dic[i][j].Amount);
				}
				else
				{
					equipSlots[i * 20 + j].Init(null, this, 0);
				}
			}
		}
	}

	#region  生命碎片和魔法碎片
	/// <summary>
	/// 更新生命碎片ui显示
	/// </summary>
	/// <param name="value"></param>
	public void HpUpdate(int value)
	{
		HpCountText.text = value.ToString();
		//Debug.Log("HP碎片数量:" + value);
		int i = 0;
		while (value >= 0)
		{
			HpSlots[i].UpdateHpImg(value);
			value = value - 2;
			i++;
		}
		if(value == -1)
		{
			HpImg.gameObject.SetActive(true);
		}
		else
		{
			HpImg.gameObject.SetActive(false);
		}
	}

	/// <summary>
	/// 更新魔力碎片ui显示
	/// </summary>
	/// <param name="value"></param>
	public void MpUpdate(int value)
	{
		MpCountText.text = value.ToString();
		//Debug.Log("MP碎片数量:" + value);
		int i = 0;
		while (value >= 0)
		{
			MpSlots[i].UpdateMpSlot(value);
			value = value - 4;
			i++;
			//Debug.Log("MP碎片数量:" + value);
		}
		if (value > -4)
		{
			for (int j = 0; j < value + 4; j++)
			{
				MpImgs[j].gameObject.SetActive(true);
			}
		}
		if(value == 0)
		{
			for (int j = 0; j < value + 4; j++)
			{
				MpImgs[j].gameObject.SetActive(false);
			}
		}
		else
		{
			for (int j = 0; j < value + 4; j++)
			{
				MpImgs[j].gameObject.SetActive(true);
			}
		}
	}
	#endregion

	private void OnMove(AxisEventData eventData, bool moved)
	{
		switch (eventData.moveDir)
		{
			case MoveDirection.Left:
				if (!moved) { PreviousSlice(); }
				break;
			case MoveDirection.Right:
				if (!moved) { NextSlice(); }
				break;

		}
	}

	/// <summary>
	/// 按下QE刷新UI 参数一是物品类型的int值 参数二是切换页数
	/// </summary>
	/// <param name="index"></param>
	/// <param name="index2"></param>
	public void RefreshUI(int index, int index2)
	{
		if(index2 == switchIndex)
		{
			return;
		}
		else if(index2 > switchIndex)
		{
			switchTexts[switchIndex].fontStyle = FontStyle.Normal;
			switchTexts[index2].fontStyle = FontStyle.Bold;
			switchIndex++;
		}
		else if (index2 < switchIndex)
		{
			switchTexts[switchIndex].fontStyle = FontStyle.Normal;
			switchTexts[index2].fontStyle = FontStyle.Bold;
			switchIndex--;
		}
		int count = dic[index-1].Count < (index2 + 1) * 20 ? dic[index-1].Count - index2 * 20 : 20;
		for (int j = 0; j < 20; j++)
		{
			if (j < count)
			{
				equipSlots[j + (index-1) * 20].Init(dic[index - 1][j + 20 * index2].Item, this, dic[index - 1][j + 20 * index2].Amount);
			}
			else
			{
				equipSlots[j + (index-1) * 20].Init(null, this,0);
			}
		}

		equipSlots[(index - 1) * 20].GetComponent<Button>().Select();
	}

	public void PreviousSlice()
	{
		if(switchIndex == 0)
		{
			return;
		}
		else
		{
			switchTexts[switchIndex].fontStyle = FontStyle.Normal;
			switchIndex--;
			switchTexts[switchIndex].fontStyle = FontStyle.Bold;
			var index = selectedEquip.tabIndex;
			int count = dic[index].Count < (switchIndex + 1) * 20 ? dic[index].Count - switchIndex * 20 : 20;
			for (int j = 0; j < 20; j++)
			{
				if (j < count)
				{
					equipSlots[j + (index) * 20].Init(dic[index][j + 20 * switchIndex].Item, this, dic[index][j + 20 * switchIndex].Amount);
				}
				else
				{
					equipSlots[j + (index) * 20].Init(null, this, 0);
				}
			}

			equipSlots[selectedEquip.slotIndex + 4].GetComponent<ActionButton>().Select();
		}
	}

	public void NextSlice()
	{
		if (switchIndex == 4)
		{
			return;
		}
		else
		{
			switchTexts[switchIndex].fontStyle = FontStyle.Normal;
			switchIndex++;
			switchTexts[switchIndex].fontStyle = FontStyle.Bold;
			var index = selectedEquip.tabIndex;
			int next = selectedEquip.slotIndex - 4;
			int count = dic[index].Count < (switchIndex + 1) * 20 ? dic[index].Count - switchIndex * 20 : 20;
			for (int j = 0; j < 20; j++)
			{
				if (j < count)
				{
					equipSlots[j + (index) * 20].Init(dic[index][j + 20 * switchIndex].Item, this, dic[index][j + 20 * switchIndex].Amount);
				}
				else
				{
					equipSlots[j + (index) * 20].Init(null, this, 0);
				}
			}

			equipSlots[next].GetComponent<ActionButton>().Select();
		}
	}
	public void UIClear()
	{

	}

	// Update is called once per frame
	void Update()
	{

	}
}
