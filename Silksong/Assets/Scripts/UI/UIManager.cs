using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Information;
using UnityEngine;


public class UIManager : MonoSingleton<UIManager>
{
	[System.Serializable]
	class UIElement
	{
		public int level;
		public string Resources;//UI存的资源路径
		public bool Cache;//UI关闭后是否销毁
		public GameObject Instance;//如果Cache，把instance存下来
		public bool need;//需要禁止人物输入
	}
	public int inventorySaveIndex;//临时测试
	public Transform[] transforms;
	private bool isOpen = false;

	[SerializeField]
	private Dictionary<Type, UIElement> UIResoureces = new Dictionary<Type, UIElement>();//保存定义的UI信息

	private bool isShifting;
	public override void Init()
	{
		DontDestroyOnLoad(gameObject);
		this.UIResoureces.Add(typeof(UIMenu_System), new UIElement() { level = 2, Resources = "UI/UIMenu_System", Cache = false , need = true});
		this.UIResoureces.Add(typeof(UIPlayerStatus), new UIElement() {level = 3, Resources = "UI/UIPlayerStatus", Cache = true, need =false});
		this.UIResoureces.Add(typeof(UIShop), new UIElement() {level = 2, Resources = "UI/UIShop", Cache = false, need= true });
		this.UIResoureces.Add(typeof(UISaveView), new UIElement() {level = 1, Resources = "UI/UISave", Cache = false , need = true });
		//Application.targetFrameRate = 60;
	}
	/// <summary>
	/// 显示UI
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <returns></returns>
	public T Show<T>()
	{
		if (isOpen) return default(T);
		Type type = typeof(T);
		if (this.UIResoureces.ContainsKey(type))
		{
			
			UIElement info = this.UIResoureces[type];
			if (info.need)
			{
				isOpen = true;
				PlayerInput.Instance.ReleaseControls();
			}
			if (info.Instance != null)
			{
				info.Instance.SetActive(true);
				//Debug.Log("激活"+typeof(T));
			}
			else
			{
				UnityEngine.Object prefab = Resources.Load(info.Resources);
				if (prefab == null)
				{
					return default(T);
				}

				info.Instance = (GameObject)GameObject.Instantiate(prefab, transforms[info.level]);
			}
			return info.Instance.GetComponent<T>();
		}
		return default(T);
	}

	public void Close(Type type)
	{

		if (this.UIResoureces.ContainsKey(type))
		{
			
			UIElement info = this.UIResoureces[type];
			if(info.need)
			{
				isOpen = false;
				PlayerInput.Instance?.GainControls();
			}
			if (!info.Cache)
			{
				info.Instance?.SetActive(false);
			}
			else
			{
				GameObject.Destroy(info.Instance);
				info.Instance = null;
			}
		}
	}
	public void Close<T>()
	{
		this.Close(typeof(T));
	}

	/// <summary>
	/// 切换UI
	/// 按下打开，再次按下关闭
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public void Shift<T>()
	{
		if (isShifting)
		{
			return;
		}

		isShifting = true;
		Type type = typeof(T);
		if (this.UIResoureces.ContainsKey(type))
		{
			UIElement info = this.UIResoureces[type];
			if (info.Instance != null && info.Instance.activeInHierarchy)
			{
				Close<T>();
			}
			else
			{
				Show<T>();
			}
		}

		DOVirtual.DelayedCall(0.5f, () => isShifting = false);
	}

	

	public void LateUpdate()
	{
		//打开界面测试
		if (PlayerInput.Instance.menu.Up)
		{
			Debug.Log("按下menu键");
			Shift<UIMenu_System>();
		}
		if (PlayerInput.Instance.quickMap.Down)
		{
			Show<UIMenu_System>()?.ChangeTab(0);
		}
		if (PlayerInput.Instance.quickMap.Up)
		{
			Close<UIMenu_System>();
		}
		// if (Input.GetKeyDown(KeyCode.H))
		// {
		// 	UIManager.Instance.Close<UIShop>();
		// }
		// if (Input.GetKeyDown(KeyCode.O))
		// {
		// 	UIManager.Instance.Show<UIShop>();
		// }
		// if (Input.GetKeyDown(KeyCode.P))
		// {
		// 	UIManager.Instance.Close<UISaveView>();
		// }
		// if (Input.GetKeyDown(KeyCode.L))
		// {
		// 	UIManager.Instance.Show<UISaveView>();
		// }

	}
}
