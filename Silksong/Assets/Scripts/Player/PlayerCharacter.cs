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
    private PlayerHpUI hpUI;
    private Image ManaBall;

    
    public UnityEvent<PlayerCharacter> onManaChangeEvent;
    //private PlayerController playerController;


  
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
        Mana = 40;
    }
    void Start()
    {
        GameObject gamingUI = GameObject.FindGameObjectWithTag("GamingUI");
        hpUI = gamingUI.GetComponentInChildren<PlayerHpUI>();
        ManaBall = gamingUI.transform.Find("ManaBall").GetComponent<Image>();
        onManaChangeEvent.AddListener(changeManaBall);

        playerInit();
    }

    public int getAttackGainManaNumber()
    {
        return Constants.playerAttackGainSoul;
    }

    public void AttackGainMana(DamagerBase damager,DamageableBase damageable)
    {
        if(damageable.playerAttackCanGainSoul)
        {
            addMana(getAttackGainManaNumber());
        }
    }
    public void addMana(int number)
    {
        Mana+=number;
    }

    private void changeManaBall(PlayerCharacter playerCharacter)
    {
        ManaBall.fillAmount = (float)playerCharacter.Mana / playerCharacter.MaxMana;
    }
   // -----------------------------------------------------------------------------
    protected void addMaxHp(int number)
    {
        MaxHp += number;
    }





}
