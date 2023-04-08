using UnityEngine;
/// <summary>
/// 掉落尖刺的触发器
/// </summary>作者：青瓜
public class SpikeDropTrigger : Trigger2DBase
{
    public DropSpikeCollider spike;
    protected override void enterEvent()
    {
        if(spike)
        {
            spike.drop();
            Destroyed_GamingSave gamingSave;
            if (TryGetComponent(out gamingSave) &&!gamingSave.ban)
            {
                gamingSave.saveGamingData(true);
            }
        }
    }
}
