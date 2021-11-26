using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; set; }
    //animator和角色状态相关
    public PlayerAnimatorStatesControl PlayerAnimatorStatesControl { get; private set; }
    //角色水平移动加减速控制，初始设定值：（总时间总是设定为1，地面上加速，地面上减速，空中加速，空中减速），动态控制见类中属性
    public CharacterMoveControl PlayerHorizontalMoveControl { get; } 
        = new CharacterMoveControl(1f, 5f, 8f, 8f, 10f);

    public bool IsGrounded { get; set; }
    public int CurrentAirExtraJumpCountLeft { get; private set; }
    //基础数值，能移动数据的可以全部移至这里方便管理
    public PlayerInfo playerInfo;

    private Vector2 m_MoveVector = new Vector2();
    private int m_LastHorizontalInputDir;

    public SpriteRenderer SpriteRenderer { get; private set; }
    public Animator PlayerAnimator { get; private set; }
    //[SerializeField, HideInInspector]
    public Rigidbody2D RB { get; private set; }

    [SerializeField] private LayerMask groundLayerMask;
    [SerializeField] private LayerMask ropeLayerMask;

    private CapsuleCollider2D m_BodyCapsuleCollider;
    [SerializeField] private CapsuleCollider2D groundCheckCapsuleCollider;
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
    }
    /// <summary>
    /// Demo code Ends
    /// </summary>

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            throw new UnityException("There cannot be more than one PlayerController script.  The instances are " + Instance.name + " and " + name + ".");
        DontDestroyOnLoad(this.gameObject);
    }

    private void OnEnable()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            throw new UnityException("There cannot be more than one PlayerController script.  The instances are " + Instance.name + " and " + name + ".");
    }

    private void OnDisable()
    {
        Instance = null;
    }

    void Start()
    {
        // _saveSystem.TestSaveGuid(_guid);
        RB = GetComponent<Rigidbody2D>();
        //RB.sharedMaterial = new PhysicsMaterial2D() { bounciness = 0, friction = 0, name = "NoFrictionNorBounciness" };
        m_BodyCapsuleCollider = GetComponent<CapsuleCollider2D>();
        SpriteRenderer = GetComponent<SpriteRenderer>();
        PlayerAnimator = GetComponent<Animator>();
        PlayerAnimatorStatesControl = new PlayerAnimatorStatesControl(this, PlayerAnimator, EPlayerState.Idle);
        WhenStartSetLastHorizontalInputDirByFacing();
    }

    private void Update()
    {
        PlayerAnimatorStatesControl.PlayerStatusUpdate();
        PlayerAnimatorStatesControl.ParamsUpdate();
    }

    private void LateUpdate()
    {
        PlayerAnimatorStatesControl.BehaviourLateUpdate();
    }

    private void FixedUpdate()
    {
        //HorizontalMove();
        //Jump();
        //Sprint();
        //Teleport();
        //VerticalMove();
    }

    public void VerticalMove()
    {
        // check is on rope
        if (IsRope())
        {
            // PInput.Vertical.Value onchange start climbing
            if (PlayerInput.Instance.vertical.Value != 0)
            {
                OnClimb();
            }

            // if jump unClimb
            if (PlayerInput.Instance.jump.Down)
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
    public void CheckJump()
    {
        //bool ground = IsGround();
        //Debug.Log(ground);
        //if (PlayerInput.Instance.jump.Down)
        //{
        //    Debug.Log(ground.ToString());
        //    if (ground)
        //    {
        //        m_secondJump = false;

        //        rb.velocity = new Vector3(RB.velocity.x, jumpHeight, 0);
        //        Debug.Log("jump");
        //    }else if (!m_secondJump)
        //    {
        //        m_secondJump = true;
        //        rb.velocity = new Vector3(RB.velocity.x, jumpHeight, 0);
        //        print("second jump");
        //    }

        //}
        if (CurrentAirExtraJumpCountLeft > 0 || IsGrounded)
        {
            if (PlayerInput.Instance.jump.IsValid)
            {
                PlayerInput.Instance.jump.SetValidToFalse();
                if (!IsGrounded)
                    --CurrentAirExtraJumpCountLeft;
                m_MoveVector.Set(RB.velocity.x, playerInfo.jumpHeight);
                RB.velocity = m_MoveVector;
            }
        }
    }

    public void ResetJumpCount() => CurrentAirExtraJumpCountLeft = playerInfo.maxAirExtraJumpCount;

    public void CheckHorizontalMove(float setAccelerationNormalizedTime)
    {
        PlayerHorizontalMoveControl.SetAccelerationLeftTimeNormalized(setAccelerationNormalizedTime);
        RecordLastInputDir();
        float desireSpeed = m_LastHorizontalInputDir * playerInfo.speed;
        float acce = PlayerHorizontalMoveControl.AccelSpeedUpdate(PlayerInput.Instance.horizontal.Value != 0, IsGrounded, desireSpeed);
        m_MoveVector.Set(acce, RB.velocity.y);
        RB.velocity = m_MoveVector;

        void RecordLastInputDir()
        {
            if (PlayerInput.Instance.horizontal.Value == 1)
                m_LastHorizontalInputDir = 1;
            else if (PlayerInput.Instance.horizontal.Value == -1)
                m_LastHorizontalInputDir = -1;
        }
    }

    public void Sprint()
    {
        if (PlayerInput.Instance.sprint.Down)
        {
            MovementScript.Sprint(playerInfo.sprintForce, transform.position, RB);
        }
    }
    public void Teleport()
    {
        if (PlayerInput.Instance.teleport.Down)
        {
            MovementScript.Teleport(telePosition.transform.position, RB);//Transfer to the specified location
        }
    }

    // 1 << 6 is ground    7 is rope    8 is player
    bool IsBlock(LayerMask ignoreMask)
    {
        Vector2 point = (Vector2)groundCheckCapsuleCollider.transform.position + groundCheckCapsuleCollider.offset;
        Collider2D collider = Physics2D.OverlapCapsule(point, groundCheckCapsuleCollider.size, groundCheckCapsuleCollider.direction, 0, ignoreMask);

        return collider != null;
    }

    public void CheckIsGrounded()
    {
        // Vector2 point = (Vector2)capsuleCollider.transform.position + capsuleCollider.offset;
        // LayerMask ignoreMask = ~(1 << 8 | 1 << 7); // fixed ignore ropeLayer
        // Collider2D collider = Physics2D.OverlapCapsule(point, capsuleCollider.size, capsuleCollider.direction, 0,ignoreMask);
        // return collider != null;
        IsGrounded = IsBlock(groundLayerMask);
    }

    public void CheckIsGroundedAndResetAirJumpCount()
    {
        CheckIsGrounded();
        if (IsGrounded)
            ResetJumpCount();
    }

    bool IsRope()
    {
        return IsBlock(ropeLayerMask);
    }

    private void OnClimb()
    {
        // velocity is rb current force
        RB.velocity = Vector3.zero;

        // if isClimb rb.pos is PInput.Vertical.Value
        if (playerInfo.isClimb)
        {
            Vector2 pos = transform.position;
            pos.y += playerInfo.climbSpeed * PlayerInput.Instance.vertical.Value * Time.deltaTime;
            RB.MovePosition(pos);
        }
        // togging isClimb and gravityScale is 0
        else
        {
            RB.gravityScale = 0;
            playerInfo.isClimb = true;
        }
    }

    private void UnClimb()
    {
        // togging isClimb and recovery gravityScale
        if (playerInfo.isClimb)
        {
            RB.gravityScale = playerInfo.gravity;
            playerInfo.isClimb = false;
        }
    }

    public void CheckFlipPlayer(float setAccelerationNormalizedTime)
    {
        if (PlayerInput.Instance.horizontal.Value == 1f & !playerInfo.playerFacingRight ||
                PlayerInput.Instance.horizontal.Value == -1f & playerInfo.playerFacingRight)
        {
            MovementScript.Flip(SpriteRenderer, ref playerInfo.playerFacingRight, m_BodyCapsuleCollider, groundCheckCapsuleCollider);
            PlayerHorizontalMoveControl.SetAccelerationLeftTimeNormalized(setAccelerationNormalizedTime);
        }
    }

    public void WhenStartSetLastHorizontalInputDirByFacing() => m_LastHorizontalInputDir = playerInfo.playerFacingRight ? 1 : -1;
}

[System.Serializable]
public struct PlayerInfo
{
    //move
    public float speed;
    public float jumpHeight;

    //jump
    public float sprintForce;
    public int maxAirExtraJumpCount;

    //climb
    public int gravity;
    public int climbSpeed;
    public bool isClimb;

    public bool playerFacingRight;
}
