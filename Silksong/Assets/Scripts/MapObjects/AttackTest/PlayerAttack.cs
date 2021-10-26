using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 
/// 临时用作玩家的普通攻击及波攻击 仅做测试用
/// </summary>作者：青瓜
public class PlayerAttack : MonoBehaviour
{
    public GameObject bullet;
    public float atkCostTime;//单次攻击花费时间
    public bool isAttacking;

    public GameObject atkTrigger;

    private float atkCostTimeCounter;
   // private PlayerController PlayerController;
    void Start()
    {
        //PlayerController = GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
       /* if(Input.GetKeyDown(KeyCode.C))//波攻击
        {
            GameObject blt = Instantiate(bullet);
            Vector3 bltPostion = transform.position;
            if (transform.localScale.x==1)//人物面向左
            {
                bltPostion.x-= 0.5f;
                blt.GetComponent<Bullet>().facingRight = -1;
                blt.GetComponent<SpriteRenderer>().flipX = true;
            }
            else
            {
                bltPostion.x += 0.5f;
                blt.GetComponent<Bullet>().facingRight = 1;
            }
            blt.transform.position = bltPostion;
        }*/


        if (Input.GetKeyDown(KeyCode.X) && isAttacking==false)//普通攻击 
        {
            isAttacking = true;
            atkCostTimeCounter = atkCostTime;
            atkTrigger.SetActive(true);
        }

        if(isAttacking)
        {
            atkCostTimeCounter -= Time.deltaTime;
            if(atkCostTimeCounter<=0)
            {
                atkTrigger.SetActive(false);
                isAttacking = false;
            }
        }

    }
}
