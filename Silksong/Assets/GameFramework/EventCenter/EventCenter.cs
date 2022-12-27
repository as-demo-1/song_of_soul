using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 事件中心，负责事件的接收与执行
/// </summary>
public class EventCenter : Singleton<EventCenter> {
    // 为了省事，这里用了一个参数的Action，后续可以优化成泛型
    private Dictionary<string, UnityAction<object>> eventCenter = new Dictionary<string, UnityAction<object>> ();

    /// <summary>
    /// 添加监听，监听事件并执行方法
    /// </summary>
    /// <param name="eventName">事件名称</param>
    /// <param name="action">需要触发的行为</param>
    public void AddEventListener (string eventName, UnityAction<object> action) {
        if (eventCenter.ContainsKey (eventName)) {
            eventCenter[eventName] += action;
        } else {
            eventCenter.Add (eventName, action);
        }
    }

    /// <summary>
    /// 移除对事件的监听
    /// </summary>
    /// <param name="eventName">事件名称</param>
    /// <param name="action">需要移除的方法</param>
    public void RemoveEventListener (string eventName, UnityAction<object> action) {
        if (eventCenter.ContainsKey (eventName)) {
            eventCenter[eventName] -= action;
        }
    }

    /// <summary>
    /// 向事件中心发送事件通知
    /// </summary>
    /// <param name="eventName">事件名称</param>
    /// <param name="obj">附加信息</param>
    public void TiggerEvent (string eventName, object obj) {
        if (eventCenter.ContainsKey (eventName)) {
            eventCenter[eventName].Invoke (obj);
        }
    }
    /// <summary>
    /// 清空事件中心
    /// </summary>
    public void ClearEventCenter () {
        eventCenter.Clear ();
    }
}