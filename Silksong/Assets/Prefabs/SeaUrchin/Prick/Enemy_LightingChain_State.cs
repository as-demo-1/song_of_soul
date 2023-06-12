using BehaviorDesigner.Runtime.Tasks.Unity.UnityGameObject;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �ҵ��뷨�ǣ������굽��Զλ�ã�Ȼ��ų�����,Ȼ��ʼ��ת
/// ��ô���׶ε�д������ enter��ʱ��ŵ磬����ǽ�Ժ�ŵ磬�Ͳ���Ҫ��ת��  
/// </summary>
public class Enemy_LightingChain_State : EnemyFSMBaseState
{
    [LabelText("������뾶")]
    public float radius;
    [LabelText("����Ԥ����")]
    public GameObject lightningChain_Pre;
    [LabelText("������")]
    [InfoBox("���̫�̵Ļ������ܱ�һֱ��")]
    public float lightningInterval;
    [LabelText("�����ٶ�")]
    public float moveSpeed;
    [LabelText("��ת�ٶ�")]
    public float rotateSpeed;
    public LayerMask player,ground;
    GameObject lightningChain;
    Transform parent;
    SeaUrchin seaUrchin;
    bool ifLightning;
    float t;
    Vector2 indexRange;
    public override void InitState(EnemyFSMManager enemyFSM)
    {
        base.InitState(enemyFSM);
        lightningChain=GameObject.Instantiate(lightningChain_Pre,enemyFSM.transform);
        lightningChain.transform.localPosition = Vector3.zero;
        lightningChain.transform.up = enemyFSM.transform.up;
        parent = enemyFSM.transform.parent;
        seaUrchin=parent.GetComponent<SeaUrchin>();
        lightningChain.SetActive(false);
    }
    public override void EnterState(EnemyFSMManager enemyFSM)
    {
        base.EnterState(enemyFSM);
        if (!seaUrchin.ifInWater)
        {
            lightningChain.SetActive(true);
            enemyFSM.transform.up = enemyFSM.transform.position - parent.transform.position;
            lightningChain.transform.up = -enemyFSM.transform.up;
            ifLightning = false;
        }else{
            int i=seaUrchin.pricks.IndexOf(enemyFSM);
            if(i<seaUrchin.pricks.Count/2){
                indexRange=new Vector2(seaUrchin.pricks.Count/2,seaUrchin.pricks.Count);
            }else{
                indexRange=new Vector2(0,seaUrchin.pricks.Count/2);
            }
            Debug.Log(i+" "+indexRange);
        }
    }//
    public override void ExitState(EnemyFSMManager enemyFSM)
    {
        base.ExitState(enemyFSM);
        lightningChain.SetActive(false);
    }
    public override void FixAct_State(EnemyFSMManager enemyFSM)
    {
        base.FixAct_State(enemyFSM);
        if (!seaUrchin.ifInWater)
        {
            if (enemyFSM.transform.localPosition.magnitude < radius)
            {
                enemyFSM.rigidbody2d.velocity = enemyFSM.transform.up * moveSpeed;
            }
            else
            {
                enemyFSM.rigidbody2d.velocity = Vector2.zero;
                enemyFSM.transform.RotateAround(parent.transform.position, Vector3.forward, rotateSpeed * Time.fixedDeltaTime);
                lightningChain.SetActive(true);
            }//
            if (t < lightningInterval)
                t += Time.fixedDeltaTime;
            else
            {
                Vector2 dir = parent.transform.position - enemyFSM.transform.position;
                RaycastHit2D hit2d = Physics2D.Raycast(enemyFSM.transform.position, dir, 100, player);
                if (hit2d.collider != null)
                {
                    t = 0;
                }
            }
        }
        else
        {
            RaycastHit2D hit2d = Physics2D.Raycast(enemyFSM.transform.position, enemyFSM.transform.up, 100, ground);
            if(!enemyFSM.rigidbody2d.IsTouchingLayers(ground)){
                enemyFSM.rigidbody2d.velocity = enemyFSM.transform.up * moveSpeed;
            }else{
                enemyFSM.rigidbody2d.velocity =Vector2.zero;
                if (t < lightningInterval){
                    t += Time.fixedDeltaTime;
                    if(t>lightningInterval*2/3){
                        lightningChain.SetActive(false);
                    }
                }
                 else{
                     t=0;    
                     int r=Random.Range((int)indexRange.x,(int)indexRange.y);
                     lightningChain.SetActive(true);
                     lightningChain.transform.up = seaUrchin.pricks[r].transform.position-enemyFSM.transform.position;
                 }
            }
        }
    }
}
