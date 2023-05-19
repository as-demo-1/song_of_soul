using UnityEngine;
using UnityEngine.UI;
using Opsive.UltimateInventorySystem.Core.InventoryCollections;
using Opsive.UltimateInventorySystem.Core.DataStructures;
using Opsive.UltimateInventorySystem.UI.DataContainers;
using Opsive.Shared.Inventory;
using Opsive.UltimateInventorySystem.Core;
using System.Collections.Generic;
using Opsive.UltimateInventorySystem.SaveSystem;
using Opsive.Shared.Utility;
using System.Linq;
using static AkMIDIEvent;
using System.Runtime.InteropServices;
using Opsive.UltimateInventorySystem.Core.AttributeSystem;
using Opsive.UltimateInventorySystem.Exchange;
using BehaviorDesigner.Runtime.Tasks;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;

public class UIEquipView : MonoBehaviour
{

	private Inventory inventory;

	public Text Name;
	public Text Description;

	public Image HpImg;
	public Image[] MpImgs;
	public HPSlot[] HpSlots;
	public HPSlot[] MpSlots;
	public EquipSlot[] equipSlots;
	public Dictionary<int, Opsive.UltimateInventorySystem.Core.DataStructures.ItemInfo[]> dic;
	public List<ItemCategory> itemCategory;

	public EquipSlot selectedEquip;
	public Image selectedImage;

	void Start()
    {
		var currencyOwner = InventorySystemManager.GetInventoryIdentifier(1).CurrencyOwner;
		CurrencyCollection ownerCurrencyCollection = currencyOwner.CurrencyAmount;
		var Hp = InventorySystemManager.GetCurrency("Hp");
		var Mp = InventorySystemManager.GetCurrency("Mp");
		HpUpdate(ownerCurrencyCollection.GetAmountOf(Hp));
		MpUpdate(ownerCurrencyCollection.GetAmountOf(Mp));

		inventory = InventorySystemManager.GetInventoryIdentifier(1).Inventory;

		dic = new Dictionary<int, Opsive.UltimateInventorySystem.Core.DataStructures.ItemInfo[]>();
		/*foreach (ItemStack o in inventory.GetItemCollection("Main").GetAllItemStacks())
		{
			if (o.Item.Category == itemCategory[0])
			{
				dic[0].Add(o.Item);
			}
			else if (o.Item.Category == itemCategory[1])
			{
				dic[1].Add(o.Item);
			}
			else if (o.Item.Category == itemCategory[2])
			{
				dic[2].Add(o.Item);
			}
			else
			{
				dic[3].Add(o.Item);
			}
		}*/
		Init();	
	}

	private void OnDestroy()
	{
	}

	/// <summary>
	/// 用的默认数据库，会有一些问题
	/// </summary>
	public void Init()
	{
		for(int i = 0;i<4;i++)
		{		
			Opsive.UltimateInventorySystem.Core.DataStructures.ItemInfo[] ccc = new Opsive.UltimateInventorySystem.Core.DataStructures.ItemInfo[] { };
			inventory.GetItemCollection("Main").GetItemInfosWithCategory(itemCategory[i], ref ccc, false);
			dic.Add(i, ccc);
		}
		
		for (int i  = 0;i<4;i++)
		{
			int count;
			count = dic[i].Length > 20 ? 20 : dic[i].Length;
			for (int j = 0;j< count; j++)
			{
				if (dic[i][j].Item == null)
					continue;
				equipSlots[i * 20 + j].Init(dic[i][j].Item,this);
			}
		}

		/*foreach (ItemStack o in inventory.GetItemCollection("Main").GetAllItemStacks())
		{
			if(o.Item.Category== itemCategory[0])
			{
				dic[0].Add(o.Item);
				equipSlots[a].Init(o.Item);
				a++;
			}
			else if(o.Item.Category == itemCategory[1])
			{
				dic[1].Add(o.Item);
				equipSlots1[b].Init(o.Item);
				b++;
			}
			else if(o.Item.Category == itemCategory[2])
			{
				dic[2].Add(o.Item);
				equipSlots2[c].Init(o.Item);
				c++;
			}
			else
			{
				dic[3].Add(o.Item);
				equipSlots3[d].Init(o.Item);
				d++;
			}
		}*/
	}

	/// <summary>
	/// 更新生命碎片ui显示
	/// </summary>
	/// <param name="value"></param>
	public void HpUpdate(int value)
	{
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
		int i = 0;
		while (value >= 0)
		{
			MpSlots[i].UpdateMpSlot(value);
			value = value - 4;
			i++;
			Debug.Log("MP碎片数量:" + value);
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

	/// <summary>
	/// 按下QE刷新UI 参数一是物品类型的int值 参数二是切换页数
	/// </summary>
	/// <param name="index"></param>
	/// <param name="index2"></param>
	public void RefreshUI(int index,int index2)
	{
		Opsive.UltimateInventorySystem.Core.DataStructures.ItemInfo[] infos = new Opsive.UltimateInventorySystem.Core.DataStructures.ItemInfo[] { };

		inventory.GetItemCollection("Main").GetItemInfosWithCategory(itemCategory[index], ref infos, true);

		int count = infos.Length < index2*20? infos.Length:index2 * 20;

		for (int j = 0; (j + 20 * index2) < count; j++)
		{
			equipSlots[j].Init(infos[j + 20 * index2].Item,this);
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
