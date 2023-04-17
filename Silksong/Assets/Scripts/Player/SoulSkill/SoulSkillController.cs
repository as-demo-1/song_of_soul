using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoulSkillController : MonoBehaviour
{
    private PlayerCharacter _playerCharacter;
    private Animator _playerAnimator;
    #region 灵魂技能
    
    public SoulSkill equpedSoulSkill;
    //public SkillName soulSkillName;// 当前选择的灵魂技能
    public bool inSoulModel;// 是否进入了灵魂状态
    public bool isHenshining;// 正在变身
    public GameObject lightningChainPrefab;
    private LightningChain _lightningChain;
    public GameObject flameGeyserBullet;
    public GameObject flameGeyserPrefab;
    private FlameGeyser _flameGeyser;
    public GameObject shadowBladePrefab;
    private ShadowBlade _shadowBlade;

    public void SoulSkillInit(PlayerController playerController)
    {
        _playerCharacter = playerController.playerCharacter;
        _playerAnimator = playerController.PlayerAnimator;
        
        //初始化所有灵魂技能
        _lightningChain = GameObject.Instantiate(lightningChainPrefab,transform).GetComponent<LightningChain>();
        _lightningChain.Init(playerController, playerController.playerCharacter);
        _lightningChain.gameObject.SetActive(false);
        _lightningChain.transform.localPosition = new Vector3(0, 0, 0);

        _flameGeyser = GameObject.Instantiate(flameGeyserPrefab,transform).GetComponent<FlameGeyser>();
        _flameGeyser.Init(playerController, playerController.playerCharacter);
        flameGeyserBullet.GetComponent<TwoTargetDamager>().damage = _flameGeyser.baseDamage;
        _flameGeyser.gameObject.SetActive(false);
        _flameGeyser.transform.localPosition = new Vector3(0, 0, 0);
        
        _shadowBlade = GameObject.Instantiate(shadowBladePrefab,transform).GetComponent<ShadowBlade>();
        _shadowBlade.Init(playerController, playerController.playerCharacter);
        _shadowBlade.atkDamager.GetComponent<TwoTargetDamager>().damage = _shadowBlade.baseDamage;
        _shadowBlade.gameObject.SetActive(false);
        _shadowBlade.transform.localPosition = new Vector3(0, 0, 0);
        
        ChangeEquip(SkillName.FlameGeyser);
    }
    
    public void TickLightningChain()
    {
        if (_playerCharacter.Mana <= 0)
        {
            _lightningChain.gameObject.SetActive(false);
            _playerCharacter.buffManager.DecreaseBuff(BuffProperty.MOVE_SPEED, _lightningChain.moveSpeedUp);
        }
        if (PlayerInput.Instance.soulSkill.IsValid) 
        {
            _playerAnimator.SetTrigger("castSkill");
            Debug.LogError("R is down");
            if (_lightningChain.isActiveAndEnabled)
            {
                Debug.LogError("light chain is active");
                //_lightningChain.TiggerAtkEvent();
                _lightningChain.gameObject.SetActive(false);
                _lightningChain.enabled = false;
            }
            else
            {
                if (_playerCharacter.Mana < _lightningChain.constPerSec)
                {
                    Debug.LogError("not enough mana");
                }
                else
                {
                    Debug.LogError("cast skill");
                    _lightningChain.gameObject.SetActive(true);
                    _playerCharacter.buffManager.AddBuff(BuffProperty.MOVE_SPEED, _lightningChain.moveSpeedUp);
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
    public void TickSoulSkill()
    {
        if(isHenshining) return;
        if (_playerCharacter.Mana <= 0 && inSoulModel)
        {
            //法力值为0时，自动退出灵魂状态
            equpedSoulSkill.ExitSoulMode();
            equpedSoulSkill.gameObject.SetActive(false);
            equpedSoulSkill.enabled = false;
        }
        
        if (PlayerInput.Instance.soulSkill.IsValid)
        {
            // 按下R键切换灵魂状态
            
            Debug.Log("R is down");

            if (inSoulModel)// 如果处于灵魂状态则退出
            {
                Debug.Log(equpedSoulSkill.name + " is active");
                //equpedSoulSkill.TiggerAtkEvent();
                equpedSoulSkill.ExitSoulMode();
                equpedSoulSkill.gameObject.SetActive(false);
                equpedSoulSkill.enabled = false;
            }
            else
            {
                if (_playerCharacter.Mana < equpedSoulSkill.constPerSec)
                {
                    Debug.Log("not enough mana");
                }
                else
                {
                    Debug.Log("cast skill");
                    //进入灵魂状态，触发对应效果
                    equpedSoulSkill.gameObject.SetActive(true);
                    equpedSoulSkill.enabled = true;
                    equpedSoulSkill.EnterSoulMode();
                }
            }
        }
    }
    public void TriggerFlameGeyser(bool option)
    {

    }
    public void TriggerLightningChain(bool option)
    {

    }
    public void ShootBullet(string _bullet)
    {
        if (_bullet.Equals("FlameGeyser"))
        {
            GameObject go = Instantiate(flameGeyserBullet, transform);
            go.transform.localPosition = Vector3.zero;
            go.transform.SetParent(null);

            /*
            Quaternion.Euler(flameGeyserBullet.transform.rotation.x
           , flameGeyserBullet.transform.localRotation.y
           , PlayerController.Instance.playerInfo.playerFacingRight ? 0.0f : 180.0f));*/

            Vector3 shootDirection = new Vector3(PlayerController.Instance.playerInfo.playerFacingRight ? 1 : -1, 0, 0);
            go.GetComponent<Rigidbody2D>().AddForce(shootDirection * 15f, ForceMode2D.Impulse);

            Destroy(go, 2f);
        }
    }
    
    public void OpenSwingEffect(bool option)
    {
       equpedSoulSkill?.atkDamager.SetActive(option);
    }

    public void ChangeEquip(SkillName skillName)
    {
        _playerAnimator.SetBool("FireAttack", false);
        _playerAnimator.SetBool("ShadowAttack", false);
        _playerAnimator.SetBool("WaveAttack", false);
        
        switch (skillName)
        {
            case SkillName.FlameGeyser:
                equpedSoulSkill = _flameGeyser;
                _playerAnimator.SetBool("FireAttack", true);
                break;
            case SkillName.ShadowBlade:
                _playerAnimator.SetBool("ShadowAttack", true);
                equpedSoulSkill = _shadowBlade;
                break;
            case SkillName.LightningChain:
                equpedSoulSkill = _lightningChain;
                break;
            case SkillName.ArcaneBlast:
                break;
            case SkillName.IceStorm:
                break;
            default:
                break;
        }
    }
    #endregion
    
    
}
