using System;
using System.Collections;
using System.Collections.Generic;
using Inventory;
using UnityEngine;

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

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; set; }
    //animator�ͽ�ɫ״̬���
    public PlayerAnimatorStatesControl PlayerAnimatorStatesControl { get; private set; }
    //��ɫˮƽ�ƶ��Ӽ��ٿ��ƣ���ʼ�趨ֵ������ʱ�������趨Ϊ1�������ϼ��٣������ϼ��٣����м��٣����м��٣�����̬���Ƽ���������
    public CharacterMoveControl PlayerHorizontalMoveControl { get; } 
        = new CharacterMoveControl(1f, 5f, 8f, 8f, 10f);
    
    public bool IsGrounded { get; set; }
    public int CurrentAirExtraJumpCountLeft { get; private set; }
    //������ֵ�����ƶ����ݵĿ���ȫ���������﷽�����
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
        //player��һ֡���°�������animator params����һ֡����state
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

    public void Interact()
    {
        if (PlayerInput.Instance.interact.Down)
        {
            InteractManager.Interact();
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


