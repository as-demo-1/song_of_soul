using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EPlayerStatus : int
{
    //None = 0,
    CanRun = 1,
    CanJump = 2,
    CanNormalAttack = 4,
    CanSprint = 8,
    CanBreakMoon = 16,
    CanHeal = 32,
    CanCastSkill = 64,
    CanToCat =128,
    CanPlunge = 256,
    CanClimbIdle=512,
    CanSing=520,
    CanDive=530,
    CanWaterSprint=540,
    CanSwim=550,
    CanHeartSword=560,

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
            {EPlayerStatus.CanRun, new PlayerStatusFlag(animatorParamsMapping.CanRunParamHash,true) },
            {EPlayerStatus.CanJump, new PlayerStatusFlag(animatorParamsMapping.CanJumpParamHash,true) },
            {EPlayerStatus.CanNormalAttack, new PlayerStatusFlag(animatorParamsMapping.CanNormalAttackParamHash,true) },
            {EPlayerStatus.CanSprint, new PlayerStatusFlag(animatorParamsMapping.CanSprintParamHash)},
            {EPlayerStatus.CanBreakMoon, new PlayerStatusFlag(animatorParamsMapping.CanBreakMoonParamHash)},
            {EPlayerStatus.CanHeal, new PlayerStatusFlagWithMana(animatorParamsMapping.CanHealParamHas,Constants.playerHealCostMana,playerController.playerCharacter,true)},
            {EPlayerStatus.CanToCat, new PlayerStatusFlag(animatorParamsMapping.CanToCatParamHas)},
            {EPlayerStatus.CanCastSkill, new PlayerStatusFlagWithMana(animatorParamsMapping.CanCastSkillParamHash, 0, playerController.playerCharacter)},
            {EPlayerStatus.CanPlunge, new PlayerStatusFlag(animatorParamsMapping.CanPlungeParamHash) },
            {EPlayerStatus.CanClimbIdle, new PlayerStatusFlag(animatorParamsMapping.CanClimbParamHash) },
            {EPlayerStatus.CanSing, new PlayerStatusFlag(animatorParamsMapping.CanSingParamHash) },
            {EPlayerStatus.CanDive, new PlayerStatusFlag(animatorParamsMapping.CanDiveParamHash) },
            {EPlayerStatus.CanWaterSprint, new PlayerStatusFlag(animatorParamsMapping.CanWaterSprintParamHash) },
            {EPlayerStatus.CanSwim, new PlayerStatusFlag(animatorParamsMapping.CanSwimParamHash) },
            {EPlayerStatus.CanHeartSword, new PlayerStatusFlag(animatorParamsMapping.CanHeartSwordParamHash) },
        };
    }

    public void SetPlayerStatusFlag(EPlayerStatus playerStatus, bool newFlag, PlayerStatusFlag.WayOfChangingFlag calcuteFlagType = PlayerStatusFlag.WayOfChangingFlag.OverrideStatuFlag)
    {
        //Debug.Log(playerStatus);
        if (m_StatusDic.ContainsKey(playerStatus) == false) return;
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

        public PlayerStatusFlag(int param,bool bornLearned=false)
        {
            animatorParam = param;
            BuffFlag = true;
            LearnFlag = bornLearned;
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
        public PlayerStatusFlagWithMana(int param,int manaCost,PlayerCharacter playerCharacter,bool bornLearned=false) :base(param,bornLearned)
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


    public void learnSkill(EPlayerStatus skill,bool v)
    {
        SetPlayerStatusFlag(skill, v, PlayerStatusFlag.WayOfChangingFlag.OverrideLearnFlag);
        GameManager.Instance.saveSystem.learnSkill(skill,v);
    }

    public void loadLearnedSkills()
    {
        foreach(EPlayerStatus skillName in Enum.GetValues(typeof(EPlayerStatus)))
        {
            SetPlayerStatusFlag(skillName, GameManager.Instance.saveSystem.getLearnedSkill(skillName),PlayerStatusFlag.WayOfChangingFlag.OverrideLearnFlag);
        }
    }

}



