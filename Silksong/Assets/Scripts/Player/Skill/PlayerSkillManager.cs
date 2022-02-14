using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class PlayerSkillManager : MonoBehaviour
{

    [Tooltip("管理所有技能的ScriptableObject")]
    public SkillCollection skillCollection;

    [Header("所有已解锁的技能")]
    public List<PlayerSkill> unlockedPlayerSkillList;

    [Header("装备中的技能")]
    public PlayerSkill equippingPlayerSkill = null;

    private bool _CanCastSkill = true;


    [Header("UI相关")]
    [SerializeField] Transform pfUnlockedSkillButton;
    [SerializeField] Transform UnlockedSkillContainer;
    [SerializeField] UnityEngine.UI.Text equippingSkill;


    private void Start()
    {
        _CanCastSkill = true;
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
    /// 检测是否装备有技能，技能是否在冷却，魂儿够不够释放技能
    /// </summary>
    /// <returns>如果可以释放技能则返回True，不能释放技能则返回False</returns>
    public bool CanCastSkill()
    {
        // TODO: check for mana (or soul)

        if (equippingPlayerSkill.Name == PlayerSkill.SkillName.None)
        {
            Debug.LogWarning("没有装备技能！");
        }
        return _CanCastSkill;
    }


    /// <summary>
    /// 装备一个已解锁的技能
    /// </summary>
    /// <param name="skill"></param>
    public void EquipSkill(PlayerSkill skill)
    {
        equippingSkill.text = skill.Name.ToString();
        equippingPlayerSkill = skill;
    }



    /// <summary>
    /// 检测要解锁的技能的名字是否在SkillCollection中
    /// 如果在SkillCollection里，就解锁
    /// 从教程UI那边偷的
    /// </summary>
    /// <param name="skillName">要解锁的技能的名字</param>
    public void UnlockSkill(PlayerSkill.SkillName skillName)
    {

        Dictionary<PlayerSkill.SkillName, PlayerSkill> SkillDictionary = new Dictionary<PlayerSkill.SkillName, PlayerSkill>();
        foreach (PlayerSkill skill in skillCollection.AllSkills)
        {
            SkillDictionary[skill.Name] = skill;
        }
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
    public void testEquipSkill()
    {
        print("testing equip skill");
        EquipSkill(unlockedPlayerSkillList[0]);
    }
}
