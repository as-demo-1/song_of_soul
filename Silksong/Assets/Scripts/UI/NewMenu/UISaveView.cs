using DG.Tweening;
using OfficeOpenXml.FormulaParsing.Excel.Functions.RefAndLookup;
using Opsive.UltimateInventorySystem.SaveSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static InputComponent;
using static Opsive.UltimateInventorySystem.DatabaseNames.DemoInventoryDatabaseNames;

public class UISaveView : MonoBehaviour
{
    public UIMainMenu uiMenu_Main;
	public UISaveSlot[] uiSaveSlots;

	[SerializeField]
	private string newGameLeve = "Level1-1";

    void Start()
    {
		foreach (var o in SaveSystemManager.Saves)
		{
			uiSaveSlots[o.Key].Init();
		}
	}

	public void NewGame(int index)
    {
		//UIManager.Instance.inventorySaveIndex= index;

		if (!SaveSystemManager.Saves.ContainsKey(index))
		{
			SaveSystemManager.Save(index);
		}
		SaveSystemManager.Load(index);//╪сть

		SceneManager.LoadScene(newGameLeve);
	}

	public void Return()
	{
		uiMenu_Main.gameObject.SetActive(true);
		uiMenu_Main.material.DOFade(1, 2);
		uiMenu_Main.uiButtons.SetActive(true);
		this.gameObject.SetActive(false);


	}
    public void DeleteSave(int index)
    {
        SaveSystemManager.DeleteSave(index);
    }
}
