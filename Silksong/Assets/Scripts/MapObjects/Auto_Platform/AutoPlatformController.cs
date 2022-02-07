using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoPlatformController : MonoBehaviour
{
    private bool IsFirstEnter = true; //是否第一次发生碰撞
    private bool IsMoving = false;//检测平台是否移动
    private bool IsTriggered = false;//是否激发开关
    public List<Transform> waypoints;//平台可移动位点List组
    public float moveSpeed;
    public int target;//当前的目标位置

    private void OnTriggerEnter2D(Collider2D other)
    {
        ChangeState(0);
    }

    public void Pub_Trigger(bool GoToOrigin)//暴露给检测盒更改平台位置
    {
        if (GoToOrigin)
            ChangeState(1);
        else
        ChangeState(0);
    }

    private void Update()
    {
        if (IsTriggered)
            transform.position = Vector3.MoveTowards(transform.position, waypoints[target].position,
                moveSpeed * Time.deltaTime);
    }

    private void FixedUpdate()
    {
        // if (transform.position == waypoints[target].position)
        // {
        //     //if (target == waypoints.Count)
        //        // target = 0;
        //     // else
        //     // {
        //     //     target += 1;
        //     // }
        // }
    }

    private void ChangeState(int target_temp)
    {
        if(IsFirstEnter && ! IsMoving)
        {
            IsFirstEnter = false;
            target = target_temp;
            StartCoroutine(MovePlatform());
        }
    }


    IEnumerator MovePlatform()
    {
        yield return new WaitForSeconds(.5f);
        IsTriggered = true;
        IsFirstEnter = true;
        //Debug.Log("Move Platform!");
    }
}
