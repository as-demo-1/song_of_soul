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

    public float breakMoonAvgSpeed;
    public AnimationCurve breakMoonPositionCurve;
    //climb
    public float normalGravityScale;

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
    public PlayerAnimatorStatesControl playerAnimatorStatesControl { get; private set; }

    public PlayerAnimatorParamsMapping animatorParamsMapping;

    public PlayerStatesBehaviour playerStatesBehaviour;

    public PlayerStatusDic playerStatusDic;

    public PlayerCharacter playerCharacter;
    public CharacterMoveControl PlayerHorizontalMoveControl { get; } 
        = new CharacterMoveControl(1f, 5f, 8f, 8f, 10f);

    public PlayerInfo playerInfo;

    private int lastHorizontalInputDir;

    public Animator PlayerAnimator;

    private Rigidbody2D RB;//外部访问刚体时，应通过setRigidGravityScale等封装后的方法

    public SpriteRenderer SpriteRenderer { get; private set; }
    //[SerializeField, HideInInspector]
    public Transform m_Transform { get; set; }
    [SerializeField] private LayerMask groundLayerMask;
    //[SerializeField] private LayerMask ropeLayerMask; ע�����ɣ����ܲ�����Ҫ��������


    private PlayerGroundedCheck playerGroundedCheck;

    [DisplayOnly]
    public bool gravityLock;//为ture时，不允许gravityScale改变
    private bool IsUnderWater;

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
        if(_backpack)
            _backpack.LoadSave();

        init();
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
        if(other.gameObject.CompareTag("UnderWater"))
        {
            IsUnderWater = true;
            //入水时慢慢将速度减为0    
            float smooth = 100f;
            //float exitWaterTime = Time.time;
            //RB.velocity = Vector2.Lerp(RB.velocity, new Vector2(RB.velocity.x, 0), (Time.time - exitWaterTime) * smooth);
            RB.gravityScale = playerInfo.normalGravityScale / 5;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        _itemToAdd = null;
        _savePoint = null;
        if (other.gameObject.CompareTag("UnderWater"))
        {
            IsUnderWater = false;
            RB.gravityScale = playerInfo.normalGravityScale;
            RB.velocity += new Vector2(0, 5);       //添加一个出水速度
        }
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
    
    public void init()
    {
        RB = GetComponent<Rigidbody2D>();
        playerCharacter = GetComponent<PlayerCharacter>();
        playerGroundedCheck = new PlayerGroundedCheck(this);

        playerAnimatorStatesControl = new PlayerAnimatorStatesControl(this, PlayerAnimator, EPlayerState.Idle);
        animatorParamsMapping = playerAnimatorStatesControl.CharacterAnimatorParamsMapping;
        playerStatesBehaviour = playerAnimatorStatesControl.CharacterStatesBehaviour;
        playerStatusDic = playerAnimatorStatesControl.PlayerStatusDic;

    }
    void Start()
    {
        playerInfo.init();
        // _saveSystem.TestSaveGuid(_guid);
        RB.gravityScale = playerInfo.normalGravityScale;
        m_Transform = GetComponent<Transform>();
        playerAnimatorStatesControl = new PlayerAnimatorStatesControl(this, PlayerAnimator, EPlayerState.Idle);
        playerGroundedCheck = new PlayerGroundedCheck(this);
        animatorParamsMapping = playerAnimatorStatesControl.CharacterAnimatorParamsMapping;
        playerStatesBehaviour = playerAnimatorStatesControl.CharacterStatesBehaviour;
        WhenStartSetLastHorizontalInputDirByFacing();

        HpDamable damable = GetComponent<HpDamable>();
        damable.takeDamageEvent.AddListener(getHurt);
        damable.onDieEvent.AddListener(die);
    }

    private void Update()
    {
        CheckIsGrounded();

        playerAnimatorStatesControl.ParamsUpdate();
        if(IsUnderWater)
        {
            SwimUnderWater();
        }
        
    }

    private void LateUpdate()
    {
        playerAnimatorStatesControl.BehaviourLateUpdate();
    }

    private void FixedUpdate()
    {
        Interact();
    }

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

    public void CheckFlipPlayer(float setAccelerationNormalizedTime)
    {
        if (PlayerInput.Instance.horizontal.Value == 1f & !playerInfo.playerFacingRight ||
                PlayerInput.Instance.horizontal.Value == -1f & playerInfo.playerFacingRight)
        {
            Flip();
            PlayerHorizontalMoveControl.SetAccelerationLeftTimeNormalized(setAccelerationNormalizedTime);
        }
    }

    public void setRigidVelocity(Vector2 newVelocity)
    {
        RB.velocity = newVelocity;
    }

    public Vector2 getRigidVelocity()
    {
        return RB.velocity;
    }

    public void setRigidGravityScale(float newScale)
    {
        if(gravityLock==false)
        RB.gravityScale = newScale;
    }

    public void setRigidGravityScaleToNormal()
    {
        setRigidGravityScale(playerInfo.normalGravityScale);
    }

    public void rigidMovePosition(Vector2 target)
    {
        RB.MovePosition(target);
    }
    public void WhenStartSetLastHorizontalInputDirByFacing() => lastHorizontalInputDir = playerInfo.playerFacingRight ? 1 : -1;

    public void Flip()
    {
        playerInfo.playerFacingRight = !playerInfo.playerFacingRight;
        Vector3 t = transform.localScale;
        transform.localScale = new Vector3(-t.x, t.y, t.z);
        playerStatesBehaviour.playerBreakMoon.findCurrentTarget();
    }

    public void getHurt(DamagerBase damager,DamageableBase damable)
    {
        PlayerAnimator.SetTrigger(animatorParamsMapping.HurtParamHas);
    }

    public void die(DamagerBase damager, DamageableBase damable)
    {
        PlayerAnimator.SetBool(animatorParamsMapping.DeadParamHas,true);
    }

    public void SwimUnderWater()
    {
        if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.A))
        {
            m_Transform.localRotation = Quaternion.Euler(0, 0, 45);
            RB.velocity = new Vector2(-1, 1).normalized * playerInfo.moveSpeed;
        }
        else if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.D))
        {
            m_Transform.localRotation = Quaternion.Euler(0, 0, -45);
            RB.velocity = new Vector3(1, 1).normalized * playerInfo.moveSpeed;
        }
        else if (Input.GetKey(KeyCode.S) && Input.GetKey(KeyCode.A))
        {
            m_Transform.localRotation = Quaternion.Euler(0, 0, 135);
            RB.velocity = new Vector2(-1, -1).normalized * playerInfo.moveSpeed;
        }
        else if (Input.GetKey(KeyCode.S) && Input.GetKey(KeyCode.D))
        {
            m_Transform.localRotation = Quaternion.Euler(0, 0, -135);
            RB.velocity = new Vector2(1, -1).normalized * playerInfo.moveSpeed;
        }
        else
        {
            if (Input.GetKey(KeyCode.W))
            {
                m_Transform.localRotation = Quaternion.Euler(0, 0, 0);
                RB.velocity = new Vector2(0, 1) * playerInfo.moveSpeed;
            }
            if (Input.GetKey(KeyCode.S))
            {
                m_Transform.localRotation = Quaternion.Euler(0, 0, 180);
                RB.velocity = new Vector2(0, -1) * playerInfo.moveSpeed;
            }
            if (Input.GetKey(KeyCode.A))
            {
                m_Transform.localRotation = Quaternion.Euler(0, 0, 90);
                RB.velocity = new Vector2(-1, 0) * playerInfo.moveSpeed;
            }
            if (Input.GetKey(KeyCode.D))
            {
                m_Transform.localRotation = Quaternion.Euler(0, 0, -90);
                RB.velocity = new Vector2(1, 0) * playerInfo.moveSpeed;
            }
        }
    }
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
                playerController.playerStatesBehaviour.playerJump.resetJumpCount();
                playerController.playerStatesBehaviour.playerSprint.resetAirSprintLeftCount();
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
                playerController.playerStatesBehaviour.playerJump.CurrentJumpCountLeft--;
            }
            isGroundedBuffer = value;
        }
    
    }

}



