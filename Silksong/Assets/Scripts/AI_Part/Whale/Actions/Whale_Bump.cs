using BehaviorDesigner.Runtime.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Whale_Bump : BattleAction
{
    public float speed;
    public override void OnStart()
    {
        base.OnStart();
        Vector2 dir = target.Value.transform.position - transform.position;
        dir.Normalize();
        GetComponent<Rigidbody2D>().velocity = dir*speed;
        transform.Find("Damager_Bump").gameObject.SetActive(true);
        transform.Find("Damager").gameObject.SetActive(false);
        animator.SetBool("rush", true);
        Debug.Log("bump");
    }

    public override void OnEnd()
    {
        base.OnEnd();
        WhaleBossManager.Instance.resetBorderSkillCdTimer();
        GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        transform.Find("Damager_Bump").gameObject.SetActive(false);
        transform.Find("Damager").gameObject.SetActive(true);
        animator.SetBool("rush", false);
    }

    public override TaskStatus OnUpdate()
    {
        return TaskStatus.Running;
    }
}
