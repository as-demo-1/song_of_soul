using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "anim", menuName = "NewInact/anim")]
public class InactChildAnimSO : InactChildBaseSO
{
    private Animator _animator;
    private AnimationClip _clip;

    public int status;
    public InactChildBaseSO NextChild => _nextChild;

    public override void DoInteract()
    {
        _animator = _inactItemSO.Go.GetComponent<Animator>();
        _clip = _animator.runtimeAnimatorController.animationClips[status - 1];

        AddAnimationEvent("Next", _clip.length);
        _animator.SetInteger("status", status);
        _status = EInteractStatus.DO_INTERACT;
    }

    public override void Finish()
    {
        CleanAllEvent();
        _animator.SetInteger("status", 0);
        base.Finish();
    }

    /// <summary>
    /// 添加动画事件
    /// </summary>
    /// <param name="_animator"></param>
    /// <param name="_eventFunctionName">事件方法名称</param>
    /// <param name="_time">添加事件时间。单位：秒</param>
    private void AddAnimationEvent(string eventFunctionName, float time)
    {
        Debug.Log("------" + status + "-------");
        AnimationEvent _event = new AnimationEvent();
        _event.functionName = eventFunctionName;
        _event.time = time;
        _clip.AddEvent(_event);
        _animator.Rebind();
    }

    /// <summary>
    /// 清除所有事件
    /// </summary>
    private void CleanAllEvent()
    {
        _clip.events = default(AnimationEvent[]);
        Debug.Log("清除所有事件");
    }
}
