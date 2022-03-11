using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProtalTransition : SceneTransitionPoint
{
    private bool canTrans;
    public GameObject TransitionPoint;
    private GameObject player;

    void Update()
    {
        if (canTrans)
        {
            enterEvent();       //切换到指定场景
            
            //player移动到指定位置
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
        {
            canTrans = true;
            player = other.gameObject;
        }
    }
    void OnTriggerExit2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
        {
            canTrans = false;
        }
    }
}
