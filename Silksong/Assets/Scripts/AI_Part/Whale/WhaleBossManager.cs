using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WhaleBossManager : MonoBehaviour
{
    public Vector2 roomLeftDownPoint;
    public Vector2 roomRightUpPoint;
    private float width;
    private float height;

    public GameObject platform;
    public GameObject cloud;
    private float createHeight;
    public float AvgplatformsCreateCd;
    public float cloudRate;
    public float AvgPlatformSpeed;
    private float moveTotalTime;
    private int AvgPlatformNumberPerCreate;

    public GameObject FirstPlatform;
    public GameObject FirstCloud;
    public Transform PlatformParent;

    void Start()
    {
        width = roomRightUpPoint.x - roomLeftDownPoint.x;
        height = roomRightUpPoint.y - roomLeftDownPoint.y;

        moveTotalTime = (height) / AvgPlatformSpeed;
        moveTotalTime /= 0.75f;

        createHeight = roomRightUpPoint.y + 1f;

        AvgPlatformNumberPerCreate = (int)(width / 5f);

       // print(AvgPlatformNumberPerCreate);

        BattleStart();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void BattleStart()
    {
        StartCoroutine(moveingPlatforms());
        FirstCloud.GetComponent<Rigidbody2D>().velocity = new Vector2(0, -AvgPlatformSpeed);
        FirstPlatform.GetComponent<Rigidbody2D>().velocity = new Vector2(0, -AvgPlatformSpeed);
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
        float horizonBase = roomLeftDownPoint.x + 2;
        for(int i=0;i<num;i++)
        {
            Vector2 pos = new Vector2(Random.Range(0f,2f)+horizonBase+i*5, createHeight+ Random.Range(-1f, 1f));
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

}
