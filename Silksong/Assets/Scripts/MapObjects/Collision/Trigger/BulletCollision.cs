using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletCollision : Trigger2DBase//子弹的碰撞事件 与伤害无关
{

    public float timeBeforeAutodestruct = -1.0f;
    private float timer=0;

    private void Update()
    {
        if (timeBeforeAutodestruct > 0)
        {
            timer += Time.deltaTime;
            if (timer > timeBeforeAutodestruct)
            {
                Destroy(gameObject);
            }
        }
    }

    protected override void enterEvent()
    {
        Destroy(gameObject);
    }


}
