using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinGenerator:MonoSingleton<CoinGenerator>
{
    //金币预制的路径
    private readonly static string coinPrefabPath = "coin";

    private List<GameObject> coinsPool = null;

    private int largeCoinMoneyNum = 5;
    private int middleCoinMoneyNum = 2;
    private int smallCoinMoneyNum = 1;

    public override void Init()
    {
        coinsPool = new List<GameObject>();
    }

    /// <summary>
    /// 尝试从缓存池里拿金币的gameobject,缓存里没有就生成一个
    /// </summary>
    /// <returns></returns>
    private GameObject TryGetCoinPrefabsFormPool() 
    {
        GameObject coin = null;
        if (coinsPool.Count > 0)
        {
            coin = coinsPool[0];
            coinsPool.RemoveAt(0);
            coin.transform.localScale = Vector3.one;
        }
        else 
        {
            coin = (GameObject)Instantiate(Resources.Load(coinPrefabPath));        
        }
        return coin;
    }

    /// <summary>
    /// 生成指定数目的金币
    /// </summary>
    /// <param name="obj">生成的位置</param>
    /// <param name="num">数量</param>
    public void GenerateCoinsByNum(GameObject obj,int num) 
    {
        int maxLargeCoin = num / largeCoinMoneyNum;
        int largeCoin = Random.Range(1,maxLargeCoin);

        num -= largeCoin * largeCoinMoneyNum;

        int maxMiddleCoin = num / middleCoinMoneyNum;
        int middleCoin = Random.Range(1,maxMiddleCoin);

        num -= middleCoin * middleCoinMoneyNum;

        int smallCoin = num;

        GenerateCoins(obj,largeCoin,middleCoin,smallCoin);
    }

    /// <summary>
    /// 生成金币
    /// </summary>
    /// <param name="obj">从这个gameobject的位置生成</param>
    /// <param name="largeCoins">大型金币的数量</param>
    /// <param name="middleCoins">中型金币的数量</param>
    /// <param name="smallCoins">小型金币的数量</param>
    public void GenerateCoins(GameObject obj, int largeCoins, int middleCoins, int smallCoins)
    {
        for (int i = 0; i < largeCoins; ++i) 
        {
            GameObject coin = TryGetCoinPrefabsFormPool();
            coin.transform.localScale = new Vector3(1.2f,1.2f,1.2f);
            coin.transform.position = obj.transform.position;
            coin.SetActive(true);
            coin.GetComponent<Coin>().SetCoinMoneyNum(largeCoinMoneyNum);
        }
        for (int i = 0; i < middleCoins; ++i)
        {
            GameObject coin = TryGetCoinPrefabsFormPool();
            coin.transform.localScale = Vector3.one;
            coin.transform.position = obj.transform.position;
            coin.SetActive(true);
            coin.GetComponent<Coin>().SetCoinMoneyNum(middleCoinMoneyNum);
        }
        for (int i = 0; i < smallCoins; ++i)
        {
            GameObject coin = TryGetCoinPrefabsFormPool();
            coin.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
            coin.transform.position = obj.transform.position;
            coin.SetActive(true);
            coin.GetComponent<Coin>().SetCoinMoneyNum(smallCoinMoneyNum);
        }
    }


    public void RecycleCoinsPrefabs(GameObject coin)
    {
        coinsPool.Add(coin);
        coin.SetActive(false);
        coin.transform.parent = this.transform;
        coin.transform.localScale = Vector3.zero;
    }

}
