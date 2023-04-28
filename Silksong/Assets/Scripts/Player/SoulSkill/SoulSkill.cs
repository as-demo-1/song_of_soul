using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;
using UnityEngine.Serialization;

public enum SkillName
{
    FlameGeyser,

    ShadowBlade,

    
    LightningChain,

    ArcaneBlast,

    IceStorm
}

[Serializable]
public abstract class SoulSkill : MonoBehaviour
{
    public SkillName skillName;
    public int constPerSec = 1;
    public int constPerAttack = 0;
    public float henshinTime;
    public int baseDamage;
    

    //protected PlayerInfomation _playerInfomation;
    protected PlayerCharacter _playerCharacter;
    protected PlayerController _playerController;
    protected Animator _playerAnimator;

    [SerializeField]
    protected GameObject charge;
    [SerializeField]
    protected ParticleSystem henshin;
    [SerializeField]
    protected Material soulMode = default;
    [FormerlySerializedAs("atkDamager")] [SerializeField]
    public GameObject atkObject;// 挥击效果或伤害
    public string animName;// 攻击动画变量名称
    public GameObject stateParticle; // 状态特效
    public GameObject hurtEffectPrefab;// 击中特效


    protected Material original;
    [SerializeField]
    protected Vector3 HSV;

    /// <summary>
    /// 技能开启事件
    /// </summary>
    public UnityEvent SkillStart;
    
    /// <summary>
    /// 技能结束事件
    /// </summary>
    public UnityEvent SkillEnd;
    

    private float timer;
    private float timeCounter = 1.0f;

    public PlayerSkillDamager damager;

    
    public virtual void Init(PlayerController playerController, PlayerCharacter playerCharacter)
    {
        this._playerController = playerController;
        this._playerCharacter = playerCharacter;
        this._playerAnimator = _playerController.PlayerAnimator;
        this.original = _playerAnimator.GetComponent<SpriteRenderer>().material;

        if (damager != null)
        {
            damager.damage = baseDamage;// set skill damage value
            damager.makeDamageEvent.AddListener(PlayHurtEffect);// 绑定受伤特效
        }
        
    }
    protected void Start()
    {
        //Debug.Log("START TIMER UPDATE");
        //_playerCharacter = GetComponentInParent<PlayerCharacter>();
        
        //MonoManager.Instance.AddUpdateEvent(Timer.Instance.TimerUpdate);
    }

    protected void OnEnable()
    {
        //Debug.Log("test!!!!!!!!!!!");
        
        
        
        // 使用定时器定时结算灵魂状态的const
        //Timer.Instance.StartTickActionLoop("TickSoulStatus", 0, 10, TickSoulStatus);
    }

    protected void OnDisable()
    {
        //Debug.Log("test disable!!!!!!!!!!!");
        //Timer.Instance.EndTickActionLoop("TickSoulStatus");
    }

    private void Update()
    {

    }

    private int debugCnt = 0;
    protected void TickSoulStatus()
    {
        _playerCharacter.CostMana(constPerSec);
    }

    public virtual void EnterSoulMode()
    {
        Sequence sequence = DOTween.Sequence();
        sequence.AppendCallback(() =>
        {
            // 修正特效旋转
            if (!_playerController.playerInfo.playerFacingRight)
            {
                charge.transform.localScale = new Vector3(-1.0f, 1.0f, 1.0f);
                henshin.transform.localScale = new Vector3(-1.0f, 1.0f, 1.0f);
            }
            else
            {
                charge.transform.localScale = Vector3.one;
                henshin.transform.localScale = Vector3.one;
            }
            charge.gameObject.SetActive(true);
            _playerController.SoulSkillController.isHenshining = true;
            _playerAnimator.SetBool("CastSkillIsValid", true);
            _playerController.GetComponent<InvulnerableDamable>().invulnerable = true;// 变身时无敌
            PlayerAnimatorParamsMapping.SetControl(false);
        });
        sequence.AppendInterval(henshinTime);
        sequence.AppendCallback(() =>
        {
            charge.gameObject.SetActive(false);
            henshin.gameObject.SetActive(true);
            
     
            _playerController.SoulSkillController.inSoulModel = true;
            
            soulMode.SetFloat("_H", HSV.x);
            soulMode.SetFloat("_S", HSV.y);
            soulMode.SetFloat("_V", HSV.z);// 专属颜色
            _playerAnimator.GetComponent<SpriteRenderer>().material = soulMode;// 更换材质
            
            SkillStart.Invoke();
        });
        sequence.AppendInterval(0.5f);
        sequence.AppendCallback(() =>
        {
            henshin.gameObject.SetActive(false);
            _playerController.SoulSkillController.isHenshining = false;
            _playerAnimator.SetBool("CastSkillIsValid", false);
            if (stateParticle != null)
            {
                stateParticle.gameObject.SetActive(true);
            }

            if (animName != "")
            {
                _playerAnimator.SetBool(animName, true);
                _playerAnimator.SetBool("isSoul", true);
            }
            PlayerAnimatorParamsMapping.SetControl(true);
            _playerController.GetComponent<InvulnerableDamable>().invulnerable = false;
            InvokeRepeating(nameof(TickSoulStatus), 0.0f, 1.0f);// 定时扣除能量
        });
    }

    public virtual void ExitSoulMode()
    {
        _playerController.SoulSkillController.inSoulModel = false;
        _playerController.PlayerAnimator.SetBool("isSoul", false);
        _playerAnimator.GetComponent<SpriteRenderer>().material = original;// 更换材质
        if (stateParticle!=null)
        {
            stateParticle.gameObject.SetActive(false);
        }
        if(animName!="")
            _playerAnimator.SetBool(animName, false);
        
        CancelInvoke(nameof(TickSoulStatus));// 停止扣除能量
        
        SkillEnd.Invoke();
    }

    // 播放特殊受伤动画
    public void PlayHurtEffect(DamagerBase damager, DamageableBase damageable)
    {
        Vector3 closestPoint = damageable.GetComponent<Collider2D>().ClosestPoint(damager.transform.position);
        Destroy( Instantiate(hurtEffectPrefab, closestPoint, Quaternion.identity),1.5f);
    }

    
    
  
}
