using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using Opsive.UltimateInventorySystem.Exchange;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenShopTest : Trigger2DBase
{
    // Start is called before the first frame update
    void Start()
    {
	}
	protected override void enterEvent()
	{
		//tip.SetActive(true);
		Debug.Log(" ’ºØÀÈ∆¨");
		UIManager.Instance.Show<UIShop>();
		//Debug.Log(GameManager.Instance.currencyOwner.CurrencyAmount.GetAmountOf(currency));
	}
}
