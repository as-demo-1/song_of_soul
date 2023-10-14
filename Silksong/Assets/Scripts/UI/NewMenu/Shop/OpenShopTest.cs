using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using Opsive.UltimateInventorySystem.Exchange;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenShopTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
	}

	private void OnTriggerStay2D(Collider2D collision)
	{
		if (Input.GetKeyDown(KeyCode.E))
		{
			UIManager.Instance.Show<UIShop>();
		}
	}
	/*protected override void enterEvent()
	{
		//tip.SetActive(true);
		Debug.Log(" ’ºØÀÈ∆¨");
		if (Input.GetKeyDown(KeyCode.P))
		{
			UIManager.Instance.Show<UIShop>();
		}
		
		//Debug.Log(GameManager.Instance.currencyOwner.CurrencyAmount.GetAmountOf(currency));
	}*/
}
