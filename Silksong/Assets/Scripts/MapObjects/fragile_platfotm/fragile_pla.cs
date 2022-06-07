using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class fragile_pla : MonoBehaviour
{
    // Start is called before the first frame update

    private bool IsFirst = true;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    private void OnTriggerEnter2D(Collider2D col)
    {
        Debug.Log("Begin Destory!");
        if (IsFirst)
        {
            IsFirst = false;
            StartCoroutine(Chaos());
        }
    }

    IEnumerator Chaos()
    {
        //TODO::播放动画
        yield return new WaitForSeconds(5);
        gameObject.SetActive(false);
    }
}
