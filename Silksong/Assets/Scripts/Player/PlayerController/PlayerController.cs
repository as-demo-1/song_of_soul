using System;
using System.Collections;
using System.Collections.Generic;
using Inventory;
using UnityEngine;

[System.Serializable]
public struct PlayerInfo
{
    private PlayerController playerController;

    public float jumpMinHeight;
    public float maxFallSpeed;

    private float jumpUpSpeed;
    private float catJumpUpSpeed;

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
    //swim
    public float swimSpeed;

    public void init(PlayerController playerController)
    {
        this.playerController = playerController;
        jumpUpSpeed = Constants.PlayerJumpHeight * 2.5f;
        catJumpUpSpeed = Constants.PlayerCatJumpHeight * 2.5f;

        sprintSpeed = sprintDistance / Constants.SprintTime;
    }

    public float getMoveSpeed()
    {
        if (playerController.playerToCat.IsCat)
        {
            if (playerController.playerToCat.isFastMoving)
                return Constants.PlayerCatFastMoveSpeed;
            else return Constants.PlayerCatMoveSpeed;
        }
        else return Constants.PlayerMoveSpeed;
    }

    public float getJumpUpSpeed()
    {
        if (playerController.playerToCat.IsCat) return catJumpUpSpeed;
        else return jumpUpSpeed;
    }

    public float getJumpHeight()
    {
        if (playerController.playerToCat.IsCat) return Constants.PlayerCatJumpHeight;
        else return Constants.PlayerJumpHeight;
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
    [SerializeField] private LayerMask underwaterLayerMask;
    //[SerializeField] private LayerMask ropeLayerMask; ע�����ɣ����ܲ�����Ҫ��������
    [DisplayOnly]
    public BoxCollider2D boxCollider;

    private PlayerGroundedCheck playerGroundedCheck;

    [DisplayOnly]
    public PlayerToCat playerToCat;

    [DisplayOnly]
    public bool gravityLock;//为ture时，不允许gravityScale改变
    public bool IsUnderWater;

    [SerializeField] private Collider2D underWaterCheckCollider;
    public BoxCollider2D groundCheckCollider;
    //Teleport
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
        //Debug.Log("Enable PlayerController");
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            throw new UnityException("There cannot be more than one PlayerController script.  The instances are " + Instance.name + " and " + name + ".");
    }

    private void OnDisable()
    {
        Instance = null;
        //Debug.Log("Disable PlayerController");
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

    public void init()
    {
        RB = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        playerCharacter = GetComponent<PlayerCharacter>();
        playerGroundedCheck = new PlayerGroundedCheck(this);
        playerToCat = new PlayerToCat(this);

        playerAnimatorStatesControl = new PlayerAnimatorStatesControl(this, PlayerAnimator, EPlayerState.Idle);
        animatorParamsMapping = playerAnimatorStatesControl.CharacterAnimatorParamsMapping;
        playerStatesBehaviour = playerAnimatorStatesControl.CharacterStatesBehaviour;
        playerStatusDic = playerAnimatorStatesControl.PlayerStatusDic;
    }
    void Start()
    {
        playerInfo.init(this);
        // _saveSystem.TestSaveGuid(_guid);
        RB.gravityScale = playerInfo.normalGravityScale;
        m_Transform = GetComponent<Transform>();

        WhenStartSetLastHorizontalInputDirByFacing();

        HpDamable damable = GetComponent<HpDamable>();
        damable.takeDamageEvent.AddListener(getHurt);
        damable.onDieEvent.AddListener(die);

    }

    private void Update()
    {
        CheckIsGrounded();
        CheckUnderWater();

        playerAnimatorStatesControl.ParamsUpdate();
        playerToCat.catUpdate();

    }

    private void LateUpdate()
    {
        playerAnimatorStatesControl.BehaviourLateUpdate();
    }

    private void FixedUpdate()
    {
        //Interact();
    }

    public void CheckHorizontalMove(float setAccelerationNormalizedTime)
    {
        PlayerHorizontalMoveControl.SetAccelerationLeftTimeNormalized(setAccelerationNormalizedTime);
        RecordLastInputDir();

        float desireSpeed = lastHorizontalInputDir * playerInfo.getMoveSpeed();
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
            InteractManager.Instance.Interact();
        }
    }

