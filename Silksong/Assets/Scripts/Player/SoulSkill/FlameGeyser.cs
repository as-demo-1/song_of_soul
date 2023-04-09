using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlameGeyser : SoulSkill
{
    // Start is called before the first frame update
    
    private void OnEnable()
    {
        base.OnEnable();
    }

    private void OnDisable()
    {
        base.OnDisable();
    }
    public override bool AtkPerTarget(Hittable target)
    {
        throw new System.NotImplementedException();
    }

    protected override bool IsAtkSuccess(Hittable target)
    {
        throw new System.NotImplementedException();
    }
}
