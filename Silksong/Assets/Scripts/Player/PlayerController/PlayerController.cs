using System;
using System.Collections;
using System.Collections.Generic;
using Inventory;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public struct PlayerInfo
{
    private PlayerController playerController;
    public float sprintSpeed { get; private set; }

    public bool hasDoubleJump;

    public AnimationCurve breakMoonPositionCurve;
    //climb
    public float normalGravityScale;

    public bool playerFacingRight;
    //swim
    public float swimSpeed;

    public float gravityUnderWater;

    // plunge
    public float plungeSpeed;

    [SerializeField]
    private CharmListSO CharmListSO;

    public void init(PlayerController playerController)
    {
        this.playerController = playerController;

        sprintSpeed = Constants.PlayerSprintDistance / Constants.SprintTime;
        gravityUnderWater = normalGravityScale / 5;
    }

    public float getMoveSpeed()
    {
        if (playerController.playerToCat.IsCat)
        {
            if (playerController.playerToCat.isFastMoving)
                return Constants.PlayerCatFastMoveSpeed;
            else return Constants.PlayerCatMoveSpeed;
        }
        else if(playerController.playerAnimatorStatesControl.CurrentPlayerState==EPlayerState.NormalAttack)
        {
            return Constants.AttackingMoveSpeed + CharmListSO.CharmMoveSpeed;
        }
        else return Constants.PlayerMoveSpeed + + CharmListSO.CharmMoveSpeed;
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

    public int getJumpCount()
    {
        if (hasDoubleJump) return Constants.PlayerMaxDoubleJumpCount;
        else return Constants.PlayerMaxJumpCount;
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

    public Transform m_Transform { get; set; }
    [SerializeField] private LayerMask underwaterLayerMask;
    [DisplayOnly]
    public BoxCollider2D boxCollider;

    private PlayerGroundedCheck playerGroundedCheck;

    [DisplayOnly]
    public PlayerToCatAndHuman playerToCat;

    [DisplayOnly]
    public bool gravityLock;//为ture时，不允许gravityScale改变
    public bool IsUnderWater;

    [SerializeField] private Collider2D underWaterCheckCollider;
    public BoxCollider2D groundCheckCollider;

    // plunge
    public float[] plungeStrengthArr = { 0.0f, 1.0f, 3.0f };  // plunge经过了PlungeStrength[i]的距离，达到强度级别i。可配置

    public float canPlungeHeight = 3.0f;  // 离地多远可以使用plunge。可配置

    [DisplayOnly]
    public float distanceToGround = -1.0f;  // 距离下方Groud距离

    //Teleport
    /// <summary>
    /// Only Demo Code for save
    /// </summary>
    [SerializeField] private string _guid;
    [SerializeField] private SaveSystem _saveSystem;
    [SerializeField] public InventoryManager _backpack;
    public GameObject _itemToAdd = null;
    public GameObject _savePoint = null;

    public Transform lookPos;

    public string GUID => GetComponent<GuidComponent>().GetGuid().ToString();

    public GameObject followPoint;

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
        if(other.gameObject.CompareTag("UnderWater"))
        {
            IsUnderWater = true;
            //入水时慢慢将速度减为0    
            float smooth = 100f;
            //float exitWaterTime = Time.time;
            //RB.velocity = Vector2.Lerp(RB.velocity, new Vector2(RB.velocity.x, 0), (Time.time - exitWaterTime) * smooth);
            // RB.gravityScale = playerInfo.normalGravityScale / 5;
            RB.gravityScale = playerInfo.gravityUnderWater;
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
        boxCollider = GetComponent<BoxCollider2D>();
        playerCharacter = GetComponent<PlayerCharacter>();
        playerGroundedCheck = new PlayerGroundedCheck(this);
        playerToCat = new PlayerToCatAndHuman(this);

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
        CheckHasWallToClimb();
        playerAnimatorStatesControl.ParamsUpdate();
        playerToCat.catUpdate();

        CalDistanceToGround(); // 计算离地距离
        CheckHasHeightToPlunge();
        CheckLookDown();
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
            if (InteractManager.Instance.CollidingObject)
            {
                InteractManager.Instance.CollidingObject.GetComponent<InteractController>().InactItem.Interact();
            }
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
       /* Vector2 t = transform.position;
        t.y += Constants.playerGroundCheckColliderOffsetY;
        Vector2 pointA= new Vector2(t.x+0.115f,t.y+0.05f);
        Vector2 pointB= new Vector2(t.x - 0.115f, t.y-0.05f);
        Debug.DrawLine(pointA, pointB);
        playerGroundedCheck.IsGrounded =Physics2D.OverlapArea(pointA,pointB,groundLayerMask);*/
    }

    public bool isGroundedBuffer()
    {
        return playerGroundedCheck.IsGroundedBuffer;
    }

    public void CheckFlipPlayer(float setAccelerationNormalizedTime)
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
        PlayerBreakMoon playerBreakMoon = (PlayerBreakMoon)playerStatesBehaviour.StateActionsDic[EPlayerState.BreakMoon];
        playerBreakMoon .findCurrentTarget();
    }

    public void getHurt(DamagerBase damager, DamageableBase damable)
    {
        PlayerAnimator.SetTrigger(animatorParamsMapping.HurtParamHas);
        playerToCat.toHuman();
    
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
                RB.velocity = new Vector2(0, 1) * playerInfo.getMoveSpeed();
            }
            if (PlayerInput.Instance.vertical.Value == -1f)                                //下
            {
                m_Transform.localRotation = Quaternion.Euler(0, 0, 180);
                RB.velocity = new Vector2(0, -1) * playerInfo.getMoveSpeed();
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

    public void CalDistanceToGround()
    {

        if (IsUnderWater) return;

        Vector2 groundCheckPos = groundCheckCollider.transform.position;
        groundCheckPos = groundCheckPos + groundCheckCollider.offset;
        Vector2 offset = new Vector2(0, -groundCheckCollider.size.y / 2);
        // Debug.DrawRay(groundCheckPos + offset, Vector2.down, Color.red, 0.2f);

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
        if (distanceToGround > canPlungeHeight) {
            PlayerAnimator.SetBool(animatorParamsMapping.HasHeightToPlungeParamHash, true);
        }
        else {
            PlayerAnimator.SetBool(animatorParamsMapping.HasHeightToPlungeParamHash, false);
        }

    }

    public bool checkHitWall(bool checkRightSide)
    {
        Vector2 t = transform.position;
        t.y -= 0.5f;
        Vector2 frontPoint;
        frontPoint = new Vector2(t.x + (checkRightSide?1:-1) * boxCollider.size.x * 0.5f , t.y);

        if (Physics2D.OverlapArea(frontPoint, t, 1 << LayerMask.NameToLayer("Ground")) != null)
        {
            return true;
        }

        return false;

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
                PlayerAnimator.SetBool(animatorParamsMapping.HasWallForClimbParamHash, false);
                return;
            }
              
        }
        
       
        PlayerAnimator.SetBool(animatorParamsMapping.HasWallForClimbParamHash,checkHitWall(checkRightSide));
    }

    private void CheckLookDown()
    {
        if (PlayerInput.Instance.vertical.Value == -1 && RB.velocity.magnitude < 0.01f)                                
        {
            lookPos.localPosition = new Vector3(0.0f, -3.0f, 0.0f);
        }
        else if (PlayerInput.Instance.vertical.Value == 1 && RB.velocity.magnitude < 0.01f)                                
        {
            lookPos.localPosition = new Vector3(0.0f, 3.0f, 0.0f);
        }
        else
        {
            lookPos.localPosition = Vector3.zero;
        }
    }
}

public class PlayerGroundedCheck
{
    private bool isGrounded;
    private PlayerController playerController;
    private int bufferTimer;
    private bool isGroundedBuffer;
    private int bufferGroundTrue;
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
                if(++bufferGroundTrue>=5)
                {
                   ( playerController.playerStatesBehaviour.StateActionsDic[EPlayerState.Jump] as PlayerJump) .resetJumpCount();
                   ( playerController.playerStatesBehaviour.StateActionsDic[EPlayerState.Sprint] as PlayerSprint).resetAirSprintLeftCount();
                    bufferTimer = Constants.IsGroundedBufferFrame;
                }
            }
            else
            {
                bufferGroundTrue = 0;
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
                (playerController.playerStatesBehaviour.StateActionsDic[EPlayerState.Jump] as PlayerJump).CurrentJumpCountLeft--;
            }
            isGroundedBuffer = value;
        }
    
    }

}

public class PlayerToCatAndHuman
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
    public PlayerToCatAndHuman(PlayerController playerController)
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

            if (playerController.checkHitWall(true))
                playerController.transform.position = new Vector2(t.x - 0.25f, t.y);   
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
    }
    public void toHuman()
    {
        if (isCat == false) return;
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





