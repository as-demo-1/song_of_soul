using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 作者：青瓜
/// 拥有生命值及相关方法的damable 
/// </summary>
public class HpDamable :Damable
{
    public int maxHp = 5;//最大生命值

    public int currentHp;//当前hp

    public bool resetHealthOnSceneReload;

    protected Vector2 damageDirection;//伤害来源的方向

    public bool canHitBack;//能否被击退 目前击退机制暂时测试用 
    public float hitBackDistance;


    public override void takeDamage(DamagerBase damager)
    {
        if ( currentHp <= 0)
        {
            return;
        }

        base.takeDamage(damager);

        addHp(-damager.getDamage(this));
        damageDirection = damager.transform.position - transform.position;

        if(canHitBack)
        hitBack();
    }

    protected void hitBack()
    {

        if(damageDirection.x>0)
        {
            transform.Translate(Vector2.left *hitBackDistance,Space.World);
        }
        else
        {
            transform.Translate(Vector2.right * hitBackDistance, Space.World);
        }
    }
    public void setHp(int hp)
    {
        currentHp = hp;
        checkHp();
    }

    protected virtual void checkHp()
    {
        if (currentHp > maxHp)
        {
            currentHp = maxHp;
        }
        if (currentHp <= 0)
        {
            die();
        }
    }
    protected void addHp(int number)//如受到伤害 number<0
    {
        currentHp += number;
        checkHp();

    }

    protected virtual void die()
    {
        Destroy(gameObject);//未完善
        Debug.Log(gameObject.name+" die");
    }



}
