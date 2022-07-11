using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
///花上浮到地面后 停止且取消伤害
/// </summary>作者：青瓜
public class BurstFlowerCollider : Collider2DBase
{
    public GameObject damager;
    public float speed;
    public Vector3 destination;
    public Vector3 homeposition;
    private float distance;
    protected override void enterEvent()//碰到ground layer
    {
        damager.SetActive(false);
        //Debug.Log("该下去了");
        //GetComponent<Rigidbody2D>().transform.position = Vector2.MoveTowards(gameObject.transform.localPosition, homeposition, speed);
    }

    public void Burst()//开始掉落
    {
        damager.SetActive(true);
        distance = Vector2.Distance(gameObject.transform.position, destination);
        //GetComponent<Rigidbody2D>().gravityScale = speed;
        transform.localPosition = Vector2.MoveTowards(gameObject.transform.localPosition, destination, speed);
        //transform.localPosition = Vector2.MoveTowards(gameObject.transform.localPosition, homeposition, speed);

    }
    public void Back()//开始掉落
    {
        damager.SetActive(false);
        distance = Vector2.Distance(gameObject.transform.position, destination);
        //GetComponent<Rigidbody2D>().gravityScale = speed;
        //transform.localPosition = Vector2.MoveTowards(gameObject.transform.localPosition, destination, speed);
        transform.localPosition = Vector2.MoveTowards(gameObject.transform.localPosition, homeposition, speed);

    }
}
