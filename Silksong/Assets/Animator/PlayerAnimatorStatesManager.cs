using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

[RequireComponent(typeof(Animator))]
public class PlayerAnimatorStatesManager : MonoBehaviour
{
    public PlayerStatus CurrentPlayerStatus { get; set; }
    public StatusBehaviour PlayerBehaviour { get; set; }

    public void Initialize(StatusBehaviour behaviour)
    {
        this.PlayerBehaviour = behaviour;
    }

    public void BehaviourUpdate()
    {
        PlayerBehaviour.StatusUpdateBehaviour(CurrentPlayerStatus);
    }

    public void ChangePlayerStatus(PlayerStatus newStatus)
    {
        PlayerBehaviour.StatusExitBehaviour(CurrentPlayerStatus);
        CurrentPlayerStatus = newStatus;
        PlayerBehaviour.StatusEnterBehaviour(newStatus);
    }
}
