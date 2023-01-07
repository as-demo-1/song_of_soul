using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchGate : Damable
{
    
    public Transform Gate;
    public Transform targetPos;
    public float speed;
    public bool ifSave;
    public bool ifChange;

    string _guid;
    bool ifopen;
    bool ifMoving;
    Vector3 startPos;
    SaveSystem _saveSystem;
    Animator anim;
    protected override void Awake()
    {
        base.Awake();
        startPos = Gate.position;
        Debug.Log(startPos);
        _saveSystem = GameManager.Instance.saveSystem;
        _guid = GetComponent<GuidComponent>().GetGuid().ToString();
        anim = GetComponent<Animator>();
        if (ifSave&&(MapObjSaveSystem.ContainsObject(_guid)||_saveSystem.ContainDestructiblePlatformGUID(_guid)))
        {
            ifopen = true;
            Gate.position = targetPos.position;
            Debug.Log("已经打开了");
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
            if (ifSave)
            {
                MapObjSaveSystem.AddNewMapObject(_guid);
            }
        }
        else
        {
            StartCoroutine(MoveGate(startPos));
            if (ifSave)
            {
                MapObjSaveSystem.RemoveMapObject(_guid);
            }
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
