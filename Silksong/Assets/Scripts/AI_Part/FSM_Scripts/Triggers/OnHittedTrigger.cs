using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnHittedTrigger :EnemyFSMBaseTrigger
{
    [DisplayOnly]
    public bool isHitted=false;

    public override void InitTrigger(EnemyFSMManager fsm_Manager)
    {
        base.InitTrigger(fsm_Manager);
        //EventsManager.Instance.AddListener(fsm_Manager.gameObject, EventType.onTakeDamage, Hitted);
        fsm_Manager.damageable.takeDamageEvent.AddListener(Hitted);
    }

    private void Hitted(DamagerBase damager,DamageableBase damageable)
    {
      isHitted = true;
       // Debug.Log(this.GetHashCode());

    }
    public override bool IsTriggerReach(EnemyFSMManager fsm_Manager)
    {
        if(isHitted)
        {
            isHitted = false;
           // Debug.Log("hitted trigger");
            return true;
        }
        else
        {
            return false;
        }
    }
}
