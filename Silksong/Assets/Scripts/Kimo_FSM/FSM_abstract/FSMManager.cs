using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// ״̬���������Ļ���
/// </summary>
/// <typeparam name="T1"></typeparam>
/// <typeparam name="T2"></typeparam>
public abstract class FSMManager<T1,T2> : MonoBehaviour
{
    public PlayerController playerController;
    public Animator animator;
    public AudioSource audios;
    public Rigidbody2D rigidbody2d;
    public bool FaceLeftFirstOriginal;//ԭͼ�Ƿ�����

   // public Collider2D triggerCollider;
    public Collision2D collision;

    public DamageableBase damageable;

    /// /// <summary>
    /// ��ǰ״̬
    /// </summary>
    public FSMBaseState<T1,T2> currentState;
    [DisplayOnly]
    public T1 currentStateID;
    /// <summary>
    /// ����״̬
    /// </summary>
    public FSMBaseState<T1,T2> anyState;
    public T1 defaultStateID;
    /// <summary>
    /// ��ǰ״̬������������״̬�б�
    /// </summary>
    public Dictionary<T1, FSMBaseState<T1,T2>> statesDic = new Dictionary<T1, FSMBaseState<T1,T2>>();
    /// <summary>
    /// ����״̬�б������Ӧ�����б���SO�ļ�
    /// </summary>


    public void ChangeState(T1 state)
    {
      //  Debug.Log(state.ToString()+"  "+gameObject.name);
        if (currentState != null)
            currentState.ExitState(this);

        if (statesDic.ContainsKey(state))
        {
            currentState = statesDic[state];
            currentStateID = state;
        }
        else
        {
            Debug.LogError("״̬�����ڣ�����Fsm�������");
        }
        currentState.EnterState(this);
        if (currentState.animName != null)
        {
            animator.Play(currentState.animName);
            currentStateInfo = animator.GetCurrentAnimatorStateInfo(0);
        }
    }

    //public FSMBaseState<T1,T2> AddState(T1 state)
    //{
    //    //Debug.Log(triggerID);

    //    Type type = Type.GetType("Enemy"+state + "State");
    //    if (type == null)
    //    {
    //        Debug.LogError(state + "�޷����ӵ�" + "��states�б�");
    //        Debug.LogError("���stateIDö��ֵ����Ӧ��������Ӧö�������ϡ�_State������ö��ֵΪIdle��״̬����ΪIdle_State���������ü��أ�");
    //        return null;
    //    }
    //    else
    //    {
    //        FSMBaseState<T1,T2> temp = Activator.CreateInstance(type) as FSMBaseState<T1,T2>;
    //        statesDic.Add(state,temp);
    //        return temp;
    //    }
    //}
    //public FSMBaseState<T1,T2> AddState(T1 state,FSMBaseState<T1,T2> stateClass)
    //{
    //    statesDic.Add(state, stateClass);
    //    return stateClass;
    //}
    //public void RemoveState(T1 state)
    //{
    //    if (statesDic.ContainsKey(state))
    //        statesDic.Remove(state);
    //}
    /// <summary>
    /// ���ڳ�ʼ��״̬���ķ�������������״̬����������ӳ�������ȡ��������ȡ�Awakeʱִ�У��ɲ�ʹ�û��෽���ֶ��������
    /// </summary>
    /// 

    public virtual void InitWithScriptableObject()
    {
    }
    public virtual void InitManager()
    {
        if (GetComponent<Animator>() != null)
        {
            animator = GetComponent<Animator>();
        }
        if (GetComponent<AudioSource>() != null)
        {
            audios = GetComponent<AudioSource>();
        }
        if (GetComponent<Rigidbody2D>() != null)
        {
            rigidbody2d = GetComponent<Rigidbody2D>();
        }
        if (!TryGetComponent(out PlayerController playerController))
        {
            this.playerController = GameObject.Find("Player").GetComponent<PlayerController>();
        }

        InitWithScriptableObject();
        ////�����ȡ
    }

