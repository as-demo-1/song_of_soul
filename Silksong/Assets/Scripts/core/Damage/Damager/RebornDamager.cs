using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 
/// /对玩家造成伤害后 将玩家送至重生点的damager 如酸水  重生暂未实现 
/// </summary>作者：青瓜
public class RebornDamager:TwoTargetDamager
{
    //private SceneManger sceneManger;
    public LayerMask rebornLayer;
    void Start()
    {
        //sceneManger = GameObject.Find("Enviroment").GetComponent<SceneManger>();
    }

    protected  override void makeDamage(DamageableBase damageable)//
    {
        // base.makeDamge(Damageable);
      /*  if(rebornLayer.Contains(damageable.gameObject))
        {
            sceneManger.rebornPlayer();
        }*/

    }
}
