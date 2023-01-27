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
            hpUI.setRepresentedDamable(playerDamable);
            hpUI.ChangeHitPointUI(playerDamable);
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
    private PlayerStatusMenu hpUI;
    
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
        hpUI.setRepresentedDamable(playerDamable);
        playerDamable.setCurrentHp(maxHp);
        MaxMana = Constants.playerInitialMaxMana;
        Mana = MaxMana;
    }
    void Start()
    {
        GameObject gamingUI = GameObject.FindGameObjectWithTag("UIMenu_PlayerStatus");
        if (gamingUI == null) return;

        hpUI = gamingUI.GetComponentInChildren<PlayerStatusMenu>();
        onManaChangeEvent.AddListener(changeManaBall);

        playerInit();

        hpUI.ChangeManaMax(this);
        hpUI.ChangeManaValue(this);
    }

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
    /// 受伤时获得能量
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
        hpUI.ChangeManaValue(playerCharacter);
    }
   // -----------------------------------------------------------------------------
    protected void addMaxHp(int number)
    {
        MaxHp += number;
    }


    private int coldValue;

    public void reduceColdValue(int value){
        coldValue -= value;
    }

}
