using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 
/// 拥有生命值及相关方法的damable 
/// </summary>作者：青瓜
public class HpDamable :Damable
{
    public int maxHp = 5;//最大生命值

    public int currentHp;//当前hp

    public bool resetHealthOnSceneReload;


    public override void takeDamage(DamagerBase damager)
    {
        if ( currentHp <= 0)
        {
            return;
        }

        base.takeDamage(damager);
        addHp(-damager.getDamage(this));

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
