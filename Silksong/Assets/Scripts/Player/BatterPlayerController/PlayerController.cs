using System;
using System.Collections;
using System.Collections.Generic;
using Inventory;
using UnityEditor;
using UnityEngine;

namespace BatterGame
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        public static PlayerController Instance;

        public PlayerInfomation playerInfo;
        private Rigidbody2D RB; //外部访问刚体时，应通过setRigidGravityScale等封装后的方法
        public BoxCollider2D boxCollider;
        public Animator m_playerAnimator;
        [SerializeField] private Collider2D underWaterCheckCollider;
        public BoxCollider2D groundCheckCollider;

        public GameObject normalAttackPrefab;
        public GameObject lightningChainPrefab;


        [DisplayOnly] private OnGroundChecker _onGroundChecker;

        private int lastHorizontalInputDir;
        

        private CharacterMoveControl PlayerHorizontalMoveControl { get; } =
            new CharacterMoveControl(1f, 5f, 8f, 8f, 10f);
        private PlayerStateController _playerStateController;
        
        public PlayerController()
        {
            Instance = this;
        }

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            Init();
        }

        
        private PlayerNormalAtk _normalAttack;
        private LightningChain _lightningChain;
        public void Init()
        {
            _normalAttack = Instantiate(normalAttackPrefab).GetComponent<PlayerNormalAtk>();
            _normalAttack.transform.SetParent(transform);
            _normalAttack.transform.localPosition = new Vector3(0, 0, 0);

            
            _lightningChain = GameObject.Instantiate(lightningChainPrefab).GetComponent<LightningChain>();
            _lightningChain.transform.SetParent(transform);
            _lightningChain.transform.localPosition = new Vector3(0,0,0);
            

            RB = GetComponent<Rigidbody2D>();
            boxCollider = GetComponent<BoxCollider2D>();
            _onGroundChecker = new OnGroundChecker(this);
            if (m_playerAnimator is null)
            {
                Debug.LogError("no animator");
            }
            _playerStateController = new PlayerStateController(m_playerAnimator);
        }


        void Start()
        {
            playerInfo.Init();
            RB.gravityScale = playerInfo.normalGravityScale;
            WhenStartSetLastHorizontalInputDirByFacing();

            // TODO : 监听战斗Event
        }


        private void Update()
        {
            CheckIsGrounded();
            CheckHorizontalMove(0.4f);
            
            _playerStateController.DirveAnimatorParameters();
            
            //TickNormalAtk();
            TickLightningChain();
        }

        private void LateUpdate()
        {
           
        }

        private void FixedUpdate()
        {
            //Interact();
        }

        public void TickNormalAtk()
        {
            if (PlayerInput.Instance.normalAttack.IsValid)
            {
                _normalAttack.TiggerAtkEvent();
            }
        }

        public void TickLightningChain()
        {
            if (playerInfo.currentMana <= 0)
            {
                _lightningChain.gameObject.SetActive(false);
            }
            if (PlayerInput.Instance.soulSkill.IsValid)
            {
                m_playerAnimator.SetTrigger("castSkill");
                Debug.LogError("R is down");
                if (_lightningChain.isActiveAndEnabled)
                {
                    Debug.LogError("light chain is active");
                    _lightningChain.TiggerAtkEvent();
                }
                else
                {
                    if (playerInfo.currentMana < _lightningChain.constPerSec)
                    {
                        Debug.LogError("not enough mana");
                    }
                    else
                    {
                        _lightningChain.gameObject.SetActive(true);
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

        public void CheckHorizontalMove(float setAccelerationNormalizedTime)
        {
            PlayerHorizontalMoveControl.SetAccelerationLeftTimeNormalized(setAccelerationNormalizedTime);
            RecordLastInputDir();

            float desireSpeed = lastHorizontalInputDir * playerInfo.GetMoveSpeed();
            float acce = PlayerHorizontalMoveControl.AccelSpeedUpdate(PlayerInput.Instance.horizontal.Value != 0,
                _onGroundChecker.IsGroundedBuffer, desireSpeed);
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
            int groundLayerMask = 1 << LayerMask.NameToLayer("Ground");

            _onGroundChecker.IsGrounded = groundCheckCollider.IsTouchingLayers(groundLayerMask);
        }

        public bool isGroundedBuffer()
        {
            return _onGroundChecker.IsGroundedBuffer;
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
        

        public void rigidMovePosition(Vector2 target)
        {
            RB.MovePosition(target);
        }

        public void WhenStartSetLastHorizontalInputDirByFacing() =>
            lastHorizontalInputDir = playerInfo.playerFacingRight ? 1 : -1;

        public void Flip()
        {
            playerInfo.playerFacingRight = !playerInfo.playerFacingRight;
            Vector3 t = transform.localScale;
            transform.localScale = new Vector3(-t.x, t.y, t.z);
        }
        
        public class OnGroundChecker
        {
            private bool isGrounded;
            private PlayerController playerController;
            private int bufferTimer;
            private bool isGroundedBuffer;
            private int bufferGroundTrue;

            public OnGroundChecker(PlayerController playerController)
            {
                this.playerController = playerController;
            }

            public bool IsGrounded
            {
                get { return isGrounded; }

                set //每次update都会调用
                {
                    if (value) //设为真
                    {
                        if (++bufferGroundTrue >= 5)
                        {
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

                    if (isGroundedBuffer && !value) //从真设为假
                    {
                        
                    }
                    isGroundedBuffer = value;
                }
            }
        }
    }
}





