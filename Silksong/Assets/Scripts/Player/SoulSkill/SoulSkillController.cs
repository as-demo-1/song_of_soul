using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public class SoulSkillItem
{
    public SkillName SkillName;
    public GameObject skillPrefab;
    public SoulSkill soulSkill;
    public bool hasLearned;
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
            skill.soulSkill.transform.localPosition = Vector3.zero;
            foreach (var skillName in GameManager.Instance.saveSystem.SaveData.learnedSoulSkills)
            {
                if (skill.SkillName.Equals(skillName))
                {
                    skill.hasLearned = true;
                    equpedSoulSkill = skill.soulSkill;
                }
            }
        }

        //ChangeEquip(SkillName.FlameGeyser);
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
        
        if (PlayerInput.Instance.soulSkill.Up && GameManager.Instance.saveSystem.getLearnedSkill(EPlayerStatus.CanCastSkill))
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
                if (equpedSoulSkill==null) return;
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
            GameObject go = Instantiate(flameGeyser.bulletPrefab.gameObject, transform);
            go.transform.localPosition = Vector3.zero;
            go.transform.SetParent(null);
            go.GetComponent<PlayerSkillDamager>().makeDamageEvent.AddListener(flameGeyser.PlayHurtEffect);

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
        if (!GameManager.Instance.saveSystem.SaveData.learnedSoulSkills.Contains(skillName))
        {
            Debug.Log("未习得该技能");
            return;
        }
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

    public void LearnSkill(SkillName skillName)
    {
        if (!GameManager.Instance.saveSystem.SaveData.learnedSoulSkills.Contains(skillName))
        {
            GameManager.Instance.saveSystem.SaveData.learnedSoulSkills.Add(skillName);
        }
        foreach (var skill in soulSkillList)
        {
            if (skill.SkillName.Equals(skillName))
            {
                skill.hasLearned = true;
            }
        }
    }


}
