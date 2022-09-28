using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class InventoryManager : MonoSingleton<InventoryManager>
{

	[SerializeField] public InventorySO _currentInventory = default;
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
	public ItemSO SearchItem(string id)
    {
		return _currentInventory.Search(id);
	}
	public void RemoveItem(string id,int count=1)
	{
		_currentInventory.Remove(_currentInventory.Search(id),count);
		_saveSystem.SaveDataToDisk();
	}
	public void LoadSave()
	{
		_currentInventory.Items.Clear();
		_saveSystem.LoadSaveDataFromDisk();
		
	}
	
}

