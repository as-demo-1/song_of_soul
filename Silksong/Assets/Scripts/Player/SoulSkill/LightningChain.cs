using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningChain : SoulSkill
{
    public float range = 1.0f;
    [Range(0,1)]
    public float extraSpeedPercent = 0;
    public float baseDamage = 0;
    [Range(0,1)]
    public float extraDamagePercent = 0;
    
    //private 
    override protected PlayerCharacter GetPlayerCharacter()
    {
        return GetComponentInParent<PlayerCharacter>();
    }
    
    override protected PlayerController GetPlayerController()
    {
        return GetComponentInParent<PlayerController>();
    }
    public void SpeedUp(bool needApply)
    {
        
    }
    
    public void Attack()
    {
        atcEventName = "LightningChain Attack";
        EventCenter.Instance.TiggerEvent(atcEventName, this);
    }

    void Awake()
    {
        base.Awake();
        SpeedUp(true);
    }

    private void Update()
    {
        Attack();
    }

    private void OnDisable()
    {
        SpeedUp(false);
    }
}
