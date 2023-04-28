using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIShopSuccess : MonoBehaviour
{
    UIShop shop;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))//返回商店列表
        {            
            shop.gameObject.SetActive(true);
            shop.RefreshShopItem();
            Destroy(this.gameObject);

        }
        if (Input.GetKeyDown(KeyCode.Z))//退出商店列表
        {
            
            Destroy(shop.gameObject);
            Destroy(this.gameObject);
        }
    }

    public void SetSuccess(UIShop Shop)
    {
        this.shop = Shop;
    }
}
