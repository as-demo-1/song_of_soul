using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShadowBlade : SoulSkill
{
    public ParticleSystem stateParticle;

    public void ChangeState(bool _option)
    {
        stateParticle.gameObject.SetActive(_option);
    }
}
