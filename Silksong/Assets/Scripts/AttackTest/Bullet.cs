using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 作者：青瓜
/// 暂时用来测试波攻击
/// </summary>
public class Bullet : MonoBehaviour
{
    // Start is called before the first frame update
    public int facingRight;//1 or -1
    public float speed;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(transform.right*Time.deltaTime*facingRight*speed, Space.World);
        if(transform.position.x<-20 || transform.position.x>20)//需要修改
        {
            Destroy(gameObject);
        }
    }
}
