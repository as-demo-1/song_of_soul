using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleAgent: MonoBehaviour
{
    // Start is called before the first frame update
    private Rigidbody2D rb;
    public bool faceLeftOrigin;
    public Animator animator;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void faceLeft()//使自身朝向左
    {
        int x = faceLeftOrigin ? 1 : -1;
        Vector3 tem = transform.localScale;
        transform.localScale = new Vector3(x * Mathf.Abs(tem.x), tem.y, tem.z);
    }
    public void faceRight()
    {
        int x = faceLeftOrigin ? 1 : -1;
        Vector3 tem = transform.localScale;
        transform.localScale = new Vector3(x * -Mathf.Abs(tem.x), tem.y, tem.z);
    }

    public void turnFace()
    {
        //Debug.Log("turn face");
        Vector3 tem = transform.localScale;
        transform.localScale = new Vector3(-tem.x, tem.y, tem.z);
    }
    public bool currentFacingLeft()
    {
        return faceLeftOrigin ? transform.localScale.x > 0 : transform.localScale.x < 0;
    }
    public void faceWithSpeed()
    {
        if (rb.velocity.x < 0)
            faceLeft();
        else faceRight();
    }



    public void moveToTargetByFixedUpdate(Vector2 pos,float speed)
    {
        if (speed == 0) return;

        //transform.LookAt(target.transform.position);
        Vector2 newPos = Vector2.MoveTowards(transform.position, pos, speed * Time.fixedDeltaTime);
        rb.MovePosition(newPos);
    }

    public void moveToTargetByFixedUpdateWithArriveDistance(Vector2 pos, float speed,float arriveDistance)
    {
        float dis = Vector2.Distance(pos, transform.position);
        if (dis <= arriveDistance) return;
        moveToTargetByFixedUpdate(pos, speed);
    }
}
