using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/SaveSystemSO", order = 1)]
public class SaveSystem : ScriptableObject
{
	
	public string saveFilename = "save.asoul";
	public string backupSaveFilename = "save.asoul.bak";
	public Save saveData = new Save();

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
			return true;
		}

		return false;
	}
	//Save data to file
	public void SaveDataToDisk()
	{
		if (FileManager.MoveFile(saveFilename, backupSaveFilename))
		{
			if (FileManager.WriteToFile(saveFilename, saveData.ToJson()))
			{
						Debug.Log("Save successful " + saveFilename);
			}
		}
	}
	
}
