using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 挂载于角色动画器上，用于响应动画事件
/// 作者：次元
/// </summary>
public class AnimEvent : MonoBehaviour
{
    private SoulSkillController _soulSkillController;
    // Start is called before the first frame update
    void Start()
    {
        _soulSkillController = GetComponentInParent<SoulSkillController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

   

    public void CostSkill(SkillName _skill)
    {
        switch (_skill)
        {
            case SkillName.FlameGeyser:
                _soulSkillController.ShootBullet("FlameGeyser");
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

    public void CostAtk()
    {
        _soulSkillController.OpenSwingEffect(true);
    }
    public void EndAtk()
    {
        _soulSkillController.OpenSwingEffect(false);
    }
}
