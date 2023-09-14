 using System;
 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 using UnityEngine.Serialization;

 /// <summary>
 /// only for Demo
 /// 测试用的解锁魂灵技能触发区
 /// </summary>
public class Trigger2D_SoulSkill : Trigger2DBase
{
    public GameObject tip;
    public KeyCode EnterKey;
    string guid;
    public SkillName skillName;// 要解锁的魂灵技能

    [SerializeField]
    private bool hasGotSkill;

    private bool isEnter;

    private void Update()
    {
        if (isEnter)
        {
            if (Input.GetKeyUp(EnterKey) && !hasGotSkill)
            {
                Debug.Log("learn skill:"+skillName.ToString());
                PlayerController.Instance.SoulSkillController.LearnSkill(skillName);// 技能学习
                hasGotSkill = true;
            }
        }
    }

    private void Awake()
    {
        //SaveSystem _saveSystem = GameManager.Instance.saveSystem;
        //guid = GetComponent<GuidComponent>().GetGuid().ToString();
    }

    protected override void enterEvent()
    {
        //tip.SetActive(true);
        isEnter = true;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {

        
    }

    protected override void exitEvent()
    {
        //tip.SetActive(false);
        isEnter = false;
    }
}
