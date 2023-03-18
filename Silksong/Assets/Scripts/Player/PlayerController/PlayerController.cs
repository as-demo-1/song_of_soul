using System;
using System.Collections;
using System.Collections.Generic;
using Inventory;
using Unity.Collections;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public struct PlayerInfo
{
    private PlayerController playerController;
    public float sprintSpeed { get; private set; }

    public float waterSprintSpeed;


    public AnimationCurve breakMoonPositionCurve;
    //climb

    public bool playerFacingRight;
    //swim

    public void init(PlayerController playerController)
    {
        this.playerController = playerController;

        sprintSpeed = Constants.PlayerSprintDistance / Constants.SprintTime;
        waterSprintSpeed= Constants.PlayerWaterSprintDistance / Constants.WaterSprintTime;

        //----------------------for test-----------------------------
#if UNITY_EDITOR
        playerController.playerStatusDic.learnSkill(EPlayerStatus.CanNormalAttack, learnAttack);
        playerController.playerStatusDic.learnSkill(EPlayerStatus.CanSprint,learnSprint);
        playerController.playerStatusDic.learnSkill(EPlayerStatus.CanBreakMoon,learnBreakMoon);
        playerController.playerStatusDic.learnSkill(EPlayerStatus.CanCastSkill,learnCastSkill);
        playerController.playerStatusDic.learnSkill(EPlayerStatus.CanToCat,learnToCat);
        playerController.playerStatusDic.learnSkill(EPlayerStatus.CanPlunge,learnPlunge);
        playerController.playerStatusDic.learnSkill(EPlayerStatus.CanClimbIdle,learnClimb);
        playerController.playerStatusDic.learnSkill(EPlayerStatus.CanSing,learnSing);
        playerController.playerStatusDic.learnSkill(EPlayerStatus.CanDive,learnDive);
        playerController.playerStatusDic.learnSkill(EPlayerStatus.CanWaterSprint, learnWaterSprint);
        playerController.playerStatusDic.learnSkill(EPlayerStatus.CanHeartSword,learnHeartSword);

        GameManager.Instance.saveSystem.setDoubleJump(haveDoubleJump);
        GameManager.Instance.saveSystem.setSoulJump(haveSoulJump);
#endif 
    }


    public float getJumpUpSpeed()
    {
        if (playerController.playerToCat.IsCat) return Constants.PlayerCatJumpUpSpeed;
        else return Constants.PlayerJumpUpSpeed;
    }

    public float getJumpHeight()
    {
        if (playerController.playerToCat.IsCat) return Constants.PlayerCatJumpHeight;
        else return Constants.PlayerJumpMaxHeight;
    }


    //----------------------for test-----------------------------
#if UNITY_EDITOR
    public bool learnAttack;
    public bool learnSprint;
    public bool learnBreakMoon;
    public bool learnCastSkill;
    public bool learnToCat;
    public bool learnPlunge;
    public bool learnClimb;
    public bool learnSing;
    public bool learnDive;
    public bool learnWaterSprint;
    public bool learnHeartSword;

    public bool haveSoulJump;
    public bool haveDoubleJump;
#endif

}


