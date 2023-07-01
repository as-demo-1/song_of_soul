using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class UIManager : MonoSingleton<UIManager>
{
	class UIElement
	{
		public string Resources;//UI存的资源路径
		public bool Cache;//UI关闭后是否销毁
		public GameObject Instance;//如果Cache，把instance存下来
	}
	public int inventorySaveIndex;//临时测试

	private Dictionary<Type, UIElement> UIResoureces = new Dictionary<Type, UIElement>();//保存定义的UI信息

	public override void Init()
	{
		DontDestroyOnLoad(gameObject);
		this.UIResoureces.Add(typeof(UIMenu_System), new UIElement() { Resources = "UI/UIMenu_System", Cache = false });
		this.UIResoureces.Add(typeof(UIPlayerStatus), new UIElement() { Resources = "UI/UIPlayerStatus", Cache = true });
		this.UIResoureces.Add(typeof(UIShop), new UIElement() { Resources = "UI/UIShop", Cache = false });
		Application.targetFrameRate = 60;
	}
	public T Show<T>()
	{
		Type type = typeof(T);
		if (this.UIResoureces.ContainsKey(type))
		{
			UIElement info = this.UIResoureces[type];
			if (info.Instance != null)
			{
				info.Instance.SetActive(true);
			}
			else
			{
				UnityEngine.Object prefab = Resources.Load(info.Resources);
				if (prefab == null)
				{
					return default(T);
				}
				info.Instance = (GameObject)GameObject.Instantiate(prefab);
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
			if (info.Cache)
			{
				info.Instance.SetActive(false);
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
    /// 这是地图的ui调取，包含了一个shop界面可以看看
    /// </summary>
	public void Update()
	{
		//打开界面测试
		if (PlayerInput.Instance.showMap.Down)
		{
			UIManager.Instance.Show<UIMenu_System>();
		}
		if (PlayerInput.Instance.quickMap.Down)
		{
			UIManager.Instance.Close<UIMenu_System>();
		}
		if (Input.GetKeyDown(KeyCode.H))
		{
			UIManager.Instance.Close<UIShop>();
		}
		if (Input.GetKeyDown(KeyCode.O))
		{
			UIManager.Instance.Show<UIShop>();
		}

	}
}
