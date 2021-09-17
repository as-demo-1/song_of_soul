using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    //move
    public float speed = 20f;
    private Rigidbody2D rb;
    private PlayerInput PInput;
    //Jump
    public float jumpForce = 20f;
    public float sprintForce = 20f;
    private bool m_secondJump = false;
    //climb
    public int ConfigGravity = 5;
    public int ConfigClimbSpeed = 30;
    public float ConfigCheckRadius = 0.3f;

    private bool m_isClimb;

    private CapsuleCollider2D capsuleCollider;
    CharacterMoveAccel characterMoveAccel;
    //Teleport
    public GameObject telePosition;
    /// <summary>
    /// Only Demo Code for save
    /// </summary>
    [SerializeField] private string _guid;
    [SerializeField] private SaveSystem _saveSystem;

    public string GUID => GetComponent<GuidComponent>().GetGuid().ToString();

    private void OnValidate()
    {
        _guid = GUID;
    }
    /// <summary>
    /// Demo code Ends
    /// </summary>

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
        PInput = GetComponent<PlayerInput>();
        characterMoveAccel = new CharacterMoveAccel();
        _saveSystem.TestSaveGuid(_guid);
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
        if (PInput.Jump.Down)
        {
            if (ground)
            {
                m_secondJump = false;
                rb.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);
                print("jump");
            }else if (!m_secondJump)
            {
                m_secondJump = true;
                rb.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);
                print("second jump");
            }

        }
    }
    void HorizontalMove()
    {
        float desirespeed = PInput.Horizontal.Value * speed * Time.deltaTime;
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

    // 1 << 6 is ground 7 is rope 8 is player
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
        return IsBlock(~(1 << 8 | 1 << 7));
    }

    bool IsRope()
    {
        return IsBlock(~(1 << 8 | 1 << 6));
    }

    private void OnClimb()
    {
        // velocity is rb current force
        rb.velocity = Vector3.zero;

        // if isClimb rb.pos is PInput.Vertical.Value
        if (m_isClimb)
        {
            Vector2 pos = transform.position;
            pos.y += ConfigClimbSpeed * PInput.Vertical.Value * Time.deltaTime;
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
            rb.gravityScale = ConfigGravity;
            m_isClimb = false;
        }
    }
}
