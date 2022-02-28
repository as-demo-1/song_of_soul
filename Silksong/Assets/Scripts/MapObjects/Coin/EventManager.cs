using System;
using System.Collections;
using System.Collections.Generic;

public abstract class invokeActionBase { }

#region 事件实例，最多支持4个参数，参数再多就该考虑是是否需要封装成一个数据类传入了
public class invokeAction : invokeActionBase
{
    public Action aciton { get; private set; }
    public invokeAction(Action action)
    {
        this.aciton = action;
    }
    public void Invoke()
    {
        aciton.Invoke();
    }
}

public class invokeAction<T> : invokeActionBase {
    public Action<T> aciton { get; private set; }
    public invokeAction(Action<T> action) {
        this.aciton = action;
    }
    public void Invoke(T arg) {
        aciton.Invoke(arg);
    }
}
public class invokeAction<T1,T2> : invokeActionBase
{
    public Action<T1, T2> aciton { get; private set; }
    public invokeAction(Action<T1, T2> action)
    {
        this.aciton = action;
    }
    public void Invoke(T1 arg1,T2 arg2)
    {
        aciton.Invoke(arg1, arg2);
    }
}

public class invokeAction<T1, T2,T3> : invokeActionBase
{
    public Action<T1, T2,T3> aciton { get; private set; }
    public invokeAction(Action<T1, T2, T3> action)
    {
        this.aciton = action;
    }
    public void Invoke(T1 arg1, T2 arg2, T3 arg3)
    {
        aciton.Invoke(arg1, arg2, arg3);
    }
}

public class invokeAction<T1, T2,T3,T4> : invokeActionBase
{
    public Action<T1, T2, T3, T4> aciton { get; private set; }
    public invokeAction(Action<T1, T2, T3, T4> action)
    {
        this.aciton = action;
    }
    public void Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4)
    {
        aciton.Invoke(arg1, arg2, arg3,arg4);
    }
}

#endregion

public class EventManager : MonoSingleton<EventManager>
{
    public readonly Dictionary<EventType, List<invokeActionBase>> eventDict = new Dictionary<EventType, List<invokeActionBase>>();

    #region  外部暴露的注册方法，会在这里先生成对应参数的事件实例，再注册到字典的list中
    public void Register(EventType eventType, Action action)
    {
        Register(eventType, new invokeAction(action));
    }
    public void Register<T>(EventType eventType, Action<T> action)
    {
        Register(eventType, new invokeAction<T>(action));
    }
    public void Register<T1,T2>(EventType eventType, Action<T1,T2> action)
    {
        Register(eventType, new invokeAction<T1, T2>(action));
    }
    public void Register<T1, T2,T3>(EventType eventType, Action<T1, T2, T3> action)
    {
        Register(eventType, new invokeAction<T1, T2, T3>(action));
    }
    public void Register<T1, T2, T3,T4>(EventType eventType, Action<T1, T2, T3, T4> action)
    {
        Register(eventType, new invokeAction<T1, T2, T3, T4>(action));
    }
    #endregion
    //内部统一的事件注册方法
    private void Register(EventType eventType, invokeActionBase action)
    {
        List<invokeActionBase> actions = null;
        if (!eventDict.TryGetValue(eventType,out actions))
        {
            actions = new List<invokeActionBase>();
            eventDict.Add(eventType, actions);
        }
        actions.Add(action);
    }

    #region 取消注册的相关方法

    public void UnRegister(EventType eventType, Action action)
    {
        List<invokeActionBase> actions = null;
        if (eventDict.TryGetValue(eventType, out actions))
        {
            for (int i = 0; i < actions.Count; i++)
            {
                var invokeaction = actions[i] as invokeAction;
                if (invokeaction.aciton.Equals(action))
                {
                    actions.RemoveAt(i);
                    break;
                }
            }
        }
    }
    public void UnRegister<T>(EventType eventType, Action<T> action) {
        List<invokeActionBase> actions = null;
        if (eventDict.TryGetValue(eventType, out actions))
        {
            for (int i = 0; i < actions.Count; i++) {
                var invokeaction = actions[i] as invokeAction<T>;
                if (invokeaction.aciton.Equals(action)) {
                    actions.RemoveAt(i);
                    break;
                }
            }
        }
    }
    public void UnRegister<T1,T2>(EventType eventType, Action<T1, T2> action)
    {
        List<invokeActionBase> actions = null;
        if (eventDict.TryGetValue(eventType, out actions))
        {
            for (int i = 0; i < actions.Count; i++)
            {
                var invokeaction = actions[i] as invokeAction<T1, T2>;
                if (invokeaction.aciton.Equals(action))
                {
                    actions.RemoveAt(i);
                    break;
                }
            }
        }
    }
    public void UnRegister<T1, T2,T3>(EventType eventType, Action<T1, T2, T3> action)
    {
        List<invokeActionBase> actions = null;
        if (eventDict.TryGetValue(eventType, out actions))
        {
            for (int i = 0; i < actions.Count; i++)
            {
                var invokeaction = actions[i] as invokeAction<T1, T2, T3>;
                if (invokeaction.aciton.Equals(action))
                {
                    actions.RemoveAt(i);
                    break;
                }
            }
        }
    }
    public void UnRegister<T1, T2, T3,T4>(EventType eventType, Action<T1, T2, T3, T4> action)
    {
        List<invokeActionBase> actions = null;
        if (eventDict.TryGetValue(eventType, out actions))
        {
            for (int i = 0; i < actions.Count; i++)
            {
                var invokeaction = actions[i] as invokeAction<T1, T2, T3, T4>;
                if (invokeaction.aciton.Equals(action))
                {
                    actions.RemoveAt(i);
                    break;
                }
            }
        }
    }
    #endregion


    #region 事件派发相关方法
    public void Dispatch(EventType eventType)
    {
        var action = eventDict[eventType];
        for (int i = 0; i < eventDict[eventType].Count; i++)
        {
            (action[i] as invokeAction).Invoke();
        }
    }
    public void Dispatch<T>(EventType eventType, T args)
    {
        var action = eventDict[eventType];
        for (int i = 0; i < eventDict[eventType].Count; i++)
        {
            (action[i] as invokeAction<T>).Invoke(args);
        }
    }
    public void Dispatch<T1,T2>(EventType eventType, T1 arg1, T2 arg2)
    {
        var action = eventDict[eventType];
        for (int i = 0; i < eventDict[eventType].Count; i++)
        {
            (action[i] as invokeAction<T1, T2>).Invoke(arg1, arg2);
        }
    }
    public void Dispatch<T1, T2,T3>(EventType eventType, T1 arg1, T2 arg2,T3 arg3)
    {
        var action = eventDict[eventType];
        for (int i = 0; i < eventDict[eventType].Count; i++)
        {
            (action[i] as invokeAction<T1, T2, T3>).Invoke(arg1, arg2,arg3);
        }
    }
    public void Dispatch<T1, T2, T3,T4>(EventType eventType, T1 arg1, T2 arg2, T3 arg3,T4 arg4) {
        var action = eventDict[eventType];
        for (int i = 0; i < eventDict[eventType].Count; i++)
        {
            (action[i]as invokeAction<T1, T2, T3, T4>).Invoke(arg1, arg2, arg3,arg4);
        }
    }
    #endregion
}