    protected void Awake()
    {
        statesDic.Clear();
        InitManager();
        damageable = GetComponent<DamageableBase>();
    }

    protected virtual void Start()
    {
        if (statesDic.Count == 0)
            return;
        //Ĭ��״̬����
        currentStateID = defaultStateID;
        ChangeState(currentStateID);
        if (anyState != null)
            anyState.EnterState(this);

        //// Debug code
        //foreach (var state in statesDic.Values)
        //    foreach (var value in state.triggers)
        //    {
        //        Debug.LogWarning(this + "  " + state + "  " + value + "  " + value.GetHashCode());
        //    }

    }

    private void Update()
    {

        if (currentState != null)
        {
            //ִ��״̬����
            currentState.Act_State(this);
            //���״̬�����б�
            currentState.TriggerState(this);
            currentStateInfo = animator.GetCurrentAnimatorStateInfo(0);
        }
        else
        {
            Debug.LogError("currentStateΪ��");
        }

        if (anyState != null)
        {
            anyState.Act_State(this);
            anyState.TriggerState(this);
        }
    }

    public void faceLeft()//ʹ����������
    {
        int x = FaceLeftFirstOriginal ? 1 : -1;
        transform.localScale = new Vector3(x, 1, 1);
    }
    public void faceRight()
    {
        int x = FaceLeftFirstOriginal ? 1 : -1;
        transform.localScale = new Vector3(-x, 1, 1);
    }

    /// <summary>
    /// ���ݸ����ٶȸı���������
    /// </summary>
    public void faceWithSpeed()
    {
        if (rigidbody2d.velocity.x > 0)
            faceRight();
        else faceLeft();
    }



}

public abstract class FSMManagerInherit<T1, T2, T3, T4> : FSMManager<T1, T2>
{
    public List<Enemy_State_SO_Config> stateConfigs;
    public Enemy_State_SO_Config anyStateConfig;
    public GameObject player;

    protected override void Start()
    {
        base.Start();
        player = GameObject.FindGameObjectWithTag("Player");
    }
    public override void InitWithScriptableObject()
    {
        if(anyStateConfig!=null)
        {

            anyState = (FSMBaseState<EnemyStates, EnemyTriggers>)ObjectClone.CloneObject(anyStateConfig.stateConfig);
            anyState.triggers = new List<FSMBaseTrigger<EnemyStates, EnemyTriggers>>();
            for (int k=0;k<anyStateConfig.triggerList.Count; k++)
            {
                anyState.triggers.Add(ObjectClone.CloneObject(anyStateConfig.triggerList[k]) as FSMBaseTrigger<EnemyStates, EnemyTriggers>);
                anyState.triggers[anyState.triggers.Count - 1].InitTrigger(this);
                //Debug.Log(this.gameObject.name+"  "+ anyState.triggers[anyState.triggers.Count - 1]+"  "+anyState.triggers[anyState.triggers.Count - 1].GetHashCode());
            }
            anyState.InitState(this);
        }
        for (int i = 0; i < stateConfigs.Count; i++)
        {
            FSMBaseState<EnemyStates, EnemyTriggers> tem = ObjectClone.CloneObject(stateConfigs[i].stateConfig) as FSMBaseState<EnemyStates, EnemyTriggers>;
            tem.triggers = new List<FSMBaseTrigger<EnemyStates, EnemyTriggers>>();
            for (int k=0;k< stateConfigs[i].triggerList.Count;k++)
            {
                tem.triggers.Add(ObjectClone.CloneObject(stateConfigs[i].triggerList[k]) as FSMBaseTrigger<EnemyStates, EnemyTriggers>);
                tem.triggers[tem.triggers.Count-1].InitTrigger(this);
                //Debug.Log(this.gameObject.name + "  " + tem.triggers[tem.triggers.Count - 1] + "  " + tem.triggers[tem.triggers.Count - 1].GetHashCode());
            }
            statesDic.Add(stateConfigs[i].stateID, tem);
            tem.InitState(this);
        }
    }

