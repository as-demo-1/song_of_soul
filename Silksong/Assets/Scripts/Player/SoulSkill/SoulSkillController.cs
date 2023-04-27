using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public class SoulSkillItem
{
    public SkillName SkillName;
    public GameObject skillPrefab;
    [FormerlySerializedAs("skill")] public SoulSkill soulSkill;
}
public class SoulSkillController : MonoBehaviour
{
    private PlayerCharacter _playerCharacter;
    private Animator _playerAnimator;
    private PlayerController _playerController;

    public SoulSkill equpedSoulSkill; // 当前选择的灵魂技能
    public bool inSoulModel;// 是否进入了灵魂状态
    public bool isHenshining;// 正在变身
    public GameObject flameGeyserBullet;//剑气子弹


    [SerializeField]
    public List<SoulSkillItem> soulSkillList = new List<SoulSkillItem>();

    public void SoulSkillInit(PlayerController playerController)
    {
        _playerController = playerController;
        _playerCharacter = playerController.playerCharacter;
        _playerAnimator = playerController.PlayerAnimator;
        
        
        //初始化所有灵魂技能
        foreach (var skill in soulSkillList)
        {
            skill.soulSkill = GameObject.Instantiate(skill.skillPrefab,transform).GetComponent<SoulSkill>();
            skill.soulSkill.Init(playerController, playerController.playerCharacter);
            skill.soulSkill.gameObject.SetActive(false);
            skill.soulSkill.transform.localPosition = new Vector3(0, 0, 0);
        }

        ChangeEquip(SkillName.FlameGeyser);
    }
    
    // public void TickLightningChain()
    // {
    //     if (_playerCharacter.Mana <= 0)
    //     {
    //         _lightningChain.gameObject.SetActive(false);
    //         _playerCharacter.buffManager.DecreaseBuff(BuffProperty.MOVE_SPEED, _lightningChain.moveSpeedUp);
    //     }
    //     if (PlayerInput.Instance.soulSkill.IsValid) 
    //     {
    //         _playerAnimator.SetTrigger("castSkill");
    //         Debug.LogError("R is down");
    //         if (_lightningChain.isActiveAndEnabled)
    //         {
    //             Debug.LogError("light chain is active");
    //             //_lightningChain.TiggerAtkEvent();
    //             _lightningChain.gameObject.SetActive(false);
    //             _lightningChain.enabled = false;
    //         }
    //         else
    //         {
    //             if (_playerCharacter.Mana < _lightningChain.constPerSec)
    //             {
    //                 Debug.LogError("not enough mana");
    //             }
    //             else
    //             {
    //                 Debug.LogError("cast skill");
    //                 _lightningChain.gameObject.SetActive(true);
    //                 _playerCharacter.buffManager.AddBuff(BuffProperty.MOVE_SPEED, _lightningChain.moveSpeedUp);
    //                 _lightningChain.enabled = true;
    //             }
    //         }
    //     }
    //
    //     if (_lightningChain.isActiveAndEnabled)
    //     {
    //         _lightningChain.TriggerAddElectricMarkEvent();
    //         _lightningChain.UpdateTargetsLink();
    //     }
    // }
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
    
    public void ShootBullet(string _bullet)
    {
        if (_bullet.Equals("FlameGeyser"))
        {
            FlameGeyser flameGeyser = (FlameGeyser)GetSoulSkill(SkillName.FlameGeyser);
            GameObject go = Instantiate(flameGeyser.bullet.gameObject, transform);
            go.transform.localPosition = Vector3.zero;
            go.transform.SetParent(null);

            Vector3 shootDirection = new Vector3(PlayerController.Instance.playerInfo.playerFacingRight ? 1 : -1, 0, 0);
            go.GetComponent<Rigidbody2D>().AddForce(shootDirection * 15f, ForceMode2D.Impulse);

            Destroy(go, 2f);
        }
    }
    
    public void OpenSwingEffect(bool option)
    {
        if(equpedSoulSkill==null) return;
        
        // 修正特效旋转
        if (!_playerController.playerInfo.playerFacingRight)
        {
            equpedSoulSkill.atkObject.transform.rotation =
                Quaternion.Euler(0.0f, 180.0f, 0.0f);
            equpedSoulSkill.atkObject.transform.localScale = new Vector3(-1.0f, 1.0f, -1.0f);
        }
        else
        {
            equpedSoulSkill.atkObject.transform.localRotation =
                Quaternion.identity;
            equpedSoulSkill.atkObject.transform.localScale = Vector3.one;
        }
       equpedSoulSkill.atkObject.SetActive(option);
       _playerAnimator.SetBool("CanRun", !option);
    }

    public void ChangeEquip(SkillName skillName)
    {
        equpedSoulSkill = GetSoulSkill(skillName);
    }

    public SoulSkill GetSoulSkill(SkillName skillName)
    {
        foreach (var skill in soulSkillList)
        {
            if (skill.SkillName.Equals(skillName))
            {
                return skill.soulSkill;
            }
        }

        return null;
    }


}
