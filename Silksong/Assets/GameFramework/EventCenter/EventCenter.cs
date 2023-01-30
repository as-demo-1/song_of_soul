using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum BattleEventType
{
    PlayerNormalAtk,
    LightningChainAtk,
    LightningAddElectricMarkEvent
}

/// <summary>
/// 事件中心，负责事件的接收与执行
/// </summary>
public class EventCenter<T> : Singleton<EventCenter<T>> {
    // 为了省事，这里用了一个参数的Action，后续可以优化成泛型
    private Dictionary<T, UnityAction<object>> eventCenter = new Dictionary<T, UnityAction<object>> ();

    /// <summary>
    /// 添加监听，监听事件并执行方法
    /// </summary>
    /// <param name="eventName">事件名称</param>
    /// <param name="action">需要触发的行为</param>
    public void AddEventListener (T eventType, UnityAction<object> action) {
        if (eventCenter.ContainsKey (eventType)) {
            eventCenter[eventType] += action;
        } else {
            eventCenter.Add (eventType, action);
        }
    }

    /// <summary>
    /// 移除对事件的监听
    /// </summary>
    /// <param name="eventName">事件名称</param>
    /// <param name="action">需要移除的方法</param>
    public void RemoveEventListener (T eventType, UnityAction<object> action) {
        if (eventCenter.ContainsKey (eventType)) {
            eventCenter[eventType] -= action;
        }
    }

    /// <summary>
    /// 向事件中心发送事件通知
    /// </summary>
    /// <param name="eventName">事件名称</param>
    /// <param name="obj">附加信息</param>
    public void TiggerEvent (T eventType, object obj) {
        if (eventCenter.ContainsKey (eventType)) {
            eventCenter[eventType].Invoke (obj);
        }
    }
    /// <summary>
    /// 清空事件中心
    /// </summary>
    public void ClearEventCenter () {
        eventCenter.Clear ();
    }
}