using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class ScenesManager : Singleton<ScenesManager>
{
    /// <summary>
    /// 同步加载场景
    /// </summary>
    /// <param name="sceneName">场景名称</param>
    /// <param name="action">加载后的行为</param>
    public void LoadScenes (string sceneName,UnityAction action)
    {
        SceneManager.LoadScene(sceneName);
        action.Invoke();
    }

    /// <summary>
    /// 异步加载场景
    /// </summary>
    /// <param name="sceneName">场景名称</param>
    /// <param name="action">加载后的行为</param>
    public void LoadScenesAsyn (string sceneName,UnityAction action)
    {
        MonoManager.Instance.StartCoroutine(LoadScenesAsynIE(sceneName,action));
    }

    // 按名称异步加载场景，action是场景加载完成后需要执行的方法
    private IEnumerator LoadScenesAsynIE (string sceneName,UnityAction action)
    {
        AsyncOperation ao = SceneManager.LoadSceneAsync(sceneName); // 这个是真的异步
        
        while(!ao.isDone) // 如果ao操作没有完成
        {
            EventCenter<String>.Instance.TiggerEvent("loading", ao.progress); // 向事件中心分发事件
            yield return ao.progress; // 每一帧返回进度
        }

        yield return ao; // 跳出协程，剩下的下一帧再继续
        action.Invoke();
    }
}
