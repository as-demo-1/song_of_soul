
using System.Collections;
using UnityEngine;
public class Damable : DamageableBase
{
    protected override void Awake()
    {
        base.Awake();     
    }
    public override void takeDamage(DamagerBase damager)
    {
        takeDamageEvent.Invoke(damager, this);
        if(damager!=null)
        {
            damageDirection = damager.transform.position - transform.position;
            Vector2 beatBack = damager.beatBackVector;
            if (beatBack != Vector2.zero && beatBackRate != 0)
            {
                beatBack *= beatBackRate;
                if (damageDirection.x > 0)
                {
                    beatBack.x = beatBack.x * -1;
                }
                StartCoroutine(beatenBack(beatBack));
            }



            Vector2 hittedPosition = GetComponent<Collider2D>().bounds.ClosestPoint(damager.transform.position);

        }


    }

    private IEnumerator beatenBack(Vector2 beatBackVector)
    {
        float timer = 0;
        while (timer < Constants.beatBackTime)
        {
            timer += Time.fixedDeltaTime;
            Vector3 t = beatBackVector * (Time.fixedDeltaTime/Constants.beatBackTime);
            //Debug.Log(t);
            GetComponent<Rigidbody2D>().MovePosition(transform.position+t);
            yield return new WaitForSeconds(Time.fixedDeltaTime);
        }
    }


}
