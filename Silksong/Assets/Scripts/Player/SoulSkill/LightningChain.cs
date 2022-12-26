using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningChain : SoulSkill
{
    override protected PlayerCharacter GetPlayerCharacter()
    {
        return GetComponentInParent<PlayerCharacter>();
    }
    
    override protected PlayerController GetPlayerController()
    {
        return GetComponentInParent<PlayerController>();
    }
    void SpeedUp()
    {
        
    }
}
