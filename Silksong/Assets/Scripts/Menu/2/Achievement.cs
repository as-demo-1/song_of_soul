using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Achievement : MonoBehaviour
{

    public List<Button> btns;

    public InventorySO shopList;
    /// content位置
    public Transform itemRoot;

    // 目标ScrollView
    public ScrollRect ScrollTarget;

    public GameObject shopitem;

    public Text itemName;

    public Text itemDescribe;

    // 滚动步长,通过计算得到
    public float scrollStep = 0;

    public int index = 0;

    void Start()
    {
        shopList = Resources.Load<InventorySO>("UI/1001");
        CalculationScrollStep();
        StartCoroutine(InitItems());
        itemName.text = shopList.Items[index].Item.Name;
        itemDescribe.text = shopList.Items[index].Item.Description;
    }


    IEnumerator InitItems()
    {
        foreach (ItemStack o in shopList.Items)
        {
            if (o.Amount > 0)
            {
                GameObject go = Instantiate(o.Item.Prefab, itemRoot);
                ShopItem items = go.GetComponent<ShopItem>();
                Button btn = go.GetComponent<Button>();
                btns.Add(btn);
                items.SetShopItem(o.Item);

            }
        }
        yield return null;
    }

    public void MoveDown()
    {
        
        if (index < shopList.Items.Count - 1)
        {
            index++;
            this.ScrollTarget.verticalNormalizedPosition = (float)(1 - ((float)index * scrollStep));
            itemName.text = shopList.Items[index].Item.Name;
            itemDescribe.text = shopList.Items[index].Item.Description;
        }
        else
        {
            index = 0;
            this.ScrollTarget.verticalNormalizedPosition = (float)(1 - ((float)index * scrollStep));
            itemName.text = shopList.Items[index].Item.Name;
            itemDescribe.text = shopList.Items[index].Item.Description;
        }
    }
    public void MoveUp()
    {
        if (index > 0)
        {
            index--;
            this.ScrollTarget.verticalNormalizedPosition += (float)(1 - ((float)index * scrollStep));
            itemName.text = shopList.Items[index].Item.Name;
            itemDescribe.text = shopList.Items[index].Item.Description;
        }
    }

    /// <summary>
    /// 根据当前列表item的数量,计算每个item滚动需要的步长
    /// </summary>
    /// <returns></returns>
    public float CalculationScrollStep()
    {
        scrollStep = (float)1f / (shopList.Items.Count - 1);
        return scrollStep;
    }
}
