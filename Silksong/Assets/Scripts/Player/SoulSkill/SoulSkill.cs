using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public enum SkillName
{
    FlameGeyser,
    ShadowBlade,
    LightningChain,
    ArcaneBlast,
    IceStorm
}

[RequireComponent(typeof(PlayerCharacter))]
public abstract class SoulSkill : MonoBehaviour
{
    public SkillName skillName;
    public uint constPerSec = 0;
    public uint constPerAttack = 0;
    protected PlayerCharacter playerCharacter;
    protected PlayerController playerController;
    protected abstract PlayerCharacter GetPlayerCharacter();
    protected abstract PlayerController GetPlayerController();
    private void Awake()
    {
        playerCharacter = GetPlayerCharacter();
        playerController = GetPlayerController();
    }
}
