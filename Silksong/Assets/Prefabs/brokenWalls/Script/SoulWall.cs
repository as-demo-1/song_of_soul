using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoulWall : Trigger2DBase
{
    public Animator animator;

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);
        if(!GameManager.Instance.saveSystem.haveSoulJump())
            GetComponent<Collider2D>().isTrigger = false;
        else
        {
            die();
        }
    }
    
    private void OnCollisionExit2D(Collision2D collision)
    {
        GetComponent<Collider2D>().isTrigger = true;
    }
    void die()
    {
        animator.Play("die");
        StartCoroutine(StartBroke());
    }
    IEnumerator StartBroke()
    {
        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
        while (info.normalizedTime <= 0.99)
        {
            yield return null;
        }
        GetComponent<Destroyed_StableSave>().saveGamingData(true);
        Destroy(gameObject);
    }
}
