using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatusBehaviour : StatusBehaviour
{
    public override void StatusEnterBehaviour(PlayerStatus playerStatus)
    {
        
    }
    public override void StatusUpdateBehaviour(PlayerStatus playerStatus)
    {

    }
    public override void StatusExitBehaviour(PlayerStatus playerStatus)
    {

    }
}

public enum PlayerStatus
{
    Idle,
    Walk,
    Attack,
}

public abstract class StatusBehaviour
{
    public abstract void StatusEnterBehaviour(PlayerStatus playerStatus);
    public abstract void StatusUpdateBehaviour(PlayerStatus playerStatus);
    public abstract void StatusExitBehaviour(PlayerStatus playerStatus);
}
