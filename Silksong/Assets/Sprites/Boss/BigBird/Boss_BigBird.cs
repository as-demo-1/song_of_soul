using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Serialization;

public class Boss_BigBird : MonoBehaviour
{
    public GameObject PlayerRef;
    [Header("MapObjectsInfo")]
    public List<GameObject> StoneMonsterLoc;
    public List<GameObject> StoneMonsterPool;
    public List<GameObject> ThickLaserPool;
    public List<GameObject> ThickLaserLoc;
    public GameObject playerleftarea;
    public float SmoothingTime;
    public GameObject TraceStart;

    [Header("MonsterStateInfo")]
    private  Vector2  PlayerTransform;
    private Vector2 BossTraceStart;
    private int State;
    private bool isFiring = false;
    private bool IsProcessFinfish = false;
    private Vector3 t_PlayerLoc;
    private Vector3 t_playerleftarea;
    private Vector2 curvelocity;

    [FormerlySerializedAs("test")] public GameObject LaserLeftStartArea;
    private Vector2 tempp;
    private Vector2 temploc;
    private Vector2 lsa;
    private int MonsterSpawnLocIndex = 0;
    void Start()
    {
        lsa = LaserLeftStartArea.transform.position;
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            SpawnStoneMonster();
        }
        t_playerleftarea = playerleftarea.transform.position;
        PlayerTransform = PlayerRef.transform.position;
        tempp = LaserLeftStartArea.transform.position;
        BossTraceStart = TraceStart.transform.position;
        if (!isFiring)
        {
            StartCoroutine(FireTrace());
        }
    }

    IEnumerator FireTrace()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);
            if(temploc != PlayerTransform) 
                temploc = Vector2.SmoothDamp(tempp, PlayerTransform, ref curvelocity,
                SmoothingTime);
            //RaycastHit2D hit = Physics2D.Raycast(transform.position, PlayerTransform - BossTraceStart,99999);
            Debug.DrawLine(BossTraceStart, temploc, Color.cyan);
            LaserLeftStartArea.transform.position= temploc;
            
        }
    }

    public void SpawnStoneMonster()  //利用简化的对象池，将怪物的位置移动到指定位置，下面的粗激光同理
    {
        if (StoneMonsterPool.Count != 0)
        {
            foreach (var Monster in StoneMonsterPool)
            {
                Monster.transform.position = StoneMonsterLoc[MonsterSpawnLocIndex].transform.position;
                MonsterSpawnLocIndex++;
            }
        }

        MonsterSpawnLocIndex = 0;
    }

    public void StartThickLaser()
    {
        if (ThickLaserPool.Count != 0)
        {
            foreach (var Laser in ThickLaserPool)
            {
                Laser.transform.position = ThickLaserLoc[MonsterSpawnLocIndex].transform.position;
                MonsterSpawnLocIndex++;
            }
        }

        MonsterSpawnLocIndex = 0;
    }


}
