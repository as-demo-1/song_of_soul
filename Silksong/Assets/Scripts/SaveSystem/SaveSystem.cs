using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Opsive.UltimateInventorySystem.Core;
using Opsive.UltimateInventorySystem.SaveSystem;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/SaveSystemSO", order = 1)]

public class SaveSystem : SerializedScriptableObject//you can get SaveSystem instance from GameManager 
{
	//[SerializeField] private InventorySO _playerInventory;
	//[SerializeField] private InventorySO _StoreInventory;
	[SerializeField]private string fileSuffix = ".asoul";
	[SerializeField]private string saveFilename = "save";
	[SerializeField]private string backupSaveFilename = "save.asoul.bak";
	[SerializeField]private Save saveData = new Save();

	public Save SaveData => saveData;
	private Dictionary<int, Save> saves = new Dictionary<int, Save>();

	public Dictionary<int, Save> Saves => saves;

	
	[SerializeField]
	private int maxSlot = 4;


	//player-------------------------------------------------------
	public int GetHealthMax()
	{
		return saveData._healthMax;
	}

	public void SetHealthMax(int healthMax)
	{
		saveData._healthMax = healthMax;
	}

	public int GetManaMax()
	{
		return saveData._manaMax;
	}

	public void SetManaMax(int manaMax)
	{
		saveData._manaMax = manaMax;
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
		/*foreach (var serializedItemStack in saveData._itemStacks)
		{
			string path = AssetDatabase.GUIDToAssetPath(serializedItemStack.itemGuid);
			ItemSO tmp = AssetDatabase.LoadAssetAtPath(path,typeof(ItemSO)) as ItemSO;
			_playerInventory.Add(tmp,serializedItemStack.amount);
		}*/
		return true;
#endif

#if UNITY_STANDALONE //can not use assetDataBase when publish, to be fixed
		return false;
#endif
	}
	//Save data to file
	public void SaveDataToDisk()
	{
		//saveData._itemStacks.Clear();
		/*foreach (var itemStack in _playerInventory.Items)
		{
			saveData._itemStacks.Add(new SerializedItemStack(itemStack.Item.Guid, itemStack.Amount));
		}*/
		//saveData._storeStacks.Clear();
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


	/// <summary>
	/// 初始化
	/// 加载所有存档数据
	/// </summary>
	public void Init()
	{
		for (int i = 0; i < maxSlot; i++)
		{
			string filename = saveFilename + "_" + i.ToString() + fileSuffix; 
			Debug.Log("Load"+filename);
			if (FileManager.LoadFromFile(filename, out var json))
			{
				if (saves.ContainsKey(i))
				{
					saves[i] = saves[i].LoadFromJson(json);
					saves[i].slotIndex = (uint)i;
				}
				else
				{
					Save _save = new Save();
					_save = _save.LoadFromJson(json);
					saves.Add(i, _save); 
				}
			}
		}
	}
	
	/// <summary>
	/// 保存指定存档，并写入本地
	/// </summary>
	/// <param name="index"></param>
	public void SaveToSlot(int index)
	{
		if (saves.ContainsKey(index))
		{
			saves[index] = saveData;
			saves[index].slotIndex = (uint)index;
		}
		else
		{
			saves.Add(index, saveData);
		}

		saveData.levelName = SceneManager.GetActiveScene().name;
		saveData.timestamp = DateTime.Now.ToFileTime();
		SaveSystemManager.Save(index);// 物品系统存档
		PixelCrushers.SaveSystem.SaveToSlot(index);// 对话系统存档
		
		string filename = saveFilename + "_" + index.ToString() + fileSuffix;
		if (FileManager.WriteToFile(filename, saveData.ToJson()))
		{
			Debug.Log("Save successful " + filename);
		}
	}

	/// <summary>
	/// 载入指定存档
	/// </summary>
	/// <param name="index"></param>
	public IEnumerator LoadGame(int index, GameObject panel = null)
	{
		if (saves.ContainsKey(index))
		{
			saveData = saves[index];
			// 物品系统部分
			
			//SceneController.TransitionToScene(saves[index].levelName);
			AsyncOperation ao = SceneManager.LoadSceneAsync(saves[index].levelName);
			yield return ao;
			GameObjectTeleporter.Instance.playerEnterSceneEntance(SceneEntrance.EntranceTag.A, Vector3.zero);//玩家到场景入口 
			PlayerInput.Instance.GainControls();
			Debug.Log("加载完成");
			// 物品系统部分
			if (SaveSystemManager.Saves.ContainsKey(index))
			{
				SaveSystemManager.Load(index);
			}
			// 对话系统部分
			PixelCrushers.SaveSystem.LoadFromSlot(index);
			
		}

		if (panel)
		{
			panel.SetActive(false);
			
		}
		

		yield return null;
	}

	public void LoadGame(int index)
	{
		saveData = saves[index];
		// 物品系统部分
		if(SaveSystemManager.Saves.ContainsKey(index))
			SaveSystemManager.Load(index);
		// 对话系统部分
		PixelCrushers.SaveSystem.LoadFromSlot(index);
		SceneManager.LoadScene(saves[index].levelName);
	}

	/// <summary>
	/// 删除某个存档
	/// </summary>
	/// <param name="index"></param>
	public void DeleteSave(int index)
	{
		
	}
	
}
