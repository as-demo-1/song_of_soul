using System;
using System.Collections;
using System.Collections.Generic;
using Inventory;
using UnityEngine;

[System.Serializable]
public struct PlayerInfo
{
    //move
    public float moveSpeed;
    public float jumpMaxHeight;
    public float jumpMinHeight;
    public float jumpUpSpeed { get; private set; }

    //jump
    public float sprintSpeed;
    public int maxAirExtraJumpCount;

    //climb
    public int gravityScale;
   /* public int climbSpeed;
    public bool isClimb;*/

    public bool playerFacingRight;

    public void init()
    {
        jumpUpSpeed = Mathf.Sqrt(-2*gravityScale*Physics2D.gravity.y*jumpMaxHeight);
    }
}

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; set; }

    public PlayerAnimatorStatesControl PlayerAnimatorStatesControl { get; private set; }

    public CharacterMoveControl PlayerHorizontalMoveControl { get; } 
        = new CharacterMoveControl(1f, 5f, 8f, 8f, 10f);
    
    public bool IsGrounded { get; set; }
    public int CurrentAirExtraJumpCountLeft { get; private set; }

    public PlayerInfo playerInfo;

    private Vector2 m_MoveVector = new Vector2();

    private int m_LastHorizontalInputDir;

    //public SpriteRenderer SpriteRenderer { get; private set; }
    public Animator PlayerAnimator { get; private set; }
    //[SerializeField, HideInInspector]
    public Rigidbody2D RB { get; private set; }

    [SerializeField] private LayerMask groundLayerMask;
    //[SerializeField] private LayerMask ropeLayerMask; 注释理由：可能不再需要攀爬功能

    private CapsuleCollider2D m_BodyCapsuleCollider;
    [SerializeField] private CapsuleCollider2D groundCheckCapsuleCollider;
    //Teleport
    [SerializeField] private GameObject telePosition;
    /// <summary>
    /// Only Demo Code for save
    /// </summary>
    [SerializeField] private string _guid;
    [SerializeField] private SaveSystem _saveSystem;
    [SerializeField] private InventoryManager _backpack;
    public GameObject _itemToAdd = null;
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
        if(_backpack)
        _backpack.LoadSave();
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

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("CollectableItem"))
        {
            Debug.Log("Colide with Item");
            _itemToAdd = other.gameObject;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        _itemToAdd = null;
    }

    public void CheckAddItem()
    {
        if (PlayerInput.Instance.Pick.IsValid)
        {
            if (_itemToAdd)
            {
                _backpack.AddItem(_itemToAdd.GetComponent<SceneItem>().GetItem());
                _itemToAdd.SetActive(false);
            }
        }
    }
    

    void Start()
    {
        playerInfo.init();

        // _saveSystem.TestSaveGuid(_guid);
        RB = GetComponent<Rigidbody2D>();
        RB.gravityScale = playerInfo.gravityScale;
        //RB.sharedMaterial = new PhysicsMaterial2D() { bounciness = 0, friction = 0, name = "NoFrictionNorBounciness" };

        m_BodyCapsuleCollider = GetComponent<CapsuleCollider2D>();
       // SpriteRenderer = GetComponent<SpriteRenderer>();
        PlayerAnimator = GetComponent<Animator>();
        PlayerAnimatorStatesControl = new PlayerAnimatorStatesControl(this, PlayerAnimator, EPlayerState.Idle);
        WhenStartSetLastHorizontalInputDirByFacing();
    }

    private void Update()
    {
        CheckIsGroundedAndResetAirJumpCount();
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
        Interact();
    }

   /* public void VerticalMove()
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
    }*/
    public void JumpStart()
    {
      //  PlayerInput.Instance.jump.SetValidToFalse();
        if (!IsGrounded)
            --CurrentAirExtraJumpCountLeft;
        m_MoveVector.Set(RB.velocity.x, playerInfo.jumpUpSpeed);
        RB.velocity = m_MoveVector;

    }

    public void JumpCheck()
    {
        
    }

    public void ResetJumpCount() => CurrentAirExtraJumpCountLeft = playerInfo.maxAirExtraJumpCount;

    public void CheckHorizontalMove(float setAccelerationNormalizedTime)
    {
        PlayerHorizontalMoveControl.SetAccelerationLeftTimeNormalized(setAccelerationNormalizedTime);
        RecordLastInputDir();
        float desireSpeed = m_LastHorizontalInputDir * playerInfo.moveSpeed;
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
            MovementScript.Sprint(playerInfo.sprintSpeed, transform.position, RB);
        }
    }
    public void Teleport()
    {
        if (PlayerInput.Instance.teleport.Down)
        {
            MovementScript.Teleport(telePosition.transform.position, RB);//Transfer to the specified location
        }
    }

    public void Interact()
    {
        if (PlayerInput.Instance.interact.Down)
        {
            InteractManager.Interact();
        }
    }

    bool IsBlock(LayerMask ignoreMask)
    {
        Vector2 point = (Vector2)groundCheckCapsuleCollider.transform.position + groundCheckCapsuleCollider.offset;
        Collider2D collider = Physics2D.OverlapCapsule(point, groundCheckCapsuleCollider.size, groundCheckCapsuleCollider.direction, 0, ignoreMask);

        return collider != null;
    }

    public void CheckIsGrounded()
    {
        IsGrounded = IsBlock(groundLayerMask);
    }

    public void CheckIsGroundedAndResetAirJumpCount()
    {
        CheckIsGrounded();
        if (IsGrounded)
            ResetJumpCount();
    }

    /*bool IsRope()
    {
        return IsBlock(ropeLayerMask);
    }*/

   /* private void OnClimb()注释理由：可能不再需要攀爬功能
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
    }*/

   /* private void UnClimb()
    {
        // togging isClimb and recovery gravityScale
        if (playerInfo.isClimb)
        {
            RB.gravityScale = playerInfo.gravity;
            playerInfo.isClimb = false;
        }
    }*/

    public void CheckFlipPlayer(float setAccelerationNormalizedTime)
    {
        if (PlayerInput.Instance.horizontal.Value == 1f & !playerInfo.playerFacingRight ||
                PlayerInput.Instance.horizontal.Value == -1f & playerInfo.playerFacingRight)
        {
            MovementScript.Flip(transform, ref playerInfo.playerFacingRight);
            PlayerHorizontalMoveControl.SetAccelerationLeftTimeNormalized(setAccelerationNormalizedTime);
        }
    }

    public void WhenStartSetLastHorizontalInputDirByFacing() => m_LastHorizontalInputDir = playerInfo.playerFacingRight ? 1 : -1;
}


