using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EPlayerAttackType
{
    NormalAttack,

    FlameGeyser,

    ShadowBlade,

    [Tooltip("…¡µÁ¡¥")]
    LightningChain,

    ArcaneBlast,

    IceStorm

}
public class PlayerMakeDamageGainSoul : makeDamageEventSetter
{
    public EPlayerAttackType attackType;
    public override void damageEvent(DamagerBase damager, DamageableBase damageable)
    {
        if (damageable.playerAttackCanGainSoul)
        {
           PlayerCharacter character = PlayerController.Instance.playerCharacter;
            character.addMana(character.getAttackGainManaNumber(getBaseGainSoulValue()));
        }
    }

    private int getBaseGainSoulValue()
    {
        switch (attackType)
        {
            case EPlayerAttackType.NormalAttack:
                {
                    return Constants.playerAttackGainSoul;
                }
            default:
                {
                    return 0;
                }
        }
    }
   
}
