using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spring : TwoTargetDamager
{
    public LayerMask rebornLayer;
    [Tooltip("弹簧施加的力(往大了写先)")]
    public Vector2 Force;
    [Tooltip("弹簧复原的时间")]
    public float backTime;
    public float times = 0;
    [Tooltip("弹起后的触发器偏移量")]
    public Vector2 offset;
    Vector2 baseOffset;
    Vector2 childBaseOffset;
    bool IfSpikes = false;
    private void Start()
    {
        baseOffset = GetComponent<Collider2D>().offset;
        childBaseOffset = transform.GetChild(0).GetComponent<Collider2D>().offset;
    }
    protected override void makeDamage(DamageableBase damageable)
    {       
        if (IfSpikes&&rebornLayer.Contains(damageable.gameObject) && (damageable as HpDamable).currentHp > 0)
        {
            base.makeDamage(damageable);
            GameObjectTeleporter.playerReborn();
        }else if (!IfSpikes&&damageable.GetComponent<PlayerController>())
        {
            damageable.GetComponent<PlayerController>().RB.AddForce(Force);
            GetComponent<Animator>().Play("Spring");
        }
    }
    public void SpringState()
    {
        transform.GetChild(0).GetComponent<Collider2D>().offset = new Vector2(0,offset.y-0.02f);
        GetComponent<Collider2D>().offset = offset;
        StartCoroutine(BackToWait());
    }
    public void Back()
    {
        times = 0;StopAllCoroutines();
        transform.GetChild(0).GetComponent<Collider2D>().offset = childBaseOffset;
        GetComponent<Collider2D>().offset = baseOffset;
    }
    IEnumerator BackToWait()
    {
        while (times < backTime)
        {
            times += Time.deltaTime;
            yield return null;
        }
        GetComponent<Animator>().Play("Back");
        times = 0;
    }
    protected override void OnTriggerEnter2D(Collider2D collision)
    {     
        if (collision.GetComponent<MoonLight>())
        {
            GetComponent<Animator>().Play("Spike");
            IfSpikes = true;
        }
        base.OnTriggerEnter2D(collision);
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.GetComponent<MoonLight>())
        {
            GetComponent<Animator>().Play("backSpring");
            IfSpikes = false;
        }
    }
}
