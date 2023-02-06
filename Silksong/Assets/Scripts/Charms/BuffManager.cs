using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffManager : MonoBehaviour
{
    [SerializeField]
    private CharmListSO charmListSO = default;

    public PlayerCharacter playerCharacter;
    public PlayerController playerController;

    public Dictionary<string, CharmBuff> charmBuffDict = new Dictionary<string, CharmBuff>();
    public List<CharmBuff> activeBuffList = new List<CharmBuff>();

    public GameObject coinCollecterPrefab;

    // Start is called before the first frame update
    void Awake()
    {
        playerController = PlayerController.Instance;
        playerCharacter = PlayerController.Instance.GetComponent<PlayerCharacter>();

        //charmListSO.InitRef();

        // 实例化所有Buff，此时均为未激活状态
        charmBuffDict.Add("50000001", new AttackBuff(this));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// 添加一个buff效果
    /// </summary>
    /// <param name="buffId">效果编号</param>
    /// <param name="buffVal">效果参数</param>
    public void AddBuff(string buffId, string buffVal)
    {
        if (!charmBuffDict[buffId].IsActive)
        {
            charmBuffDict[buffId].InitBuff(buffVal);
            activeBuffList.Add(charmBuffDict[buffId]);
        }
        else
        {
            charmBuffDict[buffId].AddEffect(buffVal);
        }  
    }

    /// <summary>
    /// 关闭一个buff效果
    /// </summary>
    /// <param name="buffId">效果编号</param>
    /// <param name="buffVal">效果参数</param>
    public void DisableBuff(string buffId, string buffVal)
    {
        if (charmBuffDict[buffId].IsActive)
        {
            charmBuffDict[buffId].DisEffect(buffVal);
        }
        else
        {
            Debug.Log("Error, this buff is not active");
        }
        
    }
}
