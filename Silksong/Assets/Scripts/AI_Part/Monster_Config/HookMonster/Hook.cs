using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * 钩子的逻辑 
 * 主要是推出去和收回来两个过程
 */
public class Hook : MonoBehaviour
{
    public float ditance = 10;
    public float shootSpeed = 3;
    public float backSpeed = 5;
    public Vector2 starPos;
    public LayerMask ground;
    Rigidbody2D rig;
    Collider2D col;
    Rigidbody2D target;
    bool ifback=false;
    bool ifShoot=false;
    private void Start()
    {
        rig = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
    }
    public void Shoot(Vector2 StartPos,Vector2 dir)
    {
        starPos = StartPos;
        if (rig == null)
        {
            rig = GetComponent<Rigidbody2D>();
            col = GetComponent<Collider2D>();
        }
        rig.velocity=dir.normalized*shootSpeed;
        ifShoot=true;
    }
    public void Back()
    {
        rig.velocity = -rig.velocity.normalized * backSpeed;
        ifback= true;
    }
    private void FixedUpdate()
    {     
        if ((Mathf.Abs(transform.position.x - starPos.x) > ditance||col.IsTouchingLayers(ground))&& !ifback)
        {
            ifShoot = false;
            Back();
        }
        else if(Mathf.Abs(transform.position.x - starPos.x) <= 0.1f && ifback)
        {
            ifback = false;           
            rig.velocity = Vector2.zero;
            target = null;        
            Debug.Log("recycle");
            gameObject.SetActive(false);
        }
        if (target != null)
        {
            target.velocity = rig.velocity;
            target.AddForce(-target.gravityScale * Physics2D.gravity);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            target = collision.GetComponent<Rigidbody2D>();
        }
    }
}
