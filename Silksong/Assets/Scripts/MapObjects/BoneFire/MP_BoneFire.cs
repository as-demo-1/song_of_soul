using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 篝火，碰撞判断是否停留，并且在停留下恢复寒冷值，在退出后结束回复寒冷值
/// </summary>
public class MP_BoneFire : MonoBehaviour
{

    private Collider2D _collider2D;

    private bool IsCoroutineStart = false;
    // Start is called before the first frame update
    void Start()
    {
        _collider2D = GetComponent<Collider2D>();
    }
    private void OnTriggerStay2D(Collider2D boxcollider)
    {
        if (boxcollider.gameObject.tag == "Player")
        {
            if (!IsCoroutineStart)
            {
                StartCoroutine(RecoverColdValue());
                IsCoroutineStart = true;
            }
            
        }
    }
    private void OnTriggerExit(Collider other)
    {
        StopCoroutine(RecoverColdValue());
    }

    IEnumerator RecoverColdValue()
    {
        yield return new WaitForSeconds(2f);
        IsCoroutineStart = false;
        Debug.Log("!");
    }
    
}
