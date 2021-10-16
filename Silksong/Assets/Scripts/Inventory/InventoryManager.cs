using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{

	[SerializeField] private InventorySO _currentInventory = default;
	[SerializeField] private SaveSystem _saveSystem;
	
	void AddItem(ItemSO item)
	{
		_currentInventory.Add(item);
		_saveSystem.SaveDataToDisk();

	}
	void AddItemStack(ItemStack itemStack)
	{
		_currentInventory.Add(itemStack.Item, itemStack.Amount);
		_saveSystem.SaveDataToDisk();

	}
	void RemoveItem(ItemSO item)
	{
		_currentInventory.Remove(item);
		_saveSystem.SaveDataToDisk();
	}
	
}

