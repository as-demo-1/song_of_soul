using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 挂载于角色动画器上，用于响应动画事件
/// 作者：次元
/// </summary>
public class AnimEvent : MonoBehaviour
{
   
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CostSoulSkill(int _skill)
    {
        SkillName skillName = (SkillName)_skill;
        switch (skillName)
        {
            case SkillName.FlameGeyser:
                break;
            case SkillName.ShadowBlade:
                break;
            case SkillName.LightningChain:
                break;
            case SkillName.ArcaneBlast:
                break;
            case SkillName.IceStorm:
                break;
            default:
                break;
        }

    }

    /// <summary>
    /// 烈焰刀 Animation Event
    /// </summary>
    public void CostFlameGeyser()
    {
        PlayerController.Instance.ShootBullet("FlameGeyser");
    }
}
