using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Ȼ��󺣵��ڶ��׶ηŵ��ʱ��ÿ��x���������������
/// ���׶�������Զ�ε����Ƶ�ʺ��������Ƶ����ͬ
/// </summary>
public class SeaUrchin_Thunder_State : EnemyFSMBaseState
{
    SeaUrchin seaUrchin;
    public string prickState;
    
    public LayerMask wallLayer;
    float angleInterval;
    public override void InitState(EnemyFSMManager enemyFSM)
    {
        base.InitState(enemyFSM);
        seaUrchin=enemyFSM as SeaUrchin;
        angleInterval = 360 / seaUrchin.pricks.Count;
    }
    public override void EnterState(EnemyFSMManager enemyFSM)
    {
        base.EnterState(enemyFSM);
        if (!seaUrchin.ifInWater)
        {
            foreach (var prick in seaUrchin.pricks)
            {
                prick.ChangeState(prickState);
            }
        }else{
            for(int i = 0; i < seaUrchin.pricks.Count; i++)
              {
                float angle = Random.Range(i * angleInterval, (i + 1) * angleInterval);
                RaycastHit2D hit2D = Physics2D.Raycast(enemyFSM.transform.position,Quaternion.AngleAxis(angle,Vector3.forward)*Vector2.up,100,wallLayer);
                seaUrchin.pricks[i].transform.up=hit2D.point-(Vector2)seaUrchin.pricks[i].transform.position;
                seaUrchin.pricks[i].ChangeState(prickState);
                Debug.Log(i);  
            }
        }
    }
    public override void ExitState(EnemyFSMManager enemyFSM)
    {
        base.ExitState(enemyFSM);
    }
}
