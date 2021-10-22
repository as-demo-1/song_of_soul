using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; set; }
    public PlayerAnimatorStatesManager PlayerAnimatorStatesManager { get; private set; }
    public bool IsGrounded => CheckIsGrounded();
    public bool playerFacingRight = false;
    public SpriteRenderer SpriteRenderer { get; private set; }
    //move
    [SerializeField] private float speed;
    public float jumpHeight;

    private Vector2 m_MoveVector = new Vector2();

    [SerializeField, HideInInspector]
    public Rigidbody2D RB { get; private set; }
    //Jump
    [SerializeField] private float sprintForce;
    [SerializeField] private int maxJumpCount;
    private int m_CurrentJumpCountLeft = 1;
    //[SerializeField] private bool m_secondJump = false;
    //climb
    [SerializeField] private int gravity;
    [SerializeField] private int climbSpeed;

    [SerializeField] private bool m_isClimb;

    [SerializeField] private LayerMask groundLayerMask;
    [SerializeField] private LayerMask robeLayerMask;

    [SerializeField] private CapsuleCollider2D capsuleCollider;
    public CharacterMoveAccel CharacterMoveAccel { get; } = new CharacterMoveAccel(1f, 50f, 500f, 80f, 500f);
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
        RB = GetComponent<Rigidbody2D>();
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
        SpriteRenderer = GetComponent<SpriteRenderer>();
        PlayerAnimatorStatesManager = new PlayerAnimatorStatesManager(GetComponent<Animator>(), PlayerStatus.Idle);
    }


    // Update is called once per frame
    void Update()
    {
        PlayerAnimatorStatesManager.ParamsUpdate();
        PlayerAnimatorStatesManager.BehaviourUpdate();
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
    public void Jump()
    {
        //bool ground = IsGround();
        //Debug.Log(ground);
        //if (PlayerInput.Instance.jump.Down)
        //{
        //    Debug.Log(ground.ToString());
        //    if (ground)
        //    {
        //        m_secondJump = false;

        //        rb.velocity = new Vector3(0, jumpHeight, 0);
        //        Debug.Log("jump");
        //    }else if (!m_secondJump)
        //    {
        //        m_secondJump = true;
        //        rb.velocity = new Vector3(0, jumpHeight, 0);
        //        print("second jump");
        //    }

        //}
        if (m_CurrentJumpCountLeft > 0)
        {
            m_MoveVector.Set(0, jumpHeight);
            RB.velocity = m_MoveVector;
            m_CurrentJumpCountLeft--;
        }
    }

    public void ResetJumpCount() => m_CurrentJumpCountLeft = maxJumpCount;

    public void HorizontalMove()
    {
        float desireSpeed = PlayerInput.Instance.horizontal.Value * speed * Time.deltaTime;
        float acce = CharacterMoveAccel.AccelSpeedUpdate(PlayerInput.Instance.horizontal.Value != 0, CheckIsGrounded(), desireSpeed);
        m_MoveVector.Set(RB.velocity.x + acce, RB.velocity.y);
        RB.velocity = m_MoveVector;
    }
    public void Sprint()
    {
        if (PlayerInput.Instance.sprint.Down)
        {
            MovementScript.Sprint(sprintForce, transform.position, RB);
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
        Vector2 point = (Vector2)capsuleCollider.transform.position + capsuleCollider.offset;
        Collider2D collider = Physics2D.OverlapCapsule(point, capsuleCollider.size, capsuleCollider.direction, 0, ignoreMask);

        return collider != null;
    }

    public bool CheckIsGrounded()
    {
        // Vector2 point = (Vector2)capsuleCollider.transform.position + capsuleCollider.offset;
        // LayerMask ignoreMask = ~(1 << 8 | 1 << 7); // fixed ignore ropeLayer
        // Collider2D collider = Physics2D.OverlapCapsule(point, capsuleCollider.size, capsuleCollider.direction, 0,ignoreMask);
        // return collider != null;
        return IsBlock(groundLayerMask);
    }


    bool IsRope()
    {
        return IsBlock(robeLayerMask);
    }

    private void OnClimb()
    {
        // velocity is rb current force
        RB.velocity = Vector3.zero;

        // if isClimb rb.pos is PInput.Vertical.Value
        if (m_isClimb)
        {
            Vector2 pos = transform.position;
            pos.y += climbSpeed * PlayerInput.Instance.vertical.Value * Time.deltaTime;
            RB.MovePosition(pos);
        }
        // togging isClimb and gravityScale is 0
        else
        {
            RB.gravityScale = 0;
            m_isClimb = true;
        }
    }

    private void UnClimb()
    {
        // togging isClimb and recovery gravityScale
        if (m_isClimb)
        {
            RB.gravityScale = gravity;
            m_isClimb = false;
        }
    }
}
