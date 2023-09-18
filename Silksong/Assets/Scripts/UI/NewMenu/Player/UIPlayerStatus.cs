using DG.Tweening;
using Opsive.UltimateInventorySystem.Core.InventoryCollections;
using Opsive.UltimateInventorySystem.Core;
using Opsive.UltimateInventorySystem.Exchange;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Opsive.UltimateInventorySystem.DatabaseNames.DemoInventoryDatabaseNames;


public class UIPlayerStatus : MonoBehaviour
{

	public HpDamable representedDamable;

	private int moneyNum = 0;       //测试用，应该从数据类拿到金币数量

	private int addAll = 0;     //增加的总量

	public float changeTime = 3.0f;     //数字记录的时间
	private float changeTimeRecord = 3.0f;
	private bool changeTimeStart = false;
	private bool changeTimeOver = false;

	public float zeroTime = 1.5f;       //数字归零的时间
	private float zeroTimeRecord = 1.5f;
	private float perReduce = 0;

	public GameObject hpItem; 
	public GameObject hpBg;//血条底头
	private Vector3 hpBgPos;
	public Transform content;
	public List<GameObject> hpItems;
	public Slider slider;

	public Text TextCoin;
	public Text TextAddCoin;

	private Inventory inventory;
	private CurrencyCollection ownerCurrencyCollection;
	Currency gold;

	// Start is called before the first frame update
	void Start()
    {
		DontDestroyOnLoad(gameObject);
		EventManager.Instance.Register<int>(EventType.onMoneyChange, ChangeMoneyNum);

		inventory = GameManager.Instance.inventory; 
		var currencyOwner = GameManager.Instance.currencyOwner;
		ownerCurrencyCollection = currencyOwner.CurrencyAmount;
		gold = InventorySystemManager.GetCurrency("魂石");
		moneyNum = ownerCurrencyCollection.GetAmountOf(gold);
		TextCoin.text = moneyNum.ToString();
		TextAddCoin.text ="";
		hpBgPos = hpBg.transform.localPosition;
    }

	// Update is called once per frame
	void Update()
    {
		UpdateMoney();
	}
	#region Hp
	public void setRepresentedDamable(HpDamable hpDamable)//设置血量上限
	{
		if (representedDamable == null)
		{
			Debug.Log(hpDamable);
			representedDamable = hpDamable;
			representedDamable.onHpChange.AddListener(ChangeHitPointUI);
		}
		else
		{
			deleteAllHpIcon();
		}
		int maxHp = representedDamable.MaxHp;
		hpBg.transform.localPosition = hpBgPos + new Vector3(maxHp * 45, 0, 0);
		//hpBg.transform.Translate(maxHp * 45,0,0);//DOMoveX(maxHp*45,1);
		for(int i = hpItems.Count;i< maxHp;i++)
		{
			GameObject go = Instantiate(hpItem, content);
			hpItems.Add(go);
		}
	}

	public void ChangeHitPointUI(HpDamable damageable)//血量变动时调用
	{
		for (int i = 0; i < hpItems.Count; i++)
		{
			hpItems[i].transform.GetChild(1).transform.GetComponent<Image>().DOColor(damageable.CurrentHp >= i + 1 ? Color.white : Color.black, 1f);
		}
	}

	private void deleteAllHpIcon()
	{
		
	}


	#endregion

	#region Mana
	public void ChangeManaValue(PlayerCharacter playerCharacter)
	{
		slider.maxValue = playerCharacter.getMaxMana();
		slider.minValue = 0;
		slider.DOValue(playerCharacter.Mana, 0.5f);
	}

	public void ChangeManaMax(PlayerCharacter playerCharacter)
	{
		slider.maxValue = playerCharacter.getMaxMana();
		slider.maxValue = 0;
		slider.DOValue(playerCharacter.Mana, 0.5f);
	}
	#endregion

	#region Money
	void UpdateMoney()
	{
		if (changeTimeStart)    //增加金币的行为开始
		{
			changeTimeRecord -= Time.deltaTime;
			if (changeTimeRecord <= 0)  //记录时间结束
			{
				changeTimeOver = true;
				changeTimeStart = false;
				PrepareCountDown();
			}
		}

		if (changeTimeOver)     //增加金币的行为结束了，即已经有一段时间没有获取到金币了，开始倒数增加金币
		{
			zeroTimeRecord -= Time.deltaTime;
			if (zeroTimeRecord >= 0)
			{
				ChangeAllNum();
			}
			else
			{
				changeTimeOver = false;
				ForceZero();
				ResetStatus();
			}
		}
	}
	
	void ChangeMoneyNum(int changeNum)
	{
		changeTimeRecord = changeTime;  //又获得了新的金币，重置时间
		if (changeNum >= 0)
		{
			ownerCurrencyCollection.AddCurrency(gold, changeNum);
			changeTimeStart = true;
			addAll += changeNum;
			TextAddCoin.DOFade(1, 1);
			TextAddCoin.text = "+" + addAll.ToString();

		}
		else
		{
			ownerCurrencyCollection.RemoveCurrency(gold, -changeNum);
			moneyNum += changeNum;
			TextCoin.text = moneyNum.ToString();

		}
		//TextAddCoin.color.g = 1;
		
	}

	void PrepareCountDown()
	{
		perReduce = addAll / zeroTime;
	}

	void ChangeAllNum()
	{
		float detal = perReduce * (zeroTime - zeroTimeRecord);
		TextCoin.text = ((int)(moneyNum + detal)).ToString();
		TextAddCoin.text = "+" + ((int)(addAll - detal)).ToString();
	}

	/// <summary>
	/// 最后一帧强制归零
	/// </summary>
	void ForceZero()
	{
		moneyNum += addAll;
		TextCoin.text = moneyNum.ToString();
		TextAddCoin.text = "+0";
	}

	void ResetStatus()
	{
		addAll = 0;
		changeTimeRecord = changeTime;
		changeTimeStart = false;
		changeTimeOver = false;
		zeroTimeRecord = zeroTime;       //数字归零的时间
		TextAddCoin.DOFade(0, 1);
	}
	#endregion
}
