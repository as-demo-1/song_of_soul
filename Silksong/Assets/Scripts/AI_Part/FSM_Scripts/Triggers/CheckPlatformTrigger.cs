using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPlatformTrigger : EnemyFSMBaseTrigger
{
    public static Dictionary<EnemyFSMManager, Vector2> jumpDirectDic;
    public Vector2 boxSize;
    public LayerMask layer;
    public Vector2 offset;
    Vector2 jumpDirect;
    
    public override void InitTrigger(EnemyFSMManager fsm_Manager)
    {
        if(jumpDirectDic==null)jumpDirectDic = new Dictionary<EnemyFSMManager, Vector2>();
        if(!jumpDirectDic.ContainsKey(fsm_Manager))jumpDirectDic.Add(fsm_Manager, jumpDirect);
        base.InitTrigger(fsm_Manager);
    }
    public override bool IsTriggerReachInFixUpdate(EnemyFSMManager enemyFSM)
    {
        return BoxCheck(enemyFSM);
    }
    public bool BoxCheck(EnemyFSMManager fsm_Manager)
    {
        Vector2 starPos = Vector2.zero;
        Vector2 EndPos = Vector2.zero;
        float dy = fsm_Manager.transform.localScale.x / Mathf.Abs(fsm_Manager.transform.localScale.x);
        Vector2 position = (Vector2)fsm_Manager.transform.position + offset * dy;
        float i = 0;
        Debug.DrawLine(position, position + (Vector2)fsm_Manager.transform.up* boxSize.x,Color.blue);
        for (; i < boxSize.y; i += 0.1f)
        {
            position.y += 0.1f * dy;//transform.upÊÇÌøÔ¾·´Ïò          
            RaycastHit2D raycast = Physics2D.Raycast(position, fsm_Manager.transform.up, boxSize.x, layer);
            Debug.DrawLine(position, position + (Vector2)fsm_Manager.transform.up * boxSize.x);
            if (raycast.collider != null)
            {
                if (starPos == Vector2.zero)
                    starPos = raycast.point;
                else
                    EndPos = raycast.point;
                if (Mathf.Abs(starPos.y - EndPos.y) >= 2.5f && EndPos != Vector2.zero)
                {
                    jumpDirect = (starPos + EndPos) / 2 - (Vector2)fsm_Manager.transform.position;
                    jumpDirectDic[fsm_Manager] = jumpDirect;
                    return true;
                }
            }
            else
            {
                starPos = EndPos = Vector2.zero;
            }
        }
        Debug.DrawLine(position, position + (Vector2)fsm_Manager.transform.up);
        jumpDirect = Vector2.zero;
        jumpDirectDic[fsm_Manager] = jumpDirect;
        return false;
    }
}
