using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TengWanController : MonoBehaviour
{
    private BoxCollider2D boxcollider;
    // Start is called before the first frame update
    void Start()
    {
        boxcollider = GetComponent<BoxCollider2D>();
    }

    private void OnCollisionEnter2D(Collision2D other) //这里是临时用的,等待歌声API的接入
    {
        if(other.gameObject.tag == "Player")
        StartCoroutine(DestoryCollider());
    }

    IEnumerator DestoryCollider()
    {
        yield return new WaitForSeconds(1f);
        Debug.Log("碰撞体销毁!");
        boxcollider.isTrigger = true;
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
