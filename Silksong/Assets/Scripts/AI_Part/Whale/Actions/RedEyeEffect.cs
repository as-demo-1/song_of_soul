using BehaviorDesigner.Runtime.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RedEyeEffect : BattleAction
{
    public string effectName;
    public override void OnStart()
    {
        GameObject effect = transform.Find("Effects").Find(effectName).gameObject;
        effect.SetActive(true);
        effect.GetComponent<ParticleSystem>().Play();
    }
    public override TaskStatus OnUpdate()
    {
        return TaskStatus.Success;
    }
}
