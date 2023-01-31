using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoulWall : Trigger2DBase
{
    public Animator animator;
    private bool breakBySprint;

    private void Start()
    {
        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        breakBySprint = collider.size.x > collider.size.y ? false : true;
    }
    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);
        
        if (GameManager.Instance.saveSystem.haveSoulJump())
        {
            if ((breakBySprint && PlayerController.Instance.playerAnimatorStatesControl.CurrentPlayerState==EPlayerState.Sprint
                ) || (!breakBySprint && PlayerController.Instance.playerAnimatorStatesControl.CurrentPlayerState == EPlayerState.Jump))
            {
                die();
                return;
            }
        }
        GetComponent<Collider2D>().isTrigger = false;
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
