using Opsive.UltimateInventorySystem.Core.AttributeSystem;
using Opsive.UltimateInventorySystem.Core;
using Opsive.UltimateInventorySystem.Core.InventoryCollections;
using UnityEngine;

static class InventoryMethod
{

	/// <summary>
	/// 打印物品的属性值
	/// </summary>
	/// <param name="inventory"></param>
	/// <param name="itemName"></param>
	/// <param name="attributeName"></param>
	public static void GetItemAttributeValue(Inventory inventory, string itemName, string attributeName)
	{
		var item = GetItem(inventory, itemName);
		var value = GetItemAttributeAsObject(item, attributeName);
		Debug.Log(value);
	}


	/// <summary>
	/// 获得物品
	/// </summary>
	/// <param name="inventory"></param>
	/// <param name="itemName"></param>
	static Item GetItem(Inventory inventory, string itemName)
	{
		Item item;

		//获得物品定义
		ItemDefinition itemDefinition = InventorySystemManager.GetItemDefinition(itemName);
		//判空
		if (itemDefinition == null)
		{
			Debug.LogError("该系统木有这玩野:" + itemName);
		}

		//获得传入库存中与之匹配的第一个物品
		var itemInfo = inventory.GetItemInfo(itemDefinition);
		//判空
		if (itemInfo.HasValue == false)
		{
			Debug.LogError("该库存木有这玩野:" + itemName);
			//不包含，但也可以从默认物品获得属性
			item = itemDefinition.DefaultItem;
		}
		else
		{
			item = itemInfo.Value.Item;
		}
		return item;
	}

	/// <summary>
	/// 获得物品属性
	/// </summary>
	/// <param name="item"></param>
	/// <param name="attributeName"></param>
	/// <returns></returns>
	public static object GetItemAttributeAsObject(Item item, string attributeName)
	{
		//类型未知，故而使用object
		var attribute = item.GetAttribute(attributeName);
		if (attribute == null)
		{
			Debug.LogError($"物品 '{item.name}' 木有找到名字为 '{attributeName}'的属性");
			return null;
		}
		var attributeValue = attribute.GetValueAsObject();
		return attributeValue;
	}
	/// <summary>
	/// 获取float类型属性
	/// </summary>
	/// <param name="item"></param>
	/// <param name="attributeName"></param>
	/// <returns></returns>
	public static float GetItemAttributeAsFloat(Item item, string attributeName)
	{
		var floatAttribute = item.GetAttribute<Attribute<float>>(attributeName);

		if (floatAttribute == null)
		{
			Debug.LogError($"物品 '{item.name}' 木有找到float类型的 '{attributeName}'的属性");
			return float.NaN;
		}

		return floatAttribute.GetValue();
	}

	/// <summary>
	/// 获取string类型属性
	/// </summary>
	/// <param name="item"></param>
	/// <param name="attributeName"></param>
	/// <returns></returns>
	public static string GetItemAttributeAsString(Item item, string attributeName)
	{
		var stringAttribute = item.GetAttribute<Attribute<string>>(attributeName);

		if (stringAttribute == null)
		{
			Debug.LogError($"物品 '{item.name}' 木有找到string类型的 '{attributeName}'的属性");
			return null;
		}

		return stringAttribute.GetValue();
	}

	/// <summary>
	/// 获取int类型属性
	/// </summary>
	/// <param name="item"></param>
	/// <param name="attributeName"></param>
	public static void GetItemAttributeAsInt(Item item, string attributeName)
	{
		var intAttribute = item.GetAttribute<Attribute<int>>(attributeName);

		if (intAttribute == null)
		{
			Debug.LogError($"物品 '{item.name}' 木有找到int类型的 '{attributeName}'的属性");
			return;
		}

		intAttribute.GetValue();
	}

	/// <summary>
	/// 获取sprite类型属性
	/// </summary>
	/// <param name="item"></param>
	/// <param name="attributeName"></param>
	/// <returns></returns>
	public static Sprite GetItemAttributeAsSprite(Item item, string attributeName)
	{
		var intAttribute = item.GetAttribute<Attribute<Sprite>>(attributeName);

		if (intAttribute == null)
		{
			Debug.LogError($"物品 '{item.name}' 木有找到Sprite类型的 '{attributeName}'的属性");
			return null;
		}

		return intAttribute.GetValue();
	}

	/// <summary>
	/// 设置物品属性
	/// </summary>
	/// <param name="inventory"></param>
	/// <param name="itemName"></param>
	/// <param name="attributeName"></param>
	/// <param name="value"></param>
	public static void SetItemAttributeValue(Inventory inventory, string itemName, string attributeName, string value)
	{
		var item = GetItem(inventory, itemName);
		SetItemAttributeAsObject(item, attributeName, value);

	}


	static void SetItemAttributeAsObject(Item item, string attributeName, string attributeValueAsStringObject)
	{
		if (item.IsMutable == false)
		{
			Debug.Log("不可变物品");
			return;
		}

		var itemAttribute = item.GetAttribute(attributeName);
		if (itemAttribute == null)
		{
			Debug.Log($"物品中没有'{item.name}'属性");
			return;
		}

		if (itemAttribute.AttachedItem == null)
		{
			Debug.Log($"'{attributeName}'不是一个属性");
			return;
		}
		//不知道属性类型，用string设置
		itemAttribute.SetOverrideValueAsObject(attributeValueAsStringObject);
	}


	static void SetItemAttributeAsInt(Item item, string attributeName, int attributeValue)
	{
		var intAttribute = item.GetAttribute<Attribute<int>>(attributeName);

		if (intAttribute == null)
		{
			Debug.Log($"物品中没有int类型的'{item.name}'属性");
			return;
		}

		intAttribute.SetOverrideValue(attributeValue);
	}

	static void SetItemAttributeAsString(Item item, string attributeName, string attributeValue)
	{
		var stringAttribute = item.GetAttribute<Attribute<string>>(attributeName);

		if (stringAttribute == null)
		{
			Debug.Log($"物品中没有string类型的'{item.name}'属性");
			return;
		}

		stringAttribute.SetOverrideValue(attributeValue);
	}

	static void SetItemAttributeAsFloat(Item item, string attributeName, float attributeValue)
	{
		var floatAttribute = item.GetAttribute<Attribute<float>>(attributeName);

		if (floatAttribute == null)
		{
			Debug.Log($"物品中没有float类型的'{item.name}'属性");
			return;
		}

		floatAttribute.SetOverrideValue(attributeValue);
	}
}
