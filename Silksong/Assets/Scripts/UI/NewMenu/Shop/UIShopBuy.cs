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

    public GameObject Panel;
    public GameObject Text;
    public bool enough = true;

	UIShop uishop;

    public int index = 0;


    void Update()
    {
        if(enough)
        {
            if (Input.GetKeyDown(KeyCode.S))
            {
                if (index == 0)
                {
                    index = 1;
                    Yes.SetActive(false);
                    No.SetActive(true);
                }
            }
            if (Input.GetKeyDown(KeyCode.W))
            {
                if (index == 1)
                {
                    index = 0;
                    Yes.SetActive(true);
                    No.SetActive(false);
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Z))//购买按钮
        {
            if(index==0)
            {
				uishop.OnClickBuySuccess();
                Destroy(this.gameObject);
            }
            else
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
        this.Name.text = shopitem.name;
        this.Preview.sprite = shopitem.icon.sprite;
        //this.Icon.sprite = Item.IconImage;
        this.Price.text = shopitem.price.ToString();
    }
    public void SetNoInfo(UIShop UIShop)
    {
		this.uishop = UIShop;
		Panel.SetActive(false);
		Text.SetActive(true);
		enough = false;
		index = 1;
	}
}
