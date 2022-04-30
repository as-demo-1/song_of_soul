using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
<<<<<<< HEAD
/// 
/// å¯¹ä¸¤ç±»ç›®æ ‡ä¸åŒä¼¤å®³çš„damagerçš„åŸºç±»   ä¾‹å¦‚é…¸æ°´,ç©å®¶çš„æ”»å‡»
/// </summary>ä½œè€…ï¼šé’ç“œ
public class TwoTargetDamager : DamagerBase
{
    public LayerMask hittableLayers2;//å¦ä¸€ç›®æ ‡
    public int damage2;//å¯¹å¦ä¸€ç›®æ ‡çš„ä¼¤å®³


    public override int getDamage(DamageableBase target)//æ ¹æ®ç›®æ ‡è¿”å›ä¼¤å®³
=======
/// ×÷Õß£ºÇà¹Ï
/// ¶ÔÁ½¸öÄ¿±ê²»Í¬ÉËº¦µÄdamagerµÄ»ùÀà   ÀıÈçËáË®,Íæ¼ÒµÄ¹¥»÷
/// </summary>
public class TwoTargetDamager : DamagerBase
{
    public LayerMask hittableLayers2;//ÁíÒ»Ä¿±ê
    public int damage2;//¶ÔÁíÒ»Ä¿±êµÄÉËº¦


    public override int getDamage(DamageableBase target)//¸ù¾İÄ¿±ê·µ»ØÉËº¦
>>>>>>> 30f6fd9d (damage test)
    {
        if (hittableLayers2.Contains(target.gameObject))
        {
            return damage2;
        }
        else
        {
            return damage;
        }
    }

<<<<<<< HEAD
<<<<<<< HEAD
    protected override void makeDamage(DamageableBase Damageable)
    {
        makeDamageEvent.Invoke(this, Damageable);
    }

=======
>>>>>>> 30f6fd9d (damage test)
=======
    protected override void makeDamage(DamageableBase Damageable)
    {

    }

>>>>>>> 8f8dac1b (damage test_1)
}
