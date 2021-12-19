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

    private int battleRound; // 正在进行的波次，从1开始计数。
    private int roundMax; // 波次总数
    private bool roundClear; // 当前波次通过
    private List<GameObject> currentMonsters = new List<GameObject>();

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
        ChangeBlock(false);
        SetUpRound();
    }

    // Update is called once per frame
    void Update()
    {
        if (roundClear)
        {
            if (breathTime > 0)// 间隔计时
            {
                breathTime -= Time.deltaTime;
            }
            else
            {
                if (battleRound < roundMax)
                {
                    battleRound++;
                    GenerateMonsters(battleRound);
                }
                else
                {
                    accessible = true;
                    ChangeBlock(false);
                }
            }
        }
    }
    private void LateUpdate()
    {
        if (onBattle)
        {
            foreach (var mst in currentMonsters)
            {
                if (mst != null)
                {
                    break;                   
                }
                roundClear = true;
            }
        }
    }

    public void TriggerBattle()
    {
        if (!onBattle && !accessible)
        {
            Debug.Log("战斗开始！锁死大门！");
            ChangeBlock(true);
            onBattle = true;
            breathTime = BreathTime;
            battleRound = 1;
            GenerateMonsters(battleRound);  
        }
    }

    private void GenerateMonsters(int _round)
    {
        currentMonsters.Clear();
        foreach (var monster in monsterInfos)
        {
            if (_round.Equals(monster.round))
            {
                GameObject mst = Instantiate(monster.monsterObject);
                mst.transform.position = new Vector2(monster.PositionX, monster.PositionY);
                currentMonsters.Add(mst);
            }
        }
    }

    private void ChangeBlock(bool _option)
    {
        foreach (GameObject gate in gates)
        {
            gate.SetActive(_option);
        }
    }

    private void SetUpRound()
    {
        roundMax = 1;
        foreach (var item in monsterInfos)
        {
            if (item.round > roundMax)
            {
                roundMax = item.round;
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