[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; set; }
    public PlayerAnimatorStatesControl playerAnimatorStatesControl { get; private set; }

    public PlayerAnimatorParamsMapping animatorParamsMapping;

    public PlayerStatesBehaviour playerStatesBehaviour;

    public PlayerStatusDic playerStatusDic;

    [HideInInspector]
    public PlayerCharacter playerCharacter;
    public CharacterMoveControl PlayerHorizontalMoveControl { get; }
        = new CharacterMoveControl(1f, 5f, 8f, 8f, 10f);

    public PlayerInfo playerInfo;

    private int lastHorizontalInputDir;

    public Animator PlayerAnimator;

    private Rigidbody2D RB;//外部访问刚体时，应通过setRigidGravityScale等封装后的方法

    public Transform m_Transform { get; set; }
    //[SerializeField] private LayerMask underwaterLayerMask;
    [ReadOnly]
    public BoxCollider2D boxCollider;

    private PlayerGroundedCheck playerGroundedCheck;

    [ReadOnly]
    public PlayerToCatAndHuman playerToCat;

    [ReadOnly]
    public bool gravityLock;//为ture时，不允许gravityScale改变
    private bool isUnderWater;
    public bool IsUnderWater
    {
        get { return isUnderWater; }
        set 
        {
            if(!isUnderWater && value)
            {
                PlayerAnimator.SetTrigger(animatorParamsMapping.IntoWaterParamHash);
            }
            isUnderWater = value;
        }
    }
    

    public CircleCollider2D underWaterCheckCollider;
    public CapsuleCollider2D groundCheckCollider;

    // plunge
    public float[] plungeStrengthArr = { 0.0f, 1.0f, 3.0f };  // plunge经过了PlungeStrength[i]的距离，达到强度级别i。可配置

    private float distanceToGround = -1.0f;  // 距离下方Groud距离

    #region 特效
    [Header("特效")]
    public ParticleSystem climp;
    public ParticleSystem climpLight;
    public ParticleSystem dash;
    public ParticleSystem jump;
    public ParticleSystem plunge;
    public ParticleSystem hurt;
    public ParticleSystem lighting;
    #endregion

    InvulnerableDamable damable;

    

    //Teleport
    /// <summary>
    /// Only Demo Code for save
    /// </summary>
    //[SerializeField] private string _guid;
    [SerializeField] public InventoryManager _backpack;
    public GameObject _itemToAdd = null;
    public GameObject _savePoint = null;
    public string GUID => GetComponent<GuidComponent>().GetGuid().ToString();

    private void OnValidate()
    {
        //_guid = GUID;
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
                GameManager.Instance.saveSystem.SaveDataToDisk();
            }
        }
    }

    public GameObject lightningChainPrefab;
    private LightningChain _lightningChain;
    public void init()
    {
        RB = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        playerCharacter = GetComponent<PlayerCharacter>();
        playerGroundedCheck = new PlayerGroundedCheck(this);
        playerToCat = new PlayerToCatAndHuman(this);

        playerAnimatorStatesControl = new PlayerAnimatorStatesControl(this, PlayerAnimator, EPlayerState.Idle);
        animatorParamsMapping = playerAnimatorStatesControl.CharacterAnimatorParamsMapping;
        playerStatesBehaviour = playerAnimatorStatesControl.CharacterStatesBehaviour;
        playerStatusDic = playerAnimatorStatesControl.PlayerStatusDic;
        
        _lightningChain = GameObject.Instantiate(lightningChainPrefab).GetComponent<LightningChain>();
        _lightningChain.gameObject.SetActive(false);
        _lightningChain.transform.SetParent(transform);
        _lightningChain.transform.localPosition = new Vector3(0, 0, 0);
    }
    
    
    void Start()
    {
        playerInfo.init(this);
        // _saveSystem.TestSaveGuid(_guid);
        RB.gravityScale = Constants.PlayerNormalGravityScale;
        m_Transform = GetComponent<Transform>();

        WhenStartSetLastHorizontalInputDirByFacing();

        damable = GetComponent<InvulnerableDamable>();
        damable.takeDamageEvent.AddListener(getHurt);
        damable.onDieEvent.AddListener(die);
    }

    
    private void Update()
    {
        CheckIsGrounded();
        CheckUnderWater();
        CheckHasWallToClimb();
        //checkWaterSurface();
        

        CalDistanceToGround(); 
        CheckHasHeightToPlunge();

        TickLightningChain();

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
       // Debug.Log(RB.velocity);
    }


    public void TickLightningChain()
    {
        if (playerCharacter.Mana <= 0)
        {
            _lightningChain.gameObject.SetActive(false);
            playerCharacter.buffManager.DecreaseBuff(BuffProperty.MOVE_SPEED, _lightningChain.moveSpeedUp);
        }
        if (PlayerInput.Instance.soulSkill.IsValid)
        {
            PlayerAnimator.SetTrigger("castSkill");
            Debug.LogError("R is down");
            if (_lightningChain.isActiveAndEnabled)
            {
                Debug.LogError("light chain is active");
                _lightningChain.TiggerAtkEvent();
                _lightningChain.gameObject.SetActive(false);
                _lightningChain.enabled = false;
            }
            else
            {
                if (playerCharacter.Mana < _lightningChain.constPerSec)
                {
                    Debug.LogError("not enough mana");
                }
                else
                {
                    Debug.LogError("cast skill");
                    _lightningChain.gameObject.SetActive(true);
                    playerCharacter.buffManager.AddBuff(BuffProperty.MOVE_SPEED, _lightningChain.moveSpeedUp);
                    _lightningChain.enabled = true;
                }
            }
        }

        if (_lightningChain.isActiveAndEnabled)
        {
            _lightningChain.TriggerAddElectricMarkEvent();
            _lightningChain.UpdateTargetsLink();
        }
    }

    public void endJumpByUpAttack()
    {
        (playerStatesBehaviour.StateActionsDic[EPlayerState.Jump] as PlayerJump).EndJump();
        setRigidVelocity(Vector2.zero);
        //print("end");
    }

    public void CheckHorizontalMove(float setAccelerationNormalizedTime)
    {
        PlayerHorizontalMoveControl.SetAccelerationLeftTimeNormalized(setAccelerationNormalizedTime);
        RecordLastInputDir();

        float desireSpeed = lastHorizontalInputDir * playerCharacter.getMoveSpeed();
        float acce = PlayerHorizontalMoveControl.AccelSpeedUpdate(PlayerInput.Instance.horizontal.Value != 0 && PlayerAnimatorParamsMapping.HaveControl(),playerGroundedCheck.IsGroundedBuffer, desireSpeed);
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
    public bool isGrounded()
    {
        return playerGroundedCheck.IsGrounded;
    }
    public int getJumpCount()
    {
        if (GameManager.Instance.saveSystem.haveDoubleJump()) return Constants.PlayerMaxDoubleJumpCount;
        else return Constants.PlayerMaxJumpCount;
    }
    public void CheckFlipPlayer(float setAccelerationNormalizedTime=1f)
    {
        if (PlayerInput.Instance.horizontal.ValueBuffer == 1f & !playerInfo.playerFacingRight ||
                PlayerInput.Instance.horizontal.ValueBuffer == -1f & playerInfo.playerFacingRight)
        {
            Flip();
            PlayerHorizontalMoveControl.SetAccelerationLeftTimeNormalized(setAccelerationNormalizedTime);
        }
    }

    public void setRigidVelocity(Vector2 newVelocity)
    {
        //Debug.Log("set"+" "+newVelocity);
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
        setRigidGravityScale(Constants.PlayerNormalGravityScale);
    }

    public void rigidMovePosition(Vector2 target)
    {
        RB.MovePosition(target);
    }

    public void setRigidLinearDrag(float linearDarg)
    {
        RB.drag = linearDarg;
    }
    public void WhenStartSetLastHorizontalInputDirByFacing() => lastHorizontalInputDir = playerInfo.playerFacingRight ? 1 : -1;

    public void Flip()
    {
        playerInfo.playerFacingRight = !playerInfo.playerFacingRight;
        Vector3 t = transform.localScale;
        transform.localScale = new Vector3(-t.x, t.y, t.z);
        PlayerBreakMoon playerBreakMoon = (PlayerBreakMoon)playerStatesBehaviour.StateActionsDic[EPlayerState.BreakMoon];
        playerBreakMoon .findCurrentTarget();
    }

    public void getHurt(DamagerBase damager, DamageableBase damable)
    {
        PlayerAnimator.SetTrigger(animatorParamsMapping.HurtParamHas);
        playerToCat.toHuman();
        //Debug.Log("受伤");
    
    }

    public void die(DamagerBase damager, DamageableBase damable)
    {
        PlayerAnimator.SetBool(animatorParamsMapping.DeadParamHas, true);
    }

    public void CheckUnderWater()
    {
        IsUnderWater = underWaterCheckCollider.IsTouchingLayers(1<<LayerMask.NameToLayer("GameWater"));
    }


    public void CalDistanceToGround()
    {

        Vector2 groundCheckPos = groundCheckCollider.transform.position;
        groundCheckPos = groundCheckPos + groundCheckCollider.offset;
        Vector2 offset = new Vector2(0, -groundCheckCollider.bounds.size.y / 2);
         //Debug.DrawRay(groundCheckPos + offset, Vector2.down, Color.red, 0.2f);

        int groundLayerMask = 1 << LayerMask.NameToLayer("Ground");

        // 向下发射射线，检测跟Ground Layer的距离。如果下方没有Ground则distanceToGround = -1.
        RaycastHit2D hit = Physics2D.Raycast(groundCheckPos + offset, Vector2.down, 100.0f, groundLayerMask);
        if (hit.collider == null)
        {
            distanceToGround = -1.0f;
        }
        else
        {
            distanceToGround = hit.distance;
        }

        // Debug.Log(distanceToGround);

    }

    public void CheckHasHeightToPlunge() {
        if (distanceToGround >Constants.canPlungeHeight) {
            PlayerAnimator.SetBool(animatorParamsMapping.HasHeightToPlungeParamHash, true);
        }
        else {
            PlayerAnimator.SetBool(animatorParamsMapping.HasHeightToPlungeParamHash, false);
        }

    }

    public bool checkHitWall(bool checkRightSide)
    {
        Vector2 t = transform.position;
        t.y += 0.5f;//at player head

        Vector2 frontPoint;
        frontPoint = new Vector2(t.x + (checkRightSide?1:-1) * boxCollider.size.x * 0.5f , t.y);

        if (Physics2D.OverlapArea(frontPoint, transform.position, 1 << LayerMask.NameToLayer("Ground")) != null)
        {
            return true;
        }

        return false;

    }

    public bool checkWaterSurface()//only callded when used in player actions
    {

        Vector2 t = transform.position;
        t.y += 0.4f;//when water below this height,return true

        Vector2 upPoint = new Vector2(t.x+0.1f,t.y+0.1f);
       // Debug.DrawLine(t, upPoint,Color.red);
        bool ret = false;
        if (Physics2D.OverlapArea(upPoint, t, 1 << LayerMask.NameToLayer("GameWater")) == null)
        {
            ret=true;
        }
        PlayerAnimator.SetBool(animatorParamsMapping.WaterSurfaceParamHash, ret);
        return ret;

    }

    private void CheckHasWallToClimb()
    {
        bool checkRightSide;
       
        float horizontalInput = PlayerInput.Instance.horizontal.Value;
        if (horizontalInput == 1) checkRightSide = true;
        else if (horizontalInput == -1) checkRightSide = false;
        else //input==0
        {
            if (playerAnimatorStatesControl.CurrentPlayerState == EPlayerState.ClimbIdle)
            {
                
                checkRightSide = playerInfo.playerFacingRight;
            }
            else
            {
                //climp.Play();
                PlayerAnimator.SetBool(animatorParamsMapping.HasWallForClimbParamHash, false);
                return;
            }
              
        }
        
       
        PlayerAnimator.SetBool(animatorParamsMapping.HasWallForClimbParamHash,checkHitWall(checkRightSide));
    }


    public void checkMaxFallSpeed()
    {
        if (RB.velocity.y < -Constants.PlayerMaxFallSpeed)
        {
           RB.velocity= new Vector2(RB.velocity.x, -Constants.PlayerMaxFallSpeed);
        }
    }
}

public class PlayerGroundedCheck
{
    private bool isGrounded;
    private PlayerController playerController;
    private int bufferTimer;
    private bool isGroundedBuffer;
    //private int bufferGroundTrue;
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
            if (value && playerController.IsUnderWater==false)//设为真
            {

                ( playerController.playerStatesBehaviour.StateActionsDic[EPlayerState.Jump] as PlayerJump) .resetAllJumpCount();
                ( playerController.playerStatesBehaviour.StateActionsDic[EPlayerState.Sprint] as PlayerSprint).resetAirSprintLeftCount();
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

            if (isGroundedBuffer &&!value && playerController.IsUnderWater == false)//从真设为假
            {
                (playerController.playerStatesBehaviour.StateActionsDic[EPlayerState.Jump] as PlayerJump).CurrentJumpCountLeft--;
            }
            isGroundedBuffer = value;
        }
    
    }

}







