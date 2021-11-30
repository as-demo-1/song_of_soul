using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{

	[SerializeField] private InventorySO _currentInventory = default;
	[SerializeField] private SaveSystem _saveSystem;
	
	public void AddItem(ItemSO item)
	{
		_currentInventory.Add(item);
		_saveSystem.SaveDataToDisk();

	}
	public void AddItemStack(ItemStack itemStack)
	{
		_currentInventory.Add(itemStack.Item, itemStack.Amount);
		_saveSystem.SaveDataToDisk();

	}
	public void RemoveItem(ItemSO item)
	{
		_currentInventory.Remove(item);
		_saveSystem.SaveDataToDisk();
	}

	public void LoadSave()
	{
		_saveSystem.LoadSaveDataFromDisk();
		foreach (var serializedItemStack in _saveSystem.saveData._itemStacks)
		{
			string path = AssetDatabase.GUIDToAssetPath(serializedItemStack.itemGuid);
			ItemSO tmp = AssetDatabase.LoadAssetAtPath(path,typeof(ItemSO)) as ItemSO;
			_currentInventory.Add(tmp,serializedItemStack.amount);
		}
	}
	
}

