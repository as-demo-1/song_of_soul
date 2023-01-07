using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoulWall : Trigger2DBase
{
    public Animator animator;
    [DisplayOnly] public string guid; 
    private void Awake()
    {
        SaveSystem _saveSystem = GameManager.Instance.saveSystem;
        guid=GetComponent<GuidComponent>().GetGuid().ToString();
        if(MapObjSaveSystem.ContainsObject(guid)||_saveSystem.ContainDestructiblePlatformGUID(guid))
        {
            Destroy(gameObject);
        }
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);
        if(!SoulJump.ifGetSoulJump)
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
        while (info.normalizedTime > 0.99)
        {
            yield return null;
        }
        MapObjSaveSystem.AddNewMapObject(guid);
        Destroy(gameObject);
    }
}
