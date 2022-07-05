using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//todo£∫ Œª∆•≈‰
public enum EPlayerStatus : int
{
    None = 0,
   // CanMove = 1,
    CanJump = 2,
    CanNormalAttack = 4,
    CanSprint = 8,
    CanBreakMoon = 16,
    CanHeal = 32,
    CanCastSkill = 64,
    CanToCat =128,
    CanPlunge = 256,
    CanClimbIdle=512,


}

public class PlayerStatusDic
{
    Dictionary<EPlayerStatus, PlayerStatusFlag> m_StatusDic;

    private PlayerController playerController;

    public PlayerStatusDic(PlayerController playerController,PlayerAnimatorParamsMapping animatorParamsMapping)
    {
        this.playerController = playerController;
        m_StatusDic = new Dictionary<EPlayerStatus, PlayerStatusFlag>
        {
           // {EPlayerStatus.CanMove, new PlayerStatusFlag() },
            {EPlayerStatus.CanJump, new PlayerStatusFlag(animatorParamsMapping.CanJumpParamHash) },
            {EPlayerStatus.CanNormalAttack, new PlayerStatusFlag(animatorParamsMapping.CanNormalAttackParamHash) },
            {EPlayerStatus.CanSprint, new PlayerStatusFlag(animatorParamsMapping.CanSprintParamHash)},
            {EPlayerStatus.CanBreakMoon, new PlayerStatusFlag(animatorParamsMapping.CanBreakMoonParamHash)},
            {EPlayerStatus.CanHeal, new PlayerStatusFlagWithMana(animatorParamsMapping.CanHealParamHas,Constants.playerHealCostMana,playerController.playerCharacter)},
            {EPlayerStatus.CanToCat, new PlayerStatusFlag(animatorParamsMapping.CanToCatParamHas)},
            {EPlayerStatus.CanCastSkill, new PlayerStatusFlagWithMana(animatorParamsMapping.CanCastSkillParamHash, playerController.gameObject.GetComponent<PlayerSkillManager>().equippingPlayerSkill.ManaCost, playerController.playerCharacter)},
            {EPlayerStatus.CanPlunge, new PlayerStatusFlag(animatorParamsMapping.CanPlungeParamHash) },
            {EPlayerStatus.CanClimbIdle, new PlayerStatusFlag(animatorParamsMapping.CanClimbParamHash) },

        };
    }

    public void SetPlayerStatusFlag(EPlayerStatus playerStatus, bool newFlag, PlayerStatusFlag.WayOfChangingFlag calcuteFlagType = PlayerStatusFlag.WayOfChangingFlag.OverrideStatuFlag)
    {
        //Debug.Log(playerStatus);
        PlayerStatusFlag flag = m_StatusDic[playerStatus];
        flag.SetFlag(newFlag, calcuteFlagType);
    }

    public bool getPlayerStatus(EPlayerStatus playerStatus)
    {
        return m_StatusDic[playerStatus].Flag;
    }

    public class PlayerStatusFlag
    {
        protected int animatorParam;
        protected bool StatuFlag;
        protected bool BuffFlag;
        protected bool LearnFlag;
        private bool flag;
        public virtual bool Flag 
        {
            get
            {
                return flag;
            }
            set
            {
                flag = value;
                PlayerController.Instance.PlayerAnimator.SetBool(animatorParam,flag);
            }
        }

        public PlayerStatusFlag(int param)
        {
            animatorParam = param;
            BuffFlag = true;
            LearnFlag = true;
        }

        public void SetFlag(bool newFlag, WayOfChangingFlag setFlagType)
        {
            switch (setFlagType)
            {
                case WayOfChangingFlag.OverrideStatuFlag:
                    StatuFlag = newFlag;
                    break;

                case WayOfChangingFlag.OverrideBuffFlag:
                    BuffFlag = newFlag;
                    break;
                case WayOfChangingFlag.OverrideLearnFlag:
                    LearnFlag = newFlag;
                    break;
                default:
                    break;
            }
    
            calcuteFlag();
        }
        protected virtual void calcuteFlag()
        {
            Flag = BuffFlag & StatuFlag &LearnFlag;

        }          

        public enum WayOfChangingFlag
        {
            OverrideStatuFlag,
            OverrideBuffFlag,
            OverrideLearnFlag,
        }

    }

    public class PlayerStatusFlagWithMana:PlayerStatusFlag
    {
        protected int manaCost;
        protected bool manaIsEnough=true;
        public PlayerStatusFlagWithMana(int param,int manaCost,PlayerCharacter playerCharacter) :base(param)
        {
           this.manaCost = manaCost;
           playerCharacter.onManaChangeEvent.AddListener(calcuteMana);
        }

        protected override void calcuteFlag()
        {
            Flag = BuffFlag & StatuFlag & manaIsEnough & LearnFlag;
        }

        protected void calcuteMana(PlayerCharacter playerCharacter)
        {
            manaIsEnough = playerCharacter.Mana >= manaCost;
            calcuteFlag();
        }
    }

}



