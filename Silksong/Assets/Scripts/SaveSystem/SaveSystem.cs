using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/SaveSystemSO", order = 1)]

public class SaveSystem : ScriptableObject//you can get SaveSystem instance from GameManager 
{
	[SerializeField] private InventorySO _playerInventory;
	[SerializeField] private InventorySO _StoreInventory;
	public string saveFilename = "save.asoul";
	public string backupSaveFilename = "save.asoul.bak";
	private Save saveData = new Save();

	//player-------------------------------------------------------
	public bool haveSoulJump()
    {
		return saveData.haveSoulJump;
    }

	public void setSoulJump(bool v)
    {
		saveData.haveSoulJump =v;
    }

	public bool haveDoubleJump()
	{
		return saveData.haveDoubleJump;
	}

	public void setDoubleJump(bool v)
	{
		saveData.haveDoubleJump = v;
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
	
	public void learnSkill(EPlayerStatus skill,bool v)
    {
		if (saveData.learnedSkills.ContainsKey(skill) == false)
			saveData.learnedSkills.Add(skill, v);
		saveData.learnedSkills[skill] = v;
    }

	public bool getLearnedSkill(EPlayerStatus skill)
    {
		if (saveData.learnedSkills.ContainsKey(skill) == false) return false;
		return saveData.learnedSkills[skill];
    }

	//mapObject-------------------------------------------------------------
	public bool ContainBossGUID(string GUID)
	{
		return saveData._bossGUID.Contains(GUID);
	}

	public void AddBossGUID(string GUID)
	{
		saveData._bossGUID.Add(GUID);
	}


	public bool ContainDestroyedGameObj(string GUID)
	{
		return saveData._destroyedGameObjs.Contains(GUID);
	}
	public void AddDestroyedGameObj(string GUID)
	{
		saveData._destroyedGameObjs.Add(GUID);
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

	

	public void TestSaveGuid(string Guid)
	{
		Debug.Log("WriteData");
		saveData._finishedObjectGuid.Add(Guid);
		SaveDataToDisk();
	}
	
	//Read save data from FileManager
	public bool LoadSaveDataFromDisk()
	{
		Debug.Log("LoadSave");
#if UNITY_EDITOR
		if (FileManager.LoadFromFile(saveFilename, out var json))
		{
			//saveData.LoadFromJson(json);
			saveData = saveData.LoadFromJson(json);
		}
		else return false;
		foreach (var serializedItemStack in saveData._itemStacks)
		{
			string path = AssetDatabase.GUIDToAssetPath(serializedItemStack.itemGuid);
			ItemSO tmp = AssetDatabase.LoadAssetAtPath(path,typeof(ItemSO)) as ItemSO;
			_playerInventory.Add(tmp,serializedItemStack.amount);
		}
		return true;
#endif

#if UNITY_STANDALONE //can not use assetDataBase when publish, to be fixed
		return false;
#endif
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
		bool noCurrentSave;
		bool moveOk;
		moveOk=FileManager.MoveFile(saveFilename, backupSaveFilename, out noCurrentSave);//move current save to back,then save new data
		if(noCurrentSave || moveOk)
		{
			if (FileManager.WriteToFile(saveFilename, saveData.ToJson()))
			{
						Debug.Log("Save successful " + saveFilename);
			}
		}
	}
	
}
