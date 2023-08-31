 using System;
 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 using UnityEngine.Serialization;

 /// <summary>
 /// only for Demo
 /// 测试用的技能学习触发区
 /// </summary>
public class Trigger2D_SoulSkill : Trigger2DBase
{
    public GameObject tip;
    public KeyCode EnterKey;
    string guid;
    public SkillName skillName ;

    [SerializeField]
    private bool hasGotSkill;

    private void Update()
    {

    }

    private void Awake()
    {
        //SaveSystem _saveSystem = GameManager.Instance.saveSystem;
        //guid = GetComponent<GuidComponent>().GetGuid().ToString();
    }

    protected override void enterEvent()
    {
        //tip.SetActive(true);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {

        if (Input.GetKeyUp(EnterKey) && !hasGotSkill)
        {
            Debug.Log("learn skill:"+skillName.ToString());
            PlayerController.Instance.SoulSkillController.LearnSkill(skillName);// 技能学习
            hasGotSkill = true;
        }
    }

    protected override void exitEvent()
    {
        //tip.SetActive(false);
    }
}
