using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
public class PlayerCharacter : MonoBehaviour
{

    [SerializeField]
    private int maxHp;
    public int MaxHp
    {
        get { return maxHp; }
        set
        {
            maxHp = value;
            playerDamable.MaxHp = maxHp;
            statuMenu.setRepresentedDamable(playerDamable);
            statuMenu.ChangeHitPointUI(playerDamable);
        }
    }


    [SerializeField]
    private int maxMana;
    public int MaxMana
    {
        get { return maxMana; }
        set
        {
            maxMana = value;
        }
    }


    [SerializeField]
    private int mana;
    public int Mana
    {
        get { return mana; }
        set
        {
            mana = Mathf.Clamp(value, 0, maxMana);
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

    public HpDamable playerDamable;
    private PlayerStatusMenu statuMenu;
    
    public UnityEvent<PlayerCharacter> onManaChangeEvent;
    //private PlayerController playerController;

    public int gluedCount;

    [SerializeField]
    private CharmListSO CharmListSO = default;

  
    private void Awake()
    {
        playerDamable = GetComponent<HpDamable>();

        //playerController = GetComponent<PlayerController>();
    }
    public void playerInit()
    { 
        playerDamable.MaxHp = maxHp;
        statuMenu.setRepresentedDamable(playerDamable);
        playerDamable.setCurrentHp(maxHp);
        MaxMana = Constants.playerInitialMaxMana;
        Mana = MaxMana;
    }
    void Start()
    {
        GameObject gamingUI = GameObject.FindGameObjectWithTag("UIMenu_PlayerStatus");
        if (gamingUI == null) return;

        statuMenu = gamingUI.GetComponentInChildren<PlayerStatusMenu>();
        onManaChangeEvent.AddListener(changeManaBall);

        playerInit();

        statuMenu.ChangeManaMax(this);
        statuMenu.ChangeManaValue(this);
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
    /// <summary>
    /// get mana when hurt
    /// </summary>
    /// <param name="damager"></param>
    /// <param name="damageable"></param>
    public void HurtGainMana(DamagerBase damager, DamageableBase damageable)
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
   // hp-----------------------------------------------------------------------------
    protected void addMaxHp(int number)
    {
        MaxHp += number;
    }

    // cold-----------------------------------------------------------------------------
    private int coldValue;

    public void reduceColdValue(int value){
        coldValue -= value;
    }

}
