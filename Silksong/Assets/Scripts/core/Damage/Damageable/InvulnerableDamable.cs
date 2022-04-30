using System.Collections;
using System.Collections.Generic;
using UnityEngine;
<<<<<<< HEAD
using UnityEngine.Events;
/// 
/// 在hpdamable的基础上 增加受击后无敌机制
/// </summary>作者：青瓜
=======
/// <summary>
/// 作者：青瓜
/// 在hpdamable的基础上 增加受击后无敌机制
/// </summary>
>>>>>>> 30f6fd9d (damage test)
public class InvulnerableDamable : HpDamable
{
    public bool invulnerableAfterDamage = true;//受伤后无敌
    public float invulnerabilityDuration = 3f;//无敌时间
    protected float inulnerabilityTimer;
<<<<<<< HEAD

=======
>>>>>>> 30f6fd9d (damage test)
    public override void takeDamage(DamagerBase damager)
    {
        base.takeDamage(damager);
        if(invulnerableAfterDamage)
        {
            enableInvulnerability();
        }
    }



    public void enableInvulnerability(bool ignoreTimer = false)//开启无敌
    {
        invulnerable = true;
        //technically don't ignore timer, just set it to an insanly big number. Allow to avoid to add more test & special case.
        inulnerabilityTimer = ignoreTimer ? float.MaxValue : invulnerabilityDuration;
    }

    void Update()
    {
<<<<<<< HEAD
        if (invulnerable&&invulnerableAfterDamage)
=======
        if (invulnerable)
>>>>>>> 30f6fd9d (damage test)
        {
            inulnerabilityTimer -= Time.deltaTime;

            if (inulnerabilityTimer <= 0f)
            {
                invulnerable = false;
<<<<<<< HEAD
                GetComponent<Collider2D>().enabled = false;//重新激活一次collider 避免在无敌时进入trigger内 导致无敌过后ontriggerEnter不触发
                GetComponent<Collider2D>().enabled = true;
=======
                GetComponent<BoxCollider2D>().enabled = false;//重新激活一次collider 避免在无敌时进入trigger内 导致无敌过后ontriggerEnter不触发
                GetComponent<BoxCollider2D>().enabled = true;
>>>>>>> 30f6fd9d (damage test)
            }
        }
    }


}
