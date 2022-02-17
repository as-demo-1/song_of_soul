using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPoint : Trigger2DBase
{
    public bool respawnFacingLeft;
    protected override void enterEvent()
    {
        GameObjectTeleporter.Instance.playerRebornPoint = transform.position;
    }
}
