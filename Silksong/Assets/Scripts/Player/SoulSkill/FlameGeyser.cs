using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class FlameGeyser : SoulSkill
{
    [FormerlySerializedAs("bullet")] public PlayerSkillDamager bulletPrefab;
    // Start is called before the first frame update

    public override void Init(PlayerController playerController, PlayerCharacter playerCharacter)
    {
        base.Init(playerController, playerCharacter);
        bulletPrefab.damage = baseDamage;
    }
}
