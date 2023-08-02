using BehaviorDesigner.Runtime.Tasks;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIMainMenu : MonoBehaviour
{
	public List<Button> btns;

	public bool isEnter = false;
	public Material material;
	//OnSureWindow onSureWindow;
	public Text tex;
	public Text extraText;
	public UISaveView uiSaveView;
	public GameObject uiButtons;

	public GameObject uiSettingPanel;
	[SerializeField]
	private string newGameLeve = "Level1-1";
	// Start is called before the first frame update
	void Start()
    {
		btns[0].onClick.AddListener(NewGame);
		btns[1].onClick.AddListener(ContinueGame);
		btns[2].onClick.AddListener(ShowSettings);
		btns[3].onClick.AddListener(ShowExtra);
		btns[4].onClick.AddListener(ClickExit);
		tex.DOFade(1, 2).SetLoops(-1, LoopType.Yoyo);
	}



	// Update is called once per frame
	void Update()
    {
		if (Input.anyKeyDown)
		{
			if (!isEnter)
			{
				isEnter = true;
				tex.gameObject.SetActive(false);
				uiButtons.SetActive(true);
				material.DOFade(1, 2);
			}
		}
	}
	public void OnSwitch()
	{
		Debug.Log("播放切换按钮音效");
	}

	public void OnSelect()
	{
		Debug.Log("播放选择按钮音效");
	}


	void NewGame()
	{
		StartCoroutine(NewGameIE());
		/*Debug.Log("on click New Game");
		UIManager.Instance.Show<UIPlayerStatus>();
		material.DOFade(0, 2);
		uiSaveView.SetActive(true);
		uiButtons.SetActive(false);
		//this.gameObject.SetActive(false);*/
		SceneManager.LoadScene(newGameLeve);
	}
	IEnumerator NewGameIE()
	{
		Debug.Log("on click New Game");
		material.DOFade(0, 2);
		
		yield return new WaitForSeconds(2.0f);
		uiButtons.SetActive(false);
	}

	void ContinueGame()
	{
		
		uiSaveView.gameObject.SetActive(true);
		/*
		Debug.Log("on click continue game");
		UIManager.Instance.Show<UIPlayerStatus>();
		SceneManager.LoadScene("Level1-1");
		*/
	}

	void ShowSettings()
	{
		Debug.Log("on click show settings");
		uiSettingPanel.SetActive(true);
	}

	void ShowExtra()
	{
		Debug.Log("on click show extra");
		extraText?.gameObject.SetActive(true);
	}

	void ClickExit()
	{
		Debug.Log("on click exit");
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;//用于退出运行
#else
		Application.Quit();
#endif
	}
}
