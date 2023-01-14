using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public class Partol : Action
{
    public Vector2 Left;

    public Vector2 Right;
    private Rigidbody2D rb;
    public float timer = 0f;
    private float timecount = 0f;
    public float Speed = 1;
    public Transform[] wayPoints;
    private Animator animator;
    private GameObject boss;
    private bool GoLeft = true;
    public bool Ishurt = false;

    private Vector2 CurTarget;
    // Start is called before the first frame update
    private Vector2 targetl;
    private bool IsFirstStart = true;
    public override void OnStart()
    {
       
        boss = GameObject.FindGameObjectWithTag("Boss");
        rb = gameObject.GetComponentInChildren<Rigidbody2D>();
        animator = gameObject.GetComponentInChildren<Animator>();
        if (!rb)
        {
            Debug.Log("null");
        }
        
        // Vector2 target = new Vector2(wayPoints[Random.Range(0,wayPoints.Length)].position.x, rb.position.y);
        // //  GoLeft = false;
        // CurTarget = target;
        // Vector2 res = Vector2.MoveTowards(rb.position, target, Speed * Time.fixedDeltaTime);
        // rb.MovePosition(res);
        targetl = new Vector2(wayPoints[Random.Range(0,wayPoints.Length)].position.x, rb.position.y);
    }   

    // Update is called once per frame
    void Update()
    {
        
    }

    void Func()
    {

        Vector2 target = new Vector2(Left.x, rb.position.y);
          //  GoLeft = false;
        Vector2 res = Vector2.MoveTowards(rb.position, target, Speed * Time.fixedDeltaTime);
        rb.MovePosition(res);
        Debug.Log("walk");
        
        
        // else
        // {
        //     Vector2 target = new Vector2(Right.x, rb.position.y);
        //     GoLeft = false;
        //     Vector2 res = Vector2.MoveTowards(rb.position, target, Speed * Time.fixedDeltaTime);
        //     rb.MovePosition(res);
        // }
    }
    public override TaskStatus OnUpdate()
    {
        if (IsFirstStart)
            //  GoLeft = false;
        {
            Debug.Log("FirstGo");
            CurTarget = targetl;
            Vector2 resl = Vector2.MoveTowards(rb.position, targetl, 2 * Time.fixedDeltaTime);
            rb.MovePosition(resl);
            
            if (Vector2.Distance(rb.position, CurTarget) == 0)
            {
                IsFirstStart = false;
            }
        }
        if (CurTarget.x > transform.position.x)
            animator.SetTrigger("MoveRight");
        else if (CurTarget.x < transform.position.x)
        {
            animator.SetTrigger("MoveRight");
        }
        
        if (Ishurt)
        {
            return TaskStatus.Inactive;
        }

         if (Vector2.Distance(rb.position, CurTarget) == 0)
         {
             Debug.Log("WAYPOINT ACHIEVE");
             
             
                 targetl = new Vector2(wayPoints[Random.Range(0, wayPoints.Length)].position.x, rb.position.y);
                 IsFirstStart = true;
                 //time1 = 0;
             
             //      Vector2 target = new Vector2(wayPoints[Random.Range(0,wayPoints.Length)].position.x, rb.position.y);
        // //    //  GoLeft = false;
        //      CurTarget = target;
        //     Vector2 res = Vector2.MoveTowards(rb.position, target, Speed * Time.fixedDeltaTime);
        //      rb.MovePosition(res);
         }
        
       

        return TaskStatus.Running;
    }
    
}
