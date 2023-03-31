using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Whale_Rain : Whale_Howl
{
    public override void OnStart()
    {
        base.OnStart();
        WhaleBossManager.Instance.Invoke("iceRainStart",2.5f);
        Debug.Log("Rain");

    }

    public override void OnEnd()
    {
        base.OnEnd();
        WhaleBossManager.Instance.resetIceRainCdTimer();
    }
}
