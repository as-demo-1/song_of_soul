using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CatState
{
    FixedPoint,
    Singing
}
public class WhiteCat : MonoBehaviour
{
    public LayerMask layerMask;
    public float testdis;
    public float range;
    public float speed;
    public float jumpHeight;
    public List<Vector2> posList;
    public CatState state;
    public float delay;
    float risingForce;
    Animator animator;
    Collider2D[] collider2Ds;
    bool ifmove = false;
    bool facingRight=true;
    bool ifGround;
    bool ifJump;
    int movePos;
    private void Start()
    {
        animator = GetComponent<Animator>();
    }
    private void FixedUpdate()
    {
        collider2Ds = Physics2D.OverlapCircleAll(transform.position, range);
        RaycastHit2D hit2D = Physics2D.Raycast(transform.position, Vector2.down, 1, layerMask);
        if (hit2D) ifGround = true;
        else ifGround = false;
        foreach (var c in collider2Ds)
        {
            Move(c);
        }
    }
    public void Move(Collider2D collider2D)
    {
        if (ifmove) return;
        if (state == CatState.FixedPoint && collider2D.CompareTag("Player"))
        {
            if (movePos < posList.Count)
            {
                StartCoroutine(Moving(posList[movePos]));
                movePos++;
            }
        }else if (state == CatState.Singing && collider2D.CompareTag("Singing"))
        {
            StartCoroutine(Moving(collider2D.transform.position));
        }
    }
    IEnumerator Moving(Vector2 target)
    {
        ifmove = true;
        animator.SetBool("run", true);
        FacingRight(target.x);
        while (Mathf.Abs(transform.position.x-target.x)>=0.1)
        {
            Jump();
            transform.position = Vector2.MoveTowards(transform.position, new Vector2(target.x,transform.position.y), speed*Time.deltaTime);
            yield return null;
        }
        animator.SetBool("run", false);
        ifmove = false;
    }
    void FacingRight(float targetX)
    {
        if ((facingRight && (targetX - transform.position.x)<-0.1f) || (!facingRight && targetX - transform.position.x>0.1f)){
            transform.right = -transform.right;
            facingRight = !facingRight;
        }
    }
    void Jump()
    {
        if (!ifGround || !ifmove) {
            animator.SetBool("jump", false);
            ifJump = false;
            return; }
        RaycastHit2D hit2D= Physics2D.Raycast(transform.position + transform.right*2, (Vector2.down+(Vector2)transform.right), testdis,layerMask);
        if (!hit2D.collider&&!ifJump)
        {
            ifJump = true;
            animator.SetBool("jump", true);
            float upSpeed = Mathf.Sqrt(jumpHeight * 2*Physics2D.gravity.y);
            GetComponent<Rigidbody2D>().velocity=Vector2.up * jumpHeight;
        }
    }
}
