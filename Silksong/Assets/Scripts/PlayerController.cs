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

    private Transform m_ropeCheck;
    private LayerMask m_ropeLayer;

    private CapsuleCollider2D capsuleCollider;
    CharacterMoveAccel characterMoveAccel;
    //Teleport
    public GameObject telePosition;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
        PInput = GetComponent<PlayerInput>();
        characterMoveAccel = new CharacterMoveAccel();
        m_ropeLayer = LayerMask.GetMask("Rope");
        m_ropeCheck = transform.Find("RopeCheck");
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
        // 判断是否在绳子上
        if (Physics2D.OverlapCircle(m_ropeCheck.position, ConfigCheckRadius, m_ropeLayer))
        {
            // 按下上键开始攀爬
            if (PInput.Vertical.Value > 0)
            {
                OnClimb();
            }

            // 下键或跳跃则取消攀爬
            if (PInput.Vertical.Value < 0)
            {
                OnUnclimb();
            }
        }
        // 否则取消攀爬
        else
        {
            OnUnclimb();
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

    bool IsGround()
    {
        Vector2 point = (Vector2)capsuleCollider.transform.position + capsuleCollider.offset;
        LayerMask ignoreMask = ~(1 << 8);
        Collider2D collider = Physics2D.OverlapCapsule(point, capsuleCollider.size, capsuleCollider.direction, 0,ignoreMask);
        return collider != null;
    }

    private void OnClimb()
    {
        // 攀爬时取消角色受力
        rb.velocity = Vector3.zero;

        // 如果攀爬中，则位置匀速上移
        if (m_isClimb)
        {
            Vector2 pos = transform.position;
            pos += (ConfigClimbSpeed * Vector2.up * Time.deltaTime);
            rb.MovePosition(pos);
        }
        // 切换到攀爬状态 将刚体重力置为0
        else
        {
            rb.gravityScale = 0;
            m_isClimb = true;
        }
    }

    private void OnUnclimb()
    {
        // 取消攀爬状态 恢复重力
        if (m_isClimb)
        {
            rb.gravityScale = ConfigGravity;
            m_isClimb = false;
        }
    }

}
