using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrokePlatformByBump : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(WhaleBossManager.Instance.stage==EBossBattleStage.StageTwo && collision.tag=="Platform")
        {
            GameObject brokenPlatform =Instantiate ( WhaleBossManager.Instance.brokenPlatform);
            brokenPlatform.transform.position = collision.transform.position;

            brokenPlatform.AddComponent<Rigidbody2D>().isKinematic = true;
            brokenPlatform.GetComponent<Rigidbody2D>().velocity = collision.GetComponent<Rigidbody2D>().velocity;
            brokenPlatform.GetComponent<BrokenPlatform>().notRecreat = true;
            brokenPlatform.transform.SetParent(collision.transform.parent);
            Destroy(brokenPlatform, 10f);
            Destroy(collision.gameObject);

            //print(123);
        }
    }
}
