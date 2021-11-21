using UnityEngine;
/// <summary>
/// 掉落尖刺的触发器
/// </summary>作者：青瓜
public class SpikeDropTrigger : Trigger2DBase
{
    public GameObject spike;
    public float dropSpeed;
    protected override void enterEvent()
    {
        Rigidbody2D rigidbody = spike.GetComponent<Rigidbody2D>();
        if(rigidbody)
        {
            //Debug.Log("drop");
            rigidbody.velocity = new Vector2(0, -dropSpeed);
        }
    }
}
