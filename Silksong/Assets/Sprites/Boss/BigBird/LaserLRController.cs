using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.Mathematics;
using UnityEngine;

public class LaserLRController : MonoBehaviour
{
    private Quaternion Player;
    public float SlerpSpeed = 5;

    private Vector3 Owner;

    private BoxCollider2D _collider;

    public bool Fire;

    
    //用来判断激光的偏转方向 根据 x 轴比大小 
    private Vector3 PlayerLoc;
    
    // Start is called before the first frame update
    void Start()
    {
       DoFire();
       _collider = GetComponent<BoxCollider2D>();
       transform.DORotate(new Vector3(-150,0,0) ,3);
    }

    // Update is called once per frame
    void Update()
    {
        if (Fire)
        {
         
        }
    }

    public void DoFire() //执行激光转动逻辑
    {
        transform.DORotate(new Vector3(-150,0,0) ,3);
        PlayerLoc = GameObject.FindGameObjectWithTag("Player").transform.position;

        var boss = GameObject.FindGameObjectWithTag("Boss").transform.position;
        if (boss.x < PlayerLoc.x)
        {
            transform.DORotate(new Vector3(-150,0,0) ,2);
        }

        if (boss.x < PlayerLoc.x)
        {
            transform.DORotate(new Vector3(150, 0, 0), 2);
        }
    }
    
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
           //TODO::玩家伤害检测
        }
    }
}