    /// <summary>
    ///���ָ�����λ�õ�vector2(��normalized) ��ѡͬʱ�ı���ﳯ��
    /// </summary>
    public Vector2 getTargetDir(bool changeFace=false)
    {
        Vector2 dir = player.transform.position - transform.position;
        if(changeFace)
        {
            if (dir.x > 0)
            {
                //Debug.Log("dir right");
                faceRight();
            }

            else
            {
                //Debug.Log("dir left");
                faceLeft();
            }
        }
        return dir;
    }
}
/// <summary>
///����NPC״̬������������Ϊ������SO���ù���
/// </summary>
public class NPCFSMManager : FSMManager<NPCStates, NPCTriggers>
{
    public List<NPC_State_SO_Config> stateConfigs;
    public Enemy_State_SO_Config anyStateConfig;
    public override void InitWithScriptableObject()
    {
        if (anyStateConfig != null)
        {
            anyState = (FSMBaseState<T1, T2>)ObjectClone.CloneObject(anyStateConfig.stateConfig);
            anyState.triggers = new List<FSMBaseTrigger<T1, T2>>();
            for (int k = 0; k < anyStateConfig.triggerList.Count; k++)
            {
                anyState.triggers.Add(ObjectClone.CloneObject(anyStateConfig.triggerList[k]) as FSMBaseTrigger<T1, T2>);
                anyState.triggers[anyState.triggers.Count - 1].InitTrigger(this);
                //Debug.Log(this.gameObject.name+"  "+ anyState.triggers[anyState.triggers.Count - 1]+"  "+anyState.triggers[anyState.triggers.Count - 1].GetHashCode());
            }
            anyState.InitState(this);
        }
        for (int i = 0; i < stateConfigs.Count; i++)
        {
            FSMBaseState<T1, T2> tem = ObjectClone.CloneObject(stateConfigs[i].stateConfig) as FSMBaseState<T1, T2>;
            tem.triggers = new List<FSMBaseTrigger<T1, T2>>();
            for (int k = 0; k < stateConfigs[i].triggerList.Count; k++)
            {
                tem.triggers.Add(ObjectClone.CloneObject(stateConfigs[i].triggerList[k]) as FSMBaseTrigger<T1, T2>);
                tem.triggers[tem.triggers.Count - 1].InitTrigger(this);
                //Debug.Log(this.gameObject.name + "  " + tem.triggers[tem.triggers.Count - 1] + "  " + tem.triggers[tem.triggers.Count - 1].GetHashCode());
            }
            statesDic.Add(stateConfigs[i].stateID, tem);
            tem.InitState(this);
        }
    }
}

/// <summary>
///����Enemy״̬������������Ϊ������SO���ù���
/// </summary>
public class EnemyFSMManager : FSMManagerInherit<EnemyStates, EnemyTriggers, EnemyFSMBaseState, EnemyFSMBaseTrigger> { }
/// <summary>
///����NPC״̬������������Ϊ������SO���ù���
/// </summary>
public class NPCFSMManager : FSMManagerInherit<NPCStates, NPCTriggers, NPCFSMBaseState, NPCFSMBaseTrigger> { }
/// <summary>
/// ����Player״̬����������Ĭ��û������SO���ù��ܣ�
/// ����Ҫ��
/// ����ȡ���������ע��
/// Ȼ���Player_State_SO_Config�ű���ȡ������Player_State_SO_Config���ע�ͼ��ɡ�
/// 
/// </summary>
public class PlayerFSMManager : FSMManagerInherit<PlayerStates, PlayerTriggers, PlayerFSMBaseState, PlayerFSMBaseTrigger> { }

