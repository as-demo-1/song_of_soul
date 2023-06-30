using Opsive.UltimateInventorySystem.Core;
using Opsive.UltimateInventorySystem.Exchange;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ShopItem : MonoBehaviour, ISelectHandler
{

    public Image icon;
    public Text priceText;

    private UIShop uiShop;
    public Item item;

    public string name;
    public int price;
    public string description;
    public int index;

	public void Init(Item Item,UIShop UIShop,int Index)
    {
        item = Item;
        uiShop = UIShop;
		item.TryGetAttributeValue<string>("NameSid", out var NameSid);
        name = NameSid;
		item.TryGetAttributeValue<Sprite>("Icon", out var Icon);
        icon.sprite = Icon;
		item.TryGetAttributeValue<int>("Price", out var Price);
		price = Price;
		item.TryGetAttributeValue<string>("DescSid", out var DescSid);
		description = DescSid;
		priceText.text = price.ToString();
		index = Index;
    }

	public void OnSelect(BaseEventData eventData)
	{
        uiShop.itembuy = this;
        uiShop.RefreshUI(this);
	}
}
