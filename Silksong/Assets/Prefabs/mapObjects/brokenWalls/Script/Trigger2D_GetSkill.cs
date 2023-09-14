 using System;
 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 using UnityEngine.Serialization;

 /// <summary>
 /// only for Demo
 /// 测试用的技能学习触发区
 /// </summary>
public class Trigger2D_GetSkill : Trigger2DBase
{
    public GameObject tip;
    public KeyCode EnterKey;
    public KeyCode unEquipKey;
    string guid;
    [FormerlySerializedAs("PlayerStatus")] public EPlayerStatus skill;

    [SerializeField]
    private bool hasGotSkill;

    private bool isEnter;

    private void Update()
    {
        if (isEnter)
        {
            if (Input.GetKeyUp(EnterKey) && !hasGotSkill)
            {
                Debug.Log("learn skill:"+skill.ToString());
                PlayerController.Instance.playerStatusDic.learnSkill(skill, true);// 技能学习
                hasGotSkill = true;
            }
            else if (Input.GetKeyUp(unEquipKey) && hasGotSkill)
            {
                Debug.Log("lose skill:"+skill.ToString());
                PlayerController.Instance.playerStatusDic.learnSkill(skill, false);
                //SetPlayerStatusFlag(skillName, GameManager.Instance.saveSystem.getLearnedSkill(skillName),PlayerStatusFlag.WayOfChangingFlag.OverrideLearnFlag);
                hasGotSkill = false;
            }
        }
    }

    private void Awake()
    {
        
    }

    protected override void enterEvent()
    {
        //tip.SetActive(true);
        isEnter = true;
    }
    

    protected override void exitEvent()
    {
        //tip.SetActive(false);
        isEnter = false;
    }
}
