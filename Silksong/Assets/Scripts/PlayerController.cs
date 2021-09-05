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
    public float speed;
    //sprint
    private bool isRun;
    public float runRate;
    float currSpeed;
    //move
    public Vector2 distance;
    public Vector2 nextDistance;
    private Rigidbody2D rd;
    //Teleport
    public GameObject telePosition;
    // Start is called before the first frame update
    void Start()
    {
        rb = transform.gameObject.GetComponent<Rigidbody2D>();
        currSpeed = speed;
        rd = GetComponent<Rigidbody2D>();
    }


    // Update is called once per frame
    void Update()
    {
        CheckIsRun();
        Jump();
    }

    private void FixedUpdate()
    {
        MoveMent();
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
    void MoveMent()
    {
        if(Input.GetKey(KeyCode.A))
        {
            Debug.Log("左");
            nextDistance = transform.position;
            Vector2 direction = Vector2.left * speed * Time.fixedDeltaTime;
            MovementScript.Move(direction, ref nextDistance);
            rd.position = nextDistance;
        }

        if (Input.GetKey(KeyCode.D))
        {
            Debug.Log("右");
            nextDistance = transform.position;
            Vector2 direction = Vector2.right * speed * Time.fixedDeltaTime;
            MovementScript.Move(direction, ref nextDistance);
            rd.position = nextDistance;
        }

        if(Input.GetKey(KeyCode.LeftShift))
        {
            Debug.Log("冲刺");
            MovementScript.Sprint(100f, transform.position, rd);
        }

        if(Input.GetKey(KeyCode.X))
        {
            Debug.Log("Teleport");
            MovementScript.Teleport(telePosition.transform.position, rd);
        }
    }

}
