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
        //判断是横向的墙还是纵向的墙
        breakBySprint = collider.size.x > collider.size.y ? false : true;
    }
    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);
        Debug.Log(GameManager.Instance.saveSystem.haveSoulJump());
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
        GetComponent<Collider2D>().enabled = false;
        while (!info.IsName("die") || info.normalizedTime <= 0.99)
        {
            info = animator.GetCurrentAnimatorStateInfo(0);
            yield return null;
        }
        Destroyed_StableSave stableSave;
        if(TryGetComponent(out stableSave) &&!stableSave.ban)
        {
            GetComponent<Destroyed_StableSave>().saveGamingData(true);
        }
     
        Destroy(gameObject);
    }
}
