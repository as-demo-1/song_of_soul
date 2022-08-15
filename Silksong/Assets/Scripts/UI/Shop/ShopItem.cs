using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopItem : MonoBehaviour
{
    public Text Name;
    public Image Icon;
    public Button button;

    public void SetShopItem(ItemSO item)
    {
        Name.text = item.Name;
        Icon.sprite = item.PreviewImage;

    }
}
