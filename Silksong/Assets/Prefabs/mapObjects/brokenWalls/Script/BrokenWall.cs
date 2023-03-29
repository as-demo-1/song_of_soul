using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrokenWall : HpDamable
{
    public Animator animator;
    public List<Sprite> sprites;
    protected override void die(DamagerBase damager)
    {
        animator.Play("die",0,0);
        StartCoroutine(StartBroke(damager));
    }
    public void ChangeSprite()
    {
        GetComponent<SpriteRenderer>().sprite = sprites[CurrentHp - 1];
    }
    IEnumerator StartBroke(DamagerBase damager)
    {
        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
        GetComponent<Collider2D>().enabled = false;
        while (!info.IsName("die")||info.normalizedTime <= 0.99)
        {
            info = animator.GetCurrentAnimatorStateInfo(0);
            yield return null;
        }
        base.die(damager);
    }
}
