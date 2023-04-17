using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
public class PlayerCharacter : MonoBehaviour
{
    //hp is on the damable Component
    public HpDamable playerDamable;

    [SerializeField]
    private int mana;
    public int Mana
    {
        get { return mana; }
        set
        {
            mana = Mathf.Clamp(value, 0,getMaxMana());
            onManaChangeEvent.Invoke(this);
        }
    }

    [SerializeField]
    private int money;
    public int Money
    {
        get { return money; }
        set
        {
            money = value;
        }
    }

    private PlayerStatusMenu statuMenu;
    
    public UnityEvent<PlayerCharacter> onManaChangeEvent;
    private PlayerController playerController;

    [HideInInspector]
    public int gluedCount;

    public BuffManager buffManager;

  
    private void Awake()
    {
        playerDamable = GetComponent<HpDamable>();
        playerController = GetComponent<PlayerController>();
        buffManager = GetComponent<BuffManager>();
        
    }
    public void playerInit()//load players data,such as maxHp,money..
    {
        refreshMaxHp();
        refreshMaxMana();

        /* GameManager.Instance.saveSystem.learnSkill(EPlayerStatus.CanBreakMoon);
         GameManager.Instance.saveSystem.SaveDataToDisk();*/
        //playerDamable.addTempHp(3,null);

#if UNITY_STANDALONE
        playerController.playerStatusDic.loadLearnedSkills();
#endif
    }
    void Start()
    {
        GameObject gamingUI = GameObject.FindGameObjectWithTag("UIMenu_PlayerStatus");
        if (gamingUI == null) return;

        statuMenu = gamingUI.GetComponentInChildren<PlayerStatusMenu>();
        onManaChangeEvent.AddListener(changeManaBall);

        playerInit();
        buffManager.Init();
    }


    // maxHp-----------------------------------------------------------------------------

    public void refreshMaxHp(bool recoverAllHp = true)
    {
        int maxHp = getMaxHp();
        playerDamable.MaxHp = maxHp;
        if (recoverAllHp) playerDamable.setCurrentHp(maxHp);
        //ui
        statuMenu.setRepresentedDamable(playerDamable);
        statuMenu.ChangeHitPointUI(playerDamable);
    }
    public int getMaxHp()
    {
        int ret = Constants.playerInitialMaxHp;
        //toadd:charm,hpUp
        ret += (int)buffManager.GetBuffProperty(BuffProperty.MAX_HEALTH);
        return ret;
    }

    public int getExtraHP()
    {
        int ret = 0;
        //toadd:charm,hpUp
        ret += (int)buffManager.GetBuffProperty(BuffProperty.EXTRA_HEALTH);
        return ret;
    }
    // maxMana-----------------------------------------------------------------------------
    public void refreshMaxMana(bool recoverAllMana = false)
    {
        int maxMana = getMaxMana();
        if (recoverAllMana) Mana = maxMana;
        //ui
        statuMenu.ChangeManaMax(this);
        statuMenu.ChangeManaValue(this);
    }
    
    public int getMaxMana()
    {
        int ret = Constants.playerInitialMaxMana;
        //toadd:charm,manaUp
        ret += (int)buffManager.GetBuffProperty(BuffProperty.MAX_HEALTH);
        return ret;
    }

    // mana-----------------------------------------------------------------------------
    public int getAttackGainManaNumber(int baseGainValue)
    {
        int ret=baseGainValue;
        ret += (int)buffManager.GetBuffProperty(BuffProperty.ATTACK_MANA);
        return ret;
    }
    public int getHurtGainManaNumber()
    {
        int ret = 0;
        ret += (int)buffManager.GetBuffProperty(BuffProperty.ATTACK_MANA);
        return ret; 
    }
    public int getBlockGainManaNumber()
    {
        int ret = 0;
        ret += (int)buffManager.GetBuffProperty(BuffProperty.BLOCK_MANA);
        return ret; 
    }

    /// <summary>
    /// 受伤回能
    /// </summary>
    /// <param name="damager"></param>
    /// <param name="damageable"></param>
    public void HurtGainMana(DamagerBase damager, DamageableBase damageable)//not used now
    {
        if (true)
        {
            addMana(getHurtGainManaNumber());
        }
    }
    
    /// <summary>
    /// 格挡回能
    /// </summary>
    /// <param name="damager"></param>
    /// <param name="damageable"></param>
    public void BlockGainMana(DamagerBase damager, DamageableBase damageable)//not used now
    {
        if (true)
        {
            addMana(getBlockGainManaNumber());
        }
    }
    public void addMana(int number)
    {
        Mana+=number;
    }
    public void CostMana(int cost)
    {
        Mana -= cost;
    }

    private void changeManaBall(PlayerCharacter playerCharacter)
    {
        statuMenu.ChangeManaValue(playerCharacter);
    }


    // speed-----------------------------------------------------------------------------
    public float getMoveSpeed()
    {
        float finalSpeed;
        if (playerController.playerToCat.IsCat)
        {
            if (playerController.playerToCat.isFastMoving)
                finalSpeed=Constants.PlayerCatFastMoveSpeed;
            else finalSpeed = Constants.PlayerCatMoveSpeed;
        }
        else if (playerController.playerAnimatorStatesControl.CurrentPlayerState == EPlayerState.NormalAttack)
        {
            finalSpeed = Constants.AttackingMoveSpeed;
        }
        else if(playerController.playerAnimatorStatesControl.CurrentPlayerState==EPlayerState.Swim)
        {
            finalSpeed = Constants.PlayerSwimSpeed;
        }
        else finalSpeed = Constants.PlayerMoveSpeed;

        //charm
        finalSpeed += buffManager.GetBuffProperty(BuffProperty.MOVE_SPEED);

        return finalSpeed;
    }

    // normalAttack-----------------------------------------------------------------------------
    public float getNormalAttackCd()
    {
        float ret=0;
        PlayerNormalAttack playerNormalAttack = (PlayerNormalAttack)playerController.playerStatesBehaviour.StateActionsDic[EPlayerState.NormalAttack];
        if(playerNormalAttack.currentAttackStage==EPlayerNormalAttackStage.First)
        {
            ret = Constants.AttackCd_First;
        }
        else if(playerNormalAttack.currentAttackStage==EPlayerNormalAttackStage.Second)
        {
            ret = Constants.AttackCd_Second;
        }
        else if(playerNormalAttack.currentAttackStage==EPlayerNormalAttackStage.Thrid)
        {
            ret = Constants.AttackCd_Third;
        }
        else
        {
            ret = Constants.AttackCd_Up;
        }

        return ret;
    }
    // sprint-----------------------------------------------------------------------------
    public float getSprintCd()
    {
        float ret=Constants.SprintCd;

        return ret;
    }
    // cold-----------------------------------------------------------------------------
    private int coldValue;

    public void reduceColdValue(int value){
        coldValue -= value;
    }
    
    // cd---------------

    public float GetSprintCd()
    {
        float finalCd;
        finalCd = Constants.SprintCd +
                  buffManager.GetBuffProperty(BuffProperty.SPRINT_CD);
        return finalCd;
    }

    // Heal------------------
    public int GetHealValue()
    {
        int finalVal;
        finalVal = Constants.playerHealBaseValue +
                   (int)buffManager.GetBuffProperty(BuffProperty.HEAL_AMOUNT);
        return finalVal;
    }
    public float GetHealTime()
    {
        float finalTime;
        finalTime = Constants.PlayerBaseHealTime +
                    buffManager.GetBuffProperty(BuffProperty.HEAL_SPEED);
        return finalTime;
    }

}
