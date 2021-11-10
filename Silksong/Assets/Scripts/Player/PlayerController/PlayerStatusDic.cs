using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatusDic
{
    Dictionary<EPlayerStatus, PlayerStatusFlag> m_StatusDic;

    public PlayerStatusDic(MonoBehaviour playerController)
    {
        PlayerStatusFlag.GetPlayerController(playerController);
        m_StatusDic = new Dictionary<EPlayerStatus, PlayerStatusFlag>
        {
            {EPlayerStatus.CanMove, new PlayerStatusFlag() },
            {EPlayerStatus.CanJump, new PlayerStatusFlag() },
            {EPlayerStatus.CanNormalAttack, new PlayerStatusFlag() },
        };
    }

    public void SetPlayerStatusFlag(EPlayerStatus playerStatus, bool newFlag, PlayerStatusFlag.WayOfChangingFlag calcuteFlagType = PlayerStatusFlag.WayOfChangingFlag.And)
    {
        m_StatusDic[playerStatus].SetFlag(newFlag, calcuteFlagType);
    }

    public PlayerStatusFlag this[EPlayerStatus playerStatus]
    {
        get { return m_StatusDic[playerStatus]; }
    }

    public static explicit operator Dictionary<EPlayerStatus, PlayerStatusFlag>(PlayerStatusDic dic) => dic.m_StatusDic;


    public class PlayerStatusFlag
    {
        public bool Flag { get; private set; }
        public void SetFlag(bool newFlag, WayOfChangingFlag setFlagType = WayOfChangingFlag.And)
        {
            switch (setFlagType)
            {
                case WayOfChangingFlag.And:
                    Flag &= newFlag;
                    break;
                case WayOfChangingFlag.Override:
                    Flag = newFlag;
                    break;
                //case WayOfChangingFlag.FinalWillUpdateStateMachine:
                //    PlayerController.StartCoroutine(WaitForNextFrameBeforeUpdate(newFlag));
                //    break;
                default:
                    break;
            }

        }

        IEnumerator WaitForNextFrameBeforeUpdate(bool newFlag)
        {
            yield return null;
            this.Flag = newFlag;
        }

        public enum WayOfChangingFlag
        {
            //&=
            And,
            //=
            Override,
            ////next frame before update; = 
            //FinalWillUpdateStateMachine,
        }
        private static MonoBehaviour PlayerController { get; set; }
        public static void GetPlayerController(MonoBehaviour monoBehaviour) => PlayerController = monoBehaviour;
        public static implicit operator bool(PlayerStatusFlag playerStatus) => playerStatus.Flag;
    }
}

//todo£∫ Œª∆•≈‰
public enum EPlayerStatus : int
{
    None = 0,
    CanMove = 1,
    CanJump = 2,
    CanNormalAttack = 4,
}
