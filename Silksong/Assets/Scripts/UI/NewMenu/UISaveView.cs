using OfficeOpenXml.FormulaParsing.Excel.Functions.RefAndLookup;
using Opsive.UltimateInventorySystem.SaveSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UISaveView : MonoBehaviour
{
    public Button[] buttons;
    public Button button;
    public GameObject uiMenu_Main;
    //public Dictionary<bool,Button> buttons = new Dictionary<bool,Button>();
    // Start is called before the first frame update
    void Start()
    {
	    

		for (int i = 0; i < 3; i++)
        {
		}

    }

	public void NewGame(int index)
    {	
        UIManager.Instance.inventorySaveIndex= index;
        if(SaveSystemManager.Savers[index] == null)
        {
            SaveSystemManager.Save(index);
        }
        SaveSystemManager.Load(index);//²âÊÔ¼ÓÔØ

		SceneManager.LoadScene("Level1-1");//CharmTest
		UIManager.Instance.Show<UIPlayerStatus>();


	}
	public void Return()
	{
		uiMenu_Main.SetActive(true);
		this.gameObject.SetActive(false);
	}
    public void DeleteSave(int index)
    {
        SaveSystemManager.DeleteSave(index);
    }
}
