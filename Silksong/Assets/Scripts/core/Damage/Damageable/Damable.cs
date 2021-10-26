

public class Damable : DamageableBase
{
    public override void takeDamage(DamagerBase damager)
    {
        //  hittedEffect();
        damageDirection = damager.transform.position - transform.position;
        EventsManager.Instance.Invoke(gameObject,EventType.onTakeDamage);
    }

    /*protected virtual void hittedEffect()//受击效果 或有必要以事件形式触发
    {
        //Debug.Log(gameObject.name + " is hitted");
    }*/

}
