

public class Damable : DamageableBase
{
    protected override void Awake()
    {
        base.Awake();     
    }
    public override void takeDamage(DamagerBase damager)
    {
        damageDirection = damager.transform.position - transform.position;
        //EventsManager.Instance.Invoke(gameObject,EventType.onTakeDamage);
        takeDamageEvent.Invoke(damager,this);
    }


}
