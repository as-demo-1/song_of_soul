using Opsive.UltimateInventorySystem.Core;
using Opsive.UltimateInventorySystem.Core.InventoryCollections;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoSingleton<ItemManager>
{
	private readonly static string itemPrefabPath = "item";

	private List<GameObject> itemsPool = null;

	private Inventory inventory;

	public override void Init()
	{
		itemsPool = new List<GameObject>();
		inventory = InventorySystemManager.GetInventoryIdentifier(1).Inventory;
	}


	private GameObject TryGetItemPrefabsFormPool()
	{
		GameObject item = null;
		if (itemsPool.Count > 0)
		{
			item = itemsPool[0];
			itemsPool.RemoveAt(0);
			item.transform.localScale = Vector3.one;
		}
		else
		{
			item = (GameObject)Instantiate(Resources.Load(itemPrefabPath));
		}
		return item;
	}

	/// <summary>
	/// 获取道具
	/// </summary>
	/// <param name="inventory"></param>
	/// <param name="itemName"></param>
	Item GetItem(Inventory inventory, string itemName)
	{
		Item item;

		ItemDefinition itemDefinition = InventorySystemManager.GetItemDefinition(itemName);
		if (itemDefinition == null)
		{
			Debug.LogError("???????????????~{!c~}:" + itemName);
		}

		//??????????????????????????????????
		var itemInfo = inventory.GetItemInfo(itemDefinition);
		//????
		if (itemInfo.HasValue == false)
		{
			Debug.LogError("???????????????~{!c~}:" + itemName);
			//??~{!c(9~}??????????????????????????????
			item = itemDefinition.DefaultItem;
		}
		else
		{
			item = itemInfo.Value.Item;
		}
		return item;
	}

	/// <summary>
	/// 生成道具
	/// </summary>
	/// <param name="transform"></param>
	/// <param name="itemName"></param>
	public void GenerateItem(Transform transform,string itemId)
	{		
		Item item = GetItem(inventory, itemId);
		GameObject itemObj = TryGetItemPrefabsFormPool();
		itemObj.transform.position = transform.position;
		itemObj.SetActive(true);
		itemObj.GetComponent<ItemObject>().SetItem(item);

	}
	public void RecycleItemsPrefabs(GameObject coin)
	{
		itemsPool.Add(coin);
		coin.SetActive(false);
		coin.transform.parent = this.transform;
		coin.transform.localScale = Vector3.zero;
	}

	public void ClearPool()
	{
		itemsPool.Clear();
	}
}
