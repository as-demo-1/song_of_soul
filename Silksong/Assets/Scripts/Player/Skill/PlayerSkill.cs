using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerSkill
{
    public enum SkillName
    {
        None,
        DesolateDive,
        DescendingDark,
    }
    public SkillName Name;

    public string Description;

    public int Damage;

    public float ManaCost; // ...or SoulCost?

    public float CoolDown;

    /// <summary>
    /// 发动技能
    /// </summary>
    public void Cast()
    {
        switch (Name)
        {
            default: break;

            case SkillName.DesolateDive:
                DesolateDive();
                break;
            case SkillName.DescendingDark:
                DescendingDark();
                break;
        }
    }



    #region 技能
    // 具体的技能效果的实现，于Cast()里调用

    private void DesolateDive()
    {
        //PlayerController.Instance...
        Debug.Log("释放荒芜俯冲");
    }
    private void DescendingDark()
    {

        Debug.Log("释放黑暗降临");
    }



    #endregion
}
