using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Whale_Howl : AnimationOverAction
{
    public override void OnStart()
    {
        base.OnStart();
        animator.SetTrigger("howl");
    }
}