    public void CheckIsGrounded()
    {
        int groundLayerMask = 1<<LayerMask.NameToLayer("Ground");
        if(playerToCat.IsCat)
        {
            groundLayerMask += 1<<LayerMask.NameToLayer("CloudMass");
        }
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
    public void addRigidVelocityY(float forceY)
    {
        RB.AddForce(new Vector2(0, forceY), ForceMode2D.Impulse);
    }

    public void setRigidGravityScale(float newScale)
    {
        if (gravityLock == false)
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

    public void getHurt(DamagerBase damager, DamageableBase damable)
    {
        PlayerAnimator.SetTrigger(animatorParamsMapping.HurtParamHas);
    }

    public void die(DamagerBase damager, DamageableBase damable)
    {
        PlayerAnimator.SetBool(animatorParamsMapping.DeadParamHas, true);
    }

    public void CheckUnderWater()
    {
        IsUnderWater = underWaterCheckCollider.IsTouchingLayers(underwaterLayerMask);
    }
    public void SwimMove()
    {
        RB.velocity = new Vector2(PlayerInput.Instance.horizontal.Value, PlayerInput.Instance.vertical.Value) * playerInfo.swimSpeed;
    }
    public void SwimUnderWater()
    {
        if (PlayerInput.Instance.horizontal.Value == -1f && PlayerInput.Instance.vertical.Value == 1f)     //左上
        {
            m_Transform.localRotation = Quaternion.Euler(0, 0, 45);
            m_Transform.localScale = new Vector3(-1, 1, 1);
        }
        else if (PlayerInput.Instance.horizontal.Value == 1f && PlayerInput.Instance.vertical.Value == 1f)    //右上
        {
            m_Transform.localRotation = Quaternion.Euler(0, 0, -45);
            m_Transform.localScale = new Vector3(1, 1, 1);
        }
        else if (PlayerInput.Instance.horizontal.Value == -1f && PlayerInput.Instance.vertical.Value == -1f)    //左下
        {
            m_Transform.localRotation = Quaternion.Euler(0, 0, 135);
            m_Transform.localScale = new Vector3(-1, 1, 1);
        }
        else if (PlayerInput.Instance.horizontal.Value == 1f && PlayerInput.Instance.vertical.Value == -1f)    //右下
        {
            m_Transform.localRotation = Quaternion.Euler(0, 0, -135);
            m_Transform.localScale = new Vector3(1, 1, 1);
        }
        else
        {
            if (PlayerInput.Instance.vertical.Value == 1f)                                //上
            {
                m_Transform.localRotation = Quaternion.Euler(0, 0, 0);
            }
            if (PlayerInput.Instance.vertical.Value == -1f)                                //下
            {
                m_Transform.localRotation = Quaternion.Euler(0, 0, 180);
            }
            if (PlayerInput.Instance.horizontal.Value == -1f)                              //左
            {
                m_Transform.localRotation = Quaternion.Euler(0, 0, 90);
                m_Transform.localScale = new Vector3(-1, 1, 1);
            }
            if (PlayerInput.Instance.horizontal.Value == 1f)                                //右
            {
                m_Transform.localRotation = Quaternion.Euler(0, 0, -90);
                m_Transform.localScale = new Vector3(1, 1, 1);
            }
        }
        if (IsUnderWater) SwimMove();
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

            if (bufferTimer > 0)
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
        get { return isGroundedBuffer; }
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

public class PlayerToCat
{
    private bool isCat;
    public bool IsCat
    {
        get { return isCat; }
        set
        {
            isCat = value;
            playerController.PlayerAnimator.SetBool(playerController.animatorParamsMapping.IsCatParamHas,value);
        }
    }

    private bool hasUpSpaceForHuman;
    public bool HasUpSpaceForHuman
    {
        get { return hasUpSpaceForHuman; }
        set
        {
            hasUpSpaceForHuman = value;
            playerController.PlayerAnimator.SetBool(playerController.animatorParamsMapping.HasUpSpaceForHumanParamHas,value);
        }
    }


    private PlayerController playerController;
    public PlayerToCat(PlayerController playerController)
    {
        this.playerController = playerController;
    }

    private Vector2 runStartPos;
    public bool isFastMoving;
    private float fastMoveStartAbsSpeed;
    public void toCat()
    {
        if (IsCat) return;

        IsCat = true;
        playerController.gameObject.layer =LayerMask.NameToLayer("PlayerCat");
        playerController.GetComponentInChildren<SpriteRenderer>().flipX = true;//now the cat image is filpx from player image
        playerController.boxCollider.offset = new Vector2(playerController.boxCollider.offset.x, Constants.playerCatBoxColliderOffsetY);
        playerController.boxCollider.size = new Vector2(Constants.playerCatBoxColliderWidth, Constants.playerCatBoxColliderHeight);

        playerController.groundCheckCollider.offset = new Vector2(playerController.groundCheckCollider.offset.x, Constants.playerCatGroundCheckColliderOffsetY);
        playerController.groundCheckCollider.size= new Vector2( Constants.playerCatBoxColliderWidth-Constants.playerGroundColliderXSizeSmall,playerController.groundCheckCollider.size.y);

    }

    public void colliderToHuman()
    {
        void checkIfNeedMoveAwayFromGround()//to prevent player from dropped in ground
        {
            Vector2 t = playerController.transform.position;
            Vector2 rightPoint = new Vector2(t.x + playerController.boxCollider.size.x * 0.5f+0.05f, t.y);//only find dropped in ground from right side

            if (Physics2D.OverlapArea(rightPoint, t, 1 << LayerMask.NameToLayer("Ground")) != null)
            {
                playerController.transform.position = new Vector2(t.x - 0.25f, t.y);
            }      
        }


        if (!IsCat) return;

        checkIfNeedMoveAwayFromGround();
        playerController.boxCollider.offset = new Vector2(playerController.boxCollider.offset.x, Constants.playerBoxColliderOffsetY);
        playerController.boxCollider.size = new Vector2(Constants.playerBoxColliderWidth, Constants.playerBoxColliderHeight);

        playerController.groundCheckCollider.offset = new Vector2(playerController.groundCheckCollider.offset.x, Constants.playerGroundCheckColliderOffsetY);
        playerController.groundCheckCollider.size = new Vector2(Constants.playerBoxColliderWidth - Constants.playerGroundColliderXSizeSmall, playerController.groundCheckCollider.size.y);   
    }

    public void stateToHuman()
    {
        if (!IsCat) return;

        IsCat = false;
        isFastMoving = false;
        playerController.gameObject.layer = LayerMask.NameToLayer("Player");
        playerController.GetComponentInChildren<SpriteRenderer>().flipX = false;//now the cat image is filpx from player image
    }
    public void toHuman()
    {
        colliderToHuman();
        stateToHuman();
    }
    public void catUpdate()
    {
        if (!IsCat) return;

        checkUpSpaceForHuman();
        checkFastMoveEnd();
    }
    private void checkUpSpaceForHuman()
    {

        Vector2 distance = new Vector2(0.125f, 0.5f);
        Vector2 YOffset = new Vector2(0, 0.5f);

        Vector2 upPoint = (Vector2)playerController.transform.position + YOffset;
        Vector2 upPointA = upPoint + distance;
        Vector2 upPointB = upPoint - distance;
       // Debug.DrawLine(upPointA, upPointB);

        Vector2 downPoint= (Vector2)playerController.transform.position - YOffset;
        Vector2 downPointA =downPoint + distance;
        Vector2 downPointB = downPoint - distance;
       // Debug.DrawLine(downPointA, downPointB);
        //Debug.Log(Physics2D.OverlapArea(upPointA, upPointB, LayerMask.NameToLayer("Ground")));
        if (Physics2D.OverlapArea(upPointA, upPointB, 1<<LayerMask.NameToLayer("Ground")) && Physics2D.OverlapArea(downPointA,downPointB, 1<<LayerMask.NameToLayer("Ground")))
        {
            HasUpSpaceForHuman = false;
        }
        else
        {
            HasUpSpaceForHuman = true;
        }
    } 

    public void moveDistanceCount()
    {
        if (!IsCat) return;

        if (!isFastMoving && Mathf.Abs(playerController.transform.position.x-runStartPos.x)>Constants.PlayerCatToFastMoveDistance)
        {
            isFastMoving = true;
            Debug.Log("cat fast move");
            fastMoveStartAbsSpeed =Mathf.Abs(playerController.getRigidVelocity().x);
        }
    }

    private void checkFastMoveEnd()
    {
        if(isFastMoving && Mathf.Abs( playerController.getRigidVelocity().x)<fastMoveStartAbsSpeed)
        {
            isFastMoving = false;
            Debug.Log("cat fast end");
            /* Debug.Log(playerController.getRigidVelocity().x);
             Debug.Log(fastMoveDir);
             Debug.Log(playerController.getRigidVelocity().x != fastMoveDir);*/
        }
    }
    public void catMoveStart()
    {
        if (!IsCat) return;
        runStartPos = playerController.transform.position;
    }

    public void extraJump()
    {
        if (!isFastMoving || playerController.isGroundedBuffer()) return;

        playerController.PlayerAnimator.Play("CatToHumanExtraJump");
        Debug.Log("extra jump");
        float speed = Mathf.Sqrt(Physics2D.gravity.y * -1 * playerController.playerInfo.normalGravityScale * 2 * Constants.PlayerCatToHumanExtraJumpHeight);
        playerController.setRigidVelocity(new Vector2( playerController.getRigidVelocity().x, speed));
    }
    

}





