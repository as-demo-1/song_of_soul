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

    /// <summary>
    /// 记录技能是否在冷却中
    /// 如果在冷却中，则animator的参数SkillReady为false
    /// </summary>
    private bool isSkillInCoolDown = false;


    [Header("UI")]
    [SerializeField] Transform pfUnlockedSkillButton;
    [SerializeField] Transform UnlockedSkillContainer;
    [SerializeField] Text equippingSkillText;


    private void Start()
    {
        isSkillInCoolDown = false;

        SkillDictionary = new Dictionary<PlayerSkill.SkillName, PlayerSkill>();
        foreach (PlayerSkill skill in skillCollection.AllSkills)
        {
            SkillDictionary[skill.Name] = skill;
        }

        // 如果PlayerCanCastSkill()返回false，则animator的参数SkillReady为false
        // 在装备一个新技能的时候会再进行一遍判定
        PlayerController playerController = gameObject.GetComponent<PlayerController>();
        playerController.PlayerAnimator.SetBool(playerController.animatorParamsMapping.SkillReadyParamHash, PlayerCanCastSkill());
    }

    private void Update()
    {
        // 用于debug，以后不用了就可以删掉
      /*  if (equippingPlayerSkill == null)
        {
            equippingSkillText.text = "null";
        }
        else
        {
            equippingSkillText.text = equippingPlayerSkill.Name.ToString();
        }*/
        
    }

    /// <summary>
    /// 发动技能！！！
    /// </summary>
    public void Cast()
    {
        switch (equippingPlayerSkill.Name)
        {
            default: break;

            case PlayerSkill.SkillName.VengefulSpirit:
                VengefulSpirit();
                break;
            case PlayerSkill.SkillName.DesolateDive:
                DesolateDive();
                break;
            case PlayerSkill.SkillName.DescendingDark:
                DescendingDark();
                break;
        }
        StartSkillCoolDown();
    }

    #region 管理技能的函数

    /// <summary>
    /// 在释放技能后调用，技能开始冷却
    /// </summary>
    private void StartSkillCoolDown()
    {
        StartCoroutine(SkillCoolDownTimer(equippingPlayerSkill.CoolDown));
    }
    IEnumerator SkillCoolDownTimer(float cooldown)
    {
        PlayerController playerController = gameObject.GetComponent<PlayerController>();

        isSkillInCoolDown = true;
        playerController.PlayerAnimator.SetBool(playerController.animatorParamsMapping.SkillReadyParamHash, false);

        yield return new WaitForSeconds(cooldown);

        isSkillInCoolDown = false;
        playerController.PlayerAnimator.SetBool(playerController.animatorParamsMapping.SkillReadyParamHash, true);
        yield break;
    }

    /// <summary>
    /// 检测是否可以释放技能
    /// 和animator的参数CanCastSkill不是同一个东西
    /// </summary>
    /// <returns>如果可以释放技能则返回True，不能释放技能则返回False</returns>
    private bool PlayerCanCastSkill()
    {
        // 检测是否装备有技能，技能是否设置了棒棒的prefab，技能是否在冷却
        // 蓝量是否够用在PlayerStatusDic中的PlayerStatusFlagWithMana进行判定
        // 在Start()和每次装备技能时都会调用

        if (equippingPlayerSkill.Name == PlayerSkill.SkillName.None)
        {
            Debug.LogWarning("You haven't equipped any skill!");
            return false;
        }
        else if (equippingPlayerSkill.SkillPrefab == null)
        {
            Debug.LogWarning("The prefab of the equipping skill is empty!");
            return false;
        }
        if (isSkillInCoolDown)
        {
            return false;
        }
        return true;
    }

    /// <summary>
    /// 装备一个已解锁的技能
    /// </summary>
    /// <param name="skill"></param>
    public void EquipSkill(PlayerSkill skill)
    {
        //equippingSkillText.text = skill.Name.ToString();
        equippingPlayerSkill = skill;

        // 如果PlayerCanCastSkill()返回false，则animator的参数SkillReady为false
        PlayerController playerController = gameObject.GetComponent<PlayerController>();
        playerController.PlayerAnimator.SetBool(playerController.animatorParamsMapping.SkillReadyParamHash, PlayerCanCastSkill());
        //Debug.Log(equippingPlayerSkill.Name);
    }

    /// <summary>
    /// 解锁一个在SkillCollection中的技能,不能重复解锁同一技能
    /// </summary>
    /// <param name="skillName">要解锁的技能的名字</param>
    public void UnlockSkill(PlayerSkill.SkillName skillName)
    {
        if (SkillDictionary.ContainsKey(skillName) && !unlockedPlayerSkillList.Contains(SkillDictionary[skillName]))
        {
            unlockedPlayerSkillList.Add(SkillDictionary[skillName]);

            Transform skillbutton = Instantiate(pfUnlockedSkillButton, UnlockedSkillContainer);
            skillbutton.gameObject.GetComponentInChildren<UnityEngine.UI.Text>().text = skillName.ToString();
            skillbutton.gameObject.GetComponent<UnlockedSkillButton>().EquipSkill += () => { EquipSkill(SkillDictionary[skillName]); };
        }
    }

    #endregion

    #region 技能
    // 具体的技能效果的实现，于Cast()里调用

    private void VengefulSpirit()
    {
        Debug.Log("Casting Vengeful Spirit");

        Transform skillTransform = Instantiate(equippingPlayerSkill.SkillPrefab, this.transform.position, Quaternion.identity);
        skillTransform.gameObject.GetComponent<PlayerSkillDamager>().damage = equippingPlayerSkill.Damage;

        Vector3 shootDirection = new Vector3(PlayerController.Instance.playerInfo.playerFacingRight ? 1 : -1, 0, 0);
        skillTransform.gameObject.GetComponent<Rigidbody2D>().AddForce(shootDirection * 15f, ForceMode2D.Impulse);

        Destroy(skillTransform.gameObject, 2f);
    }

    private void DesolateDive()
    {
        Debug.Log("Casting Desolate Dive");
    }

    private void DescendingDark()
    {

        Debug.Log("Casting Descending Dark");
    }



    #endregion

    #region 用于测试的函数
    public void testUnlockDesolateDive()
    {
        //print("testing unlock skill");
        UnlockSkill(PlayerSkill.SkillName.DesolateDive);
    }
    public void testUnlockDecendingDark()
    {
        //print("testing unlock skill");
        UnlockSkill(PlayerSkill.SkillName.DescendingDark);
    }
    public void testUnlockVengefulSpirit()
    {
        //print("testing unlock skill");
        UnlockSkill(PlayerSkill.SkillName.VengefulSpirit);
    }
    #endregion
}
