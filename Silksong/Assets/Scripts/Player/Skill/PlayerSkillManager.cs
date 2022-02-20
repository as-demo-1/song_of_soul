using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(PlayerController))]
public class PlayerSkillManager : MonoBehaviour
{

    [Tooltip("The ScriptableObject that holds all the skills")]
    public SkillCollection skillCollection;

    /// <summary>
    /// 用于检测需要解锁的技能是否在skillCollection中
    /// </summary>
    Dictionary<PlayerSkill.SkillName, PlayerSkill> SkillDictionary;

    /// <summary>
    /// 玩家已经解锁的所有技能
    /// </summary>
    public List<PlayerSkill> unlockedPlayerSkillList;

    /// <summary>
    /// 玩家装备的技能
    /// </summary>
    public PlayerSkill equippingPlayerSkill = null;

    private bool _CanCastSkill = true;


    [Header("UI")]
    [SerializeField] Transform pfUnlockedSkillButton;
    [SerializeField] Transform UnlockedSkillContainer;
    [SerializeField] Text equippingSkillText;


    private void Start()
    {
        _CanCastSkill = true;

        SkillDictionary = new Dictionary<PlayerSkill.SkillName, PlayerSkill>();
        foreach (PlayerSkill skill in skillCollection.AllSkills)
        {
            SkillDictionary[skill.Name] = skill;
        }

        // if the equipping skill is none, animator parameter SkillReady will never be true
        PlayerController playerController = gameObject.GetComponent<PlayerController>();
        if (equippingPlayerSkill.Name == PlayerSkill.SkillName.None)
        {
            playerController.PlayerAnimator.SetBool(playerController.animatorParamsMapping.SkillReadyParamHash, false);
        }
    }

    private void Update()
    {
        
        if (equippingPlayerSkill == null)
        {
            equippingSkillText.text = "null";
        }
        else
        {
            equippingSkillText.text = equippingPlayerSkill.Name.ToString();
        }
        
    }



    /// <summary>
    /// 在释放技能后调用，技能开始冷却
    /// </summary>
    public void StartSkillCoolDown()
    {
        StartCoroutine(SkillCoolDownTimer(equippingPlayerSkill.CoolDown));
    }
    IEnumerator SkillCoolDownTimer(float cooldown)
    {
        PlayerController playerController = gameObject.GetComponent<PlayerController>();

        _CanCastSkill = false;
        playerController.PlayerAnimator.SetBool(playerController.animatorParamsMapping.SkillReadyParamHash, false);

        yield return new WaitForSeconds(cooldown);

        _CanCastSkill = true;
        playerController.PlayerAnimator.SetBool(playerController.animatorParamsMapping.SkillReadyParamHash, true);
        yield break;
    }

    /// <summary>
    /// 检测是否可以释放技能
    /// 检测是否装备有技能，技能是否在冷却，蓝够不够释放技能
    /// </summary>
    /// <returns>如果可以释放技能则返回True，不能释放技能则返回False</returns>
    public bool CanCastSkill()
    {
        // TODO: check for mana (or soul)

        Debug.Log(equippingPlayerSkill.Name);

        if (equippingPlayerSkill.Name == PlayerSkill.SkillName.None)
        {
            Debug.LogWarning("You haven't equipped any skill!");
        }
        return _CanCastSkill;
    }


    /// <summary>
    /// 装备一个已解锁的技能
    /// </summary>
    /// <param name="skill"></param>
    public void EquipSkill(PlayerSkill skill)
    {
        //equippingSkillText.text = skill.Name.ToString();
        equippingPlayerSkill = skill;

        // if the equipping skill is not none, set animator parameter SkillReady to true
        PlayerController playerController = gameObject.GetComponent<PlayerController>();
        if (skill.Name != PlayerSkill.SkillName.None)
        {
            playerController.PlayerAnimator.SetBool(playerController.animatorParamsMapping.SkillReadyParamHash, true);
        }
        else
        {
            playerController.PlayerAnimator.SetBool(playerController.animatorParamsMapping.SkillReadyParamHash, false);
        }
        //Debug.Log(equippingPlayerSkill.Name);
    }



    /// <summary>
    /// 解锁一个在SkillCollection中的技能
    /// </summary>
    /// <param name="skillName">要解锁的技能的名字</param>
    public void UnlockSkill(PlayerSkill.SkillName skillName)
    {
        if (SkillDictionary.ContainsKey(skillName))
        {
            unlockedPlayerSkillList.Add(SkillDictionary[skillName]);
        }

        Transform skillbutton = Instantiate(pfUnlockedSkillButton, UnlockedSkillContainer);
        skillbutton.gameObject.GetComponentInChildren<UnityEngine.UI.Text>().text = skillName.ToString();
        skillbutton.gameObject.GetComponent<UnlockedSkillButton>().EquipSkill += () => { EquipSkill(SkillDictionary[skillName]); };
    }



    public void testUnlockDesolateDive()
    {
        print("testing unlock skill");
        UnlockSkill(PlayerSkill.SkillName.DesolateDive);
    }
    public void testUnlockDecendingDark()
    {
        print("testing unlock skill");
        UnlockSkill(PlayerSkill.SkillName.DescendingDark);
    }
}
