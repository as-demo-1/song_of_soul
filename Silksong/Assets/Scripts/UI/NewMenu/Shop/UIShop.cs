using DG.Tweening;
using OfficeOpenXml.FormulaParsing.Excel.Functions.RefAndLookup;
using Opsive.UltimateInventorySystem.Core;
using Opsive.UltimateInventorySystem.Core.DataStructures;
using Opsive.UltimateInventorySystem.Core.InventoryCollections;
using Opsive.UltimateInventorySystem.Exchange;
using Opsive.UltimateInventorySystem.Exchange.Shops;
using Opsive.UltimateInventorySystem.SaveSystem;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Opsive.UltimateInventorySystem.DatabaseNames.DemoInventoryDatabaseNames;

public class UIShop : MonoBehaviour
{

    /// content位置
    public Transform itemRoot;

    private Inventory inventory;
    private CurrencyOwner currencyOwner;

    public ShopItem itembuy;

    public GameObject shopItme;
    public GameObject uiShopBuy;
    public GameObject uiShopSuccess;


	public Text itemName;
    public Text itemDescribe;

	List<ShopItem> itemList = new List<ShopItem>();

    void Start()
    {		
		PlayerInput.Instance.ReleaseControls();
		inventory = InventorySystemManager.GetInventoryIdentifier(GameManager.Instance.saveSystem.SaveData.inventoryIndex).Inventory;

		currencyOwner = InventorySystemManager.GetInventoryIdentifier(GameManager.Instance.saveSystem.SaveData.inventoryIndex).CurrencyOwner;
		var gold = InventorySystemManager.GetCurrency("Gold");
		Init();
	}



	void Update()
    {
        if(Input.GetKeyDown(KeyCode.Z))//购买按钮
        {
			OnClickBuy();
        }
        if (Input.GetKeyDown(KeyCode.A))//关闭商店
        {
			Destroy(this.gameObject);
		}
        
    }


    public void  Init()
    {
        List<ItemStack> items = (List<ItemStack>)inventory.GetItemCollection("Shop1").GetAllItemStacks();
		for (int i = 0; i < items.Count; i++)
		{
			GameObject go = Instantiate(shopItme, itemRoot);
			ShopItem item = go.GetComponent<ShopItem>();
            item.Init(items[i].Item, this, i);
			itemList.Add(item);
		}
        itemList[0].GetComponent<Selectable>().Select();
    }


	//移动列表
	public void Move(int index)
	{
        itemRoot.DOLocalMoveY(index * 120,1f);
	}

	/// <summary>
	/// 返回商店时刷新道具信息
	/// </summary>
    public void RefreshShopItem()
    {
        itemList.Remove(itembuy);
		Destroy(itembuy.gameObject);
        
        for (int i = 0;i< itemList.Count; i++)
        {
            itemList[i].index = i;
        }
        itemList[0].GetComponent<Selectable>().Select();
		//EventSystem.current.SetSelectedGameObject(itemRoot.GetChild(0).gameObject);

		Debug.Log(itemRoot.GetChild(0).gameObject);

	}

	//刷新UI
    public void RefreshUI(ShopItem shopItem)
    {
        itemName.text = shopItem.name;
		itemDescribe.text = shopItem.description;
        Move(shopItem.index);
	}

	/// <summary>
	/// 购买商品
	/// </summary>
	public void OnClickBuy()
	{
		CurrencyCollection ownerCurrencyCollection = currencyOwner.CurrencyAmount;
		var gold = InventorySystemManager.GetCurrency("魂石");

		if (ownerCurrencyCollection.GetAmountOf(gold) > itembuy.price)
		{
			EventSystem.current.SetSelectedGameObject(null);
			GameObject go = Instantiate(uiShopBuy, transform.position, transform.rotation);
			UIShopBuy buy = go.GetComponent<UIShopBuy>();
			buy.SetBuyInfo(itembuy, this);
			this.gameObject.SetActive(false);
		}
		else
		{
			
			Debug.LogFormat("购买失败");
		}
	}

	/// <summary>
	/// 购买成功页面按钮调用
	/// </summary>
	public void OnClickBuySuccess()
	{
		Debug.Log("购买成功");

		CurrencyCollection ownerCurrencyCollection = currencyOwner.CurrencyAmount;
		var gold = InventorySystemManager.GetCurrency("魂石");

		inventory.GetItemCollection("Shop1").RemoveItem(itembuy.item, 1);
		inventory.GetItemCollection("Main").AddItem(itembuy.item, 1);
		ownerCurrencyCollection.RemoveCurrency(gold, itembuy.price);
		EventManager.Instance.Dispatch(EventType.onMoneyChange,-itembuy.price);
		SaveSystemManager.Save(UIManager.Instance.inventorySaveIndex);//保存数据

		GameObject go = Instantiate(uiShopSuccess);
		UIShopSuccess suc = go.GetComponent<UIShopSuccess>();
		suc.SetSuccess(this);
	}
	private void OnDestroy()
	{
		PlayerInput.Instance.GainControls();
	}
}


