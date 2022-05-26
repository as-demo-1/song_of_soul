using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackingBullet : BulletCollision
{
    private float angle;
    private PlayerCharacter player;
    [SerializeField]
    private float Speed = 1;
    [SerializeField]
    float minBall = 25f;
    [SerializeField]
    float maxBall = 50f;

    float BallisticAngle;

    // Update is called once per frame
    void Update()
    {

        if (player != null)
        {
            StartCoroutine(TrackingPlayer(player));

        }
        else
        {
            print("没有找到玩家");
            player = FindObjectOfType<PlayerCharacter>();
        }

    }


    IEnumerator TrackingPlayer(PlayerCharacter player)
    {
        BallisticAngle = Random.Range(minBall, maxBall);
        Vector3 targetDirecton = player.transform.position - this.transform.position;//获取玩家位置
        angle = Mathf.Atan2(targetDirecton.y, targetDirecton.x) * Mathf.Rad2Deg;//装换为旋转角给光弹
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        transform.Translate(Vector3.right * Speed * 2 * Time.deltaTime);
        transform.rotation *= Quaternion.Euler(0f, 0f, BallisticAngle);//将随机角度给光弹 使光弹运行轨迹有弧度
        transform.Translate(Vector3.right * Speed * 1.1f * Time.deltaTime);
        yield return null;
    }

}
