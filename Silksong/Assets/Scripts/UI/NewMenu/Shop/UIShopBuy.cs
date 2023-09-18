using Opsive.UltimateInventorySystem.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIShopBuy : MonoBehaviour
{


    public Text Name;
    public Image Preview;
    //public Image Icon;
    public Text Price;

    public GameObject Yes;
    public GameObject No;

    ShopItem buyItem;
    UIShop uishop;

    private int a = 0;


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            if(a==0)
            {
                a = 1;
                Yes.SetActive(false);
                No.SetActive(true);
            }
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            if(a==1)
            {
                a = 0;
                Yes.SetActive(true);
                No.SetActive(false);
            }
        }
        if (Input.GetKeyDown(KeyCode.Z))//购买按钮
        {
            if(a==0)
            {
				uishop.OnClickBuySuccess();
                Destroy(this.gameObject);
            }
            if(a==1)
            {
				uishop.gameObject.SetActive(true);
                Destroy(this.gameObject);
            }
        }
        if(Input.GetKeyDown(KeyCode.A))
        {
			uishop.gameObject.SetActive(true);
            uishop.itembuy.IsSelected();//.GetComponent<Selectable>().Select();
			Destroy(this.gameObject);
        }
    }
    public void SetBuyInfo(ShopItem shopitem, UIShop UIShop)
    {
        this.uishop = UIShop;
        this.buyItem = shopitem;
        this.Name.text = shopitem.name;
        this.Preview.sprite = shopitem.icon.sprite;
        //this.Icon.sprite = Item.IconImage;
        this.Price.text = shopitem.price.ToString();
    }
}
