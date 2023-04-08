using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrokenWall : HpDamable
{
    public Animator animator;
    protected override void die(DamagerBase damager)
    {
        animator.Play("die");
        StartCoroutine(StartBroke(damager));
    }
    IEnumerator StartBroke(DamagerBase damager)
    {
        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
        while (info.normalizedTime <= 0.99)
        {
            yield return null;
        }
       // Debug.Log("die");
        base.die(damager);
    }
}
