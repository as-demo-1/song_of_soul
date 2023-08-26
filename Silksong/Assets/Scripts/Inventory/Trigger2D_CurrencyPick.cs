

using System;
using Opsive.UltimateInventorySystem.Core;
using Opsive.UltimateInventorySystem.Core.InventoryCollections;
using Opsive.UltimateInventorySystem.Exchange;
using Opsive.UltimateInventorySystem.Interactions;
using UnityEngine;

/// <summary>
/// 用于拾取货币的触发区
/// 主角触碰直接拾取无需按键
/// </summary>
public class Trigger2D_CurrencyPick: Trigger2DBase
{
    public Currency currency;
    public int count;

    private void Start()
    {
        
    }

    protected override void enterEvent()
    {
        //tip.SetActive(true);
        Debug.Log("收集碎片");
        GameManager.Instance.currencyOwner.AddCurrency(currency, count);
        //Debug.Log(GameManager.Instance.currencyOwner.CurrencyAmount.GetAmountOf(currency));
        Destroy(gameObject);
    }
}