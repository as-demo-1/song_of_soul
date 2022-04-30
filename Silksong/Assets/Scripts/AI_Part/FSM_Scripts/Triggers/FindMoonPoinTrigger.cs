using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
 * 寻找下一个月亮碎片的trigger
 * 有两种寻找方式一种是随机，另一种是朝着玩家方向
 */
public class FindMoonPoinTrigger : EnemyFSMBaseTrigger
{
    BreakMoonPoint[] breakMoonPoints;//所有的月亮碎片数组
    //moonPointDic保存怪物对应的要跳向的下一个月亮碎片，用静态字典可以让HookToMoonPoint_State直接找到要跳的下一个月亮碎片
    public static Dictionary<EnemyFSMManager, BreakMoonPoint> moonPointDic;
    // preMoonPointDic同上，但是保存的是前一个月亮碎片
    public static Dictionary<EnemyFSMManager, BreakMoonPoint> preMoonPointDic;
    //NearMoonPoints保存的是一个月亮碎片对应的周围其他的月亮碎片，要找下一个月亮碎片的位置就是靠这个
    public Dictionary<Transform,List<BreakMoonPoint>> NearMoonPoints;
    //怪物能从一个月亮碎片跳到另一个月亮碎片的范围，再这个范围内的所有月亮碎片都可以跳
    public float checkRadius;
    //怪物的警戒范围，如果玩家进入这个范围，怪物寻找下一个月亮碎片的逻辑从随机寻找变成寻找离玩家最近的碎片
    public float alertRadius;
    float sqrCheckRadius;
    BreakMoonPoint target;//要跳向的月亮碎片

    public override void InitTrigger(EnemyFSMManager fsm_Manager)
    {
        if(moonPointDic==null) moonPointDic = new Dictionary<EnemyFSMManager, BreakMoonPoint>();
        if(preMoonPointDic==null) preMoonPointDic = new Dictionary<EnemyFSMManager, BreakMoonPoint>();
        NearMoonPoints=new Dictionary<Transform, List<BreakMoonPoint>>();
        if (!moonPointDic.ContainsKey(fsm_Manager))moonPointDic.Add(fsm_Manager,null);
        if (!preMoonPointDic.ContainsKey(fsm_Manager)) preMoonPointDic.Add(fsm_Manager, null);
        breakMoonPoints = GameObject.FindObjectsOfType<BreakMoonPoint>();
        sqrCheckRadius = checkRadius * checkRadius;
        for(int i = 0; i < breakMoonPoints.Length; i++)
        {
            NearMoonPoints.Add(breakMoonPoints[i].transform, new List<BreakMoonPoint>());
            for(int j = 0; j < breakMoonPoints.Length; j++)
            {
                if (i != j && (breakMoonPoints[i].transform.position - breakMoonPoints[j].transform.position).sqrMagnitude <= sqrCheckRadius)
                {
                    NearMoonPoints[breakMoonPoints[i].transform].Add(breakMoonPoints[j]);
                }
            }
        }
        NearMoonPoints.Add(fsm_Manager.transform, new List<BreakMoonPoint>());
        for (int j = 0; j < breakMoonPoints.Length; j++)
        {           
            if ((fsm_Manager.transform.position - breakMoonPoints[j].transform.position).sqrMagnitude <= sqrCheckRadius)
            {
                NearMoonPoints[fsm_Manager.transform].Add(breakMoonPoints[j]);
            }
        }       
        base.InitTrigger(fsm_Manager);
    }
    public BreakMoonPoint RandPoint(EnemyFSMManager enemyFSM)
    {
        if (preMoonPointDic[enemyFSM] != null)
        {
            int length = NearMoonPoints[preMoonPointDic[enemyFSM].transform].Count;
            return NearMoonPoints[preMoonPointDic[enemyFSM].transform][Random.Range(0, length)];
        }
        else
        {
            int length = NearMoonPoints[enemyFSM.transform].Count;
            return NearMoonPoints[enemyFSM.transform][Random.Range(0, length)];
        }
    }
    public BreakMoonPoint ClosestPoint(EnemyFSMManager enemyFSM)
    {
        Vector2 playerPos=(Vector2)enemyFSM.transform.position+enemyFSM.getTargetDir();
        float minDis = 10000;
        BreakMoonPoint ans=null;
        foreach(var point in NearMoonPoints[preMoonPointDic[enemyFSM].transform])
        {
            float dis = ((Vector2)point.transform.position - playerPos).magnitude;
            if (dis < minDis)
            {
                minDis = dis;
                ans = point;
            }
        }
        return ans;
    }
    public override bool IsTriggerReachInFixUpdate(EnemyFSMManager enemyFSM)
    {
        if (target == null)
        {
            if (enemyFSM.getTargetDir().magnitude < alertRadius) target=ClosestPoint(enemyFSM); 
            else target=RandPoint(enemyFSM);
            moonPointDic[enemyFSM] = target;
            target.bePicked();
        }
        if (target != null )
        {
            target = null;            
            return true;
        }
        return false;
    }
}
