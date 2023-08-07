

using System;
using Opsive.UltimateInventorySystem.Core;
using Opsive.UltimateInventorySystem.Core.InventoryCollections;
using Opsive.UltimateInventorySystem.Exchange;
using Opsive.UltimateInventorySystem.Interactions;
using UnityEngine;

public class Trigger2D_CurrencyPick: Trigger2DBase
{
    private CurrencyOwner currencyOwner;
    public Currency currency;
    public int count;

    private void Start()
    {
        currencyOwner = InventorySystemManager.GetInventoryIdentifier(GameManager.Instance.saveSystem.SaveData.inventoryIndex).CurrencyOwner;
    }

    protected override void enterEvent()
    {
        //tip.SetActive(true);
        Debug.Log("收集生命碎片");

        currencyOwner.AddCurrency(currency, count);
        Debug.Log(currencyOwner.CurrencyAmount.GetAmountOf(currency));
       
    }
}