

public class Damable : DamageableBase
{
    public override void takeDamage(DamagerBase damager)
    {
        hittedEffect();//或者是event.invoke()
    }

    protected virtual void hittedEffect()//受击效果 或有必要以事件形式触发
    {
        //Debug.Log(gameObject.name + " is hitted");
    }

}
