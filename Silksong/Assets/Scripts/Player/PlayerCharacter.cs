using System.Collections;
using System.Collections.Generic;
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

    [SerializeField]
    private CharmListSO CharmListSO = default;

  
    private void Awake()
    {
        playerDamable = GetComponent<HpDamable>();
        playerController = GetComponent<PlayerController>();
    }
    public void playerInit()//load players data,such as maxHp,money..
    {
        refreshMaxHp();
        refreshMaxMana();

        /* GameManager.Instance.saveSystem.learnSkill(EPlayerStatus.CanBreakMoon);
         GameManager.Instance.saveSystem.SaveDataToDisk();*/

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
        return ret;
    }

    // mana-----------------------------------------------------------------------------
    public int getAttackGainManaNumber()
    {
        int ret=Constants.playerAttackGainSoul;
        if (CharmListSO)
            ret += CharmListSO.CharmAttackGainSoul;
        return ret;
    }
    public int getHurtGainManaNumber()
    {
        return CharmListSO.CharmHurtGainSoul;
    }

    public void AttackGainMana(DamagerBase damager,DamageableBase damageable)
    {
        if(damageable.playerAttackCanGainSoul)
        {
            addMana(getAttackGainManaNumber());
        }
    }
    public void HurtGainMana(DamagerBase damager, DamageableBase damageable)//not used now
    {
        if (true)
        {
            addMana(getHurtGainManaNumber());
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

        //if have 护符  finalSpeed加上护符的属性

        return finalSpeed;
    }


    // cold-----------------------------------------------------------------------------
    private int coldValue;

    public void reduceColdValue(int value){
        coldValue -= value;
    }

}
