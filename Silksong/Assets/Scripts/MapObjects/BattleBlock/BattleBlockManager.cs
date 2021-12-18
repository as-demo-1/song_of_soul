using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
struct MonsterInfo
{
    public GameObject monsterObject;
    public int round;
    public float PositionX;
    public float PositionY;
}

public class BattleBlockManager : MonoBehaviour
{
    private bool accessible;// 是否可以通过
    private bool onBattle;// 正在战斗中

    private int battleRound; // 正在进行的波次

    private float breathTime;
    public float BreathTime;// 每波中间的间隔时间

    [Header("战斗时需要封锁的门")] 
    public List<GameObject> gates = new List<GameObject>();

    private Dictionary<string, GameObject> monsterDic = new Dictionary<string, GameObject>();

    //怪物配置方案1：读取表格（减少程序工作，需要搭建人员创建表格）

    //怪物配置方案2：Gizmos标点（有利于关卡设计调试）
    [SerializeField]
    private List<MonsterInfo> monsterInfos = new List<MonsterInfo>();

    

    // Start is called before the first frame update
    void Start()
    {
        foreach (GameObject gate in gates)
        {
            gate.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TriggerBattle()
    {
        if (!onBattle && !accessible)
        {
            Debug.Log("战斗开始！锁死大门！");
            foreach (GameObject gate in gates)
            {
                gate.SetActive(true);
            }
            onBattle = true;
            battleRound = 1;
            GenerateMonsters(battleRound);  
        }
        
    }

    private void GenerateMonsters(int _round)
    {
        foreach (var monster in monsterInfos)
        {
            if (_round.Equals(monster.round))
            {
                GameObject mst = Instantiate(monster.monsterObject);
                mst.transform.position = new Vector2(monster.PositionX, monster.PositionY);
            }
        }
    }

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        foreach (var monster in monsterInfos)
        {
            Gizmos.DrawWireCube(new Vector3(monster.PositionX, monster.PositionY, 0.0f), Vector3.one);
        }
    }
}
