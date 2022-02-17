using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCharacter : MonoBehaviour
{
    public int maxHp;
    public int maxSoul;
    public int soul;
    public int money;
    private HpDamable playerDamable;
    private PlayerHpUI hpUI;
    /// <summary>
    /// 结算buff 获得最终的攻击加魂数量
    /// </summary>
    /// <returns></returns>
    public int getAttackGainSoulNumber()
    {
        return Constants.playerAttackGainSoul;
    }
    private void Awake()
    {
        playerDamable = GetComponent<HpDamable>();
        hpUI = GameObject.FindGameObjectWithTag("GamingUI").GetComponentInChildren<PlayerHpUI>();
    }
    public void playerInit()
    {
        playerDamable.maxHp = maxHp;
        hpUI.setRepresentedDamable(playerDamable);
        playerDamable.setHp(maxHp);
    }
    void Start()
    {
        playerInit();
    }

    public void AttackGainSoul(DamagerBase damager,DamageableBase damageable)
    {
        if(damageable.playerAttackCanGainSoul)
        {
            addSoul(getAttackGainSoulNumber());
        }
    }

    protected void addSoul(int number)
    {
        setSoul(soul + number);
    }
    public void setSoul(int number)
    {
        soul = number;
        checkSoul();
    }
    protected virtual void checkSoul()
    {
        soul = Mathf.Clamp(soul, 0, maxSoul);
    }
   // -----------------------------------------------------------------------------
    protected void addMaxHp(int number)
    {
        setMaxHp(maxHp + number);
    }

    public void setMaxHp(int number)
    {
        maxHp = number;
        playerDamable.maxHp = number;
        hpUI.setRepresentedDamable(playerDamable);
        hpUI.ChangeHitPointUI(playerDamable);
    }




}
