using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlameGeyser : SoulSkill
{
    public PlayerSkillDamager bullet;
    // Start is called before the first frame update

    public override void Init(PlayerController playerController, PlayerCharacter playerCharacter)
    {
        base.Init(playerController, playerCharacter);
        bullet.damage = baseDamage;
    }
}
