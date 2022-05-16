using UnityEngine;
/// <summary>
/// 漩涡 主动卷入玩家的传送触发器
/// </summary>作者：Nothing
public class WhirlPool : SceneTransitionPoint
{
    private bool canTrans;
    public int cnt = 1;

    void Update()
    {
        if (canTrans)
        {
            enterEvent();       //移动到指定场景出口
            cnt = 0;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            //TODO:玩家被卷入动画
            canTrans = true;
        }
    }
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            canTrans = false;
            cnt = 1;
        }
    }
}
