using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M)) 
        {
			CoinManager.Instance.GenerateCoins(this.gameObject,0,1,0);
        }
    }
	public void TestCoin()
	{
		CoinManager.Instance.GenerateCoins(this.gameObject, 0, 1, 0);
	}
}
