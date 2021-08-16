using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    public int healthMax;
    private int health;

    public float speed;
    public float jumpForce;

    private Rigidbody2D rb;
    //private Vector2 moveVelocity;
    private float moveInput;

    private bool facingRight;

    public bool isGrounded;
    public Transform groundCheck;
    public float checkRadius;
    public LayerMask ground;

    //jump function
    public int jumpValue;
    public int jumpCount;
    public float jumpTime;
    private float jumpTimeCounter;
    private bool isJumping;
    private bool isFalling;

    //private Animator anim;

    //attack function
    private float timeBtwAttack;
    public float attackSpeed;
    public Transform attackPos;
    public float attackRange;
    public LayerMask EnemyLayer;
    public int damage;

    //受伤延迟效果
    public float startDazeTime;
    private float dazeTime;
    private bool isDazing;




    // Start is called before the first frame update
    void Start()
    {
        //anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        jumpCount = jumpValue;
        timeBtwAttack = attackSpeed;
        dazeTime = startDazeTime;
    }


    // Update is called once per frame
    void Update()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, ground);
        if (!isDazing)
        {
            if (timeBtwAttack <= 0)
            {
                if (Input.GetKeyDown(KeyCode.X))
                {
                    //anim.SetBool("attack", true);
                    //在动画事件中触发Attack（）函数
                    Debug.Log("player attack");
                    timeBtwAttack = attackSpeed;//reset time
                }
            }
            else
            {
                timeBtwAttack -= Time.deltaTime;
            }
            /*if (Input.GetKeyUp(KeyCode.X))
            {
                anim.SetBool("attack", false);
            }*/

            //jump function
            if (Input.GetKeyDown(KeyCode.Z) && isGrounded)
            {
                rb.velocity = Vector2.up * jumpForce;
                //anim.SetTrigger("takeOf");
                isJumping = true;

                jumpTimeCounter = jumpTime;
            }
            else if (Input.GetKeyDown(KeyCode.Z) && jumpCount > 0)
            {
                rb.velocity = Vector2.up * jumpForce;
                jumpCount--;
                //anim.SetTrigger("takeOf");

                isJumping = true;

                jumpTimeCounter = jumpTime;
            }

            //jump higher
            if (Input.GetKey(KeyCode.Z) && isJumping)
            {
                if (jumpTimeCounter > 0)
                {
                    rb.velocity = Vector2.up * jumpForce;
                    jumpTimeCounter -= Time.deltaTime;
                }
                else
                {
                    isJumping = false;
                }
            }
            if (Input.GetKeyUp(KeyCode.Z))
            {
                isJumping = false;
            }
        }


        // jump animation
        if (isGrounded)
        {
            jumpCount = jumpValue;
            //anim.SetBool("isJumping", false);

            if (isFalling)
            {
                //anim.SetTrigger("isFalled");
                isFalling = false;
            }

        }
        else
        {
            //anim.SetBool("isJumping", true);
            isFalling = true;

        }

        if (isDazing)
        {
            dazeTime -= Time.deltaTime;

        }
        if (dazeTime <= 0)
        {
            isDazing = false;
            dazeTime = startDazeTime;
        }
    }

    private void FixedUpdate()
    {

        //// moving on the ground
        //Vector2 moveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        //moveVelocity = moveInput.normalized * speed;
        //rb.MovePosition(rb.position + moveVelocity * Time.fixedDeltaTime);

        // moving at the platform
        if (!isDazing)
        {
            moveInput = Input.GetAxisRaw("Horizontal");
            rb.velocity = new Vector2(moveInput * speed, rb.velocity.y);
        }



        /*if (moveInput == 0)
        {
            anim.SetBool("isRunning", false);

        }
        else
        {
            anim.SetBool("isRunning", true);

        }*/
        // flip player to right direction
        if (facingRight == false && moveInput > 0)
        {
            Filp();
        }
        else if (facingRight == true && moveInput < 0)
        {
            Filp();
        }




    }

    /*public void Attack()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(attackPos.position, attackRange, EnemyLayer);
        if (enemies != null)
        {
            for (int i = 0; i < enemies.Length; i++)
            {

                enemies[i].GetComponent<EnemyController>().TakeDamage(damage);
            }
        }
    }*/
    /*public void Hurt(int _damage)
    {
        anim.SetTrigger("isHurting");
        health -= damage;
        isDazing = true;
        rb.velocity = Vector2.zero;
        Debug.Log("is dazing");

    }*/
    private void Filp()
    {
        facingRight = !facingRight;
        Vector2 scaler = transform.localScale;
        scaler.x *= -1;
        transform.localScale = scaler;
    }
    /*private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPos.position, attackRange);
    }*/




}
