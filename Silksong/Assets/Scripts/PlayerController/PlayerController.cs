using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    //move
    [SerializeField] private float speed = 20f;
    private float speedRate=1;// 1为正常速度
    public float SpeedRate
    {
        set { if (value > 0) speedRate = value; }//允许加速
    }

    public float jumpHeight;

    public bool canJump;
    
    [SerializeField, HideInInspector]
    Rigidbody2D rb;
    private PlayerInput PInput;
    //Jump
    [SerializeField] private float sprintForce = 20f;
    [SerializeField] private bool m_secondJump = false;
    //climb
    [SerializeField] private int gravity = 5;
    [SerializeField] private int climbSpeed = 30;

    [SerializeField] private bool m_isClimb;

    [SerializeField] private LayerMask groundLayerMask;
    [SerializeField] private LayerMask ropeLayerMask;

    [SerializeField]private CapsuleCollider2D capsuleCollider;
    private CharacterMoveAccel characterMoveAccel = new CharacterMoveAccel();
    //Teleport
    [SerializeField] private GameObject telePosition;
    /// <summary>
    /// Only Demo Code for save
    /// </summary>
    [SerializeField] private string _guid;
    [SerializeField] private SaveSystem _saveSystem;

    public string GUID => GetComponent<GuidComponent>().GetGuid().ToString();

    private void OnValidate()
    {
        _guid = GUID;
        rb = GetComponent<Rigidbody2D>();
    }
    /// <summary>
    /// Demo code Ends
    /// </summary>

    void Start()
    {
        PInput = GetComponent<PlayerInput>();
        // _saveSystem.TestSaveGuid(_guid);
    }


    // Update is called once per frame
    void Update()
    {
      
    }

    private void FixedUpdate()
    {
        HorizontalMove();
        Jump();
        Sprint();
        Teleport();
        VerticalMove();
    }

    void VerticalMove()
    {
        // check is on rope
        if (IsRope())
        {
            // PInput.Vertical.Value onchange start climbing
            if (PInput.Vertical.Value != 0)
            {
                OnClimb();
            }

            // if jump unClimb
            if (PInput.Jump.Down)
            {
                UnClimb();
            }
        }
        // unClimb
        else
        {
            UnClimb();
        }
    }
    void Jump()
    {
        bool ground = IsGround();
        //Debug.Log(ground);
        if (PInput.Jump.Down &&canJump)
        {
           // Debug.Log(ground.ToString());
            if (ground)
            {
                m_secondJump = false;

                rb.velocity = new Vector3(0, jumpHeight, 0);

            }else if (!m_secondJump)
            {
                m_secondJump = true;
                rb.velocity = new Vector3(0, jumpHeight, 0);
            }

        }
    }
    void HorizontalMove()
    {
        float desirespeed = PInput.Horizontal.Value * speed* speedRate * Time.deltaTime;
        float acce = characterMoveAccel.AccelSpeedUpdate(PInput.Horizontal.Value !=0,IsGround(),desirespeed);
        rb.position = new Vector2(rb.position.x + acce, rb.position.y);
    }
    void Sprint()
    {
        if (PInput.Sprint.Down)
        {
            MovementScript.Sprint(sprintForce, transform.position, rb);
        }
    }
    void Teleport()
    {
        if (PInput.Teleport.Down)
        {
            MovementScript.Teleport(telePosition.transform.position, rb);//Transfer to the specified location
        }
    }

    // 1 << 6 is ground    7 is rope    8 is player
    bool IsBlock(LayerMask ignoreMask)
    {
        Vector2 point = (Vector2)capsuleCollider.transform.position + capsuleCollider.offset;
        Collider2D collider = Physics2D.OverlapCapsule(point, capsuleCollider.size, capsuleCollider.direction, 0, ignoreMask);

        return collider != null;
    }

    bool IsGround()
    {
        // Vector2 point = (Vector2)capsuleCollider.transform.position + capsuleCollider.offset;
        // LayerMask ignoreMask = ~(1 << 8 | 1 << 7); // fixed ignore ropeLayer
        // Collider2D collider = Physics2D.OverlapCapsule(point, capsuleCollider.size, capsuleCollider.direction, 0,ignoreMask);
        // return collider != null;
        return IsBlock(groundLayerMask);
    }

    bool IsRope()
    {
        return IsBlock(ropeLayerMask);
    }

    private void OnClimb()
    {
        // velocity is rb current force
        rb.velocity = Vector3.zero;

        // if isClimb rb.pos is PInput.Vertical.Value
        if (m_isClimb)
        {
            Vector2 pos = transform.position;
            pos.y += climbSpeed * PInput.Vertical.Value * Time.deltaTime;
            rb.MovePosition(pos);
        }
        // togging isClimb and gravityScale is 0
        else
        {
            rb.gravityScale = 0;
            m_isClimb = true;
        }
    }

    private void UnClimb()
    {
        // togging isClimb and recovery gravityScale
        if (m_isClimb)
        {
            rb.gravityScale = gravity;
            m_isClimb = false;
        }
    }
}
