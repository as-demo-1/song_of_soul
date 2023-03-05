using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum bossBattleStage
{
    Prepare,
    StageOne,
    StageTwo,
    StageThree,
}

public class WhaleBossManager : MonoSingleton<WhaleBossManager>
{
    public bossBattleStage stage=bossBattleStage.Prepare;


    public Transform roomLeftDown;
    public Transform roomRightUp;

    public Vector2 roomLeftDownPoint
    { get { return roomLeftDown.position; } }
    public Vector2 roomRightUpPoint
    { get { return roomRightUp.position; } }

    private float width;
    private float height;
    public float whaleOutCameraDistanceX;
    public float whaleOutCameraDistanceY;

    public GameObject platform;
    public GameObject cloud;
    private float createHigher;
    public float AvgplatformsCreateCd;
    public float cloudRate;
    public float AvgPlatformSpeed;
    private float moveTotalTime;
    private int AvgPlatformNumberPerCreate;
    private float PlafromHorizonDistance=6f;
    public float PlafromHorizonDistanceBear;

    public GameObject FirstPlatform;
    public GameObject FirstPlatform2;
    public GameObject FirstCloud;
    public Transform PlatformParent;

    public float borderSkillCd;
    public float normalSkillCd;
    [HideInInspector]
    public float borderSkillCdTimer;
    [HideInInspector]
    public float normalSkillCdTimer;

    public float rightBorderX;
    public float leftBorderX;

    public BattleAgent battleAgent;
    private HpDamable whaleHp;

    public GameObject ice;
    public GameObject iceParent;
    public float iceRainTotalTime;
    public float iceRainWaveTime;
    public int iceNumberPerWave;
    public float iceSpeed;

    void Start()
    {
        width = roomRightUpPoint.x - roomLeftDownPoint.x;
        height = roomRightUpPoint.y - roomLeftDownPoint.y;

        moveTotalTime = (height) / AvgPlatformSpeed;
        moveTotalTime /= 0.75f;

        AvgPlatformNumberPerCreate = (int)(width /PlafromHorizonDistance);

        // print(AvgPlatformNumberPerCreate);

        PlayerAnimatorParamsMapping.SetControl(false);

        createHigher = 1;
        whaleHp = battleAgent.GetComponent<HpDamable>();

    }

    // Update is called once per frame
    void Update()
    {
        if(stage==bossBattleStage.Prepare)
        {
            CameraShakeManager.Instance.cameraShake(1f);
        }
        else
        {
            if (normalSkillCdTimer > 0)
            {
                normalSkillCdTimer -= Time.deltaTime;
            }
            if (borderSkillCdTimer > 0)
            {
                borderSkillCdTimer -= Time.deltaTime;
            }
        }

    }

    public void resetNormalSkillCdTimer()
    {
        normalSkillCdTimer = normalSkillCd;
    }

    public void resetVomitSkillCdTimer()
    {
        borderSkillCdTimer = borderSkillCd;
    }

    public void BattleStart()
    {
        stage = bossBattleStage.StageOne;
        PlayerAnimatorParamsMapping.SetControl(true);
        StartCoroutine(moveingPlatforms());
        FirstCloud.GetComponent<Rigidbody2D>().velocity = new Vector2(0, -AvgPlatformSpeed);
        FirstPlatform.GetComponent<Rigidbody2D>().velocity = new Vector2(0, -AvgPlatformSpeed);
        FirstPlatform2.GetComponent<Rigidbody2D>().velocity = new Vector2(0, -AvgPlatformSpeed);

        resetNormalSkillCdTimer();
        resetVomitSkillCdTimer();
    }

    private IEnumerator moveingPlatforms()
    {
        while(true)
        {
            createPlatformsAndClouds(AvgPlatformNumberPerCreate);
            yield return new WaitForSeconds(Random.Range(0.8f,1.2f)*AvgplatformsCreateCd);
        }
    }

    private void createPlatformsAndClouds(int num)
    {
        /*
        bool havePlatform = false;
        bool haveCloud = false;*/
        float horizonBase = roomLeftDownPoint.x;
        float preX=-200;
        for(int i=0;i<num;i++)
        {
            float x;
            x = Random.Range(0f, 2f) + horizonBase + i * PlafromHorizonDistance;
            if (preX!=-200)
            {
                while(x-preX<=PlafromHorizonDistance-PlafromHorizonDistanceBear)
                {
                    x += 0.2f;
                }
            }
            preX = x;
            Vector2 pos = new Vector2(x, createHigher+roomRightUpPoint.y+ Random.Range(-0.5f, 0.5f));
            float a = Random.Range(0f, 1f);
            if (a > cloudRate )//||(!havePlatform && i==num-1))
            {
             
               // havePlatform = true;
                createPlatform(platform,pos);
            }
            else 
            {
               // haveCloud = true;
                createPlatform(cloud,pos);
            }
        }

       /* if (!haveCloud) createPlatform(cloud);
        if (!havePlatform) createPlatform(platform);*/
    }

    private void createPlatform(GameObject prefab,Vector2 pos)
    {
        GameObject target ;
        target = Instantiate(prefab);
        target.transform.position = pos;
        target.GetComponent<Rigidbody2D>().velocity = new Vector2(0, -AvgPlatformSpeed);
        Destroy(target, moveTotalTime);
        target.transform.SetParent(PlatformParent);
        if(prefab==platform)
        {
            GameObjectTeleporter.Instance.playerRebornPoint = target.transform.position + new Vector3(0, 0.5f, 0);
        }
    }

    public bool goingOutInBorder()
    {
        float left = WhaleBossManager.Instance.leftBorderX;
        float right = WhaleBossManager.Instance.rightBorderX;
        float currentX = battleAgent.transform.position.x;
     ///   print("border");
        if ((battleAgent.currentFacingLeft() && approEqual(right, currentX)) ||
             (!battleAgent.currentFacingLeft() && approEqual(left, currentX)))
        {
           // Debug.Log("appro");
            return true;
        }
        else return false;
    }

    private bool approEqual(float a,float b)
    {
        float d = Mathf.Abs(a - b);
      //  print(d);
        return d < 0.5f;
    }

    public void iceRainStart()
    {
        StartCoroutine(iceRain());
    }

    private IEnumerator iceRain()
    {
        float timer = 0;
        while(timer<iceRainTotalTime)
        {
            createIceRainWave(iceNumberPerWave);
            timer += iceRainWaveTime;
            yield return new WaitForSeconds(iceRainWaveTime);
        }
    }

    private void createIceRainWave(int num)
    {
   
        float horizonBase = roomLeftDownPoint.x;
        float x = width / num;
        for (int i = 0; i < num; i++)
        {
            Vector2 pos = new Vector2(Random.Range(0f, 4f) + horizonBase + i * x, createHigher + roomRightUpPoint.y + Random.Range(-1f, 1f));
            createIce(pos);
        }
    }

    private void createIce(Vector2 pos)
    {
        GameObject target;
        target = Instantiate(ice);
        target.transform.position = pos;
        target.GetComponent<Rigidbody2D>().velocity = new Vector2(0, -iceSpeed);
        Destroy(target, moveTotalTime);///////
        target.transform.SetParent(iceParent.transform);
    }
}
