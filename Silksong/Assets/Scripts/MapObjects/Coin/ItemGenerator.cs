using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemGenerator : MonoSingleton<ItemGenerator>
{
    //���Ԥ�Ƶ�·��
    private readonly static string coinPrefabPath = "item";

    private List<GameObject> coinsPool = null;

    public override void Init()
    {
        coinsPool = new List<GameObject>();
    }

    /// <summary>
    /// ���Դӻ�������ý�ҵ�gameobject,������û�о�����һ��
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
    /// ���ɽ��
    /// </summary>
    /// <param name="obj">�����gameobject��λ������</param>
    /// <param name="item">���ͽ�ҵ�����</param>
    /// <param name="dropItemNums">���ͽ�ҵ�����</param>
    public void GenerateItems(GameObject obj, DropInfo drop)
    {
        GameObject coin = TryGetCoinPrefabsFormPool();
        coin.GetComponent<SpriteRenderer>().sprite = drop.info.Icon;
        coin.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
        coin.transform.position = obj.transform.position;
        coin.SetActive(true);
        coin.GetComponent<DropItem>().SetDropInfo(drop);
    }

    public void RecycleCoinsPrefabs(GameObject coin)
    {
        coinsPool.Add(coin);
        coin.SetActive(false);
        coin.transform.parent = this.transform;
        coin.transform.localScale = Vector3.zero;
    }

}
