using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// 冰风暴技能，会召唤一个奶淇淋追踪并攻击敌人
/// </summary>
public class IceStorm : SoulSkill
{
    /// <summary>
    /// 召唤物
    /// </summary>
    public GameObject snowManPrefab;

    private GameObject snowMan;

    public GameObject explodePrefab;
    
    public override void Init(PlayerController playerController, PlayerCharacter playerCharacter)
    {
        this.SkillStart.AddListener(() =>
        {
            snowMan = Instantiate(snowManPrefab, transform.position, transform.rotation);
            damager = snowMan.GetComponentInChildren<PlayerSkillDamager>();
            damager.damage = baseDamage;
            damager.gameObject.SetActive(false);
            DontDestroyOnLoad(snowMan);
        });
        this.SkillEnd.AddListener(()=>
        {
             Destroy(Instantiate(explodePrefab, snowMan.transform.position, quaternion.identity), 2.0f);
            snowMan.SetActive(false);
        });
        base.Init(playerController, playerCharacter);
    }
}
