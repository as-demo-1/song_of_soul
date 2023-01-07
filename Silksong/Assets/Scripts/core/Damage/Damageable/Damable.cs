
using UnityEngine;
public class Damable : DamageableBase
{
    protected override void Awake()
    {
        base.Awake();     
    }
    public override void takeDamage(DamagerBase damager)
    {
        damageDirection = damager.transform.position - transform.position;

        if(takeDamageAudio)
        {
            takeDamageAudio.PlayAudioCue();
        }

        if(takeDamageSfxSO)
        {
            //Debug.Log("creat hitted sfx");
            Vector2 hittedPosition = Vector2.zero;

            hittedPosition=GetComponent<Collider2D>().bounds.ClosestPoint(damager.transform.position);

            SfxManager.Instance.creatHittedSfx(hittedPosition, hittedPosition-(Vector2)transform.position ,takeDamageSfxSO);
        }

    }


}
