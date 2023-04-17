using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;


[Serializable]
public abstract class SoulSkill : MonoBehaviour
{
    public EPlayerAttackType skillName;
    public int constPerSec = 1;
    public int constPerAttack = 0;
    public float henshinTime;
    public int baseDamage;
    

    //protected PlayerInfomation _playerInfomation;
    protected PlayerCharacter _playerCharacter;
    protected PlayerController _playerController;
    protected Animator _playerAnimator;

    [SerializeField]
    protected ParticleSystem charge;
    [SerializeField]
    protected ParticleSystem henshin;
    [SerializeField]
    protected Material soulMode;
    [SerializeField]
    public GameObject atkDamager;// 挥击效果或伤害

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

    //protected SoulSkill()
    //{
    //    _playerCharacter = GetComponentInParent<PlayerCharacter>();
    //    //_playerInfomation = GetComponentInParent<PlayerInfomation>();
    //}

    private float timer;
    private float timeCounter = 1.0f;
    public void Init(PlayerController playerController, PlayerCharacter playerCharacter)
    {
        this._playerController = playerController;
        this._playerCharacter = playerCharacter;
        this._playerAnimator = _playerController.PlayerAnimator;
        this.original = _playerAnimator.GetComponent<SpriteRenderer>().material;
    }
    protected void Start()
    {
        Debug.LogError("START TIMER UPDATE");
        _playerCharacter = GetComponentInParent<PlayerCharacter>();
        
        //MonoManager.Instance.AddUpdateEvent(Timer.Instance.TimerUpdate);
    }

    protected void OnEnable()
    {
        Debug.LogError("test!!!!!!!!!!!");
        
        
        
        // 使用定时器定时结算灵魂状态的const
        //Timer.Instance.StartTickActionLoop("TickSoulStatus", 0, 10, TickSoulStatus);
    }

    protected void OnDisable()
    {
        Debug.LogError("test disable!!!!!!!!!!!");
        //Timer.Instance.EndTickActionLoop("TickSoulStatus");
    }

    private void Update()
    {
        if (timer < 0)
        {
            TickSoulStatus();
            timer = timeCounter;
        }
        else
        {
            timer -= Time.deltaTime;
        }
    }

    private int debugCnt = 0;
    protected void TickSoulStatus()
    {
        //_playerInfomation.CostMana(constPerSec);
        _playerCharacter.CostMana(constPerSec);
        //Debug.Log("!!!!!!!!! soul skill ticking cost mana"+constPerSec);
        //Debug.LogError("!!!!!!!!! soul skill ticking" + debugCnt);
        //debugCnt++;
    }

    public virtual void EnterSoulMode()
    {
        Sequence sequence = DOTween.Sequence();
        sequence.AppendCallback(() =>
        {
            charge.gameObject.SetActive(true);
            _playerController.SoulSkillController.isHenshining = true;
            _playerAnimator.SetBool("CastSkillIsValid", true);
        });
        sequence.AppendInterval(henshinTime);
        sequence.AppendCallback(() =>
        {
            charge.gameObject.SetActive(false);
            henshin.gameObject.SetActive(true);
            
     
            _playerController.SoulSkillController.inSoulModel = true;
            _playerAnimator.SetBool("isSoul", true);
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
        });
    }

    public virtual void ExitSoulMode()
    {
        _playerController.SoulSkillController.inSoulModel = false;
        _playerController.PlayerAnimator.SetBool("isSoul", false);
        _playerAnimator.GetComponent<SpriteRenderer>().material = original;// 更换材质
        SkillEnd.Invoke();
    }

    
    
  
}
