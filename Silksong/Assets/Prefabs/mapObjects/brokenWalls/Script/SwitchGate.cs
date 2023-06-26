using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchGate : Damable
{
    
    public Transform Gate;
    public Transform targetPos;
    public float speed;
    public bool ifChange;

    bool ifopen;
    bool ifMoving;
    Vector3 startPos;
    Animator anim;
    protected override void Awake()
    {
        base.Awake();
        startPos = Gate.position;
        Debug.Log(startPos);
        anim = GetComponent<Animator>();

        BoolGamingSave gamingSave;
        if (TryGetComponent(out gamingSave) && !gamingSave.ban)
        {
            bool error;
            bool open = gamingSave.loadGamingData(out error);
            if (error) return;
            if (open)  Gate.position = targetPos.position;
            ifopen = open;
        }
    }

    public void WhenHit()
    {
        if ((!ifChange && ifopen)||ifMoving) return;
        ifopen = !ifopen;
        anim.SetTrigger("SwitchTrigger");
        if (ifopen)
        {
            StartCoroutine(MoveGate(targetPos.position));
        }
        else
        {
            StartCoroutine(MoveGate(startPos));
        }
        BoolGamingSave gamingSave;
        if (TryGetComponent(out gamingSave) &&!gamingSave.ban)
        {
            gamingSave.saveGamingData(ifopen);
        }
    }
    IEnumerator MoveGate(Vector3 targetPos)
    {
        ifMoving = true;
        while(Vector2.Distance(Gate.position, targetPos) > 0.05)
        {
            Gate.position=Vector2.MoveTowards(Gate.position,targetPos,speed*Time.deltaTime);
            yield return null;
        }
        ifMoving = false;
    }
}
