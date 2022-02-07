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
    public float maxFallSpeed;
    public float jumpUpSpeed { get; private set; }

    //jump
    public float sprintDistance;
    public float sprintSpeed { get; private set; }
    public int maxAirSprintCount;
    public int maxJumpCount;

    //climb
    public float normalGravityScale;
   /* public int climbSpeed;
    public bool isClimb;*/

    public bool playerFacingRight;

    public void init()
    {

        jumpUpSpeed = jumpMaxHeight * 2.5f;
        sprintSpeed = sprintDistance / Constants.SprintTime;
        //Debug.Log(jumpUpSpeed);

    }
}

[RequireComponent(typeof(Rigidbody))]
//[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; set; }
    public PlayerAnimatorStatesControl PlayerAnimatorStatesControl { get; private set; }

    public PlayerAnimatorParamsMapping animatorParamsMapping;

    public PlayerStatesBehaviour PlayerStatesBehaviour;
    public CharacterMoveControl PlayerHorizontalMoveControl { get; } 
        = new CharacterMoveControl(1f, 5f, 8f, 8f, 10f);

    public PlayerInfo playerInfo;

    public int lastHorizontalInputDir;

    private int m_LastHorizontalInputDir;

    public SpriteRenderer SpriteRenderer { get; private set; }
    public Animator PlayerAnimator { get; private set; }
    //[SerializeField, HideInInspector]
    public Rigidbody2D RB { get; private set; }
    public Transform m_Transform { get; set; }
    [SerializeField] private LayerMask groundLayerMask;
    //[SerializeField] private LayerMask ropeLayerMask; 注释理由：可能不再需要攀爬功能

    private PlayerGroundedCheck playerGroundedCheck;
    private CapsuleCollider2D m_BodyCapsuleCollider;
    [SerializeField] private Collider2D groundCheckCollider;
    //Teleport
    [SerializeField] private GameObject telePosition;
    /// <summary>
    /// Only Demo Code for save
    /// </summary>
    [SerializeField] private string _guid;
    [SerializeField] private SaveSystem _saveSystem;
    [SerializeField] private InventoryManager _backpack;
    public GameObject _itemToAdd = null;
    public GameObject _savePoint = null;
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
        if (_backpack)
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
        if (other.gameObject.CompareTag("SavePoint"))
        {
            Debug.Log("Colide with SavePoint");
            _savePoint = other.gameObject;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        _itemToAdd = null;
        _savePoint = null;
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
    public void CheckSavePoint()
    {
        if (PlayerInput.Instance.Pick.IsValid)
        {
            if (_savePoint)
            {
                _saveSystem.SaveDataToDisk();
            }
        }
    }
    
    void Start()
    {
        playerInfo.init();

        // _saveSystem.TestSaveGuid(_guid);
        RB = GetComponent<Rigidbody2D>();
        RB.gravityScale = playerInfo.normalGravityScale;

        PlayerAnimatorStatesControl = new PlayerAnimatorStatesControl(this, PlayerAnimator, EPlayerState.Idle);
        playerGroundedCheck = new PlayerGroundedCheck(this);
        animatorParamsMapping = PlayerAnimatorStatesControl.CharacterAnimatorParamsMapping;
        PlayerStatesBehaviour = PlayerAnimatorStatesControl.CharacterStatesBehaviour;
        WhenStartSetLastHorizontalInputDirByFacing();
    }

    private void Update()
    {
        CheckIsGrounded();

        PlayerAnimatorStatesControl.ParamsUpdate();
    }

    private void LateUpdate()
    {
        PlayerAnimatorStatesControl.BehaviourLateUpdate();
    }

    private void FixedUpdate()
    {
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

    public void CheckHorizontalMove(float setAccelerationNormalizedTime)
    {
        PlayerHorizontalMoveControl.SetAccelerationLeftTimeNormalized(setAccelerationNormalizedTime);
        RecordLastInputDir();

        float desireSpeed = lastHorizontalInputDir * playerInfo.moveSpeed;
        float acce = PlayerHorizontalMoveControl.AccelSpeedUpdate(PlayerInput.Instance.horizontal.Value != 0,playerGroundedCheck.IsGroundedBuffer, desireSpeed);
        RB.velocity = new Vector2(acce, RB.velocity.y);

        void RecordLastInputDir()
        {
            if (PlayerInput.Instance.horizontal.Value == 1)
                lastHorizontalInputDir = 1;
            else if (PlayerInput.Instance.horizontal.Value == -1)
                lastHorizontalInputDir = -1;
        }
    }

    
  /*  public void Teleport()
    {
        if (PlayerInput.Instance.teleport.Down)
        {
            MovementScript.Teleport(telePosition.transform.position, RB);//Transfer to the specified location
        }
    }*/

    public void Interact()
    {
        if (PlayerInput.Instance.interact.Down)
        {
            InteractManager.Interact();
        }
    }

    public void CheckIsGrounded()
    {
        playerGroundedCheck.IsGrounded = groundCheckCollider.IsTouchingLayers(groundLayerMask);
    }

    public bool isGroundedBuffer()
    {
        return playerGroundedCheck.IsGroundedBuffer;
    }

  /*  public void CheckIsGroundedAndResetAirJumpCount()
    {
        CheckIsGrounded();
        if (IsGrounded)
            ResetJumpCount();
    }*/

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
    public void SwimUnderWater()
    {
        if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.A))
        {
            m_Transform.localRotation = Quaternion.Euler(0, 0, 45);
            RB.velocity = new Vector2(-1, 1).normalized * playerInfo.speed;
        }
        else if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.D))
        {
            m_Transform.localRotation = Quaternion.Euler(0, 0, -45);
            RB.velocity = new Vector3(1, 1).normalized * playerInfo.speed;
        }
        else if (Input.GetKey(KeyCode.S) && Input.GetKey(KeyCode.A))
        {
            m_Transform.localRotation = Quaternion.Euler(0, 0, 135);
            RB.velocity = new Vector2(-1, -1).normalized * playerInfo.speed;
        }
        else if (Input.GetKey(KeyCode.S) && Input.GetKey(KeyCode.D))
        {
            m_Transform.localRotation = Quaternion.Euler(0, 0, -135);
            RB.velocity = new Vector2(1, -1).normalized * playerInfo.speed;
        }
        else
        {
            if (Input.GetKey(KeyCode.W))
            {
                m_Transform.localRotation = Quaternion.Euler(0, 0, 0);
                RB.velocity = new Vector2(0, 1) * playerInfo.speed;
            }
            if (Input.GetKey(KeyCode.S))
            {
                m_Transform.localRotation = Quaternion.Euler(0, 0, 180);
                RB.velocity = new Vector2(0, -1) * playerInfo.speed;
            }
            if (Input.GetKey(KeyCode.A))
            {
                m_Transform.localRotation = Quaternion.Euler(0, 0, 90);
                RB.velocity = new Vector2(-1, 0) * playerInfo.speed;
            }
            if (Input.GetKey(KeyCode.D))
            {
                m_Transform.localRotation = Quaternion.Euler(0, 0, -90);
                RB.velocity = new Vector2(1, 0) * playerInfo.speed;
            }
        }
    }

    public void CheckFlipPlayer(float setAccelerationNormalizedTime)
    {
        if (PlayerInput.Instance.horizontal.Value == 1f & !playerInfo.playerFacingRight ||
                PlayerInput.Instance.horizontal.Value == -1f & playerInfo.playerFacingRight)
        {
            MovementScript.Flip(transform, ref playerInfo.playerFacingRight);
            PlayerHorizontalMoveControl.SetAccelerationLeftTimeNormalized(setAccelerationNormalizedTime);
        }
    }

    public void WhenStartSetLastHorizontalInputDirByFacing() => lastHorizontalInputDir = playerInfo.playerFacingRight ? 1 : -1;
}

public class PlayerGroundedCheck
{
    private bool isGrounded;
    private PlayerController playerController;
    private int bufferTimer;
    private bool isGroundedBuffer;

    public PlayerGroundedCheck(PlayerController playerController)
    {
        this.playerController = playerController;
    }
    public bool IsGrounded
    {
        get
        {
            return isGrounded;
        }
        set//每次update都会调用
        {
            if (value)//设为真
            {
                playerController.PlayerStatesBehaviour.playerJump.resetJumpCount();
                playerController.PlayerStatesBehaviour.playerSprint.resetAirSprintLeftCount();
                bufferTimer = Constants.IsGroundedBufferFrame;
            }

            if (bufferTimer>0)
            {
                bufferTimer--;
                IsGroundedBuffer = true;
            }
            else
            {
                IsGroundedBuffer = false;
            }

            isGrounded = value;
        }
    }

    public bool IsGroundedBuffer
    {
        get {return isGroundedBuffer; }
        set
        {      
            if (isGroundedBuffer &&!value)//从真设为假
            {
                playerController.PlayerStatesBehaviour.playerJump.CurrentJumpCountLeft--;
            }
            isGroundedBuffer = value;
        }
    
    }

}



