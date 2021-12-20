using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinPile : HpDamable
{
    private int smallAmount = 1;
    private int medmediumAmount = 2;
    private int largeAmount = 5;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void CreateCoins()
    {

    }
    protected override void die()
    {
        base.die();
        CreateCoins();
    }
}
