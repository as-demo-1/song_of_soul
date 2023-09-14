using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

/// 
[System.Serializable]
/// 在hpdamable的基础上 增加受击后无敌机制
/// </summary>作者：青瓜
public class InvulnerableDamable : HpDamable
{
    public bool invulnerableAfterDamage = true; //受伤后无敌
    public float invulnerabilityDuration = 3f; //无敌时间
    protected float inulnerabilityTimer;

    private bool ignoreTimer;

    private Tweener playerFlash;

    private void Start()
    {
        playerFlash = PlayerController.Instance.PlayerAnimator.GetComponent<SpriteRenderer>()
            .material.DOFade(0.5f, "_Color", 0.5f).SetLoops(-1, LoopType.Yoyo);
        playerFlash.Pause();
        playerFlash.SetAutoKill(false);
    }

    public override void takeDamage(DamagerBase damager)
    {
        base.takeDamage(damager);
        if (invulnerableAfterDamage)
        {
            enableInvulnerability();
        }
    }


    public void enableInvulnerability(bool ignoreTimer = false) //开启无敌
    {
       
        playerFlash.Play();
        Debug.Log("开启玩家无敌");
        invulnerable = true;
        this.ignoreTimer = ignoreTimer;
        inulnerabilityTimer = ignoreTimer ? 1 : invulnerabilityDuration;
        //technically don't ignore timer, just set it to an insanly big number. Allow to avoid to add more test & special case.
        //inulnerabilityTimer = ignoreTimer ? float.MaxValue : invulnerabilityDuration;
    }

    public void disableInvulnerability()
    {
        playerFlash.Pause();
        Debug.Log("关闭玩家无敌");
        this.ignoreTimer = false;
        inulnerabilityTimer = 0;
    }

    void Update()
    {
        if (invulnerable && invulnerableAfterDamage)
        {
            if (!ignoreTimer)
            {
                inulnerabilityTimer -= Time.deltaTime;
            }

            if (inulnerabilityTimer <= 0f)
            {
                invulnerable = false;
                playerFlash.Pause();
                GetComponent<Collider2D>().enabled =
                    false; //重新激活一次collider 避免在无敌时进入trigger内 导致无敌过后ontriggerEnter不触发
                GetComponent<Collider2D>().enabled = true;
            }
        }
    }
}