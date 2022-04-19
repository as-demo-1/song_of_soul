using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/SaveSystemSO", order = 1)]
public class SaveSystem : ScriptableObject
{
	[SerializeField] private InventorySO _playerInventory;
	[SerializeField] private InventorySO _StoreInventory;
	public string saveFilename = "save.asoul";
	public string backupSaveFilename = "save.asoul.bak";
	private Save saveData = new Save();

	public bool ContainBossGUID(string GUID)
	{
		return saveData._bossGUID.Contains(GUID);
	}

	public void AddBossGUID(string GUID)
	{
		saveData._bossGUID.Add(GUID);
	}

	public bool ContainDestructiblePlatformGUID(string GUID)
    {
		return saveData._destructiblePlatformGuid.Contains(GUID);
	}
	
	public void AddDestructiblePlatformGUID(string GUID)
    {
		saveData._destructiblePlatformGuid.Add(GUID);
	}

	/// <summary>
	/// Check the interactive item count
	/// </summary>
	/// <param name="param">sceneID - sequenceID</param>
	/// <returns>count of trigger time, -1 for not inside the dictionary</returns>
	public int ContainSceneInteractive(string param)
	{
		if (saveData._sceneInterative.ContainsKey(param))
		{
			return saveData._sceneInterative[param];
		}

		return -1;
	}

	/// <summary>
	/// Update interacitve item count
	/// </summary>
	/// <param name="param">sceneID - sequenceID</param>
	public void AddSceneInteractive(string param)
	{
		if (saveData._sceneInterative.ContainsKey(param))
		{
			saveData._sceneInterative[param]++;
		}
		else
		{
			saveData._sceneInterative.Add(param, 1);
		}

	}

	public int GetHealthMax()
	{
		return saveData._healthMax;
	}

	public void SetHealthMax(int healthMax)
	{
		saveData._healthMax = healthMax;
	}
	
	public uint GetGoldAmount()
	{
		return saveData._goldAmount;
	}

	public void SetGoldAmount(uint goldamount)
	{
		saveData._goldAmount = goldamount;
	}
	public uint GetWeaponLevel()
	{
		return saveData._weaponLevel;
	}

	public void SetWeaponLevel(uint weaponLevel)
	{
		saveData._weaponLevel = weaponLevel;
	}

	public void TestSaveGuid(string Guid)
	{
		Debug.Log("WriteData");
		saveData._finishedObjectGuid.Add(Guid);
		SaveDataToDisk();
	}
	
	//Read save data from FileManager
	public bool LoadSaveDataFromDisk()
	{
		if (FileManager.LoadFromFile(saveFilename, out var json))
		{
			saveData.LoadFromJson(json);
		}
		else return false;
		foreach (var serializedItemStack in saveData._itemStacks)
		{
			string path = AssetDatabase.GUIDToAssetPath(serializedItemStack.itemGuid);
			ItemSO tmp = AssetDatabase.LoadAssetAtPath(path,typeof(ItemSO)) as ItemSO;
			_playerInventory.Add(tmp,serializedItemStack.amount);
		}
		return true;
	}
	//Save data to file
	public void SaveDataToDisk()
	{
		saveData._itemStacks.Clear();
		foreach (var itemStack in _playerInventory.Items)
		{
			saveData._itemStacks.Add(new SerializedItemStack(itemStack.Item.Guid, itemStack.Amount));
		}
		saveData._storeStacks.Clear();
		// foreach (var storeStack in _StoreInventory.Items)
		// {
		// 	saveData._storeStacks.Add(new SerializedItemStack(storeStack.Item.Guid, storeStack.Amount));
		// }
		if (FileManager.MoveFile(saveFilename, backupSaveFilename))
		{
			if (FileManager.WriteToFile(saveFilename, saveData.ToJson()))
			{
						Debug.Log("Save successful " + saveFilename);
			}
		}
	}
	
}
