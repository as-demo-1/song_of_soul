using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleBehaviour : StateMachineBehaviour
{
    public float angleSpeed;
    public float radio;
    [Tooltip("0为绕自身当前位置，1为绕玩家")]
    public int centreOfCircle=0;
    public bool lock_x=false,lock_y=false;
    public bool isFaceToPlayer = true;
    private Vector2 centre =new Vector2(99999,99999);
    private float timer;

    private MonsterInformation monsterInfo; 
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        monsterInfo = animator.GetComponent<MonsterInformation>();
    }
    
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

        Debug.Log("idel update"); 
        monsterInfo = animator.GetComponent<MonsterInformation>();
        if (monsterInfo == null)
        {
            return;
            Debug.Log("no monster info");
           
        }
        var endPos = monsterInfo.GetTargetPos();
        var pos = Vector2.Lerp(monsterInfo.transform.position, endPos, 0.3f);
        if (lock_x)
            monsterInfo.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
        if (lock_y)
            monsterInfo.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;
        monsterInfo.GetComponent<Rigidbody2D>().MovePosition(pos);
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    //override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
