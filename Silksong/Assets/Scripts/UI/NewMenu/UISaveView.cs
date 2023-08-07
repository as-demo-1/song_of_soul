using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using Button = UnityEngine.UI.Button;

public class UISaveView : MonoBehaviour
{
    public UIMainMenu uiMenu_Main;
	public UISaveSlot[] uiSaveSlots;

	

	public Button backBtn;
	public SaveSystem SaveSystem;

    void Start()
    {
	    DontDestroyOnLoad(this);
	    SaveSystem.Init();
	    foreach (var o in SaveSystem.Saves)
		{
			Debug.Log("设置存档槽");
			uiSaveSlots[o.Key].Init(o.Value);
			if (PlayerController.Instance==null)
			{
				uiSaveSlots[o.Key].saveBtn.enabled = false;
			}
		}
	}

    /// <summary>
    /// 保存游戏
    /// 如果存档为空则新建一个存档
    /// </summary>
    /// <param name="index"></param>
	public void SaveGame(int index)
    {
		//UIManager.Instance.inventorySaveIndex= index;
		
		SaveSystem.SaveToSlot(index);
	    uiSaveSlots[index].Init(SaveSystem.Saves[index]);
		
    }

    /// 加载游戏
    public void ContinueGame(int index)
    {
	    StartCoroutine(SaveSystem.LoadGame(index, gameObject));
    }

	public void Return()
	{
		if (uiMenu_Main!=null)
		{
			uiMenu_Main.gameObject.SetActive(true);
			uiMenu_Main.material.DOFade(1, 2);
			uiMenu_Main.uiButtons.SetActive(true);
		}
		this.gameObject.SetActive(false);


	}
    public void DeleteSave(int index)
    {
        //SaveSystemManager.DeleteSave(index);
    }
}
