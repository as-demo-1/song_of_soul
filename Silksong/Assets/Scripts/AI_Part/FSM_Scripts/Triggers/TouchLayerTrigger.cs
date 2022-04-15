using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchLayerTrigger :EnemyFSMBaseTrigger
{
    public LayerMask Layer;
    public override bool IsTriggerReachOnTriggerEnter(EnemyFSMManager enemyFSM, Collider2D collision)
    {
        return ((collision.gameObject.layer & Layer)!=0);
    }
    public override bool IsTriggerReachOnCollisionEnter(EnemyFSMManager enemyFSM, Collision2D collision)
    {
        Debug.Log((1 << collision.collider.gameObject.layer));
        Debug.Log(Layer.value);
        return ((1<<collision.collider.gameObject.layer) & Layer)!=0;
    }
}
