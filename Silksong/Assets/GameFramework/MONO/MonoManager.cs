using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MonoManager : Singleton<MonoManager>
{
    private MonoController monoController;


    // MonoController中，update和fixed内部沟通的机制
    public void SetInnerInfo(object obj)
    {
        monoController.innerInfo = obj;
    }

    public MonoManager()
    {
        GameObject obj = new GameObject("MonoController");
        monoController = obj.AddComponent<MonoController>();
    }

    /// <summary>
    /// 用Mono去添加需要在update中执行的方法
    /// </summary>
    /// <param name="action">需要在update中更新的方法</param>
    public void AddUpdateEvent(UnityAction action)
    {
        monoController.AddUpdateEvent(action);
    }

    /// <summary>
    /// 移除在update中执行的方法
    /// </summary>
    /// <param name="action">需要移除的方法</param>
    public void RemoveUpdateEvent(UnityAction action)
    {
        monoController.RemoveUpdateEvent(action);
    }

    /// <summary>
    /// 用Mono去添加需要在FixedUpdate中执行的方法
    /// </summary>
    /// <param name="action">需要在FixedUpdate中更新的方法</param>
    public void AddFixedUpdateEvent(UnityAction action)
    {
        monoController.AddFixedUpdateEvent(action);
    }

    /// <summary>
    /// 移除在FixedUpdate中执行的方法
    /// </summary>
    /// <param name="action">需要移除的方法</param>
    public void RemoveFixedUpdateEvent(UnityAction action)
    {
        monoController.RemoveFixedUpdateEvent(action);
    }

    /// <summary>
    /// 开启协程
    /// </summary>
    /// <param name="coroutin">IEnumerator</param>
    /// <returns>coroutin</returns>
    public Coroutine StartCoroutine(IEnumerator coroutin)
    {
        return monoController.StartCoroutine(coroutin);
    }

    /// <summary>
    /// 停止协程
    /// </summary>
    /// <param name="coroutin">IEnumerator</param>
    public void StopCoroutine(IEnumerator coroutin)
    {
        monoController.StopCoroutine(coroutin);
    }
}
