using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    //move
    public float speed;
    private Rigidbody2D rb;
    private int filp;

    //Jump
    public float jumpForce;

    //run
    private bool isRun = false;
    public float runRate;
    float currSpeed;



    // Start is called before the first frame update
    void Start()
    {
        rb = transform.gameObject.GetComponent<Rigidbody2D>();
        currSpeed = speed;

    }


    // Update is called once per frame
    void Update()
    {
        CheckIsRun();
        Jump();
    }

    private void FixedUpdate()
    {
        Move();
        
    }

    void Move()
    {
        float H = Input.GetAxis("Horizontal");
        if (H != 0)
        {
            float playerSpeed;
            if (!isRun)
            {
                playerSpeed = H * speed;
            }
            else
            {
                playerSpeed = H * speed * runRate;
            }
            rb.velocity = new Vector2(playerSpeed, transform.position.y);
        }else{
            return;
        }

        filp = H < 0 ? -1 : 1;
        transform.localScale = new Vector3(filp, transform.localScale.y, transform.localScale.z);

    }
    /// <summary>
    /// //冲刺判断
    /// </summary>
    void CheckIsRun()
    {

        if (Input.GetKey(KeyCode.LeftShift))
        {
            isRun = true;
        }
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            isRun = false;
            speed = currSpeed;
        }
    }

    void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            
        rb.AddForce(new Vector2(0f,jumpForce));
        print("jump");
        }
    }

}
