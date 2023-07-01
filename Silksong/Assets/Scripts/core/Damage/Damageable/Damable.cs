
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
        takeDamageEvent.Invoke(damager, this); // 在这里激活了伤害计算判断
        if(damager!=null)
        {
            damageDirection = damager.transform.position - transform.position;//这个对象还有方向参数啊，可能指向受击击退
            Vector2 beatBack = damager.beatBackVector;
            if (beatBack != Vector2.zero && beatBackRate != 0)
            {
                beatBack *= beatBackRate;//这个参数来源不明，怀疑代表了击退率，可能存在默认值，在比例变大下击退距离变大
                if (damageDirection.x > 0)
                {
                    beatBack.x = beatBack.x * -1;
                }
                StartCoroutine(beatenBack(beatBack));//还真是，这里计算了受击击退，beatback代表方向，在这里乘上了
            }

            if (takeDamageSfxSO)
            {
                //Debug.Log("creat hitted sfx");
                Vector2 hittedPosition = Vector2.zero;

                hittedPosition = GetComponent<Collider2D>().bounds.ClosestPoint(damager.transform.position);

                SfxManager.Instance.creatHittedSfx(hittedPosition, hittedPosition - (Vector2)transform.position, takeDamageSfxSO);
            }
        }

        if (takeDamageAudio)
        {
            takeDamageAudio.PlayAudioCue(); // 受击audio在这里
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
