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

    public void SetPlayerStatusFlag(EPlayerStatus playerStatus, bool newFlag, PlayerStatusFlag.WayOfChangingFlag calcuteFlagType = PlayerStatusFlag.WayOfChangingFlag.Override)
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
        public bool BuffFlags { get; private set; } = true;
        private bool m_Flag = true;
        public bool Flag 
        {
            get
            {
                return m_Flag & BuffFlags;
            }
            private set
            {
                m_Flag = value;
            }
        }
        public void SetFlag(bool newFlag, WayOfChangingFlag setFlagType = WayOfChangingFlag.Override)
        {
            switch (setFlagType)
            {
                case WayOfChangingFlag.Override:
                    Flag = newFlag;
                    break;
                case WayOfChangingFlag.AndBuffFlag:
                    BuffFlags &= newFlag;
                    break;
                case WayOfChangingFlag.OverrideBuffFlags:
                    BuffFlags = newFlag;
                    break;
                default:
                    break;
            }
        }

        //IEnumerator WaitForNextFrameBeforeUpdate(bool newFlag)
        //{
        //    yield return null;
        //    this.Flag = newFlag;
        //}

        public enum WayOfChangingFlag
        {
            Override,
            AndBuffFlag,
            OverrideBuffFlags,
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
